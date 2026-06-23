using UnityEngine;
using UnityEngine.Rendering;

namespace GliqeWorld.Player
{
    /// <summary>
    /// Data container for all tunable player controller values.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "GliqeWorld/Player/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 7.5f;
        public float duckSpeed = 2f;

        [Header("Stance")]
        [Tooltip("Capsule height multiplier while ducking (0–1).")]
        public float duckHeightMultiplier = 0.6f;
        public float peekAngle = 15f;
        public float peekDuration = 0.25f;

        [Header("Physics")]
        public float jumpHeight = 1.2f;
        public float gravity = -20f;

        [Header("Camera")]
        public float mouseSensitivity = 0.15f;
        public float defaultFOV = 75f;

        [Header("FOV Kick")]
        public float sprintFOV = 85f;
        [Range(1f, 20f)] public float fovSmoothing = 8f;

        [Header("TPS Camera")]
        public float tpsDistance = 4f;
        public float tpsHeightOffset = 0.8f;
        [Range(1f, 20f)] public float tpsCameraSmoothing = 8f;
        public float tpsFOV = 65f;
    }
}
