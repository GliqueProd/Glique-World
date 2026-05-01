using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using GliqeWorld.CreativeState;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Manages a pool of DecalProjector instances for world painting.
    /// Controls sorting, max decal count, and acts as the placement point for tools.
    /// NOTE: Requires URP Decal Renderer Feature enabled on the active URP Renderer asset.
    /// </summary>
    public class WorldDecalCanvas : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const int DefaultPoolSize = 256;

        // ── Singleton ────────────────────────────────────────────────────────────

        public static WorldDecalCanvas Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private DecalProjector decalPrefab;
        [SerializeField] private int maxDecals = DefaultPoolSize;
        [SerializeField] private CreativeStateManager creativeState;

        // ── Private ──────────────────────────────────────────────────────────────

        private readonly Queue<DecalProjector> _pool = new();
        private readonly List<DecalProjector> _active = new();

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            PrewarmPool();
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>
        /// Places a decal at the hit point oriented to the surface normal.
        /// Returns the activated DecalProjector, or null if the pool is exhausted
        /// and no recyclable decal is available.
        /// </summary>
        public DecalProjector PlaceDecal(RaycastHit hit, Material decalMaterial, Vector2 size, float fadeFactor = 1f)
        {
            DecalProjector projector = GetFromPool();

            projector.transform.position = hit.point + hit.normal * 0.01f;
            projector.transform.rotation = Quaternion.LookRotation(-hit.normal);
            projector.size = new Vector3(size.x, size.y, 0.5f);
            projector.material = decalMaterial;
            projector.fadeFactor = fadeFactor;
            projector.gameObject.SetActive(true);

            _active.Add(projector);
            creativeState?.AddCreativeAction(CreativeActionType.DecalPlaced);
            return projector;
        }

        /// <summary>Returns all active decals to the pool (e.g. on zone unload).</summary>
        public void ClearAll()
        {
            foreach (DecalProjector d in _active)
                ReturnToPool(d);
            _active.Clear();
        }

        // ── Pool ─────────────────────────────────────────────────────────────────

        private void PrewarmPool()
        {
            if (decalPrefab == null) return;

            for (int i = 0; i < maxDecals; i++)
            {
                DecalProjector d = Instantiate(decalPrefab, transform);
                d.gameObject.SetActive(false);
                _pool.Enqueue(d);
            }
        }

        private DecalProjector GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            // Recycle the oldest active decal
            if (_active.Count > 0)
            {
                DecalProjector oldest = _active[0];
                _active.RemoveAt(0);
                return oldest;
            }

            // Fallback: instantiate (should not happen with correct pool size)
            return Instantiate(decalPrefab, transform);
        }

        private void ReturnToPool(DecalProjector d)
        {
            d.gameObject.SetActive(false);
            _pool.Enqueue(d);
        }
    }
}
