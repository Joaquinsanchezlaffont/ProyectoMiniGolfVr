using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace MiniGolfVR
{
    public sealed class AnimateHandOnInput : MonoBehaviour
    {
        [SerializeField] private InputActionProperty triggerValue;
        [SerializeField] private InputActionProperty gripValue;
        [SerializeField] private Animator handAnimator;
        [SerializeField] private XRNode controller;

        private static readonly int Grip = Animator.StringToHash("Grip");
        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private SkinnedMeshRenderer malla;

        private void Awake() => malla = GetComponentInChildren<SkinnedMeshRenderer>(true);

        private void Update()
        {
            var control = controller == XRNode.LeftHand ? XRController.leftHand : XRController.rightHand;
            bool visible = control != null && control.isTracked.isPressed;
            malla.enabled = visible;
            if (!visible) return;

            handAnimator.SetFloat(Grip, Leer(gripValue));
            handAnimator.SetFloat(Trigger, Leer(triggerValue));
        }

        private static float Leer(InputActionProperty entrada) =>
            entrada.action == null ? 0f : entrada.action.ReadValue<float>();
    }
}
