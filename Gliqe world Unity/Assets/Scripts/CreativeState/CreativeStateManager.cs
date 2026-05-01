using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GliqeWorld.CreativeState
{
    public enum CreativeActionType
    {
        PaintStroke,
        DecalPlaced,
        SketchbookDraw,
        PhotoTaken,
        NPCCreated,
        Contamination
    }

    /// <summary>
    /// Central singleton tracking the player's creative output score.
    /// Drives the global visual transition from desaturated to vibrant
    /// via a global shader float and a post-process volume blend weight.
    /// </summary>
    public class CreativeStateManager : MonoBehaviour
    {
        // ── Shader property ──────────────────────────────────────────────────────

        private static readonly int CreativeIntensityId = Shader.PropertyToID("_CreativeIntensity");

        // ── Singleton ────────────────────────────────────────────────────────────

        public static CreativeStateManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField, Range(0f, 1f)] private float startIntensity = 0f;
        [SerializeField] private Volume creativeVolume;

        [Header("Action Weights")]
        [SerializeField] private float paintStrokeWeight = 0.005f;
        [SerializeField] private float decalWeight = 0.01f;
        [SerializeField] private float sketchbookWeight = 0.003f;
        [SerializeField] private float photoWeight = 0.02f;
        [SerializeField] private float npcWeight = 0.05f;

        // ── Public API ───────────────────────────────────────────────────────────

        public float CreativeIntensity { get; private set; }
        public event Action<float> OnIntensityChanged;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CreativeIntensity = Mathf.Clamp01(startIntensity);
        }

        private void Update()
        {
            Shader.SetGlobalFloat(CreativeIntensityId, CreativeIntensity);

            if (creativeVolume != null)
                creativeVolume.weight = CreativeIntensity;
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>
        /// Adds a weighted contribution to the creative intensity.
        /// Negative weights are allowed (e.g. contamination penalty).
        /// </summary>
        public void AddCreativeAction(CreativeActionType type, float weight = float.NaN)
        {
            float w = float.IsNaN(weight) ? GetDefaultWeight(type) : weight;
            CreativeIntensity = Mathf.Clamp01(CreativeIntensity + w);
            OnIntensityChanged?.Invoke(CreativeIntensity);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private float GetDefaultWeight(CreativeActionType type) => type switch
        {
            CreativeActionType.PaintStroke  => paintStrokeWeight,
            CreativeActionType.DecalPlaced  => decalWeight,
            CreativeActionType.SketchbookDraw => sketchbookWeight,
            CreativeActionType.PhotoTaken   => photoWeight,
            CreativeActionType.NPCCreated   => npcWeight,
            CreativeActionType.Contamination => -0.05f,
            _ => 0f
        };
    }
}
