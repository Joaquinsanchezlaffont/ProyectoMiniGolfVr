using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace MiniGolfVR
{
    // Valem's hand prefab has a 2D blend tree with Grip and Trigger parameters.
    // Read the same XRI Starter Assets actions used by the controller interactors.
    public sealed class AnimateHandOnInput : MonoBehaviour
    {
        [SerializeField] private InputActionProperty triggerValue;
        [SerializeField] private InputActionProperty gripValue;
        [SerializeField] private Animator handAnimator;
        [SerializeField] private XRNode controller;

        private static readonly int Grip = Animator.StringToHash("Grip");
        private static readonly int Trigger = Animator.StringToHash("Trigger");
        private Renderer[] parts;

        public void Configure(InputActionReference trigger, InputActionReference grip,
            Animator animator, XRNode node)
        {
            triggerValue = new InputActionProperty(trigger);
            gripValue = new InputActionProperty(grip);
            handAnimator = animator;
            controller = node;
        }

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
