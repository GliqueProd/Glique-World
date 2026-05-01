using UnityEngine;
using GliqeWorld.Player;

namespace GliqeWorld.Portals
{
    /// <summary>
    /// Placed on the trigger collider inside PortalDoor.
    /// Calls SceneTransitionManager.EnterPortal when the player walks through.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PortalTransitionController : MonoBehaviour
    {
        [SerializeField] private PortalSceneConfig portalConfig;

        private void Awake() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            SceneTransitionManager.Instance?.EnterPortal(portalConfig);
        }
    }
}
