using BepInEx.Configuration;
using ServerSync;
using System;
using static CombineSpearAndPolearmSkills.CombineSpearAndPolearmSkillsPlugin;

namespace CombineSpearAndPolearmSkills
{
    internal class CombineConfig
    {
        internal static ConfigEntry<bool> DisplayCombinedSkillNameAndTooltip;

        internal static ConfigEntry<CombineSkillStyle> CombineSkillStyle;
        internal static ConfigEntry<ChangeStyle> SynchronizationStyle;
        internal static ConfigEntry<bool> SyncStatusEffectRaiseSkillEffects;
        internal static ConfigEntry<bool> SyncStatusEffectSkillLevelEffects;
        internal static ConfigEntry<bool> AdjustExpMultipliers;

        internal static ConfigEntry<PolearmSync> SyncPolearmsWith;
        internal static ConfigEntry<KnifeSync> SyncKnivesWith;
        internal static ConfigEntry<CrossbowSync> SyncCrossbowsWith;

        internal static ConfigEntry<SortSkillsMenu> SkillsMenuSorting;
        internal static ConfigEntry<bool> ShowCombinedSkillsMenuEntries;
        internal static ConfigEntry<bool> UpdateSkillsMenuOnChange;

        internal static ConfigEntry<bool> ShowDebugLogs;

        internal static ConfigSync serverSyncInstance;
        internal static ConfigEntry<bool> UseServerSync;

        internal static void LoadConfig(CombineSpearAndPolearmSkillsPlugin plugin)
        {
            var sectionName = "0 - General";

            serverSyncInstance = ServerSyncWrapper.CreateRequiredConfigSync(GUID, NAME, VERSION);

            UseServerSync = plugin.Config.BindHiddenForceEnabledSyncLocker(serverSyncInstance, sectionName, nameof(UseServerSync));

            DisplayCombinedSkillNameAndTooltip = plugin.Config.Bind(sectionName, nameof(DisplayCombinedSkillNameAndTooltip), true, string.Empty);

            sectionName = "1 - Combinations";

            SyncPolearmsWith = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SyncPolearmsWith), PolearmSync.SyncWithSpears, string.Empty);
            SyncPolearmsWith.SettingChanged += (a, b) => ResetCombinationAndCheckForNewMerge(Skills.SkillType.Polearms);
            SyncKnivesWith = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SyncKnivesWith), KnifeSync.Disabled, string.Empty);
            SyncKnivesWith.SettingChanged += (a, b) => ResetCombinationAndCheckForNewMerge(Skills.SkillType.Knives);
            SyncCrossbowsWith = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SyncCrossbowsWith), CrossbowSync.Disabled, string.Empty);
            SyncCrossbowsWith.SettingChanged += (a, b) => ResetCombinationAndCheckForNewMerge(Skills.SkillType.Crossbows);

            sectionName = "2 - Skill Menu";

            ShowCombinedSkillsMenuEntries = plugin.Config.Bind(sectionName, nameof(ShowCombinedSkillsMenuEntries), true, string.Empty);
            ShowCombinedSkillsMenuEntries.SettingChanged += SkillsMenuSettingChanged;
            SkillsMenuSorting = plugin.Config.Bind(sectionName, nameof(SkillsMenuSorting), SortSkillsMenu.ByLevel, string.Empty);
            SkillsMenuSorting.SettingChanged += SkillsMenuSettingChanged;
            UpdateSkillsMenuOnChange = plugin.Config.Bind(sectionName, nameof(UpdateSkillsMenuOnChange), true, string.Empty);
            UpdateSkillsMenuOnChange.SettingChanged += SkillsMenuSettingChanged;

            sectionName = "3 - Technical & Math";

            CombineSkillStyle = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(CombineSkillStyle), CombineSpearAndPolearmSkills.CombineSkillStyle.RecalculateSum, string.Empty);
            SynchronizationStyle = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SynchronizationStyle), ChangeStyle.SetToHigher, string.Empty);

            SyncStatusEffectRaiseSkillEffects = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SyncStatusEffectRaiseSkillEffects), true, string.Empty);
            SyncStatusEffectSkillLevelEffects = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(SyncStatusEffectSkillLevelEffects), true, string.Empty);

            AdjustExpMultipliers = plugin.Config.BindSynced(serverSyncInstance, sectionName, nameof(AdjustExpMultipliers), true, string.Empty);

            sectionName = "9 - Debugging";

            ShowDebugLogs = plugin.Config.Bind(sectionName, nameof(ShowDebugLogs), false, string.Empty);
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
        //SyncWithSwords = 2,
    }
}