using UnityEngine;

namespace GliqeWorld.TigerCarp
{
    /// <summary>
    /// Data asset defining a single Tiger-Carp dialogue beat.
    /// Created as a ScriptableObject so dialogue authors can configure entries in the Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueEntry", menuName = "GliqeWorld/TigerCarp/DialogueEntry")]
    public class DialogueEntry : ScriptableObject
    {
        [TextArea(2, 5)]
        public string[] lines;

        public CarpExpression expression = CarpExpression.Idle;

        [Tooltip("Seconds to wait after the trigger fires before speaking.")]
        public float delay = 0.5f;
    }
}
