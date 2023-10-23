using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    internal class PlayerSkillRecalculateModule
    {
        internal static void AdjustExpMultipliers(Player player)
        {
            if (CombineConfig.AdjustExpMultipliers.Value)
            {
                return;
            }

            // archery

            var crossbowSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Crossbows);
            var bowSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Bows);

            float archeryStepMult = Mathf.Max(crossbowSkillDef.m_increseStep, bowSkillDef.m_increseStep);

            crossbowSkillDef.m_increseStep = archeryStepMult;
            bowSkillDef.m_increseStep = archeryStepMult;

            // polearms

            var spearSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Spears);
            var atgeirSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Polearms);

            float polearmStepMult = (atgeirSkillDef.m_increseStep + spearSkillDef.m_increseStep) / 2f;

            spearSkillDef.m_increseStep = polearmStepMult;
            atgeirSkillDef.m_increseStep = polearmStepMult;

            // rogue weapons

            var unarmedSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Unarmed);
            var knivesSkillDef = player.m_skills.GetSkillDef(Skills.SkillType.Knives);

            float rogueStepMult = (unarmedSkillDef.m_increseStep + knivesSkillDef.m_increseStep) / 2f;

            unarmedSkillDef.m_increseStep = rogueStepMult;
            knivesSkillDef.m_increseStep = rogueStepMult;
        }

        internal static void CheckForExpRecalculate(Player player, Skills.SkillType skillType)
        {
            if (!player || player != Player.m_localPlayer)
            {
                return;
            }

            var info = SkillSetupInfo.GetSupportedTypeData(skillType);

            if (player.m_customData == null)
            {
                player.m_customData = new Dictionary<string, string>();
            }

            CombineSkillStyle? combineSkillStyle = null;

            Skills.SkillType lastOtherSkill = 0;

            if (!player.m_customData.TryGetValue(info.lastSkillTypeKey, out string lastOtherSkillString))
            {
                combineSkillStyle = CombineConfig.CombineSkillStyle.Value;
            }
            else if (!Enum.TryParse(lastOtherSkillString, out lastOtherSkill))
            {
                lastOtherSkill = 0;
            }

            var combinedSkills = CombinedSkill.GetCombinedSkill(player.m_skills, skillType);
            var currentOtherSkill = combinedSkills?.otherSkill?.m_info?.m_skill;

            Helper.Log($"pre revert check data: last skill: {lastOtherSkill} ({lastOtherSkillString}), current skill: {currentOtherSkill}");

            bool shouldSuppressSecondTotalExpLog = false;

            if (lastOtherSkill != Skills.SkillType.None && lastOtherSkill != currentOtherSkill)
            {
                shouldSuppressSecondTotalExpLog = true;
                CustomDataAccessor.RevertCombinedSkill(player, CombinedSkill.CreateCombinedSkill(player.m_skills, skillType, lastOtherSkill), info.lastSkillTypeKey, info.lastCombineMethodKey);
            }

            if (currentOtherSkill != null && lastOtherSkill != currentOtherSkill.Value)
            {
                combineSkillStyle = CombineConfig.CombineSkillStyle.Value;
            }

            if (combineSkillStyle == null)
            {
                // this is an intended exit in most situations, not an edge case
                //Helper.Log("combine skill style is null");

                return;
            }

            if (combinedSkills == null)
            {
                Helper.Log("combined skills is null");
                return;
            }

            var thisSkill = combinedSkills.Value.thisSkill;
            var otherSkill = combinedSkills.Value.otherSkill;

            float totalPrimaryExp = GetTotalExpEarnedForSkill(thisSkill, shouldSuppressSecondTotalExpLog);
            float totalSecondaryExp = GetTotalExpEarnedForSkill(otherSkill, shouldSuppressSecondTotalExpLog);

            CustomDataAccessor.SaveEverythingInCustomData(player, thisSkill, totalPrimaryExp, otherSkill, totalSecondaryExp, info, combineSkillStyle.Value);

            if (combineSkillStyle == CombineSkillStyle.RecalculateSum)
            {
                SetToTotalRecalculatedLevel(thisSkill, otherSkill, totalPrimaryExp + totalSecondaryExp);
            }
            else if (combineSkillStyle == CombineSkillStyle.SetToHighest)
            {
                SetToHigher(thisSkill, otherSkill);
            }
        }

        private static void SetToTotalRecalculatedLevel(Skills.Skill thisSkill, Skills.Skill otherSkill, float totalCombinedExp)
        {
            RecalculateLevel(thisSkill, totalCombinedExp);

            // if we are in 'set to higher', 'RecalculateLevel(otherSkill, totalCombinedExp)' will get the same result too, so no need to calculate it again
            if (CombineConfig.SynchronizationStyle.Value == ChangeStyle.SetToHigher)
            {
                Helper.Log($"Setting {otherSkill.m_info.m_skill} to same level");

                otherSkill.m_level = thisSkill.m_level;
                otherSkill.m_accumulator = thisSkill.m_accumulator;
            }
            else
            {
                RecalculateLevel(otherSkill, totalCombinedExp);

                SetToHigher(thisSkill, otherSkill);
            }
        }

        internal static void SetToHigher(Skills.Skill thisSkill, Skills.Skill otherSkill)
        {
            if (thisSkill.CompareSkillLevel(otherSkill) >= 0)
            {
                otherSkill.m_level = thisSkill.m_level;
                otherSkill.m_accumulator = thisSkill.m_accumulator;
            }
            else
            {
                thisSkill.m_level = otherSkill.m_level;
                thisSkill.m_accumulator = otherSkill.m_accumulator;
            }

            Helper.Log($"Set both skills to level {thisSkill.m_level}, progress {thisSkill.m_accumulator}");
        }

        internal static void RecalculateLevel(Skills.Skill skill, float totalCombinedExp)
        {
            float leftOverExp = totalCombinedExp;

            skill.m_level -= Mathf.Floor(skill.m_level);
            skill.m_accumulator = 0;

            float expForNextLevel = skill.GetNextLevelRequirement();

            while (expForNextLevel <= leftOverExp && skill.m_level < 100f)
            {
                leftOverExp -= expForNextLevel;
                skill.m_level++;
                expForNextLevel = skill.GetNextLevelRequirement();
            }

            if (skill.m_level < 100)
            {
                skill.m_accumulator = leftOverExp;
            }

            Helper.Log($"Recalculate total exp for skill {skill.m_info.m_skill}: level {skill.m_level}, progress {leftOverExp}, total exp {totalCombinedExp}");
        }

        internal static float GetTotalExpEarnedForSkill(Skills.Skill skill, bool suppressLog = false)
        {
            // could also create a copy through serialization, but these are only fields, so no side effects
            float currentLevel = skill.m_level;

            float totalExp = skill.m_accumulator;

            // done in case some mod or the base game ever changes GetNextLevelRequirement to look at the progress (unlikely, but whatever)
            float currentProgress = skill.m_accumulator;
            skill.m_accumulator = 0;

            while (skill.m_level > 0)
            {
                skill.m_level--;
                totalExp += skill.GetNextLevelRequirement();
            }

            skill.m_level = currentLevel;
            skill.m_accumulator = currentProgress;

            if (!suppressLog)
            {
                Helper.Log($"Get total exp for skill {skill.m_info.m_skill}: level {currentLevel}, progress {currentProgress}, total exp {totalExp}");
            }

            return totalExp;
        }
    }
}