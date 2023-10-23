using System.Globalization;
using UnityEngine;
using static CombineSpearAndPolearmSkills.PlayerSkillRecalculateModule;

namespace CombineSpearAndPolearmSkills
{
    internal class CustomDataAccessor
    {
        private const string previousTotalExpKeyPrefix = CombineSpearAndPolearmSkillsPlugin.GUID + ".previousTotalExp.";
        private const string previousTotalExpKeyPartnerSuffix = CombineSpearAndPolearmSkillsPlugin.GUID + ".partnerSkill";

        internal const string recalculateSumValue = "RecalculateSum";
        internal const string setToHighestValue = "SetToHighest";

        internal static void SaveEverythingInCustomData(Player player, Skills.Skill thisSkill, float totalPrimaryExp, Skills.Skill otherSkill, float totalSecondaryExp, SkillSetupInfo info, CombineSkillStyle combineSkillStyle)
        {
            if (thisSkill.m_info.m_increseStep != otherSkill.m_info.m_increseStep)
            {
                Helper.LogWarning($"Combined skills {thisSkill.m_info.m_skill} and {otherSkill.m_info.m_skill} have different step size ({thisSkill.m_info.m_increseStep} vs {otherSkill.m_info.m_increseStep}), so they will desync when used in 'raise' mode!");
            }

            player.m_customData[info.lastSkillTypeKey] = otherSkill.m_info.m_skill.ToString();
            player.m_customData[info.lastCombineMethodKey] = combineSkillStyle == CombineSkillStyle.RecalculateSum ? recalculateSumValue : setToHighestValue;

            var primaryKey = $"{previousTotalExpKeyPrefix}{thisSkill.m_info.m_skill}";
            var secondaryKey = $"{previousTotalExpKeyPrefix}{thisSkill.m_info.m_skill}{previousTotalExpKeyPartnerSuffix}";

            player.m_customData[primaryKey] = totalPrimaryExp.ToString(CultureInfo.InvariantCulture);
            player.m_customData[secondaryKey] = totalSecondaryExp.ToString(CultureInfo.InvariantCulture);
        }

        internal static void RevertCombinedSkill(Player player, CombinedSkill? combined, string lastSkillKeyToRemove, string lastMethodKeyToRemove)
        {
            if (combined == null)
            {
                return;
            }

            var thisSkill = combined.Value.thisSkill;
            var otherSkill = combined.Value.otherSkill;

            var primaryKey = $"{previousTotalExpKeyPrefix}{thisSkill.m_info.m_skill}";
            var secondaryKey = $"{previousTotalExpKeyPrefix}{thisSkill.m_info.m_skill}{previousTotalExpKeyPartnerSuffix}";

            if (!player.m_customData.TryGetValue(primaryKey, out string previousTotalPrimaryExpData)
                || !player.m_customData.TryGetValue(secondaryKey, out string previousTotalSecondaryExpData))
            {
                Helper.LogWarning("Either no previousTotalPrimaryExp or no previousTotalSecondaryExp found");
                return;
            }

            if (!float.TryParse(previousTotalPrimaryExpData, NumberStyles.Any, CultureInfo.InvariantCulture, out float previousTotalPrimaryExp)
                || !float.TryParse(previousTotalSecondaryExpData, NumberStyles.Any, CultureInfo.InvariantCulture, out float previousTotalSecondaryExp))
            {
                Helper.LogWarning("Failed parsing previousTotalPrimaryExp or previousTotalSecondaryExp");
                return;
            }

            if (!player.m_customData.ContainsKey(lastSkillKeyToRemove))
            {
                Helper.LogWarning("No lastSkill found");
                return;
            }

            player.m_customData.Remove(lastSkillKeyToRemove);

            if (!player.m_customData.TryGetValue(lastMethodKeyToRemove, out string lastCombineMethod))
            {
                Helper.LogWarning("No lastCombineMethod found");
                return;
            }

            player.m_customData.Remove(lastCombineMethod);

            player.m_customData.Remove(primaryKey);
            player.m_customData.Remove(secondaryKey);

            float currentTotalPrimaryExp = GetTotalExpEarnedForSkill(thisSkill);
            float currentTotalSecondaryExp = GetTotalExpEarnedForSkill(otherSkill);

            float earnedExp = 0f;

            if (lastCombineMethod == recalculateSumValue)
            {
                float previousTotalExp = previousTotalPrimaryExp + previousTotalSecondaryExp;

                earnedExp += currentTotalPrimaryExp - previousTotalExp;
                earnedExp += currentTotalSecondaryExp - previousTotalExp;
            }
            else if (lastCombineMethod == setToHighestValue)
            {
                float previousHighest = Mathf.Max(previousTotalPrimaryExp, previousTotalSecondaryExp);

                earnedExp += currentTotalSecondaryExp - previousHighest;
                earnedExp += currentTotalPrimaryExp - previousHighest;
            }

            earnedExp = Mathf.Abs(earnedExp) < 0.001f ? 0f : earnedExp / 2f;

            Helper.Log($"Revert: earned exp for each skill {earnedExp} (already halved)");

            RecalculateLevel(thisSkill, previousTotalPrimaryExp + earnedExp);
            RecalculateLevel(otherSkill, previousTotalSecondaryExp + earnedExp);
        }
    }
}