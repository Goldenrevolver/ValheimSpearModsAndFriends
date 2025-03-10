using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoyalSpears
{
    internal class PlayerWeightReserverTrackerComponent : MonoBehaviour
    {
        public readonly List<WeightReserverComponent> WeightReservers = new List<WeightReserverComponent>();
    }

    internal class WeightReserverComponent : MonoBehaviour
    {
        public ItemDrop.ItemData AttachedItemData { get => attachedItemData; }

        public Player OriginalOwner { get => originalOwner; }

        protected ItemDrop.ItemData attachedItemData;

        protected Player originalOwner;

        public void Setup(ItemDrop.ItemData attachedItemData, Player originalOwner)
        {
            if (originalOwner == null)
            {
                Destroy(this);
                return;
            }

            this.attachedItemData = attachedItemData;
            this.originalOwner = originalOwner;

            if (!this.originalOwner.TryGetComponent<PlayerWeightReserverTrackerComponent>(out var playerWeightReserverTracker))
            {
                playerWeightReserverTracker = this.originalOwner.gameObject.AddComponent<PlayerWeightReserverTrackerComponent>();
            }

            playerWeightReserverTracker.WeightReservers.Add(this);

            StartTimer();
        }

        protected virtual void StartTimer()
        {
            // use string overload because we also use the string overload to potentially stop it:
            // https://docs.unity3d.com/ScriptReference/MonoBehaviour.StopCoroutine.html
            this.StartCoroutine(nameof(UnreserveInABit));
        }

        public virtual void StopTimer()
        {
            this.StopCoroutine(nameof(UnreserveInABit));
        }

        private IEnumerator UnreserveInABit()
        {
            float seconds = LoyalSpearsPlugin.MaxSecondsToReserveCarryingCapacityForThrownSpears.Value;

            if (seconds > 0)
            {
                yield return new WaitForSeconds(seconds);
            }

            Destroy(this);
        }

        public void OnDestroy()
        {
            if (this.originalOwner.TryGetComponent<PlayerWeightReserverTrackerComponent>(out var playerWeightReserverTracker))
            {
                playerWeightReserverTracker.WeightReservers.Remove(this);
            }
        }
    }
}