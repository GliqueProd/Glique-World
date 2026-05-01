using UnityEngine;
using UnityEngine.UIElements;

namespace GliqeWorld.NPCs
{
    /// <summary>
    /// World-space UI Toolkit panel attached to a newly placed BillboardNPC.
    /// Allows the player to author the NPC's name, dialogue lines, and choices in-world.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class NPCCreatorUI : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        private static NPCCreatorUI _instance;

        // ── State ────────────────────────────────────────────────────────────────

        private UIDocument _doc;
        private VisualElement _root;
        private TextField _nameField;
        private TextField _linesField;
        private Button _confirmButton;
        private BillboardNPC _targetNPC;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            _instance = this;
            _doc = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            _nameField = _root.Q<TextField>("npc-name-field");
            _linesField = _root.Q<TextField>("dialogue-lines-field");
            _confirmButton = _root.Q<Button>("confirm-btn");

            if (_confirmButton != null)
                _confirmButton.clicked += OnConfirm;

            _root.style.display = DisplayStyle.None;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Opens the creator UI for the given BillboardNPC.</summary>
        public static void Open(BillboardNPC npc)
        {
            if (_instance == null || npc == null) return;
            _instance._targetNPC = npc;
            _instance._root.style.display = DisplayStyle.Flex;
        }

        // ── Confirm ──────────────────────────────────────────────────────────────

        private void OnConfirm()
        {
            if (_targetNPC == null) return;

            NPCData data = _targetNPC.Data;
            if (data == null) return;

            if (_nameField != null)
                data.npcName = _nameField.value;

            if (_linesField != null)
            {
                data.dialogueLines.Clear();
                foreach (string line in _linesField.value.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        data.dialogueLines.Add(trimmed);
                }
            }

            _root.style.display = DisplayStyle.None;
            _targetNPC = null;
        }
    }
}
