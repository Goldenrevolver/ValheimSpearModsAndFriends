using System.Collections;
using UnityEngine;

namespace LoyalSpears
{
    internal class LoyaltyComponent : WeightReserverComponent
    {
        // ask IronGate why death count is a float
        private float ownerDeathCountOnThrow;

        private ItemDrop attachedItemDrop;

        public void Setup(ItemDrop attachedItemDrop, Player originalOwner, float ownerDeathCountOnThrow)
        {
            this.attachedItemDrop = attachedItemDrop;
            this.ownerDeathCountOnThrow = ownerDeathCountOnThrow;

            base.Setup(attachedItemDrop.m_itemData, originalOwner);
        }

        protected override void StartTimer()
        {
            // use string overload because we also use the string overload to potentially stop it:
            // https://docs.unity3d.com/ScriptReference/MonoBehaviour.StopCoroutine.html
            this.StartCoroutine(nameof(ReturnInABit));
        }

        public override void StopTimer()
        {
            this.StopCoroutine(nameof(ReturnInABit));
        }

        private IEnumerator ReturnInABit()
        {
            float seconds = LoyalSpearsPlugin.GroundSecondsUntilAutoReturn.Value;

            if (seconds > 0)
            {
                yield return new WaitForSeconds(seconds);
            }

            if (originalOwner && attachedItemDrop && attachedItemDrop.CanPickup())
            {
                originalOwner.m_nview.InvokeRPC("RPC_PickupLoyaltySpear", attachedItemDrop.m_nview.GetZDO().m_uid, ownerDeathCountOnThrow);
            }

            // we only try to return once
            Destroy(this);
        }
    }
}