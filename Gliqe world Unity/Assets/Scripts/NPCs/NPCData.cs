using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GliqeWorld.NPCs
{
    [Serializable]
    public class NPCDialogueChoice
    {
        public string label;
        public UnityEvent action;
    }

    /// <summary>
    /// Data asset per placed NPC: portrait, dialogue lines, choices, and linked NPC chains.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "GliqeWorld/NPCs/NPCData")]
    public class NPCData : ScriptableObject
    {
        public string npcName;
        public Sprite portrait;

        [TextArea(1, 4)]
        public List<string> dialogueLines = new();

        public List<NPCDialogueChoice> choices = new();
        public List<NPCData> linkedNPCs = new();
    }
}
