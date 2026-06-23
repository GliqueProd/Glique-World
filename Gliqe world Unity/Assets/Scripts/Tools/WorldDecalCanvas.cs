using System.Collections.Generic;
using UnityEngine;
using GliqeWorld.CreativeState;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Manages a pool of Quad GameObjects for world painting.
    /// Each quad is oriented to the surface normal at the paint hit point.
    /// No URP Decal Renderer Feature required.
    /// </summary>
    public class WorldDecalCanvas : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const int DefaultPoolSize = 256;

        // ── Singleton ────────────────────────────────────────────────────────────

        public static WorldDecalCanvas Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private int maxDecals = DefaultPoolSize;
        [SerializeField] private CreativeStateManager creativeState;

        // ── Private ──────────────────────────────────────────────────────────────

        private static Mesh _quadMesh;
        private readonly Queue<MeshRenderer> _pool = new();
        private readonly List<MeshRenderer> _active = new();

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            PrewarmPool();
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>
        /// Places a paint quad at the hit point, oriented to the surface normal.
        /// </summary>
        public void PlaceDecal(RaycastHit hit, Material decalMaterial, Vector2 size, float fadeFactor = 1f)
        {
            MeshRenderer renderer = GetFromPool();
            Transform t = renderer.transform;

            t.position = hit.point + hit.normal * 0.005f;

            // Avoid LookRotation singularity when normal is nearly vertical
            Vector3 worldUp = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.99f
                ? Vector3.forward
                : Vector3.up;
            t.rotation = Quaternion.LookRotation(hit.normal, worldUp);
            t.localScale = new Vector3(size.x, size.y, 1f);

            renderer.material = decalMaterial;
            renderer.gameObject.SetActive(true);

            _active.Add(renderer);
            creativeState?.AddCreativeAction(CreativeActionType.DecalPlaced);
        }

        /// <summary>Returns all active quads to the pool (e.g. on zone unload).</summary>
        public void ClearAll()
        {
            foreach (MeshRenderer r in _active)
                ReturnToPool(r);
            _active.Clear();
        }

        // ── Pool ─────────────────────────────────────────────────────────────────

        private void PrewarmPool()
        {
            for (int i = 0; i < maxDecals; i++)
            {
                MeshRenderer r = CreatePoolItem();
                r.gameObject.SetActive(false);
                _pool.Enqueue(r);
            }
        }

        private MeshRenderer GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            // Recycle the oldest active quad
            if (_active.Count > 0)
            {
                MeshRenderer oldest = _active[0];
                _active.RemoveAt(0);
                return oldest;
            }

            // Fallback: create a new item (should not happen with correct pool size)
            return CreatePoolItem();
        }

        private void ReturnToPool(MeshRenderer r)
        {
            r.gameObject.SetActive(false);
            _pool.Enqueue(r);
        }

        private MeshRenderer CreatePoolItem()
        {
            GameObject go = new GameObject("PaintQuad");
            go.transform.SetParent(transform);
            go.AddComponent<MeshFilter>().sharedMesh = GetQuadMesh();
            return go.AddComponent<MeshRenderer>();
        }

        // ── Quad Mesh ─────────────────────────────────────────────────────────────

        private static Mesh GetQuadMesh()
        {
            if (_quadMesh != null) return _quadMesh;

            _quadMesh = new Mesh { name = "PaintQuad" };
            _quadMesh.SetVertices(new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3( 0.5f,  0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f)
            });
            _quadMesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            _quadMesh.SetNormals(new[]
            {
                Vector3.forward, Vector3.forward,
                Vector3.forward, Vector3.forward
            });
            _quadMesh.SetUVs(0, new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            });
            return _quadMesh;
        }
    }
}
