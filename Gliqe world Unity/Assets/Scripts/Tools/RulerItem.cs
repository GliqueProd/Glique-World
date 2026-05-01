using UnityEngine;
using GliqeWorld.Hands;
using TMPro;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Technical measurement tool. Places two endpoints via OnUse and renders
    /// a LineRenderer between them, displaying the real-world distance in world-space text.
    /// </summary>
    public class RulerItem : HandItem
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float MeasureRange = 20f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private LineRenderer line;
        [SerializeField] private TextMeshPro distanceLabel;

        // ── State ────────────────────────────────────────────────────────────────

        private Vector3? _start;
        private Vector3? _end;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor) { }

        public override void OnUnequip() { Reset(); }

        public override void OnUse()
        {
            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, MeasureRange)) return;

            if (_start == null)
            {
                _start = hit.point;
                if (line != null)
                {
                    line.enabled = true;
                    line.SetPosition(0, _start.Value);
                    line.SetPosition(1, _start.Value);
                }
            }
            else
            {
                _end = hit.point;
                UpdateLine();
            }
        }

        public override void OnAltUse() => Reset();

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_start.HasValue && !_end.HasValue && line != null)
            {
                Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, MeasureRange))
                    line.SetPosition(1, hit.point);
            }
        }

        private void UpdateLine()
        {
            if (!_start.HasValue || !_end.HasValue || line == null) return;

            line.SetPosition(0, _start.Value);
            line.SetPosition(1, _end.Value);

            float dist = Vector3.Distance(_start.Value, _end.Value);
            if (distanceLabel != null)
            {
                distanceLabel.text = $"{dist:F2} m";
                distanceLabel.transform.position = (_start.Value + _end.Value) * 0.5f + Vector3.up * 0.1f;
                distanceLabel.gameObject.SetActive(true);
            }
        }

        private void Reset()
        {
            _start = null;
            _end = null;

            if (line != null) line.enabled = false;
            if (distanceLabel != null) distanceLabel.gameObject.SetActive(false);
        }
    }
}
