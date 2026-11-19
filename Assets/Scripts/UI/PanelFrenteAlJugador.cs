using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace MiniGolfVR
{
    [RequireComponent(typeof(Canvas))]
    public class PanelFrenteAlJugador : MonoBehaviour
    {
        private void LateUpdate()
        {
            var visor = InputSystem.GetDevice<XRHMD>();
            if (visor == null || !visor.isTracked.isPressed) return;

            Transform cabeza = GetComponent<Canvas>().worldCamera.transform;
            Vector3 frente = Vector3.ProjectOnPlane(cabeza.forward, Vector3.up).normalized;
            if (frente.sqrMagnitude < 0.1f) return;

            // El menú queda al alcance del índice y no se escapa al acercar la cara.
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(cabeza.position + frente * 0.55f - Vector3.up * 0.12f,
                Quaternion.LookRotation(frente, Vector3.up));
            transform.localScale = Vector3.one * 0.00042f;
            enabled = false;
        }
    }
}
