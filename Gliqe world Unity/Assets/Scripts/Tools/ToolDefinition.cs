using UnityEngine;

namespace GliqeWorld.Tools
{
    public enum ToolUnlockCondition { Spawn, CityEntrance, PostAscent, PortalExit }

    /// <summary>
    /// Data asset per tool: prefab reference, icon, name, and unlock condition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewToolDefinition", menuName = "GliqeWorld/Tools/ToolDefinition")]
    public class ToolDefinition : ScriptableObject
    {
        public string toolName;
        public GameObject prefab;
        public Sprite icon;
        public ToolUnlockCondition unlockCondition;

        [Tooltip("Required when unlockCondition == PortalExit. Matches the portal scene's config sceneName.")]
        public string portalSceneId;
    }
}
