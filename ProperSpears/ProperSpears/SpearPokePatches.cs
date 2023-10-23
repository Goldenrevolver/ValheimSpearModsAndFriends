using HarmonyLib;
using UnityEngine;
using static ProperSpears.SpearIdentifier;

namespace ProperSpears
{
    [HarmonyPatch]
    internal class SpearPokePatches
    {
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem)), HarmonyPrefix]
        public static void ModifyVanillaSpears(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return;
            }

            if (IsSpearWithPokeAttack(item.m_shared))
            {
                item.m_shared.m_attack.m_attackAnimation = "sword_secondary";

                // default: 1.9
                item.m_shared.m_attack.m_attackRange = 3.2f;
                //item.m_shared.m_attack.m_attackRange += 1.3f;

                // default 1.5
                item.m_shared.m_attack.m_attackHeight = 1f;

                // defaults: spear: 40, atgeir: 20
                item.m_shared.m_attack.m_attackAngle = 30;

                // defaults: spear: 0.5, atgeir: 0.3
                item.m_shared.m_attack.m_attackRayWidth = 0.4f;
            }
        }

        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachItem)), HarmonyPostfix]
        public static void RotateSpearOnAttach(int itemHash, GameObject __result)
        {
            if (!__result)
            {
                return;
            }

            if (IsHashForHarpoonOrSpearWithPokeAttack(itemHash, out var isSpecialSpear))
            {
                bool isHarpoonModInstalled = ProperSpearsPlugin.IsHarpoonAttackModInstalled();

                if (isSpecialSpear == SpecialSpears.ChitinSpearHarpoon && !isHarpoonModInstalled)
                {
                    return;
                }

                SpearPositioner.FixSpearRotatationAndPosition(__result, false, isSpecialSpear == SpecialSpears.FangSpear);
            }
        }

        [HarmonyPatch(typeof(ZSyncAnimation), nameof(ZSyncAnimation.SyncParameters)), HarmonyPostfix]
        public static void SpeedUpSpearThrustAndFixThrow(ZSyncAnimation __instance)
        {
            var humanoid = __instance.GetComponent<Humanoid>();

            if (!humanoid || !humanoid.m_nview || !IsSpearWithSwordAttack(humanoid.GetCurrentWeapon()?.m_shared))
            {
                return;
            }

            foreach (var item in __instance.m_animator.GetCurrentAnimatorClipInfo(0))
            {
                if (item.clip == null)
                {
                    continue;
                }

                if (item.clip.name == "Sword-Attack-R4" && !humanoid.m_currentAttackIsSecondary)
                {
                    // due to continued multiplication this ramps up over the course of the animation
                    // this leads to the intended effect of a sudden thrust attack after the build up
                    // since ZSyncAnimation.SyncParameters gets called in FixedUpdate, we also apply fixed delta time
                    __instance.m_animator.speed *= 1.18f * 50 * Time.fixedDeltaTime;
                    break;
                }
                else if (item.clip.name == "throw_spear")
                {
                    var shared = humanoid.GetCurrentWeapon().m_shared;
                    var spear = humanoid.m_visEquipment.m_rightItemInstance;

                    // changing m_attackAnimation back will make IsModifiedSpear return false, this way we only do this once
                    // this is a fallback if you manage to get your throwing animation interrupted somehow
                    // since this attack works with the default positions
                    // also works really well for the harpoon
                    shared.m_attack.m_attackAnimation = "spear_poke";

                    SpearPositioner.FixSpearRotatationAndPosition(spear, true, IsFangSpear(shared));
                    break;
                }
            }
        }

        // 'sender == ZDOMan.GetSessionID()' should be equivalent to '__instance.m_nview.IsOwner()' but I don't trust it
        [HarmonyPatch(typeof(ZSyncAnimation), nameof(ZSyncAnimation.RPC_SetTrigger)), HarmonyPrefix]
        public static void RevertFlippedThrowInMultiplayer(ZSyncAnimation __instance, long sender, ref string name)
        {
            // easier to read than the combined and negated version
            bool shouldTryRevert = name == "spear_poke";
            shouldTryRevert |= name == "spear_throw" && sender != ZDOMan.GetSessionID();

            if (!shouldTryRevert)
            {
                return;
            }

            var humanoid = __instance.GetComponent<Humanoid>();

            if (!humanoid)
            {
                return;
            }

            var hopefullySpear = humanoid.m_visEquipment.m_rightItemInstance;

            if (hopefullySpear == null)
            {
                return;
            }

            if (IsHashForHarpoonOrSpearWithPokeAttack(humanoid.m_visEquipment.m_currentRightItemHash, out var isSpecialSpear))
            {
                if (name == "spear_throw")
                {
                    hopefullySpear.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    hopefullySpear.transform.localPosition = Vector3.zero;

                    // could also call this, but I don't trust that this rpc isn't triggered multiple times
                    // and I don't have a fallback for that, unlike when you use it yourself
                    // so I prefer just setting it all to zero

                    //SpearPositioner.FixSpearRotatationAndPosition(hopefullySpear, true, isSpecialSpear == SpecialSpears.FangSpear);
                }
                else if (isSpecialSpear == SpecialSpears.ChitinSpearHarpoon)
                {
                    // in theory, this code could also run for non harpoons, but why risk it

                    name = "sword_secondary";

                    var currentWeapon = humanoid.GetCurrentWeapon();

                    if (sender == ZDOMan.GetSessionID()
                        && currentWeapon != null
                        && IsHarpoon(currentWeapon.m_shared))
                    {
                        currentWeapon.m_shared.m_attack.m_attackAnimation = "sword_secondary";
                    }

                    hopefullySpear.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    hopefullySpear.transform.localPosition = Vector3.zero;

                    SpearPositioner.FixSpearRotatationAndPosition(hopefullySpear, false, false);
                }
            }
        }
    }
}