using UnityEngine;
using GliqeWorld.Hands;
using GliqeWorld.Tools;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Bridges PlayerInputHandler use inputs to the IPaintTool stroke lifecycle.
    /// Handles both hands independently: left mouse drives the left-hand tool,
    /// right mouse drives the right-hand tool.
    /// Attach on the same GameObject as PlayerInputHandler.
    /// </summary>
    public class PaintStrokeController : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float PaintRange = 10f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private HandAnchor leftHandAnchor;
        [SerializeField] private HandAnchor rightHandAnchor;

        /// <summary>Layers that the paint raycast can hit. Set in Inspector.</summary>
        [SerializeField] private LayerMask paintMask;

        // ── Private ──────────────────────────────────────────────────────────────

        private PlayerInputHandler _input;
        private bool _leftStroking;
        private bool _rightStroking;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake() => _input = GetComponent<PlayerInputHandler>();

        private void Update()
        {
            HandleHand(leftHandAnchor,  _input.LeftHandUseHeld,  ref _leftStroking);
            HandleHand(rightHandAnchor, _input.RightHandUseHeld, ref _rightStroking);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void HandleHand(HandAnchor anchor, bool useHeld, ref bool stroking)
        {
            // If the hand is empty or holds a non-painting item, end any active stroke.
            if (anchor.IsEmpty || anchor.HeldItem is not IPaintTool tool)
            {
                if (stroking)
                    stroking = false;
                return;
            }

            if (useHeld)
            {
                Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

                if (Physics.Raycast(ray, out RaycastHit hit, PaintRange, paintMask))
                {
                    if (!stroking)
                    {
                        tool.BeginStroke(hit);
                        stroking = true;
                    }
                    else
                    {
                        tool.ContinueStroke(hit);
                    }
                }
                else if (stroking)
                {
                    // Raycast missed mid-stroke (e.g. player aimed at sky): end cleanly.
                    tool.EndStroke();
                    stroking = false;
                }
            }
            else if (stroking)
            {
                tool.EndStroke();
                stroking = false;
            }
        }
    }
}
