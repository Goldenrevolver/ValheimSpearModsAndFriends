using UnityEngine;

namespace ProperSpears
{
    internal class SpearPositioner
    {
        internal static void FixSpearRotatationAndPosition(GameObject gameObject, bool isRevert, bool isFangSpear)
        {
            if (!gameObject)
            {
                return;
            }

            if (!isRevert)
            {
                FixSpearRotation(gameObject);
                FixSpearPosition(gameObject, isRevert, isFangSpear);
            }
            else
            {
                FixSpearPosition(gameObject, isRevert, isFangSpear);
                FixSpearRotation(gameObject);
            }
        }

        private static void FixSpearRotation(GameObject gameObject)
        {
            gameObject.transform.Rotate(180f, 25f, 0f);
        }

        private static void FixSpearPosition(GameObject gameObject, bool isRevert, bool isFangSpear)
        {
            var direction = isRevert ? Vector3.back : Vector3.forward;

            // custom further change for the fang spear ($item_spear_wolffang), so it looks better
            if (isFangSpear)
            {
                gameObject.transform.Translate(direction * -0.72f);
            }
            else
            {
                gameObject.transform.Translate(direction * -0.5f);
            }
        }
    }
}