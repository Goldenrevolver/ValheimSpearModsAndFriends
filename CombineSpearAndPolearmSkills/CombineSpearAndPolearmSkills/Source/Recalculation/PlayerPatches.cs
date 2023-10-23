using HarmonyLib;

namespace CombineSpearAndPolearmSkills
{
    [HarmonyPatch]
    public class PlayerPatches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned)), HarmonyPostfix]
        public static void OnSpawnedPatch(Player __instance)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            //foreach (var skill in __instance.m_skills.m_skillData.Values)
            //{
            //    Helper.Log($"Player {__instance.GetPlayerName()} on spawned: {skill.m_info.m_skill} {skill.m_level} {skill.m_accumulator}");
            //}

            PlayerSkillRecalculateModule.AdjustExpMultipliers(__instance);

            foreach (var skillType in SkillSetupInfo.supportedTypes)
            {
                PlayerSkillRecalculateModule.CheckForExpRecalculate(__instance, skillType);
            }
        }

        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        private static void Postfix(Humanoid __instance, bool __result)
        {
            // if the item wasn't equipped, skip
            if (!__result)
            {
                return;
            }

            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            UIPatches.UpdateSkillsMenu(Player.m_localPlayer);
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        private static void Postfix(Humanoid __instance)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            UIPatches.UpdateSkillsMenu(Player.m_localPlayer);
        }
    }
}