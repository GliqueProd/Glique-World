using System.Collections.Generic;
using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Cylinder-traced UV stamp tool. Records the roller's path along a surface
    /// and stamps a continuous band of paint via RenderTexture blits.
    /// </summary>
    public class PaintRollerTool : HandItem, IPaintTool
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Transform rollerMesh;
        [SerializeField] private Material paintMaterial;
        [SerializeField] private Color rollColor = Color.blue;
        [SerializeField] private float rollerWidth = 0.3f;

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public PaintTarget CurrentTarget { get; set; } = PaintTarget.World;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _rolling;
        private Vector2 _lastUV;
        private RenderTexture _targetRT;
        private HandAnchor _anchor;
        private float _distanceCovered;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            _anchor = anchor;
            ContainedSubstance = SubstanceType.Paint;
        }

        public override void OnUnequip()
        {
            if (_rolling) EndStroke();
            _anchor = null;
        }

        public override void OnUse() { }

        public override void OnAltUse() { }

        // ── IPaintTool ───────────────────────────────────────────────────────────

        public void BeginStroke(RaycastHit hit)
        {
            _rolling = true;
            _lastUV = hit.textureCoord;
            _targetRT = GetTargetRT(hit);
            _distanceCovered = 0f;
        }

        public void ContinueStroke(RaycastHit hit)
        {
            if (!_rolling || _targetRT == null) return;

            Vector2 currentUV = hit.textureCoord;
            float uvDelta = Vector2.Distance(currentUV, _lastUV);
            _distanceCovered += uvDelta;

            // Rotate roller mesh proportionally to distance
            if (rollerMesh != null)
                rollerMesh.Rotate(Vector3.right, uvDelta * 360f, Space.Self);

            BlitStrip(_lastUV, currentUV, _targetRT);
            _lastUV = currentUV;
        }

        public void EndStroke()
        {
            _rolling = false;
            _targetRT = null;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private RenderTexture GetTargetRT(RaycastHit hit)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) return null;

            if (rend.material.mainTexture is RenderTexture rt)
                return rt;

            return null;
        }

        private void BlitStrip(Vector2 from, Vector2 to, RenderTexture rt)
        {
            if (paintMaterial == null) return;

            paintMaterial.SetColor("_Color", rollColor);
            paintMaterial.SetVector("_UVFrom", new Vector4(from.x, from.y, 0, 0));
            paintMaterial.SetVector("_UVTo", new Vector4(to.x, to.y, 0, 0));
            paintMaterial.SetFloat("_Width", rollerWidth / rt.width);

            Graphics.Blit(rt, rt, paintMaterial);
        }
    }
}
