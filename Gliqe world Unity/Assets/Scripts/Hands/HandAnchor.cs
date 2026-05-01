using UnityEngine;

namespace GliqeWorld.Hands
{
    /// <summary>
    /// Attached to LeftHandAnchor and RightHandAnchor transforms.
    /// Manages equip/unequip of one HandItem and reports substance info
    /// to the contamination system.
    /// </summary>
    public class HandAnchor : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private HandSide side;

        // ── Public API ───────────────────────────────────────────────────────────

        public HandItem HeldItem { get; private set; }
        public HandSide Side => side;
        public bool IsEmpty => HeldItem == null;

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Equips the given item into this anchor, calling OnEquip.</summary>
        public void Equip(HandItem item)
        {
            if (item == null) return;

            Unequip();

            HeldItem = item;
            HeldItem.SetHand(side);
            HeldItem.transform.SetParent(transform, false);
            HeldItem.transform.localPosition = Vector3.zero;
            HeldItem.transform.localRotation = Quaternion.identity;
            HeldItem.OnEquip(this);
        }

        /// <summary>Unequips the currently held item, calling OnUnequip.</summary>
        public void Unequip()
        {
            if (HeldItem == null) return;

            HeldItem.OnUnequip();
            HeldItem.transform.SetParent(null);
            HeldItem = null;
        }
    }
}
