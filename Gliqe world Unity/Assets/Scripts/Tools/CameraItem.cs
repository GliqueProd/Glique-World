using System.Collections.Generic;
using UnityEngine;
using GliqeWorld.Hands;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Handheld camera tool. Captures a RenderTexture screenshot of the WorldCamera
    /// and stores it as a Texture2D in the PhotoLibrary for pasting into the Sketchbook.
    /// </summary>
    public class CameraItem : HandItem
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private AudioSource shutterSound;
        [SerializeField] private int captureWidth = 512;
        [SerializeField] private int captureHeight = 512;

        // ── Public API ───────────────────────────────────────────────────────────

        public IReadOnlyList<Texture2D> PhotoLibrary => _photos;

        // ── Private ──────────────────────────────────────────────────────────────

        private readonly List<Texture2D> _photos = new();

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor) { }

        public override void OnUnequip() { }

        public override void OnUse() => Capture();

        public override void OnAltUse() { }

        // ── Capture ──────────────────────────────────────────────────────────────

        private void Capture()
        {
            if (worldCamera == null) return;

            shutterSound?.Play();

            RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
            RenderTexture prev = worldCamera.targetTexture;

            worldCamera.targetTexture = rt;
            worldCamera.Render();

            RenderTexture.active = rt;
            Texture2D shot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            shot.Apply();

            RenderTexture.active = null;
            worldCamera.targetTexture = prev;
            rt.Release();

            _photos.Add(shot);
        }
    }
}
