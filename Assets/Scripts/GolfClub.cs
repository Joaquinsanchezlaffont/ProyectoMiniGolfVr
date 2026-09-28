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

        private Vector3 previousHeadPosition;
        private Vector3 headVelocity;
        private bool wasHeld;
        private float nextHitAt;

        public void Configure(MiniGolfGame owner, Transform clubHead, Collider trigger)
        {
            game = owner;
            head = clubHead;
            strikingCollider = trigger;
            grab = GetComponent<XRGrabInteractable>();
            strikingCollider.enabled = false;
        }

        private void Awake()
        {
            if (grab == null) grab = GetComponent<XRGrabInteractable>();
            if (strikingCollider != null) strikingCollider.enabled = false;
        }

        private void FixedUpdate()
        {
            bool held = grab != null && grab.isSelected && head != null;
            if (strikingCollider != null) strikingCollider.enabled = held;

            if (held)
            {
                Vector3 position = head.position;
                headVelocity = wasHeld
                    ? (position - previousHeadPosition) / Time.fixedDeltaTime
                    : Vector3.zero;
                previousHeadPosition = position;
            }
            else headVelocity = Vector3.zero;
            wasHeld = held;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!wasHeld || Time.time < nextHitAt || game == null) return;
            GolfBall ball = other.GetComponent<GolfBall>();
            if (ball == null) return;

            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(headVelocity, Vector3.up);
            if (horizontalVelocity.magnitude < 0.35f) return;
            float power = Mathf.InverseLerp(0.35f, 2.5f, horizontalVelocity.magnitude);
            if (game.TryStrike(ball, horizontalVelocity, power)) nextHitAt = Time.time + 0.5f;
        }
    }
}
