using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

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

        private void Awake()
        {
            malla = GetComponentInChildren<SkinnedMeshRenderer>(true);
            // El rayo aparece sobre un botón u objeto, pero no queda dibujado en el aire.
            var rayo = transform.parent.GetComponentInChildren<CurveVisualController>(true);
            if (rayo != null)
            {
                rayo.noValidHitProperties.startWidth = 0;
                rayo.noValidHitProperties.endWidth = 0;
            }
        }

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
