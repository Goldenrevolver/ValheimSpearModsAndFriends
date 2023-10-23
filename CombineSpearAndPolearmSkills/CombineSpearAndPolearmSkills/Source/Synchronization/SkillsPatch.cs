using HarmonyLib;
using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    [HarmonyPatch(typeof(Skills))]
    public class SkillsPatch
    {
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(nameof(Skills.RaiseSkill)), HarmonyPostfix]
        private static void RaiseSkillPatch(Skills __instance, Skills.SkillType skillType, float factor)
        {
            if (__instance.m_player != Player.m_localPlayer)
            {
                return;
            }

            UIPatches.UpdateSkillsMenu(__instance.m_player);

            var combined = CombinedSkill.GetCombinedSkill(__instance, skillType);

            if (combined == null)
            {
                return;
            }

            var thisSkill = combined.Value.thisSkill;
            var otherSkill = combined.Value.otherSkill;

            if (CombineConfig.SynchronizationStyle.Value == ChangeStyle.SetToHigher)
            {
                int levelDiff = Mathf.FloorToInt(thisSkill.m_level) - Mathf.FloorToInt(otherSkill.m_level);

                otherSkill.m_level = thisSkill.m_level;
                otherSkill.m_accumulator = thisSkill.m_accumulator;

                if (levelDiff > 0)
                {
                    __instance.m_player.OnSkillLevelup(otherSkill.m_info.m_skill, otherSkill.m_level);
                }
            }
            else
            {
                bool raiseIncrementedLevel = otherSkill.Raise(factor);

                int levelDiff = Mathf.FloorToInt(thisSkill.m_level) - Mathf.FloorToInt(otherSkill.m_level);

                PlayerSkillRecalculateModule.SetToHigher(thisSkill, otherSkill);

                if (levelDiff > 0 || raiseIncrementedLevel)
                {
                    __instance.m_player.OnSkillLevelup(otherSkill.m_info.m_skill, otherSkill.m_level);
                }
            }

            if (thisSkill.m_level != otherSkill.m_level || thisSkill.m_accumulator != otherSkill.m_accumulator)
            {
                Helper.Log("Combined skill got out of sync");
            }
        }
    }
}