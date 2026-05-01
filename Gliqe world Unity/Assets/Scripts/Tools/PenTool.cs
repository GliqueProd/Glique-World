using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Thin precision raycast line tool. Writes fine strokes to RenderTexture
    /// surfaces or Sketchbook pages via Graphics.Blit.
    /// </summary>
    public class PenTool : HandItem, IPaintTool
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color inkColor = Color.black;
        [SerializeField] private float inkMax = 100f;
        [SerializeField, Range(1, 5)] private int lineThicknessPx = 2;

        // ── Public ───────────────────────────────────────────────────────────────

        public float InkRemaining { get; private set; }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public PaintTarget CurrentTarget { get; set; } = PaintTarget.World;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _drawing;
        private Vector2 _lastUV;
        private RenderTexture _targetRT;
        private HandAnchor _anchor;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            _anchor = anchor;
            ContainedSubstance = SubstanceType.Ink;
            InkRemaining = inkMax;
        }

        public override void OnUnequip()
        {
            if (_drawing) EndStroke();
            _anchor = null;
        }

        public override void OnUse() { }

        public override void OnAltUse() { }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public void BeginStroke(RaycastHit hit)
        {
            if (InkRemaining <= 0f) return;

            _drawing = true;
            _lastUV = hit.textureCoord;
            _targetRT = GetTargetRT(hit);
        }

        public void ContinueStroke(RaycastHit hit)
        {
            if (!_drawing || _targetRT == null || InkRemaining <= 0f)
            {
                EndStroke();
                return;
            }

            Vector2 currentUV = hit.textureCoord;
            BlitLine(_lastUV, currentUV, _targetRT);
            InkRemaining -= Vector2.Distance(_lastUV, currentUV) * 10f;
            _lastUV = currentUV;
        }

        public void EndStroke()
        {
            _drawing = false;
            _targetRT = null;
        }

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Refills the ink cartridge. Called by the loot system.</summary>
        public void Refill(float amount) => InkRemaining = Mathf.Min(InkRemaining + amount, inkMax);

        // ── Helpers ──────────────────────────────────────────────────────────────

        private RenderTexture GetTargetRT(RaycastHit hit)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null && rend.material.mainTexture is RenderTexture rt)
                return rt;
            return null;
        }

        private void BlitLine(Vector2 from, Vector2 to, RenderTexture rt)
        {
            if (lineMaterial == null) return;

            lineMaterial.SetColor("_Color", inkColor);
            lineMaterial.SetVector("_UVFrom", new Vector4(from.x, from.y, 0, 0));
            lineMaterial.SetVector("_UVTo", new Vector4(to.x, to.y, 0, 0));
            lineMaterial.SetFloat("_ThicknessPx", lineThicknessPx);

            Graphics.Blit(rt, rt, lineMaterial);
        }
    }
}
