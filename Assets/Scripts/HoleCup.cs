using UnityEngine;

namespace MiniGolfVR
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class HoleCup : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame game;

        public void Configure(MiniGolfGame owner) => game = owner;

        private void OnTriggerEnter(Collider other) => TryCapture(other);
        private void OnTriggerStay(Collider other) => TryCapture(other);

        private void TryCapture(Collider other)
        {
            GolfBall ball = other.GetComponent<GolfBall>();
            if (game == null || ball == null) return;
            Vector3 gap = ball.transform.position - transform.position;
            if (new Vector2(gap.x, gap.z).sqrMagnitude <= 0.13f * 0.13f)
                game.BallEnteredCup(this, ball);
        }
    }
}
