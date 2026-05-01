using UnityEngine;

namespace GliqeWorld.NPCs
{
    /// <summary>
    /// A world-space quad that always faces the player camera on the Y axis.
    /// Holds an NPCData reference for appearance, name, and dialogue.
    /// </summary>
    public class BillboardNPC : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private NPCData data;

        // ── Public API ───────────────────────────────────────────────────────────

        public NPCData Data
        {
            get => data;
            set => data = value;
        }

        // ── State ────────────────────────────────────────────────────────────────

        private Camera _playerCamera;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Start()
        {
            _playerCamera = Camera.main;
        }

        private void Update()
        {
            FaceCamera();
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Opens the NPC dialogue UI with this NPC's data.</summary>
        public void Interact()
        {
            NPCDialogueUI.Open(data);
        }

        /// <summary>Links this NPC to another for chained dialogue triggers.</summary>
        public void LinkTo(BillboardNPC other)
        {
            if (other == null) return;
            // Trigger chaining is handled via NPCData.linkedNPCs wired in the editor
            other.Interact();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void FaceCamera()
        {
            if (_playerCamera == null) return;

            Vector3 dir = _playerCamera.transform.position - transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
