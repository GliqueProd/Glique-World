using System.Collections.Generic;
using UnityEngine;

namespace GliqeWorld.World
{
    [CreateAssetMenu(fileName = "NewScatterDefinition", menuName = "GliqeWorld/World/ScatterDefinition")]
    public class ScatterDefinition : ScriptableObject
    {
        public string buildingId;
        public List<GameObject> lootablePrefabs;
        public int density = 5;
        public Bounds spawnBounds;
    }

    /// <summary>
    /// Manages pools of lootable props scattered inside Zone 1 buildings.
    /// Reads ScatterDefinition assets for the active zone and populates the pool on scene load.
    /// Looted items are tracked in the save file so they don't respawn.
    /// </summary>
    public class LootableScatterSystem : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private List<ScatterDefinition> scatterDefinitions;
        [SerializeField] private Persistence.WorldSaveSystem saveSystem;

        // ── State ────────────────────────────────────────────────────────────────

        private readonly List<GameObject> _spawnedItems = new();

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Start() => Scatter();

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Marks a lootable item as taken and disables it.</summary>
        public void OnItemLooted(string itemId, GameObject item)
        {
            item.SetActive(false);
            saveSystem?.MarkLooted(itemId);
        }

        // ── Scatter ──────────────────────────────────────────────────────────────

        private void Scatter()
        {
            HashSet<string> looted = saveSystem != null
                ? new HashSet<string>(saveSystem.GetLootedIds())
                : new HashSet<string>();

            foreach (ScatterDefinition def in scatterDefinitions)
            {
                if (def == null || def.lootablePrefabs.Count == 0) continue;

                for (int i = 0; i < def.density; i++)
                {
                    string id = $"{def.buildingId}_{i}";
                    if (looted.Contains(id)) continue;

                    GameObject prefab = def.lootablePrefabs[Random.Range(0, def.lootablePrefabs.Count)];
                    if (prefab == null) continue;

                    Vector3 pos = RandomPointInBounds(def.spawnBounds);
                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
                    _spawnedItems.Add(obj);
                }
            }
        }

        private static Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
    }
}
