using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Raycast-based world interaction system. Handles pickup, drop, and swap
    /// of HandItems between LeftHandAnchor and RightHandAnchor.
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float InteractRange = 2.5f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private HandAnchor leftHandAnchor;
        [SerializeField] private HandAnchor rightHandAnchor;
        [SerializeField] private LayerMask interactMask;

        private PlayerInputHandler _input;
        private HandItem _focusedItem;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake() => _input = GetComponent<PlayerInputHandler>();

        private void OnEnable()
        {
            _input.OnInteractPressed += HandleInteract;
            _input.OnSwapHands += HandleSwapHands;
        }

        private void OnDisable()
        {
            _input.OnInteractPressed -= HandleInteract;
            _input.OnSwapHands -= HandleSwapHands;
        }

        private void Update() => ScanForInteractable();

        // ── Raycast ──────────────────────────────────────────────────────────────

        private void ScanForInteractable()
        {
            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            _focusedItem = null;

            if (Physics.Raycast(ray, out RaycastHit hit, InteractRange, interactMask))
            {
                HandItem item = hit.collider.GetComponentInParent<HandItem>();
                if (item != null)
                    _focusedItem = item;
            }
        }

        // ── Interact ─────────────────────────────────────────────────────────────

        private void HandleInteract()
        {
            if (_focusedItem == null) return;

            // Tools go to the right hand by default; everything else to the left
            bool isToolOrDefault = _focusedItem.CompareTag("HandItem") || _focusedItem.CompareTag("Interactable");
            HandAnchor target = isToolOrDefault ? rightHandAnchor : leftHandAnchor;
            target.Equip(_focusedItem);
        }

        private void HandleSwapHands()
        {
            HandItem left = leftHandAnchor.HeldItem;
            HandItem right = rightHandAnchor.HeldItem;

            // Temporarily detach so Equip doesn't double-unequip
            if (left != null) leftHandAnchor.Unequip();
            if (right != null) rightHandAnchor.Unequip();

            if (right != null) leftHandAnchor.Equip(right);
            if (left != null) rightHandAnchor.Equip(left);
        }

        /// <summary>Drop item from the right hand at the current look hit, or at feet.</summary>
        public void DropRightHand()
        {
            if (rightHandAnchor.IsEmpty) return;

            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 dropPos = Physics.Raycast(ray, out RaycastHit hit, InteractRange)
                ? hit.point
                : transform.position + Vector3.down * 0.5f;

            HandItem item = rightHandAnchor.HeldItem;
            rightHandAnchor.Unequip();
            item.transform.position = dropPos;
        }
    }
}
