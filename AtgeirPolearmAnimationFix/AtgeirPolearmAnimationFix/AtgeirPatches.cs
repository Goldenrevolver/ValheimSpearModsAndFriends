using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace AtgeirPolearmAnimationFix
{
    [HarmonyPatch]
    internal class AtgeirPatches
    {
        internal static bool IsAtgeirPolearm(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null && shared.m_skillType == Skills.SkillType.Polearms && shared.m_attack.m_attackAnimation == "atgeir_attack";
        }

        internal static bool IsHashForAtgeirPolearm(int hash)
        {
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

            return IsAtgeirPolearm(shared);
        }

        [HarmonyPatch(typeof(ZSyncAnimation), nameof(ZSyncAnimation.RPC_SetTrigger)), HarmonyPrefix]
        public static void RevertFlippedThrowInMultiplayer(ZSyncAnimation __instance, long sender, ref string name)
        {
            if (name != "atgeir_attack0")
            {
                return;
            }

            var humanoid = __instance.GetComponent<Humanoid>();

            if (!humanoid)
            {
                return;
            }

            var hopefullyAtgeir = humanoid.m_visEquipment.m_rightItemInstance;

            if (hopefullyAtgeir == null)
            {
                return;
            }

            if (IsHashForAtgeirPolearm(humanoid.m_visEquipment.m_currentRightItemHash))
            {
                hopefullyAtgeir.transform.localRotation = Quaternion.Euler(Vector3.zero);
                hopefullyAtgeir.transform.localPosition = Vector3.zero;

                hopefullyAtgeir.transform.localRotation = Quaternion.Euler(0f, -5f, 40f);

                // for testing
                //__instance.m_animator.speed *= 0.2f;

                AtgeirPolearmAnimationFixPlugin.plugin.StartCoroutine(RevertAtgeirRotation(hopefullyAtgeir.transform, __instance.m_animator.speed));
            }
        }

        internal static IEnumerator RevertAtgeirRotation(Transform atgeir, float animatorSpeeed)
        {
            float speedMult = 1f / animatorSpeeed;

            yield return new WaitForSeconds(0.36f * speedMult);

            if (atgeir == null)
            {
                yield break;
            }

            //atgeir.transform.localRotation = Quaternion.Euler(-10f, -10f, 22f);

            //yield return new WaitForSeconds(0.04f * speedMult);

            //if (atgeir == null)
            //{
            //    yield break;
            //}

            atgeir.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }
}