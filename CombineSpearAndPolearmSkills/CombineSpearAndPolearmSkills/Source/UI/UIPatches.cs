using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace CombineSpearAndPolearmSkills
{
    [HarmonyPatch]
    internal class UIPatches
    {
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillList)), HarmonyPostfix]
        private static void GetUISkillListPatch(Skills __instance, ref List<Skills.Skill> __result)
        {
            if (Helper.HasAugaInstalled())
            {
                return;
            }

            if (CombineConfig.ShowCombinedSkillsMenuEntries.Value)
            {
                ApplyCombinedSkillsMenuEntries(__instance, ref __result);
                FixLevelBarLengths(ref __result);
            }

            if (CombineConfig.SkillsMenuSorting.Value != SortSkillsMenu.Disabled)
            {
                __result.Sort((a, b) => a.CustomCompare(b));
            }
        }

        // TODO does not work with auga

        private static void FixLevelBarLengths(ref List<Skills.Skill> uiSkillList)
        {
            for (int i = 0; i < uiSkillList.Count; i++)
            {
                Skills.Skill skill = uiSkillList[i];

                var newSkill = skill.ShallowCopy();
                newSkill.m_level = Mathf.Floor(skill.m_level);

                uiSkillList[i] = newSkill;
            }
        }

        private static void ApplyCombinedSkillsMenuEntries(Skills skillsSet, ref List<Skills.Skill> uiSkillList)
        {
            var skillsToDelete = new HashSet<Skills.SkillType>();
            var skillsToAdd = new List<Skills.Skill>();

            foreach (var item in SkillSetupInfo.supportedTypes)
            {
                CombinedSkill? combinedSkill = CombinedSkill.GetCombinedSkill(skillsSet, item);

                if (combinedSkill == null)
                {
                    continue;
                }

                var thisSkill = combinedSkill.Value.thisSkill;
                var otherSkill = combinedSkill.Value.otherSkill;

                // skills aren't synced anymore, display actual skills
                if (thisSkill.CompareSkillLevel(otherSkill) != 0)
                {
                    continue;
                }

                var newSkill = thisSkill.ShallowCopy();

                switch (otherSkill.m_info.m_skill)
                {
                    case Skills.SkillType.Spears:
                        newSkill.m_info.m_icon = SpriteLoader.polearmSprite;
                        break;

                    case Skills.SkillType.Swords:
                        newSkill.m_info.m_icon = SpriteLoader.slashSprite;
                        break;

                    case Skills.SkillType.Unarmed:
                        newSkill.m_info.m_icon = SpriteLoader.rogueSprite;
                        break;

                    case Skills.SkillType.Bows:
                        newSkill.m_info.m_icon = SpriteLoader.archerySprite;
                        break;
                }

                skillsToAdd.Add(newSkill);

                skillsToDelete.Add(thisSkill.m_info.m_skill);
                skillsToDelete.Add(otherSkill.m_info.m_skill);
            }

            for (int i = uiSkillList.Count - 1; i >= 0; i--)
            {
                Skills.Skill item = uiSkillList[i];

                if (skillsToDelete.Contains(item.m_info.m_skill))
                {
                    uiSkillList.RemoveAt(i);
                }
            }

            uiSkillList.AddRange(skillsToAdd);
        }

        private static bool safeToCallSetup = false;

        [HarmonyPatch(typeof(SkillsDialog), nameof(SkillsDialog.Setup)), HarmonyPostfix]
        private static void AfterFirstSkillsDialogSetup()
        {
            safeToCallSetup = true;
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPrefix]
        internal static void ResetOnLogout()
        {
            safeToCallSetup = false;
        }

        internal static void UpdateSkillsMenu(Player player)
        {
            if (!safeToCallSetup
                || !Helper.HasAugaInstalled()
                || !CombineConfig.UpdateSkillsMenuOnChange.Value
                || !InventoryGui.instance || !InventoryGui.instance.m_skillsDialog
                || !InventoryGui.instance.m_skillsDialog.gameObject
                || !InventoryGui.instance.m_skillsDialog.gameObject.activeSelf
                || !Player.m_localPlayer || player != Player.m_localPlayer)
            {
                return;
            }

            InventoryGui.instance.m_skillsDialog.Setup(player);
        }

        [HarmonyPatch(typeof(Localization), nameof(Localization.Translate)), HarmonyPostfix]
        public static void Translate_Postfix(Localization __instance, string word, ref string __result)
        {
            if (!CombineConfig.DisplayCombinedSkillNameAndTooltip.Value)
            {
                return;
            }

            if (CombineConfig.SyncCrossbowsWith.Value == CrossbowSync.SyncWithBows)
            {
                if (word == "skill_crossbows" || word == "skill_bows")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("ArcherySkillName"));
                }
                else if (word == "skill_crossbows_description" || word == "skill_bows_description")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("ArcherySkillDescription"));
                }
            }

            if (CombineConfig.SyncPolearmsWith.Value == PolearmSync.SyncWithSpears)
            {
                if (word == "skill_polearms" || word == "skill_spears")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("CombinedPolearmsSkillName"));
                }
                else if (word == "skill_polearms_description" || word == "skill_spears_description")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("CombinedPolearmsSkillDescription"));
                }
            }

            if (CombineConfig.SyncKnivesWith.Value == KnifeSync.SyncWithFists)
            {
                if (word == "skill_knives" || word == "skill_unarmed")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("RogueWeaponsSkillName"));
                }
                else if (word == "skill_knives_description" || word == "skill_unarmed_description")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("RogueWeaponsSkillDescription"));
                }
            }

            if (CombineConfig.SyncKnivesWith.Value == KnifeSync.SyncWithSwords)
            {
                if (word == "skill_knives" || word == "skill_swords")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("BladesSkillName"));
                }
                else if (word == "skill_knives_description" || word == "skill_swords_description")
                {
                    __result = __instance.Translate(LocalizationLoader.ToTranslateKey("BladesSkillDescription"));
                }
            }
        }
    }
}