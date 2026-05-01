using System;
using System.Collections.Generic;
using UnityEngine;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Singleton on the Player that tracks all unlocked tools and fires events on unlock.
    /// Persists unlock state to PlayerPrefs using tool names as keys.
    /// </summary>
    public class ToolInventory : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        public static ToolInventory Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private List<ToolDefinition> allToolDefinitions;

        // ── Public API ───────────────────────────────────────────────────────────

        public IReadOnlyList<ToolDefinition> UnlockedTools => _unlocked;
        public event Action<ToolDefinition> OnToolUnlocked;

        // ── Private ──────────────────────────────────────────────────────────────

        private readonly List<ToolDefinition> _unlocked = new();

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadFromPrefs();
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Unlock a tool and persist the state.</summary>
        public void UnlockTool(ToolDefinition tool)
        {
            if (tool == null || IsUnlocked(tool)) return;

            _unlocked.Add(tool);
            PlayerPrefs.SetInt(PrefKey(tool), 1);
            PlayerPrefs.Save();
            OnToolUnlocked?.Invoke(tool);
        }

        /// <summary>Returns true if the given tool has been unlocked.</summary>
        public bool IsUnlocked(ToolDefinition tool)
        {
            return tool != null && _unlocked.Contains(tool);
        }

        // ── Persistence ──────────────────────────────────────────────────────────

        private void LoadFromPrefs()
        {
            _unlocked.Clear();
            foreach (ToolDefinition def in allToolDefinitions)
            {
                if (def != null && PlayerPrefs.GetInt(PrefKey(def), 0) == 1)
                    _unlocked.Add(def);
            }
        }

        private static string PrefKey(ToolDefinition tool) => $"Tool_Unlocked_{tool.toolName}";
    }
}
