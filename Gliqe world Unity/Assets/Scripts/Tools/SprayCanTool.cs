using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Particle system burst + soft-falloff decal stamp art tool.
    /// Depletes AerosolAmount on use; refillable by looting spray cans in the city.
    /// </summary>
    public class SprayCanTool : HandItem, IPaintTool
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private ParticleSystem nozzleParticles;
        [SerializeField] private Material sprayDecalMaterial;
        [SerializeField] private Texture2D stencilMask;
        [SerializeField] private float aerosolMax = 100f;
        [SerializeField, Range(0.05f, 0.4f)] private float stampRadius = 0.2f;
        [SerializeField] private Color sprayColor = Color.red;

        // ── Public ───────────────────────────────────────────────────────────────

        public float AerosolAmount { get; private set; }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public PaintTarget CurrentTarget { get; set; } = PaintTarget.World;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _spraying;
        private HandAnchor _anchor;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            _anchor = anchor;
            ContainedSubstance = SubstanceType.Paint;
            AerosolAmount = aerosolMax;
        }

        public override void OnUnequip()
        {
            if (_spraying) EndStroke();
            _anchor = null;
        }

        public override void OnUse()
        {
            // Continuous use handled in ContinueStroke
        }

        public override void OnAltUse() { }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public void BeginStroke(RaycastHit hit)
        {
            if (AerosolAmount <= 0f) return;
            _spraying = true;

            if (nozzleParticles != null && !nozzleParticles.isPlaying)
                nozzleParticles.Play();

            SprayDecal(hit);
        }

        public void ContinueStroke(RaycastHit hit)
        {
            if (!_spraying || AerosolAmount <= 0f)
            {
                EndStroke();
                return;
            }

            AerosolAmount -= Time.deltaTime * 10f;
            SprayDecal(hit);
        }

        public void EndStroke()
        {
            _spraying = false;

            if (nozzleParticles != null && nozzleParticles.isPlaying)
                nozzleParticles.Stop();
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Refills the canister. Called by the loot / consumable system.</summary>
        public void Refill(float amount) => AerosolAmount = Mathf.Min(AerosolAmount + amount, aerosolMax);

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SprayDecal(RaycastHit hit)
        {
            if (WorldDecalCanvas.Instance == null) return;

            float scale = Random.Range(stampRadius * 0.8f, stampRadius * 1.2f);
            Material mat = new Material(sprayDecalMaterial);
            mat.SetColor("_BaseColor", sprayColor);
            if (stencilMask != null)
                mat.SetTexture("_Mask", stencilMask);

            WorldDecalCanvas.Instance.PlaceDecal(hit, mat, Vector2.one * scale, Random.Range(0.5f, 1f));
        }
    }
}
