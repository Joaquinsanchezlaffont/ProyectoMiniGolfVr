using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace MiniGolfVR
{
    // Las animaciones de las manos usan el grip y el gatillo de los controles.
    public sealed class AnimateHandOnInput : MonoBehaviour
    {
        [SerializeField] private InputActionProperty triggerValue;
        [SerializeField] private InputActionProperty gripValue;
        [SerializeField] private Animator handAnimator;
        [SerializeField] private XRNode controller;

        private static readonly int Grip = Animator.StringToHash("Grip");
        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private Renderer[] parts;

        private void Awake() => parts = GetComponentsInChildren<Renderer>(true);

        private void LateUpdate()
        {
            UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(controller);
            bool tracked = device.isValid &&
                (!device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool isTracked) || isTracked);
            if (parts == null) parts = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer part in parts) part.enabled = tracked;
            if (!tracked || handAnimator == null) return;

            handAnimator.SetFloat(Grip, Mathf.Clamp01(Read(gripValue)));
            handAnimator.SetFloat(Trigger, Mathf.Clamp01(Read(triggerValue)));
        }

        private static float Read(InputActionProperty property) =>
            property.action != null && property.action.enabled
                ? property.action.ReadValue<float>() : 0f;
    }
}
