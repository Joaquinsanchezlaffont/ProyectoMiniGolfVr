using UnityEngine;
using Unity.XR.CoreUtils;

namespace MiniGolfVR
{
    public sealed class MiniGolfRig : MonoBehaviour
    {
        [SerializeField] private Transform head;
        private void Awake() => UseFloorTracking();

        private void UseFloorTracking()
        {
            XROrigin origin = GetComponent<XROrigin>();
            if (origin == null) return;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            if (head != null)
            {
                head.localPosition = Vector3.zero;
                head.localRotation = Quaternion.identity;
            }
            // El visor ya indica su altura cuando usamos el modo Floor.
        }
    }
}
