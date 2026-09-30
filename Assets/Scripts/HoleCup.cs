using UnityEngine;

namespace MiniGolfVR
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class HoleCup : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame game;
        [SerializeField] private float captureRadius;

        public void Configure(MiniGolfGame owner) => game = owner;

        public void SetCaptureRadius(float radius) => captureRadius = Mathf.Max(0f, radius);

        private void OnTriggerEnter(Collider other) => TryCapture(other);
        private void OnTriggerStay(Collider other) => TryCapture(other);

        private void TryCapture(Collider other)
        {
            GolfBall ball = other.GetComponent<GolfBall>();
            if (game == null || ball == null) return;
            Vector3 gap = ball.transform.position - transform.position;
            if (captureRadius > 0f && new Vector2(gap.x, gap.z).sqrMagnitude >
                captureRadius * captureRadius) return;
            game.BallEnteredCup(this, ball);
        }
    }
}
