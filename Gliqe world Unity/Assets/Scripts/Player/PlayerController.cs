using System;
using System.Collections;
using UnityEngine;

namespace GliqeWorld.Player
{
    public enum PlayerState { Idle, Walk, Sprint, Duck, Peek }

    /// <summary>
    /// Core character controller: movement, gravity, slope handling, ducking, peeking.
    /// Broadcasts state and velocity to dependent systems (Tiger-Carp, tools, etc.).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Camera worldCamera;

        // ── Public API ───────────────────────────────────────────────────────────

        public float MoveSpeed => config.walkSpeed * _speedMultiplier;
        public float SprintSpeed => config.sprintSpeed * _speedMultiplier;
        public bool IsSprinting => _state == PlayerState.Sprint;
        public bool IsDucking => _state == PlayerState.Duck;
        public bool IsPeeking => _state == PlayerState.Peek;
        public Vector3 Velocity => _velocity;

        public event Action<PlayerState> OnStateChanged;

        // ── Private ──────────────────────────────────────────────────────────────

        private CharacterController _cc;
        private PlayerInputHandler _input;

        private Vector3 _velocity;
        private float _verticalVelocity;
        private float _speedMultiplier = 1f;
        private Coroutine _speedModCoroutine;

        private PlayerState _state = PlayerState.Idle;
        private PlayerState _prevState;

        private float _defaultHeight;
        private Vector3 _defaultCenter;
        private float _defaultCameraY;

        private float _peekAngleCurrent;
        private float _cameraPitch;

        // Applied by PortalSceneConfig
        private float _walkMultiplier = 1f;
        private float _sprintMultiplier = 1f;
        private bool _duckingAllowed = true;
        private bool _peekingAllowed = true;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHandler>();

            _defaultHeight = _cc.height;
            _defaultCenter = _cc.center;

            if (cameraRig != null)
                _defaultCameraY = cameraRig.localPosition.y;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleDuck();
            HandlePeek();
            UpdateState();
        }

        // ── Look ─────────────────────────────────────────────────────────────────

        private void HandleLook()
        {
            Vector2 look = _input.LookInput * config.mouseSensitivity;

            // Horizontal — rotate player body
            transform.Rotate(Vector3.up, look.x);

            // Vertical — clamp camera pitch
            _cameraPitch -= look.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -85f, 85f);

            if (cameraRig != null)
            {
                Vector3 angles = cameraRig.localEulerAngles;
                angles.x = _cameraPitch;
                angles.z = _peekAngleCurrent;
                cameraRig.localEulerAngles = angles;
            }
        }

        // ── Movement ─────────────────────────────────────────────────────────────

        private void HandleMovement()
        {
            bool grounded = _cc.isGrounded;

            if (grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += config.gravity * Time.deltaTime;

            Vector2 moveInput = _input.MoveInput;
            bool sprinting = _input.SprintHeld && moveInput.magnitude > 0.1f && !IsDucking;
            float targetSpeed = IsDucking ? config.duckSpeed
                                : sprinting ? config.sprintSpeed * _sprintMultiplier * _speedMultiplier
                                : config.walkSpeed * _walkMultiplier * _speedMultiplier;

            Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

            // Slope projection
            if (_cc.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
                move = Vector3.ProjectOnPlane(move, hit.normal).normalized;

            move *= targetSpeed;
            move.y = _verticalVelocity;

            _cc.Move(move * Time.deltaTime);
            _velocity = _cc.velocity;
        }

        // ── Duck ─────────────────────────────────────────────────────────────────

        private void HandleDuck()
        {
            if (!_duckingAllowed) return;

            bool wantDuck = _input.DuckHeld;
            float targetHeight = wantDuck ? _defaultHeight * config.duckHeightMultiplier : _defaultHeight;
            float targetCenterY = wantDuck ? (_defaultCenter.y * config.duckHeightMultiplier) : _defaultCenter.y;

            _cc.height = Mathf.Lerp(_cc.height, targetHeight, Time.deltaTime * 10f);
            _cc.center = new Vector3(_cc.center.x, Mathf.Lerp(_cc.center.y, targetCenterY, Time.deltaTime * 10f), _cc.center.z);

            if (cameraRig != null)
            {
                float targetCamY = wantDuck ? _defaultCameraY * config.duckHeightMultiplier : _defaultCameraY;
                Vector3 pos = cameraRig.localPosition;
                pos.y = Mathf.Lerp(pos.y, targetCamY, Time.deltaTime * 10f);
                cameraRig.localPosition = pos;
            }
        }

        // ── Peek ─────────────────────────────────────────────────────────────────

        private void HandlePeek()
        {
            if (!_peekingAllowed) { _peekAngleCurrent = 0f; return; }

            float targetAngle = 0f;
            if (_input.PeekLeftHeld) targetAngle = config.peekAngle;
            else if (_input.PeekRightHeld) targetAngle = -config.peekAngle;

            float speed = config.peekAngle / Mathf.Max(config.peekDuration, 0.01f);
            _peekAngleCurrent = Mathf.MoveTowards(_peekAngleCurrent, targetAngle, speed * Time.deltaTime);
        }

        // ── State machine ────────────────────────────────────────────────────────

        private void UpdateState()
        {
            PlayerState newState;
            bool moving = _velocity.WithY(0f).magnitude > 0.1f;

            if (_input.DuckHeld && _duckingAllowed)
                newState = PlayerState.Duck;
            else if ((_input.PeekLeftHeld || _input.PeekRightHeld) && _peekingAllowed)
                newState = PlayerState.Peek;
            else if (moving && _input.SprintHeld)
                newState = PlayerState.Sprint;
            else if (moving)
                newState = PlayerState.Walk;
            else
                newState = PlayerState.Idle;

            if (newState != _state)
            {
                _state = newState;
                OnStateChanged?.Invoke(_state);
            }
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Temporarily multiplies movement speed. Used by consumables.</summary>
        public void ApplySpeedModifier(float multiplier, float duration)
        {
            if (_speedModCoroutine != null)
                StopCoroutine(_speedModCoroutine);
            _speedModCoroutine = StartCoroutine(SpeedModCoroutine(multiplier, duration));
        }

        /// <summary>Apply portal scene overrides to this controller.</summary>
        public void ApplyPortalConfig(PortalSceneConfig cfg)
        {
            if (cfg == null) return;
            _walkMultiplier = cfg.walkSpeedMultiplier;
            _sprintMultiplier = cfg.sprintSpeedMultiplier;
            _duckingAllowed = cfg.duckingEnabled;
            _peekingAllowed = cfg.peekingEnabled;

            if (worldCamera != null)
                worldCamera.fieldOfView = cfg.fovOverride;
        }

        /// <summary>Reset portal overrides back to defaults from PlayerConfig.</summary>
        public void ResetPortalConfig()
        {
            _walkMultiplier = 1f;
            _sprintMultiplier = 1f;
            _duckingAllowed = true;
            _peekingAllowed = true;

            if (worldCamera != null)
                worldCamera.fieldOfView = config.defaultFOV;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private IEnumerator SpeedModCoroutine(float multiplier, float duration)
        {
            _speedMultiplier = multiplier;
            yield return new WaitForSeconds(duration);
            _speedMultiplier = 1f;
            _speedModCoroutine = null;
        }
    }

    internal static class Vector3Extensions
    {
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    }
}
