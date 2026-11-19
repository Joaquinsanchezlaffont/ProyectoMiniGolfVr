using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MiniGolfVR
{
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public sealed class GolfClub : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Collider strikingCollider;
        [SerializeField] private MiniGolfGame game;
        [SerializeField] private XRGrabInteractable grab;
        [SerializeField, Min(0.1f)] private float velocidadMaxima = 2.5f;

        public float Fuerza { get; private set; }
        private Vector3 posicionAnterior;
        private Vector3 velocidad;
        private bool agarrado;
        private bool golpeDado;
        private float listoDesde;

        private void Awake()
        {
            if (head == null || strikingCollider == null || grab == null)
            {
                Debug.LogError("Revisá la cabeza, el collider y el agarre del palo.", this);
                enabled = false;
                return;
            }
            strikingCollider.enabled = false;
        }

        private void FixedUpdate()
        {
            bool enLaMano = grab.isSelected;
            if (enLaMano && !agarrado) listoDesde = Time.fixedTime + 0.18f;
            // Al agarrarlo, el palo primero se acomoda a la mano.
            strikingCollider.enabled = enLaMano && Time.fixedTime >= listoDesde;
            velocidad = strikingCollider.enabled && agarrado
                ? Vector3.ProjectOnPlane((head.position - posicionAnterior) / Time.fixedDeltaTime, Vector3.up)
                : Vector3.zero;
            float rapidez = velocidad.magnitude;
            Fuerza = rapidez < 0.35f ? 0f : Mathf.Clamp01(rapidez / velocidadMaxima);
            if (!enLaMano || Fuerza == 0f) golpeDado = false;
            posicionAnterior = head.position;
            agarrado = enLaMano;
        }

        private void OnTriggerEnter(Collider otro) => ProbarGolpe(otro);
        private void OnTriggerStay(Collider otro) => ProbarGolpe(otro);

        private void ProbarGolpe(Collider otro)
        {
            if (!agarrado || golpeDado || Fuerza <= 0f || game == null) return;
            if (otro.TryGetComponent(out GolfBall pelota))
                golpeDado = game.TryStrike(pelota, velocidad, Fuerza);
        }
    }
}
