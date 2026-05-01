using UnityEngine;

namespace GliqeWorld.Hands
{
    public enum HandSide { Left, Right }

    public enum SubstanceType { None, Coffee, Paint, Ash, Food, PaintWater, Ink }

    /// <summary>
    /// Abstract base class for every object the player can hold in either hand.
    /// Subclasses implement equip/unequip logic and use actions.
    /// </summary>
    public abstract class HandItem : MonoBehaviour
    {
        // ── Public API ───────────────────────────────────────────────────────────

        public HandSide CurrentHand { get; private set; }
        public SubstanceType ContainedSubstance { get; protected set; } = SubstanceType.None;

        // ── Abstract methods ─────────────────────────────────────────────────────

        /// <summary>Called when this item is equipped into a hand anchor.</summary>
        public abstract void OnEquip(HandAnchor anchor);

        /// <summary>Called when this item is removed from a hand anchor.</summary>
        public abstract void OnUnequip();

        /// <summary>Primary use action (left-click / R2 / RT).</summary>
        public abstract void OnUse();

        /// <summary>Alternate use action (right-click / L2 / LT).</summary>
        public abstract void OnAltUse();

        // ── Virtual methods ──────────────────────────────────────────────────────

        /// <summary>
        /// Called by SubstanceContaminationSystem when a foreign substance is detected
        /// in proximity. Override to handle contamination-specific visuals and effects.
        /// </summary>
        public virtual void OnContaminationReceived(SubstanceType source) { }

        // ── Internal helpers ─────────────────────────────────────────────────────

        /// <summary>Sets the hand side. Called by HandAnchor on equip.</summary>
        internal void SetHand(HandSide side) => CurrentHand = side;
    }
}
