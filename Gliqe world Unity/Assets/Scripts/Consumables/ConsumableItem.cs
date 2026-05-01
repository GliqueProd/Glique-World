using UnityEngine;
using UnityEngine.Events;
using GliqeWorld.Hands;
using GliqeWorld.Player;

namespace GliqeWorld.Consumables
{
    /// <summary>
    /// Extends HandItem with sip/bite/drag mechanics, use-count tracking,
    /// and SubstanceType management.
    /// </summary>
    public class ConsumableItem : HandItem
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Consumable Config")]
        [SerializeField] private int maxUses = 3;
        [SerializeField] private float effectDuration = 10f;
        [SerializeField] private SubstanceType outputSubstance = SubstanceType.None;

        [Header("Speed Effect")]
        [SerializeField] private bool appliesSpeedModifier;
        [SerializeField] private float speedMultiplier = 1f;

        [Header("Events")]
        [SerializeField] private UnityEvent onConsumed;
        [SerializeField] private UnityEvent onDepleted;
        [SerializeField] private UnityEvent onContaminated;

        // ── Public API ───────────────────────────────────────────────────────────

        public int UsesRemaining { get; private set; }
        public float EffectDuration => effectDuration;
        public SubstanceType OutputSubstance => outputSubstance;

        // ── State ────────────────────────────────────────────────────────────────

        private PlayerController _playerController;
        private bool _isContaminated;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            ContainedSubstance = outputSubstance;
            UsesRemaining = maxUses;
            _playerController = FindFirstObjectByType<PlayerController>();
        }

        public override void OnUnequip()
        {
            _playerController = null;
        }

        public override void OnUse()
        {
            if (UsesRemaining <= 0) return;
            if (_isContaminated) { HandleContaminatedUse(); return; }

            UsesRemaining--;
            onConsumed?.Invoke();

            if (appliesSpeedModifier && _playerController != null)
                _playerController.ApplySpeedModifier(speedMultiplier, effectDuration);

            if (UsesRemaining == 0)
                onDepleted?.Invoke();
        }

        public override void OnAltUse() { }

        public override void OnContaminationReceived(SubstanceType source)
        {
            if (_isContaminated) return;

            _isContaminated = true;
            ContainedSubstance = source;
            onContaminated?.Invoke();

            // Visual blend driven by shader property on this renderer
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
                rend.material.SetFloat("_ContaminationBlend", 1f);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void HandleContaminatedUse()
        {
            // Contaminated: play gag anim / deny effect
            Debug.Log($"{name} is contaminated — no effect.");
        }
    }
}
