using System.Collections.Generic;
using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Technical tape-measure tool. Draws a persistent LineRenderer in the world
    /// that acts as a spatial reference. Use again to start a new tape run.
    /// Alt-use clears all placed tape.
    /// </summary>
    public class TapeItem : HandItem
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float PlaceRange = 20f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private GameObject tapeLinePrefab;
        [SerializeField] private Material tapeMaterial;

        // ── State ────────────────────────────────────────────────────────────────

        private readonly List<GameObject> _tapeLines = new();
        private LineRenderer _currentLine;
        private readonly List<Vector3> _currentPoints = new();

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor) { }

        public override void OnUnequip() { }

        public override void OnUse()
        {
            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, PlaceRange)) return;

            if (_currentLine == null)
                StartNewTape(hit.point);
            else
                ExtendTape(hit.point);
        }

        public override void OnAltUse() => ClearAllTape();

        // ── Tape Logic ───────────────────────────────────────────────────────────

        private void StartNewTape(Vector3 point)
        {
            GameObject obj = tapeLinePrefab != null
                ? Instantiate(tapeLinePrefab)
                : new GameObject("TapeLine");

            LineRenderer lr = obj.GetComponent<LineRenderer>() ?? obj.AddComponent<LineRenderer>();
            if (tapeMaterial != null) lr.material = tapeMaterial;
            lr.widthMultiplier = 0.01f;

            _currentLine = lr;
            _currentPoints.Clear();
            _currentPoints.Add(point);
            UpdateLine();
            _tapeLines.Add(obj);
        }

        private void ExtendTape(Vector3 point)
        {
            if (_currentLine == null) return;
            _currentPoints.Add(point);
            UpdateLine();
        }

        private void UpdateLine()
        {
            if (_currentLine == null) return;
            _currentLine.positionCount = _currentPoints.Count;
            _currentLine.SetPositions(_currentPoints.ToArray());
        }

        private void ClearAllTape()
        {
            foreach (GameObject tape in _tapeLines)
            {
                if (tape != null) Destroy(tape);
            }
            _tapeLines.Clear();
            _currentLine = null;
            _currentPoints.Clear();
        }
    }
}
