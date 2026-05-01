using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Thin wrapper over Unity's Input System. Reads and exposes all player input
    /// as typed events and properties. Attach to the Player GameObject.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Properties ──────────────────────────────────────────────────────────

        /// <summary>Normalised WASD / left-stick movement.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Mouse delta / right-stick look.</summary>
        public Vector2 LookInput { get; private set; }

        public bool SprintHeld { get; private set; }
        public bool DuckHeld { get; private set; }
        public bool PeekLeftHeld { get; private set; }
        public bool PeekRightHeld { get; private set; }

        // ── Events ───────────────────────────────────────────────────────────────

        public event Action OnInteractPressed;
        public event Action OnLeftHandUse;
        public event Action OnRightHandUse;
        public event Action OnSwapHands;

        // ── Input action callbacks ────────────────────────────────────────────────

        public void OnMove(InputValue value) => MoveInput = value.Get<Vector2>();

        public void OnLook(InputValue value) => LookInput = value.Get<Vector2>();

        public void OnSprint(InputValue value) => SprintHeld = value.isPressed;

        public void OnDuck(InputValue value) => DuckHeld = value.isPressed;

        public void OnPeekLeft(InputValue value) => PeekLeftHeld = value.isPressed;

        public void OnPeekRight(InputValue value) => PeekRightHeld = value.isPressed;

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
                OnInteractPressed?.Invoke();
        }

        public void OnLeftHandUseAction(InputValue value)
        {
            if (value.isPressed)
                OnLeftHandUse?.Invoke();
        }

        public void OnRightHandUseAction(InputValue value)
        {
            if (value.isPressed)
                OnRightHandUse?.Invoke();
        }

        public void OnSwapHandsAction(InputValue value)
        {
            if (value.isPressed)
                OnSwapHands?.Invoke();
        }
    }
}
