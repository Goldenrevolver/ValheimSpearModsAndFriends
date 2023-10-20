using HarmonyLib;
using UnityEngine.SceneManagement;

namespace HarpoonMeleeAttack
{
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    internal static class PatchObjectDB
    {
        private const string harpoonItemName = "$item_spear_chitin";
        private const string bronzeSpearItemName = "$item_spear_bronze";
        private const string chitinItem = "$item_chitin";

        public static void Postfix(ObjectDB __instance)
        {
            if (SceneManager.GetActiveScene().name != "main")
            {
                return;
            }

            Attack bronzeSpearPokeAttack = null;

            foreach (var gameObject in __instance.m_items)
            {
                if (gameObject == null)
                {
                    continue;
                }

                var item = gameObject.GetComponent<ItemDrop>();

                if (item == null)
                {
                    continue;
                }

                ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

                if (shared.m_name == bronzeSpearItemName)
                {
                    bronzeSpearPokeAttack = shared.m_attack;
                }
            }

            foreach (var gameObject in __instance.m_items)
            {
                if (gameObject == null)
                {
                    continue;
                }

                var item = gameObject.GetComponent<ItemDrop>();

                if (item == null)
                {
                    continue;
                }

                ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

                if (shared.m_name == harpoonItemName)
                {
                    item.m_itemData.m_durability = 100;

                    shared.m_maxQuality = 4;
                    shared.m_maxDurability = 100;
                    shared.m_damages.m_pierce = 45;
                    shared.m_damagesPerLevel.m_pierce = 6;
                    shared.m_backstabBonus = 3;

                    shared.m_secondaryAttack = shared.m_attack.Clone();
                    shared.m_secondaryAttack.m_damageMultiplier = 0.2f;

                    shared.m_attack = bronzeSpearPokeAttack.Clone();
                }
            }

            foreach (var recipe in __instance.m_recipes)
            {
                if (recipe.m_item?.m_itemData.m_shared.m_name == harpoonItemName)
                {
                    foreach (var item in recipe.m_resources)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        item.m_amountPerLevel = 0;

                        if (item.m_resItem.m_itemData.m_shared.m_name == chitinItem)
                        {
                            item.m_amount = 20;
                            item.m_amountPerLevel = 10;
                        }
                    }
                }
            }
        }
    }
}