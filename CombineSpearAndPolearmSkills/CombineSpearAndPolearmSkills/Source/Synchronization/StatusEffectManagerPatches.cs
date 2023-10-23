using HarmonyLib;
using System;

namespace CombineSpearAndPolearmSkills
{
    [HarmonyPatch(typeof(SEMan))]
    public class StatusEffectManagerPatches
    {
        [HarmonyPatch(nameof(SEMan.ModifyRaiseSkill), new Type[] { typeof(Skills.SkillType), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref }), HarmonyPostfix]
        public static void ModifyRaiseSkill(SEMan __instance, Skills.SkillType skill, ref float multiplier)
        {
            if (!CombineConfig.SyncStatusEffectRaiseSkillEffects.Value)
            {
                return;
            }

            if (__instance.m_character != Player.m_localPlayer)
            {
                return;
            }

            var flippedSkill = SkillFlipper.FlipToCombinedSkill(skill);

            if (flippedSkill == Skills.SkillType.None)
            {
                return;
            }

            foreach (StatusEffect statusEffect in __instance.m_statusEffects)
            {
                statusEffect.ModifyRaiseSkill(flippedSkill, ref multiplier);
            }
        }

        [HarmonyPatch(nameof(SEMan.ModifySkillLevel), new Type[] { typeof(Skills.SkillType), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref }), HarmonyPostfix]
        public static void ModifySkillLevel(SEMan __instance, Skills.SkillType skill, ref float level)
        {
            if (!CombineConfig.SyncStatusEffectSkillLevelEffects.Value)
            {
                return;
            }

            if (__instance.m_character != Player.m_localPlayer)
            {
                return;
            }

            var flippedSkill = SkillFlipper.FlipToCombinedSkill(skill);

            if (flippedSkill == Skills.SkillType.None)
            {
                return;
            }

            foreach (StatusEffect statusEffect in __instance.m_statusEffects)
            {
                statusEffect.ModifySkillLevel(flippedSkill, ref level);
            }
        }
    }
}