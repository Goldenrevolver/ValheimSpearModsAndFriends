using System.Collections;
using UnityEngine;

namespace LoyalSpears
{
    internal class LoyaltyComponent : MonoBehaviour
    {
        private ItemDrop attachedItem;

        private Player originalOwner;

        // ask IronGate why death count is a float
        private float ownerDeathCountOnThrow;

        public void Setup(ItemDrop attachedItem, Player originalOwner, float ownerDeathCountOnThrow)
        {
            this.attachedItem = attachedItem;
            this.originalOwner = originalOwner;
            this.ownerDeathCountOnThrow = ownerDeathCountOnThrow;
        }

        public void StartReturnTimer()
        {
            if (attachedItem)
            {
                // use string overload because we also use the string overload to potentially stop it:
                // https://docs.unity3d.com/ScriptReference/MonoBehaviour.StopCoroutine.html
                this.StartCoroutine(nameof(ReturnInABit));
            }
            else
            {
                // we only try once
                Destroy(this);
            }
        }

        internal IEnumerator ReturnInABit()
        {
            var seconds = LoyalSpearsPlugin.GroundSecondsUntilAutoReturn.Value;

            if (seconds > 0)
            {
                yield return new WaitForSeconds(seconds);
            }

            if (originalOwner && attachedItem && attachedItem.CanPickup())
            {
                originalOwner.m_nview.InvokeRPC("RPC_PickupLoyaltySpear", attachedItem.m_nview.GetZDO().m_uid, ownerDeathCountOnThrow);
            }

            // we only try once
            Destroy(this);
        }
    }
}