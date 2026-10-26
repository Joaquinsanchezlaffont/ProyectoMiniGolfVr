using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace MiniGolfVR
{
    public sealed class MiniGolfRig : MonoBehaviour
    {
        [SerializeField] private Transform head;
        private Vector3 station;

        public bool VrActive => XRSettings.isDeviceActive;

        private void Awake() => UseFloorTracking();

        public void Configure(Transform cameraTransform)
        {
            head = cameraTransform;
            UseFloorTracking();
        }

        public void SetStation(Vector3 tee)
        {
            station = tee;
            transform.position = new Vector3(tee.x, 0f, tee.z - 0.58f);
        }

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
            // In Floor mode the headset already reports its real height.
            // Never position the tracked camera directly from this script.
        }
    }
}
