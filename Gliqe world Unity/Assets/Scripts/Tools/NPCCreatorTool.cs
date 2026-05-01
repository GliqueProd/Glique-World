using UnityEngine;
using GliqeWorld.Hands;
using GliqeWorld.NPCs;

namespace GliqeWorld.Tools
{
    /// <summary>
    /// Right-hand tool unlocked by exiting Nivvelair.
    /// Raycasts into the world and instantiates BillboardNPC instances at the hit point.
    /// Opens an inspector-style UI for authoring the NPC's data on placement.
    /// </summary>
    public class NPCCreatorTool : HandItem
    {
        // ── Constants ────────────────────────────────────────────────────────────

        private const float PlaceRange = 5f;

        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private Camera worldCamera;
        [SerializeField] private GameObject billboardNPCPrefab;
        [SerializeField] private LayerMask placementMask;

        // ── HandItem ─────────────────────────────────────────────────────────────

        public override void OnEquip(HandAnchor anchor) { }

        public override void OnUnequip() { }

        public override void OnUse()
        {
            PlaceNPC();
        }

        public override void OnAltUse() { }

        // ── Private Methods ──────────────────────────────────────────────────────

        private void PlaceNPC()
        {
            if (worldCamera == null || billboardNPCPrefab == null) return;

            Ray ray = worldCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, PlaceRange, placementMask)) return;

            GameObject npcObj = Instantiate(billboardNPCPrefab, hit.point, Quaternion.identity);
            BillboardNPC npc = npcObj.GetComponent<BillboardNPC>();

            if (npc == null) return;

            // Create a blank NPCData at runtime
            NPCData blankData = ScriptableObject.CreateInstance<NPCData>();
            blankData.npcName = "New NPC";
            npc.Data = blankData;

            // Open the creation UI
            NPCCreatorUI.Open(npc);
        }
    }
}
