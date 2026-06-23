using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Thin wrapper over Unity's Input System. All inputs are polled each frame
    /// via InputAction APIs to avoid SendMessages phase-firing inconsistencies.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Properties ───────────────────────────────────────────────────────────

        /// <summary>Normalised WASD / left-stick movement.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Mouse delta / right-stick look.</summary>
        public Vector2 LookInput { get; private set; }

        public bool SprintHeld { get; private set; }
        public bool DuckHeld { get; private set; }
        public bool PeekLeftHeld { get; private set; }
        public bool PeekRightHeld { get; private set; }

        /// <summary>True while the left-hand use button is held (used by paint tools).</summary>
        public bool LeftHandUseHeld { get; private set; }

        /// <summary>True while the right-hand use button is held (used by paint tools).</summary>
        public bool RightHandUseHeld { get; private set; }

        // ── Events ───────────────────────────────────────────────────────────────

        public event Action OnInteractPressed;
        public event Action OnToggleCameraViewPressed;

        /// <summary>Fired on the frame the swap-hands button is pressed.</summary>
        public event Action OnSwapHands;

        public event Action OnLeftHandUsePressed;
        public event Action OnRightHandUsePressed;

        // ── Private ──────────────────────────────────────────────────────────────

        private PlayerInput _playerInput;
        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _crouch;
        private InputAction _peekLeft;
        private InputAction _peekRight;
        private InputAction _leftHandUse;
        private InputAction _rightHandUse;
        private InputAction _interact;
        private InputAction _toggleCamera;
        private InputAction _swapHands;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            var a = _playerInput.actions;

            _move         = a.FindAction("Move",             true);
            _look         = a.FindAction("Look",             true);
            _sprint       = a.FindAction("Sprint",           true);
            _crouch       = a.FindAction("Crouch",           true);
            _peekLeft     = a.FindAction("PeekLeft",         false);
            _peekRight    = a.FindAction("PeekRight",        false);
            _leftHandUse  = a.FindAction("LeftHandUse",      true);
            _rightHandUse = a.FindAction("RightHandUse",     true);
            _interact     = a.FindAction("Interact",         true);
            _toggleCamera = a.FindAction("ToggleCameraView", true);
            _swapHands    = a.FindAction("SwapHands",        true);
        }

        private void Update()
        {
            MoveInput = _move.ReadValue<Vector2>();
            LookInput = _look.ReadValue<Vector2>();

            SprintHeld       = _sprint.IsPressed();
            DuckHeld         = _crouch.IsPressed();
            LeftHandUseHeld  = _leftHandUse.IsPressed();
            RightHandUseHeld = _rightHandUse.IsPressed();

            // Optional peek — absent on some input maps
            PeekLeftHeld  = _peekLeft?.IsPressed()  ?? false;
            PeekRightHeld = _peekRight?.IsPressed() ?? false;

            // One-shot press events
            if (_interact.WasPressedThisFrame())     OnInteractPressed?.Invoke();
            if (_toggleCamera.WasPressedThisFrame()) OnToggleCameraViewPressed?.Invoke();
            if (_swapHands.WasPressedThisFrame())    OnSwapHands?.Invoke();
            if (_leftHandUse.WasPressedThisFrame())  OnLeftHandUsePressed?.Invoke();
            if (_rightHandUse.WasPressedThisFrame()) OnRightHandUsePressed?.Invoke();
        }
    }
}
