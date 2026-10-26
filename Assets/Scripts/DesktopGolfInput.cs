using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGolfVR
{
    // Kept only so previously generated scenes can remove this component cleanly.
    public sealed class DesktopGolfInput : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame game;
        [SerializeField] private Transform aimMarker;
        private float yaw;

        private void Update()
        {
            if (game == null || aimMarker == null) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            yaw += ((keyboard.dKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed ? 1f : 0f)) * 85f * Time.deltaTime;
            Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            if (keyboard.spaceKey.wasPressedThisFrame)
                game.TryStrike(game.Ball, direction, 0.6f);
        }
    }
}
