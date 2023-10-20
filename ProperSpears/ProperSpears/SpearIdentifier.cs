using UnityEngine;

namespace ProperSpears
{
    internal class SpearIdentifier
    {
        // without other mods, excludes harpoon, aka chitin spear
        // with a mod that gives it a poke attack, it may fall under this (intentionally)
        internal static bool IsSpearWithPokeAttack(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_skillType == Skills.SkillType.Spears && shared.m_attack.m_attackAnimation == "spear_poke";
        }

        internal static bool IsSpearWithSwordAttack(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_skillType == Skills.SkillType.Spears && shared.m_attack.m_attackAnimation == "sword_secondary";
        }

        internal static bool IsFangSpear(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_name == "$item_spear_wolffang";
        }

        internal static bool IsHarpoon(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_name == "$item_spear_chitin";
        }

        internal enum SpecialSpears
        {
            None,
            FangSpear,
            ChitinSpearHarpoon
        }

        internal static bool IsHashForHarpoonOrSpearWithPokeAttack(int hash, out SpecialSpears isSpecialSpear)
        {
            isSpecialSpear = SpecialSpears.None;

            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);

            if (!itemPrefab)
            {
                return false;
            }

            var drop = itemPrefab.GetComponent<ItemDrop>();

            if (!drop || drop.m_itemData == null)
            {
                return false;
            }

            var shared = drop.m_itemData.m_shared;

            if (IsHarpoon(shared) || IsSpearWithPokeAttack(shared))
            {
                switch (shared.m_name)
                {
                    case "$item_spear_wolffang":
                        isSpecialSpear = SpecialSpears.FangSpear;
                        break;

                    case "$item_spear_chitin":
                        isSpecialSpear = SpecialSpears.ChitinSpearHarpoon;
                        break;
                }

                return true;
            }

            return false;
        }
    }
}