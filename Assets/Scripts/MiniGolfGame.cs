using UnityEngine;

namespace MiniGolfVR
{
    // A single practice hole. The ball respawns after scoring or falling off the plane.
    public sealed class MiniGolfGame : MonoBehaviour
    {
        [SerializeField] private Transform tee;
        [SerializeField] private HoleCup cup;
        [SerializeField] private GolfBall ball;
        private float lastStrikeAt = -100f;
        private float respawnAt = -1f;

        public GolfBall Ball => ball;

        public void Configure(Transform start, HoleCup target, GolfBall golfBall)
        {
            tee = start;
            cup = target;
            ball = golfBall;
        }

        private void Start() => ResetBall();

        private void Update()
        {
            if (ball == null || tee == null) return;
            if (respawnAt > 0f && Time.time >= respawnAt)
            {
                respawnAt = -1f;
                ResetBall();
            }
            else if (respawnAt < 0f && ball.transform.position.y < -1f)
                ResetBall();
        }

        public bool TryStrike(GolfBall target, Vector3 direction, float power)
        {
            if (target != ball || respawnAt > 0f || Time.time - lastStrikeAt < 0.35f ||
                ball.Body.linearVelocity.sqrMagnitude > 0.05f)
                return false;

            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude < 0.001f) return false;
            ball.Body.WakeUp();
            ball.Body.AddForce(direction.normalized * Mathf.Lerp(0.12f, 0.65f,
                Mathf.Clamp01(power)), ForceMode.Impulse);
            lastStrikeAt = Time.time;
            return true;
        }

        public void BallEnteredCup(HoleCup target, GolfBall golfBall)
        {
            if (target != cup || golfBall != ball || respawnAt > 0f) return;
            ball.Body.linearVelocity = Vector3.zero;
            ball.Body.angularVelocity = Vector3.zero;
            respawnAt = Time.time + 1f;
        }

        private void ResetBall()
        {
            if (ball != null && tee != null)
                ball.ResetAt(tee.position);
        }
    }
}
