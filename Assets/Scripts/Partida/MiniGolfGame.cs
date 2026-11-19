using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolfVR
{
    public sealed class MiniGolfGame : MonoBehaviour
    {
        [SerializeField] private Transform tee;
        [SerializeField] private HoleCup cup;
        [SerializeField] private GolfBall ball;
        [SerializeField] private GolfClub palo;
        [SerializeField, Min(0.01f)] private float impulsoMaximo = 0.65f;

        private Vector3[] posiciones;
        private float tiempoQuieta;
        private double habilitarGolpe;
        private float fuerzaDelTiro;
        private int ultimoJugador = -1;
        private double mostrarGolpeHasta;

        public PartidaHoyo Partida { get; private set; }
        public Vector3 InicioDelTiro => posiciones[Partida.Turno];
        public bool MuestraUltimoGolpe => Partida.TiroEnCurso ||
            (Partida.Turno == ultimoJugador && Ahora < mostrarGolpeHasta);
        public float Fuerza => MuestraUltimoGolpe ? fuerzaDelTiro : palo.Fuerza;
        private static double Ahora => Time.realtimeSinceStartupAsDouble;

        private void Start()
        {
            if (tee == null || cup == null || ball == null || palo == null)
            {
                Debug.LogError("Falta asignar una referencia de la pista.", this);
                enabled = false;
                return;
            }
            Partida = new PartidaHoyo(SesionMinigolf.Jugadores, Ahora);
            posiciones = new Vector3[Partida.Jugadores];
            for (int i = 0; i < posiciones.Length; i++) posiciones[i] = tee.position;

            // El mango sirve para agarrar el palo. La cabeza es la que golpea la pelota.
            Physics.IgnoreCollision(palo.GetComponent<CapsuleCollider>(), ball.GetComponent<SphereCollider>());
            PrepararTurno();
        }

        private void Update()
        {
            Partida.RevisarTiempo(Ahora);
            if (!Partida.Terminada) return;
            enabled = false;
            SesionMinigolf.Resultado = Partida;
            SceneManager.LoadScene("MenuPrincipal");
        }

        public bool TryStrike(GolfBall pelota, Vector3 direccion, float fuerza)
        {
            if (Partida == null || pelota != ball || Ahora < habilitarGolpe || fuerza <= 0) return false;
            direccion.y = 0;
            if (direccion.sqrMagnitude < 0.0001f || !Partida.AnotarGolpe(Ahora)) return false;

            fuerzaDelTiro = Mathf.Clamp01(fuerza);
            ultimoJugador = Partida.Turno;
            mostrarGolpeHasta = Ahora + 1;
            ball.Golpear(direccion.normalized * (fuerzaDelTiro * impulsoMaximo));
            tiempoQuieta = 0f;
            return true;
        }

        private void FixedUpdate()
        {
            if (Partida == null || !Partida.TiroEnCurso) return;
            if (ball.Body.position.y < tee.position.y - 0.6f)
            {
                // Si sale de la pista conserva el golpe y vuelve a la posición anterior.
                CerrarTiro(false);
                return;
            }
            if (ball.Body.linearVelocity.sqrMagnitude > 0.01f)
            {
                tiempoQuieta = 0f;
                return;
            }

            tiempoQuieta += Time.fixedDeltaTime;
            if (tiempoQuieta < 0.3f) return;
            posiciones[Partida.Turno] = ball.Body.position;
            CerrarTiro(false);
        }

        public void BallEnteredCup(HoleCup hoyo, GolfBall pelota)
        {
            if (hoyo == cup && pelota == ball && Partida != null && Partida.TiroEnCurso)
                CerrarTiro(true);
        }

        private void CerrarTiro(bool entro)
        {
            Partida.CerrarTiro(entro, Ahora);
            if (!Partida.Terminada) PrepararTurno();
        }

        private void PrepararTurno()
        {
            ball.ResetAt(InicioDelTiro);
            tiempoQuieta = 0f;
            habilitarGolpe = Ahora + 0.45;
        }
    }
}
