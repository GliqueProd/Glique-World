using UnityEngine;
using UnityEngine.Rendering;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Per-portal-scene overrides applied to the player controller and cameras on entry.
    /// </summary>
    [CreateAssetMenu(fileName = "PortalSceneConfig", menuName = "GliqeWorld/Portals/PortalSceneConfig")]
    public class PortalSceneConfig : ScriptableObject
    {
        [Header("Scene")]
        public string sceneName;

        [Header("Camera")]
        public float fovOverride = 75f;

        [Header("Movement Multipliers")]
        public float walkSpeedMultiplier = 1f;
        public float sprintSpeedMultiplier = 1f;

        [Header("Abilities")]
        public bool duckingEnabled = true;
        public bool peekingEnabled = true;

        [Header("Post Processing")]
        public VolumeProfile postProcessOverride;

        [Header("Tool Grant")]
        [Tooltip("Tool definition granted to the player on portal exit. Leave null for no grant.")]
        public Tools.ToolDefinition grantedTool;
    }
}
