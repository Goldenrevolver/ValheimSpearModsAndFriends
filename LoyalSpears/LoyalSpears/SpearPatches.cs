using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace LoyalSpears
{
    [HarmonyPatch]
    internal class SpearPatches
    {
        public static bool IsSpear(ItemDrop.ItemData itemData)
        {
            return itemData != null && itemData.m_shared != null && itemData.m_shared.m_skillType == Skills.SkillType.Spears;
        }

        public static bool IsNonOwnedSpear(ItemDrop itemDrop)
        {
            if (!itemDrop || itemDrop.m_itemData == null)
            {
                return false;
            }

            return IsSpear(itemDrop.m_itemData) && !itemDrop.m_nview.IsOwner();
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake)), HarmonyPostfix]
        private static void AddLoyaltySpearRPC(Player __instance)
        {
            __instance.m_nview.Register<ZDOID, float>("RPC_PickupLoyaltySpear", (player, item, deaths) => RPC_PickupLoyaltySpear(__instance, item, deaths));
        }

        private static void RPC_PickupLoyaltySpear(Player player, ZDOID item, float deathCountOnThrow)
        {
            var instance = ZNetScene.instance.FindInstance(item);

            if (instance == null)
            {
                return;
            }

            var itemDrop = instance.GetComponent<ItemDrop>();

            if (itemDrop == null)
            {
                return;
            }

            var currentDeathCount = Game.instance.m_playerProfile.m_playerStats[PlayerStatType.Deaths];

            // if you die before picking up your spear, I don't want it to teleport across dimensions back into your inventory
            // also tracking the death count is easier/more clean than tracking the coroutine and trying to kill it in time
            if (currentDeathCount > deathCountOnThrow)
            {
                return;
            }

            itemDrop.Pickup(player);
        }

        private static ItemDrop AddLoyaltySpearPickupComponent(ItemDrop item, Projectile projectile)
        {
            // return type is for the transpiler to work

            if (LoyalSpearsPlugin.GroundSecondsUntilAutoReturn.Value < 0)
            {
                return item;
            }

            if (projectile.m_owner is Player player && player == Player.m_localPlayer && item && IsSpear(item.m_itemData))
            {
                if (item.gameObject.TryGetComponent<LoyaltyComponent>(out var loyalty))
                {
                    loyalty.StopCoroutine(nameof(LoyaltyComponent.ReturnInABit));
                }
                else
                {
                    loyalty = item.gameObject.AddComponent<LoyaltyComponent>();
                }

                var deathCount = Game.instance.m_playerProfile.m_playerStats[PlayerStatType.Deaths];

                loyalty.Setup(item, player, deathCount);

                loyalty.StartReturnTimer();
            }

            return item;
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.SpawnOnHit)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Projectile_SpawnOnHit_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo DropItemMethod = AccessTools.DeclaredMethod(typeof(ItemDrop), nameof(ItemDrop.DropItem));
            MethodInfo AddLoyaltyToSpearMethod = AccessTools.DeclaredMethod(typeof(SpearPatches), nameof(AddLoyaltySpearPickupComponent));

            foreach (var instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode == OpCodes.Call && instruction.OperandIs(DropItemMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // 'this' object
                    yield return new CodeInstruction(OpCodes.Call, AddLoyaltyToSpearMethod);
                }
            }
        }

        // if you throw your item off of the world and it never lands, this will catch it
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.LateUpdate)), HarmonyPostfix]
        public static void Projectile_LateUpdate_Postfix(Projectile __instance)
        {
            var item = __instance.m_spawnItem;
            var player = __instance.m_owner;

            if (player is Player && item != null && IsSpear(item))
            {
                var v = player.transform.position - __instance.transform.position;
                var distSq = v.sqrMagnitude;

                var autoReturnDistance = LoyalSpearsPlugin.FlightDistanceUntilAutoReturn.Value;

                if (autoReturnDistance < 0)
                {
                    return;
                }

                if (distSq > autoReturnDistance * autoReturnDistance)
                {
                    var itemDrop = ItemDrop.DropItem(__instance.m_spawnItem, 0, __instance.transform.position, __instance.transform.rotation);
                    player.m_nview.InvokeRPC("RPC_PickupLoyaltySpear", itemDrop.m_nview.GetZDO().m_uid);
                    __instance.m_spawnItem = null;
                    ZNetScene.instance.Destroy(__instance.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.AutoPickup)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AutoPickupTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo AutoPickupField = AccessTools.DeclaredField(typeof(ItemDrop), nameof(ItemDrop.m_autoPickup));
            MethodInfo ShouldAutoPickup = AccessTools.DeclaredMethod(typeof(SpearPatches), nameof(ShouldAutoPickup));

            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(i => i.LoadsField(AutoPickupField)))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Dup)
                )
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, ShouldAutoPickup)
                )
                .Instructions();
        }

        public static bool ShouldAutoPickup(ItemDrop itemDrop, bool isAutoPickupable)
        {
            return isAutoPickupable && !IsNonOwnedSpear(itemDrop);
        }
    }
}