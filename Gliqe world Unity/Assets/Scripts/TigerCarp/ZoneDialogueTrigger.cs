using UnityEngine;

namespace GliqeWorld.TigerCarp
{
    /// <summary>
    /// Placed in zone trigger volumes along the ascent road.
    /// Fires a DialogueEntry to the Tiger-Carp when the player enters the volume.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneDialogueTrigger : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private DialogueEntry dialogueEntry;
        [SerializeField] private TigerCarpController carp;
        [SerializeField] private bool fireOnce = true;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _fired;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fireOnce && _fired) return;
            if (!other.CompareTag("Player")) return;
            if (dialogueEntry == null || carp == null) return;

            _fired = true;
            carp.QueueDialogue(dialogueEntry);
        }
    }
}
