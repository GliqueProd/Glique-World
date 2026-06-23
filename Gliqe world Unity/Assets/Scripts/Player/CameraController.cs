using UnityEngine;

namespace GliqeWorld.Player
{
    public enum CameraMode { FPS, TPS }

    /// <summary>
    /// Manages FPS/TPS camera switching, FOV kick on sprint, and TPS orbit offset.
    /// Requires CameraRig to handle pitch (done by PlayerController).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private PlayerConfig config;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerInputHandler input;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Camera bodyCamera;
        [SerializeField] private GameObject bodyMesh;

        // ── Public API ───────────────────────────────────────────────────────────

        public CameraMode CurrentMode { get; private set; } = CameraMode.FPS;

        // ── Private ──────────────────────────────────────────────────────────────

        private int _firstPersonBodyMask;

        private float _targetFOV;
        private Vector3 _targetLocalPos;

        private static readonly Vector3 FPSLocalPos = Vector3.zero;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _firstPersonBodyMask = 1 << LayerMask.NameToLayer("FirstPersonBody");
            input.OnToggleCameraViewPressed += ToggleMode;
            playerController.OnStateChanged += OnStateChanged;
            ApplyMode(CameraMode.FPS, instant: true);
        }

        private void OnDestroy()
        {
            input.OnToggleCameraViewPressed -= ToggleMode;
            playerController.OnStateChanged -= OnStateChanged;
        }

        private void Update()
        {
            InterpolateFOV();
            InterpolateCameraPosition();
        }

        // ── Mode Toggle ──────────────────────────────────────────────────────────

        /// <summary>Switches between FPS and TPS.</summary>
        public void ToggleMode()
        {
            ApplyMode(CurrentMode == CameraMode.FPS ? CameraMode.TPS : CameraMode.FPS, instant: false);
        }

        private void ApplyMode(CameraMode mode, bool instant)
        {
            CurrentMode = mode;

            if (mode == CameraMode.FPS)
            {
                _targetLocalPos = FPSLocalPos;
                _targetFOV = config.defaultFOV;
                bodyCamera.enabled = true;
                bodyMesh.layer = LayerMask.NameToLayer("FirstPersonBody");
                // Remove FirstPersonBody from WorldCamera culling mask
                worldCamera.cullingMask &= ~_firstPersonBodyMask;
            }
            else // TPS
            {
                _targetLocalPos = new Vector3(0f, config.tpsHeightOffset, -config.tpsDistance);
                _targetFOV = config.tpsFOV;
                bodyCamera.enabled = false;
                // Make body visible to WorldCamera in TPS
                worldCamera.cullingMask |= _firstPersonBodyMask;
                bodyMesh.layer = LayerMask.NameToLayer("Default");
            }

            if (instant)
            {
                worldCamera.transform.localPosition = _targetLocalPos;
                worldCamera.fieldOfView = _targetFOV;
            }
        }

        // ── FOV Kick ─────────────────────────────────────────────────────────────

        private void OnStateChanged(PlayerState state)
        {
            if (CurrentMode == CameraMode.TPS) return;

            _targetFOV = state == PlayerState.Sprint ? config.sprintFOV : config.defaultFOV;
        }

        private void InterpolateFOV()
        {
            worldCamera.fieldOfView = Mathf.Lerp(
                worldCamera.fieldOfView,
                _targetFOV,
                Time.deltaTime * config.fovSmoothing
            );
        }

        // ── TPS Position ─────────────────────────────────────────────────────────

        private void InterpolateCameraPosition()
        {
            worldCamera.transform.localPosition = Vector3.Lerp(
                worldCamera.transform.localPosition,
                _targetLocalPos,
                Time.deltaTime * config.tpsCameraSmoothing
            );
        }
    }
}
