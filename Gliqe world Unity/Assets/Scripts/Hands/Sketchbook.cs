using UnityEngine;

namespace GliqeWorld.Hands
{
    /// <summary>
    /// Manages the 3D book GameObject, its array of page RenderTextures,
    /// page-turn state, and routes draw calls from right-hand tools to the active page.
    /// </summary>
    public class Sketchbook : HandItem
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private int totalPages = 24;
        [SerializeField] private int pageWidth = 1024;
        [SerializeField] private int pageHeight = 1024;
        [SerializeField] private Renderer pageRenderer;
        [SerializeField] private string pageTextureProp = "_MainTex";
        [SerializeField] private Animator bookAnimator;

        // ── Public API ───────────────────────────────────────────────────────────

        public int CurrentPage { get; private set; }
        public int TotalPages => totalPages;
        public RenderTexture ActivePageRT => _pages[CurrentPage];

        // ── Private ──────────────────────────────────────────────────────────────

        private RenderTexture[] _pages;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor)
        {
            InitPages();
            ApplyPageToRenderer();
            bookAnimator?.SetTrigger("Open");
        }

        public override void OnUnequip()
        {
            bookAnimator?.SetTrigger("Close");
        }

        public override void OnUse() => TurnPageForward();

        public override void OnAltUse() => TurnPageBack();

        // ── Public Methods ───────────────────────────────────────────────────────

        /// <summary>Advances to the next page, clamped to TotalPages - 1.</summary>
        public void TurnPageForward()
        {
            if (CurrentPage >= totalPages - 1) return;
            CurrentPage++;
            ApplyPageToRenderer();
            bookAnimator?.SetTrigger("TurnForward");
        }

        /// <summary>Goes back one page, clamped to 0.</summary>
        public void TurnPageBack()
        {
            if (CurrentPage <= 0) return;
            CurrentPage--;
            ApplyPageToRenderer();
            bookAnimator?.SetTrigger("TurnBack");
        }

        /// <summary>
        /// Blits a Texture2D region into the active page RenderTexture
        /// at the specified target rect (normalised 0–1 UV space).
        /// </summary>
        public void PasteTextureToPage(Texture2D source, Rect targetRegion)
        {
            if (source == null || ActivePageRT == null) return;

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = ActivePageRT;

            int x = Mathf.RoundToInt(targetRegion.x * ActivePageRT.width);
            int y = Mathf.RoundToInt(targetRegion.y * ActivePageRT.height);
            int w = Mathf.RoundToInt(targetRegion.width * ActivePageRT.width);
            int h = Mathf.RoundToInt(targetRegion.height * ActivePageRT.height);

            Graphics.DrawTexture(new Rect(x, y, w, h), source);

            RenderTexture.active = prev;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void InitPages()
        {
            if (_pages != null && _pages.Length == totalPages) return;

            _pages = new RenderTexture[totalPages];
            for (int i = 0; i < totalPages; i++)
            {
                _pages[i] = new RenderTexture(pageWidth, pageHeight, 0, RenderTextureFormat.ARGB32);
                _pages[i].name = $"SketchbookPage_{i}";
                ClearPage(_pages[i]);
            }
        }

        private void ClearPage(RenderTexture rt)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = prev;
        }

        private void ApplyPageToRenderer()
        {
            if (pageRenderer == null || _pages == null) return;
            pageRenderer.material.SetTexture(pageTextureProp, _pages[CurrentPage]);
        }
    }
}
