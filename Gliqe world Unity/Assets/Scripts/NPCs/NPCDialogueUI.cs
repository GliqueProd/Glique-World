using UnityEngine;
using UnityEngine.UIElements;

namespace GliqeWorld.NPCs
{
    /// <summary>
    /// Manages the UI Toolkit dialogue panel for BillboardNPC interactions.
    /// Static helper opens/closes the panel and populates it with NPCData.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class NPCDialogueUI : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        private static NPCDialogueUI _instance;

        // ── State ────────────────────────────────────────────────────────────────

        private UIDocument _doc;
        private VisualElement _root;
        private Label _nameLabel;
        private Label _lineLabel;
        private VisualElement _choicesContainer;
        private VisualElement _portraitElement;

        private NPCData _currentData;
        private int _currentLine;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            _instance = this;
            _doc = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            _nameLabel = _root.Q<Label>("npc-name");
            _lineLabel = _root.Q<Label>("dialogue-line");
            _choicesContainer = _root.Q<VisualElement>("choices-container");
            _portraitElement = _root.Q<VisualElement>("portrait");

            _root.style.display = DisplayStyle.None;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Opens the dialogue panel for the given NPCData.</summary>
        public static void Open(NPCData data)
        {
            if (_instance == null || data == null) return;
            _instance.Show(data);
        }

        /// <summary>Closes the dialogue panel.</summary>
        public static void Close()
        {
            if (_instance == null) return;
            _instance.Hide();
        }

        // ── Private Methods ──────────────────────────────────────────────────────

        private void Show(NPCData data)
        {
            _currentData = data;
            _currentLine = 0;

            if (_nameLabel != null) _nameLabel.text = data.npcName;
            if (_portraitElement != null && data.portrait != null)
                _portraitElement.style.backgroundImage = new StyleBackground(data.portrait);

            _root.style.display = DisplayStyle.Flex;
            DisplayLine();
        }

        private void Hide()
        {
            _root.style.display = DisplayStyle.None;
            _currentData = null;
        }

        private void DisplayLine()
        {
            if (_currentData == null || _lineLabel == null) return;

            if (_currentLine < _currentData.dialogueLines.Count)
            {
                _lineLabel.text = _currentData.dialogueLines[_currentLine];
                _currentLine++;
            }
            else
            {
                ShowChoices();
            }
        }

        private void ShowChoices()
        {
            if (_choicesContainer == null || _currentData == null) return;

            _choicesContainer.Clear();
            foreach (NPCDialogueChoice choice in _currentData.choices)
            {
                Button btn = new Button { text = choice.label };
                NPCDialogueChoice captured = choice;
                btn.clicked += () =>
                {
                    captured.action?.Invoke();
                    Hide();
                };
                _choicesContainer.Add(btn);
            }

            // Trigger linked NPCs after dialogue
            foreach (NPCData linked in _currentData.linkedNPCs)
            {
                // Open the next NPC's dialogue in sequence (first-found instance)
                BillboardNPC[] allNPCs = FindObjectsByType<BillboardNPC>(FindObjectsSortMode.None);
                foreach (BillboardNPC npc in allNPCs)
                {
                    if (npc.Data == linked) { npc.Interact(); break; }
                }
            }
        }
    }
}
