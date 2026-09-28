using UnityEngine;

namespace MiniGolfVR
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class GolfBall : MonoBehaviour
    {
        private Rigidbody body;

        public Rigidbody Body
        {
            get
            {
                if (body == null) body = GetComponent<Rigidbody>();
                return body;
            }
        }

        public void ResetAt(Vector3 position)
        {
            Body.position = position;
            Body.rotation = Quaternion.identity;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.Sleep();
            transform.SetPositionAndRotation(position, Quaternion.identity);
        }
    }
}
