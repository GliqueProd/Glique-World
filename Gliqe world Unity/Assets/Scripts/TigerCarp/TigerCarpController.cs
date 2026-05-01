using System.Collections.Generic;
using UnityEngine;
using GliqeWorld.Player;

namespace GliqeWorld.TigerCarp
{
    public enum CarpExpression { Idle, Excited, Curious, Alarmed }

    /// <summary>
    /// Procedural animation controller for the Tiger-Carp companion.
    /// Spring-follower lead point with segmented spine undulation and secondary arm motion.
    /// Renders on BodyCamera layer (FirstPersonBody) to avoid z-fighting with the world.
    /// </summary>
    public class TigerCarpController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Follow Target")]
        [SerializeField] private Transform playerHead;
        [SerializeField] private float floatDistance = 1.2f;
        [SerializeField] private float floatHeight = 0.4f;

        [Header("Spring")]
        [SerializeField] private float stiffness = 8f;
        [SerializeField] private float damping = 4f;
        [SerializeField] private float lateralInfluence = 0.3f;

        [Header("Spine")]
        [SerializeField] private List<Transform> spineSegments;
        [SerializeField] private List<float> segmentLag;

        [Header("Undulation")]
        [SerializeField] private float undulationFrequency = 2f;
        [SerializeField] private float undulationAmplitude = 0.08f;
        [SerializeField] private float phaseOffset = 0.8f;

        [Header("Arms")]
        [SerializeField] private Transform armLeft;
        [SerializeField] private Transform armRight;
        [SerializeField] private float armDamping = 5f;

        [Header("Dialogue")]
        [SerializeField] private TigerCarpDialogueSystem dialogueSystem;

        [Header("Animator")]
        [SerializeField] private Animator animator;

        // ── Private ──────────────────────────────────────────────────────────────

        private Vector3 _leadPosition;
        private Vector3 _leadVelocity;
        private Vector3 _prevPlayerVelocity;

        private PlayerController _player;

        private static readonly int ExpressionHash = Animator.StringToHash("Expression");

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            _player = FindFirstObjectByType<PlayerController>();
            if (playerHead == null && _player != null)
                playerHead = _player.transform;

            _leadPosition = transform.position;
        }

        private void Update()
        {
            if (playerHead == null) return;

            UpdateLeadPoint();
            UpdateSpine();
            UpdateArms();
        }

        // ── Spring Lead Point ────────────────────────────────────────────────────

        private void UpdateLeadPoint()
        {
            Vector3 playerVel = _player != null ? _player.Velocity : Vector3.zero;
            Vector3 velDelta = playerVel - _prevPlayerVelocity;
            _prevPlayerVelocity = playerVel;

            Vector3 target = playerHead.position
                + playerHead.forward * floatDistance
                + playerHead.up * floatHeight
                + playerHead.right * (velDelta.x * lateralInfluence);

            _leadVelocity += (target - _leadPosition) * stiffness * Time.deltaTime;
            _leadVelocity -= _leadVelocity * damping * Time.deltaTime;
            _leadPosition += _leadVelocity * Time.deltaTime;

            transform.position = _leadPosition;

            // Orient toward player head
            Vector3 dir = playerHead.position - _leadPosition;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // ── Segmented Spine ──────────────────────────────────────────────────────

        private void UpdateSpine()
        {
            if (spineSegments == null || spineSegments.Count == 0) return;

            Vector3 prevPos = _leadPosition;

            for (int i = 0; i < spineSegments.Count; i++)
            {
                if (spineSegments[i] == null) continue;

                float lag = i < segmentLag.Count ? segmentLag[i] : 0.1f;
                Vector3 undulation = transform.right * Mathf.Sin(Time.time * undulationFrequency + i * phaseOffset) * undulationAmplitude;
                Vector3 target = prevPos + undulation;

                spineSegments[i].position = Vector3.Lerp(spineSegments[i].position, target, Time.deltaTime / Mathf.Max(lag, 0.01f));
                prevPos = spineSegments[i].position;
            }
        }

        // ── Arms (pendulum damper) ───────────────────────────────────────────────

        private void UpdateArms()
        {
            if (spineSegments == null || spineSegments.Count < 3) return;

            Vector3 spinePos = spineSegments[Mathf.Min(2, spineSegments.Count - 1)].position;

            PendulumDamp(armLeft, spinePos);
            PendulumDamp(armRight, spinePos);
        }

        private void PendulumDamp(Transform arm, Vector3 anchorPos)
        {
            if (arm == null) return;
            arm.position = Vector3.Lerp(arm.position, anchorPos + Vector3.down * 0.15f, Time.deltaTime * armDamping);
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Triggers an expression blend on the Animator.</summary>
        public void TriggerExpression(CarpExpression expression)
        {
            if (animator != null)
                animator.SetInteger(ExpressionHash, (int)expression);
        }

        /// <summary>Queues a dialogue entry for the Tiger-Carp to speak.</summary>
        public void QueueDialogue(DialogueEntry entry)
        {
            dialogueSystem?.Enqueue(entry);
        }

        /// <summary>Shows or hides the Tiger-Carp entirely.</summary>
        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
