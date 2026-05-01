using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GliqeWorld.Persistence
{
    // ── Save Data Structures ─────────────────────────────────────────────────────

    [Serializable]
    public class DecalSaveEntry
    {
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
        public float sizeX, sizeY;
        public string materialPath;
    }

    [Serializable]
    public class NPCSaveEntry
    {
        public string npcName;
        public float posX, posY, posZ;
        public List<string> dialogueLines = new();
    }

    [Serializable]
    public class WorldSaveData
    {
        public List<DecalSaveEntry> worldDecals = new();
        public List<NPCSaveEntry> placedNPCs = new();
        public List<string> lootedItemIds = new();
        public List<string> unlockedToolIds = new();
        public float creativeIntensity;
        public List<string> sketchbookPages = new(); // Base64-encoded PNG bytes
    }

    /// <summary>
    /// Serialises and deserialises mutable world state to JSON on disk.
    /// Covers placed decals, placed NPCs, looted items, sketchbook pages,
    /// and tool unlock states.
    /// Save is triggered on portal exit, zone transition, and player pause.
    /// </summary>
    public class WorldSaveSystem : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        public static WorldSaveSystem Instance { get; private set; }

        // ── Constants ────────────────────────────────────────────────────────────

        private const string SaveFileName = "gliqe_world_save.json";

        // ── State ────────────────────────────────────────────────────────────────

        private WorldSaveData _data = new();
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Load();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Saves the current world state to disk.</summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(SavePath, json);
        }

        /// <summary>Loads saved world state from disk.</summary>
        public void Load()
        {
            if (!File.Exists(SavePath)) return;

            try
            {
                string json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<WorldSaveData>(json) ?? new WorldSaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[WorldSaveSystem] Failed to load save: {e.Message}");
                _data = new WorldSaveData();
            }
        }

        /// <summary>Marks an item as looted so it doesn't respawn.</summary>
        public void MarkLooted(string itemId)
        {
            if (!_data.lootedItemIds.Contains(itemId))
                _data.lootedItemIds.Add(itemId);
        }

        /// <summary>Returns all looted item IDs.</summary>
        public IReadOnlyList<string> GetLootedIds() => _data.lootedItemIds;

        /// <summary>Saves the current creative intensity value.</summary>
        public void SetCreativeIntensity(float value) => _data.creativeIntensity = value;

        /// <summary>Returns the saved creative intensity.</summary>
        public float GetCreativeIntensity() => _data.creativeIntensity;

        /// <summary>Adds an unlocked tool ID to the save.</summary>
        public void AddUnlockedTool(string toolId)
        {
            if (!_data.unlockedToolIds.Contains(toolId))
                _data.unlockedToolIds.Add(toolId);
        }

        /// <summary>Saves a sketchbook page as a Base64-encoded PNG.</summary>
        public void SaveSketchbookPage(int pageIndex, RenderTexture rt)
        {
            if (rt == null) return;

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = prev;

            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);

            string encoded = Convert.ToBase64String(bytes);

            while (_data.sketchbookPages.Count <= pageIndex)
                _data.sketchbookPages.Add(string.Empty);

            _data.sketchbookPages[pageIndex] = encoded;
        }

        /// <summary>Restores a sketchbook page from the save into the given RenderTexture.</summary>
        public void LoadSketchbookPage(int pageIndex, RenderTexture target)
        {
            if (target == null || pageIndex >= _data.sketchbookPages.Count) return;

            string encoded = _data.sketchbookPages[pageIndex];
            if (string.IsNullOrEmpty(encoded)) return;

            byte[] bytes = Convert.FromBase64String(encoded);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            Graphics.Blit(tex, target);
            Destroy(tex);
        }
    }
}
