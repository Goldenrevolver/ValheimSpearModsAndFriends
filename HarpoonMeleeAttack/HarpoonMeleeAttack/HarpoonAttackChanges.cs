using HarmonyLib;

namespace HarpoonMeleeAttack
{
    [HarmonyPatch]
    internal class HarpoonAttackChanges
    {
        internal static bool IsHarpoon(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_name == "$item_spear_chitin";
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack)), HarmonyPrefix]
        public static void RemoveHarpoonMeleeStatus(Attack __instance, ref StatusEffect __state)
        {
            __state = null;

            if (IsHarpoon(__instance.m_weapon.m_shared))
            {
                if (__instance.m_attackProjectile == null)
                {
                    __state = __instance.m_weapon.m_shared.m_attackStatusEffect;
                    __instance.m_weapon.m_shared.m_attackStatusEffect = null;
                }
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack)), HarmonyPostfix]
        public static void ReaddHarpoonMeleeStatus(Attack __instance, StatusEffect __state)
        {
            if (__state != null && IsHarpoon(__instance.m_weapon.m_shared))
            {
                __instance.m_weapon.m_shared.m_attackStatusEffect = __state;
            }
        }
    }
}