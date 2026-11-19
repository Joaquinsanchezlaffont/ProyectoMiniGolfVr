using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MiniGolfVR
{
    // Permite tocar los botones desde la ventana Game con el mouse.
    public class ClickConMouse : MonoBehaviour
    {
        private Camera camara;

        private void Start()
        {
            camara = GetComponent<Canvas>().worldCamera;
        }

        private void Update()
        {
            if (camara == null || Mouse.current == null ||
                !Mouse.current.leftButton.wasPressedThisFrame) return;

            var visor = InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRHMD>();
            if (visor != null && visor.isTracked.isPressed) return;

            Vector2 posicion = Mouse.current.position.ReadValue();
            foreach (Button boton in GetComponentsInChildren<Button>())
            {
                RectTransform rectangulo = boton.GetComponent<RectTransform>();
                if (boton.interactable &&
                    RectTransformUtility.RectangleContainsScreenPoint(rectangulo, posicion, camara))
                {
                    boton.onClick.Invoke();
                    break;
                }
            }
        }
    }
}
