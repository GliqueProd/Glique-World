using UnityEngine;
using GliqeWorld.CreativeState;

namespace GliqeWorld.Hands
{
    /// <summary>
    /// Detects proximity-based cross-contamination between both hands each frame
    /// and applies substance mixing to the receiving HandItem.
    /// </summary>
    public class SubstanceContaminationSystem : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float ContaminationRange = 0.15f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private HandAnchor leftAnchor;
        [SerializeField] private HandAnchor rightAnchor;
        [SerializeField] private CreativeStateManager creativeStateManager;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Update()
        {
            if (leftAnchor.IsEmpty || rightAnchor.IsEmpty) return;

            float distance = Vector3.Distance(leftAnchor.transform.position, rightAnchor.transform.position);
            if (distance >= ContaminationRange) return;

            HandItem leftItem = leftAnchor.HeldItem;
            HandItem rightItem = rightAnchor.HeldItem;

            // Contaminate right item with left substance and vice versa
            if (leftItem.ContainedSubstance != SubstanceType.None && rightItem.ContainedSubstance == SubstanceType.None)
            {
                rightItem.OnContaminationReceived(leftItem.ContainedSubstance);
                creativeStateManager?.AddCreativeAction(CreativeActionType.Contamination, -0.05f);
            }

            if (rightItem.ContainedSubstance != SubstanceType.None && leftItem.ContainedSubstance == SubstanceType.None)
            {
                leftItem.OnContaminationReceived(rightItem.ContainedSubstance);
                creativeStateManager?.AddCreativeAction(CreativeActionType.Contamination, -0.05f);
            }
        }
    }
}
