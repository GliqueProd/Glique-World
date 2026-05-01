using UnityEngine;

namespace GliqeWorld.Hands
{
    /// <summary>
    /// "Scan" mechanic — samples a surface texture from the world and blits
    /// it into the current sketchbook page.
    /// Requires the Sketchbook to be held in the left hand.
    /// </summary>
    public class WorldCopyPaste : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float ScanRange = 3f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private HandAnchor leftHandAnchor;
        [SerializeField] private Material copyMaterial;
        [SerializeField, Range(0.01f, 0.5f)] private float sampleRegionSize = 0.2f;
        [SerializeField] private ParticleSystem scanFlashVFX;

        // ── State ────────────────────────────────────────────────────────────────

        private bool _scanInput;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Update()
        {
            // Scan triggered externally — hook this to PlayerInputHandler in production
            if (_scanInput)
                TryScan();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Call this from PlayerInputHandler's scan action callback.</summary>
        public void OnScanInput() => _scanInput = true;

        // ── Scan ─────────────────────────────────────────────────────────────────

        private void TryScan()
        {
            _scanInput = false;

            Sketchbook book = leftHandAnchor?.HeldItem as Sketchbook;
            if (book == null) return;

            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, ScanRange)) return;

            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null || rend.material.mainTexture == null) return;

            Texture sourceTexture = rend.material.mainTexture;

            // Blit the sampled region onto the active sketchbook page
            RenderTexture target = book.ActivePageRT;
            if (target == null) return;

            if (copyMaterial != null)
            {
                Vector2 uv = hit.textureCoord;
                copyMaterial.SetVector("_SampleCenter", new Vector4(uv.x, uv.y, 0, 0));
                copyMaterial.SetFloat("_SampleSize", sampleRegionSize);
                Graphics.Blit(sourceTexture, target, copyMaterial);
            }
            else
            {
                Graphics.Blit(sourceTexture, target);
            }

            scanFlashVFX?.Play();
        }
    }
}
