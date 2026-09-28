using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniGolfVR
{
    public sealed class DesktopGolfInput : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame game;
        [SerializeField] private Transform aimMarker;
        [SerializeField, Range(0f, 1f)] private float power = 0.6f;

        private float yaw;

        public void Configure(MiniGolfGame owner, Transform marker)
        {
            game = owner;
            aimMarker = marker;
        }

        private void Update()
        {
            if (game == null || game.Rig == null || aimMarker == null) return;
            bool desktop = !game.Rig.VrActive;
            aimMarker.gameObject.SetActive(desktop && !game.MatchEnded);
            if (!desktop) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.rKey.wasPressedThisFrame) game.Restart();
            if (game.CanChoosePlayers)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) game.SetPlayerCount(1);
                if (keyboard.digit2Key.wasPressedThisFrame) game.SetPlayerCount(2);
                if (keyboard.digit3Key.wasPressedThisFrame) game.SetPlayerCount(3);
                if (keyboard.digit4Key.wasPressedThisFrame) game.SetPlayerCount(4);
            }

            float turn = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float strength = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) -
                (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            yaw += turn * 85f * Time.deltaTime;
            power = Mathf.Clamp(power + strength * 0.65f * Time.deltaTime, 0.12f, 1f);
            Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

            if (!game.MatchEnded)
            {
                aimMarker.position = game.Ball.transform.position + direction * 0.32f + Vector3.up * 0.12f;
                aimMarker.rotation = Quaternion.LookRotation(direction);
                aimMarker.localScale = new Vector3(0.045f + power * 0.03f, 0.025f, 0.45f + power * 0.35f);
            }
            if (keyboard.spaceKey.wasPressedThisFrame)
                game.TryStrike(game.Ball, direction, power);
        }
    }
}
