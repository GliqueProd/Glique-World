using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using GliqeWorld.Player;
using GliqeWorld.Tools;

namespace GliqeWorld.Portals
{
    /// <summary>
    /// Handles the full portal enter/exit flow:
    /// fade → additive scene load → controller config swap → tool grant → fade in.
    /// Uses LoadSceneMode.Additive so the main world stays loaded throughout.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────

        public static SceneTransitionManager Instance { get; private set; }

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ToolInventory toolInventory;
        [SerializeField] private Volume postProcessVolume;

        // ── State ────────────────────────────────────────────────────────────────

        private Player.PortalSceneConfig _activePortalConfig;
        private bool _inPortal;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Begins the portal entry sequence.</summary>
        public void EnterPortal(Player.PortalSceneConfig config)
        {
            if (_inPortal) return;
            StartCoroutine(EnterPortalRoutine(config));
        }

        /// <summary>Begins the portal exit sequence, restoring the main world config.</summary>
        public void ExitPortal()
        {
            if (!_inPortal) return;
            StartCoroutine(ExitPortalRoutine());
        }

        // ── Coroutines ───────────────────────────────────────────────────────────

        private IEnumerator EnterPortalRoutine(Player.PortalSceneConfig config)
        {
            _inPortal = true;
            _activePortalConfig = config;

            yield return StartCoroutine(Fade(0f, 1f));

            AsyncOperation load = SceneManager.LoadSceneAsync(config.sceneName, LoadSceneMode.Additive);
            yield return load;

            // Apply portal overrides
            playerController?.ApplyPortalConfig(config);
            if (config.postProcessOverride != null && postProcessVolume != null)
                postProcessVolume.profile = config.postProcessOverride;

            // Move player to spawn point in the loaded scene
            MovePlayerToSpawnInScene(config.sceneName);

            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator ExitPortalRoutine()
        {
            yield return StartCoroutine(Fade(0f, 1f));

            if (_activePortalConfig != null)
            {
                // Grant tool on exit
                if (_activePortalConfig.grantedTool != null)
                    toolInventory?.UnlockTool(_activePortalConfig.grantedTool);

                // Unload portal scene
                AsyncOperation unload = SceneManager.UnloadSceneAsync(_activePortalConfig.sceneName);
                yield return unload;

                // Restore main world config
                playerController?.ResetPortalConfig();
            }

            _inPortal = false;
            _activePortalConfig = null;

            yield return StartCoroutine(Fade(1f, 0f));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private IEnumerator Fade(float from, float to)
        {
            if (fadeCanvas == null) yield break;

            fadeCanvas.gameObject.SetActive(true);
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            fadeCanvas.alpha = to;

            if (to == 0f)
                fadeCanvas.gameObject.SetActive(false);
        }

        private static void MovePlayerToSpawnInScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform spawn = root.transform.Find("SpawnPoint");
                if (spawn == null) continue;

                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                    player.transform.position = spawn.position;
                return;
            }
        }
    }
}
