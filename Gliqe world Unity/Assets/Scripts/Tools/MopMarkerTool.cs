using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// First unlocked art tool. Wide stamp brush that places paint quads
    /// on world surfaces via WorldDecalCanvas.
    /// </summary>
    public class MopMarkerTool : HandItem, IPaintTool
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float MinStampDistance = 0.05f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Material mopDecalMaterial;
        [SerializeField] private Vector2 stampSize = new Vector2(0.4f, 0.15f);
        [SerializeField] private Color paintColor = Color.white;

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public PaintTarget CurrentTarget { get; set; } = PaintTarget.World;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _stroking;
        private Vector3 _lastStampPosition;
        private HandAnchor _anchor;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            _anchor = anchor;
            ContainedSubstance = SubstanceType.Paint;
        }

        public override void OnUnequip()
        {
            if (_stroking) EndStroke();
            _anchor = null;
        }

        public override void OnUse()
        {
            // Handled via IPaintTool stroke lifecycle from the interaction system
        }

        public override void OnAltUse() { }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public void BeginStroke(RaycastHit hit)
        {
            _stroking = true;
            _lastStampPosition = hit.point;
            Stamp(hit);
        }

        public void ContinueStroke(RaycastHit hit)
        {
            if (!_stroking) return;

            float dist = Vector3.Distance(hit.point, _lastStampPosition);
            if (dist >= MinStampDistance)
            {
                Stamp(hit);
                _lastStampPosition = hit.point;
            }
        }

        public void EndStroke()
        {
            _stroking = false;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void Stamp(RaycastHit hit)
        {
            if (WorldDecalCanvas.Instance == null) return;

            Material mat = new Material(mopDecalMaterial);
            mat.SetColor("_BaseColor", paintColor);
            WorldDecalCanvas.Instance.PlaceDecal(hit, mat, stampSize);
        }
    }
}
