using UnityEngine;

namespace MiniGolfVR
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class HoleCup : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame game;

        public void Configure(MiniGolfGame owner) => game = owner;

        private void OnTriggerEnter(Collider other)
        {
            GolfBall ball = other.GetComponent<GolfBall>();
            if (game != null && ball != null) game.BallEnteredCup(this, ball);
        }
    }
}
