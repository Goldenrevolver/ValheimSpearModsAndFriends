using BepInEx;
using BepInEx.Configuration;
using System;

namespace CombineSpearAndPolearmSkills
{
    internal class CombineConfig
    {
        internal static ConfigEntry<ChangeStyle> SynchronizationStyle;

        internal static ConfigEntry<bool> DisplayCombinedSkillNameAndTooltip;

        //internal static ConfigEntry<bool> ForceResetSkillPolearmCombination;
        //internal static ConfigEntry<bool> ForceResetSkillKnifeCombination;
        //internal static ConfigEntry<bool> ForceResetSkillCrossbowCombination;
        //internal static ConfigEntry<CombineSkillStyle> ForceResetCombineSkillStyle;

        internal static ConfigEntry<bool> SyncStatusEffectSkillExpMultiplier;
        internal static ConfigEntry<bool> SyncStatusEffectSkillLevelBoosts;

        internal static ConfigEntry<CombineSkillStyle> CombineSkillStyle;

        internal static ConfigEntry<PolearmSync> SyncPolearmsWith;
        internal static ConfigEntry<KnifeSync> SyncKnivesWith;
        internal static ConfigEntry<CrossbowSync> SyncCrossbowsWith;

        internal static ConfigEntry<SortSkillsMenu> SkillsMenuSorting;
        internal static ConfigEntry<bool> ShowCombinedSkillsMenuEntries;
        internal static ConfigEntry<bool> UpdateSkillsMenuOnChange;

        internal static ConfigEntry<bool> ShowDebugLogs;

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            var sectionName = "0 - General";

            CombineSkillStyle = plugin.Config.Bind(sectionName, nameof(CombineSkillStyle), CombineSpearAndPolearmSkills.CombineSkillStyle.RecalculateSum, string.Empty);
            SynchronizationStyle = plugin.Config.Bind(sectionName, nameof(SynchronizationStyle), ChangeStyle.SetToHigher, string.Empty);

            DisplayCombinedSkillNameAndTooltip = plugin.Config.Bind(sectionName, nameof(DisplayCombinedSkillNameAndTooltip), false, string.Empty);

            SyncStatusEffectSkillExpMultiplier = plugin.Config.Bind(sectionName, nameof(SyncStatusEffectSkillExpMultiplier), true, string.Empty);
            SyncStatusEffectSkillLevelBoosts = plugin.Config.Bind(sectionName, nameof(SyncStatusEffectSkillLevelBoosts), true, string.Empty);

            sectionName = "1 - Combinations";

            SyncPolearmsWith = plugin.Config.Bind(sectionName, nameof(SyncPolearmsWith), PolearmSync.SyncWithSpears, string.Empty);
            SyncPolearmsWith.SettingChanged += (a, b) => ResetCombinationAndCheckForNewMerge(Skills.SkillType.Polearms);
            SyncKnivesWith = plugin.Config.Bind(sectionName, nameof(SyncKnivesWith), KnifeSync.Disabled, string.Empty);
            SyncKnivesWith.SettingChanged += (a, b) => ResetCombinationAndCheckForNewMerge(Skills.SkillType.Knives);
            SyncCrossbowsWith = plugin.Config.Bind(sectionName, nameof(SyncCrossbowsWith), CrossbowSync.Disabled, string.Empty);
            SyncCrossbowsWith.SettingChanged += (a, b) => { ResetCombinationAndCheckForNewMerge(Skills.SkillType.Crossbows); };

            sectionName = "2 - Skill Menu";

            ShowCombinedSkillsMenuEntries = plugin.Config.Bind(sectionName, nameof(ShowCombinedSkillsMenuEntries), false, string.Empty);
            ShowCombinedSkillsMenuEntries.SettingChanged += SkillsMenuSettingChanged;
            SkillsMenuSorting = plugin.Config.Bind(sectionName, nameof(SkillsMenuSorting), SortSkillsMenu.ByLevel, string.Empty);
            SkillsMenuSorting.SettingChanged += SkillsMenuSettingChanged;
            UpdateSkillsMenuOnChange = plugin.Config.Bind(sectionName, nameof(UpdateSkillsMenuOnChange), true, string.Empty);
            UpdateSkillsMenuOnChange.SettingChanged += SkillsMenuSettingChanged;

            sectionName = "9 - Debugging";

            ShowDebugLogs = plugin.Config.Bind(sectionName, nameof(ShowDebugLogs), false, string.Empty);

            /*
            ForceResetCombineSkillStyle = plugin.Config.Bind(sectionName, nameof(ForceResetCombineSkillStyle), CombineSpearAndPolearmSkills.CombineSkillStyle.SetToHighest, string.Empty);

            ForceResetSkillPolearmCombination = plugin.Config.Bind(sectionName, nameof(ForceResetSkillPolearmCombination), false, string.Empty);
            ForceResetSkillPolearmCombination.SettingChanged += ForceResetSkillPolearmCombination_SettingChanged;
            ForceResetSkillKnifeCombination = plugin.Config.Bind(sectionName, nameof(ForceResetSkillKnifeCombination), false, string.Empty);
            ForceResetSkillKnifeCombination.SettingChanged += ForceResetSkillKnifeCombination_SettingChanged;
            ForceResetSkillCrossbowCombination = plugin.Config.Bind(sectionName, nameof(ForceResetSkillCrossbowCombination), false, string.Empty);
            ForceResetSkillCrossbowCombination.SettingChanged += ForceResetSkillCrossbowCombination_SettingChanged;
            */
        }

        private static void SkillsMenuSettingChanged(object sender, EventArgs e)
        {
            UIPatches.UpdateSkillsMenu(Player.m_localPlayer);
        }

        private static void ResetCombinationAndCheckForNewMerge(Skills.SkillType skillType)
        {
            PlayerSkillRecalculateModule.CheckForExpRecalculate(Player.m_localPlayer, skillType);
            SkillsMenuSettingChanged(null, null);
        }

        /*
        private static void ForceResetSkillPolearmCombination_SettingChanged(object sender, EventArgs e)
        {
            if (ForceResetSkillPolearmCombination.Value)
            {
                PlayerSkillRecalculateModule.CheckForExpRecalculate(Player.m_localPlayer, Skills.SkillType.Polearms);
            }
        }

        private static void ForceResetSkillKnifeCombination_SettingChanged(object sender, EventArgs e)
        {
            if (ForceResetSkillKnifeCombination.Value)
            {
                PlayerSkillRecalculateModule.CheckForExpRecalculate(Player.m_localPlayer, Skills.SkillType.Knives);
            }
        }

        private static void ForceResetSkillCrossbowCombination_SettingChanged(object sender, EventArgs e)
        {
            if (ForceResetSkillCrossbowCombination.Value)
            {
                PlayerSkillRecalculateModule.CheckForExpRecalculate(Player.m_localPlayer, Skills.SkillType.Crossbows);
            }
        }
        */
    }

    public enum SortSkillsMenu
    {
        Disabled = 0,
        ByName = 1,
        ByLevel = 2,
    }

    public enum ChangeStyle
    {
        SetToHigher = 0,
        RaiseThenSetToHigher = 1,
    }

    public enum CombineSkillStyle
    {
        RecalculateSum = 0,
        SetToHighest = 1,
    }

    public enum PolearmSync
    {
        Disabled = 0,
        SyncWithSpears = 1,
    }

    public enum CrossbowSync
    {
        Disabled = 0,
        SyncWithBows = 1,
    }

    public enum KnifeSync
    {
        Disabled = 0,
        SyncWithFists = 1,
        SyncWithSwords = 2,
    }
}