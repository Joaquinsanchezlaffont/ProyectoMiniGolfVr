using UnityEngine;
using UnityEngine.XR;

namespace MiniGolfVR
{
    // Attach to the Starter Assets XR Origin (XR Rig): it handles tracked poses,
    // input action references, interactors and locomotion.
    public sealed class MiniGolfRig : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private MiniGolfGame game;
        private Vector3 station;
        private bool previousPrimary;
        private bool previousSecondary;

        // A headset can render through OpenXR even if its position feature is
        // unavailable for a frame. Use the active XR display to choose the mode.
        public bool VrActive => XRSettings.isDeviceActive;

        public void Configure(Transform cameraTransform, MiniGolfGame owner)
        {
            head = cameraTransform;
            game = owner;
        }

        private void Update()
        {
            if (!VrActive || game == null) return;
            InputDevice controller = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            bool primary = controller.isValid &&
                controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool p) && p;
            bool secondary = controller.isValid &&
                controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool s) && s;
            if (primary && !previousPrimary && game.CanChoosePlayers)
                game.SetPlayerCount(game.PlayerCount % 4 + 1);
            if (secondary && !previousSecondary) game.Restart();
            previousPrimary = primary;
            previousSecondary = secondary;
        }

        public void SetStation(Vector3 tee)
        {
            station = tee;
            // Keep the player inside the starting rail and within reach of the club.
            transform.position = new Vector3(tee.x, 0f, tee.z - 0.58f);
            if (!VrActive) PlaceDesktopCamera();
        }

        private void LateUpdate()
        {
            if (VrActive) return;
            previousPrimary = previousSecondary = false;
            PlaceDesktopCamera();
        }

        private void PlaceDesktopCamera()
        {
            if (head == null) return;
            head.position = new Vector3(station.x, 3.5f, station.z - 3.6f);
            head.rotation = Quaternion.LookRotation(
                new Vector3(station.x, 0.12f, station.z + 3.7f) - head.position);
        }

    }
}
