using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace MiniGolfVR
{
    public sealed class MiniGolfGame : MonoBehaviour
    {
        [Serializable]
        public sealed class HoleLayout
        {
            public string name;
            public GameObject root;
            public Transform tee;
            public HoleCup cup;
        }

        [SerializeField] private HoleLayout[] holes;
        [SerializeField] private GolfBall ball;
        [SerializeField] private MiniGolfRig rig;
        [SerializeField] private TextMesh vrScoreboard;
        [SerializeField, Range(1, 4)] private int playerCount = 2;
        [SerializeField] private float lastPlayerSeconds = 60f;

        private int[] totalStrokes;
        private int[,] strokesByHole;
        private int currentHole;
        private int currentPlayer;
        private float timeRemaining;
        private float lastStrokeAt = -100f;
        private bool endingTurn;
        private bool matchEnded;
        private string finalResult;

        public MiniGolfRig Rig => rig;
        public GolfBall Ball => ball;
        public bool MatchEnded => matchEnded;
        public int PlayerCount => playerCount;
        public bool CanChoosePlayers => strokesByHole != null &&
            currentHole == 0 && currentPlayer == 0 && strokesByHole[0, 0] == 0;

        public void Configure(HoleLayout[] layouts, GolfBall golfBall, MiniGolfRig playerRig, TextMesh scoreboard)
        {
            holes = layouts;
            ball = golfBall;
            rig = playerRig;
            vrScoreboard = scoreboard;
        }

        private void Start() => StartMatch();

        public void SetPlayerCount(int count)
        {
            if (!CanChoosePlayers || count < 1 || count > 4) return;
            playerCount = count;
            StartMatch();
        }

        public void Restart() => StartMatch();

        private void StartMatch()
        {
            if (holes == null || holes.Length == 0 || ball == null || rig == null)
            {
                Debug.LogError("Minigolf VR: faltan referencias. Usa Minigolf VR > Crear escena inicial.");
                return;
            }

            StopAllCoroutines();
            playerCount = Mathf.Clamp(playerCount, 1, 4);
            totalStrokes = new int[playerCount];
            strokesByHole = new int[holes.Length, playerCount];
            endingTurn = matchEnded = false;
            finalResult = string.Empty;
            currentHole = currentPlayer = 0;
            lastStrokeAt = -100f;
            ball.gameObject.SetActive(true);
            BeginHole();
        }

        private void BeginHole()
        {
            for (int i = 0; i < holes.Length; i++)
                holes[i].root.SetActive(i == currentHole);
            currentPlayer = 0;
            BeginTurn();
        }

        private void BeginTurn()
        {
            // With sequential turns, the final player starts after all other players
            // have putted. Their 60-second timer starts right here.
            timeRemaining = playerCount > 1 && currentPlayer == playerCount - 1
                ? lastPlayerSeconds : -1f;
            Transform tee = holes[currentHole].tee;
            ball.ResetAt(tee.position);
            rig.SetStation(tee.position);
            UpdateScoreboard();
        }

        private void Update()
        {
            if (totalStrokes == null || matchEnded) return;
            if (!endingTurn && timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    totalStrokes[currentPlayer] += 5;
                    strokesByHole[currentHole, currentPlayer] += 5;
                    StartCoroutine(AdvanceAfterDelay());
                }
            }

            if (!endingTurn && ball.transform.position.y < -1.2f)
                ball.ResetAt(holes[currentHole].tee.position);
            UpdateScoreboard();
        }

        public bool TryStrike(GolfBall target, Vector3 direction, float power)
        {
            if (target != ball || totalStrokes == null || matchEnded || endingTurn ||
                Time.time - lastStrokeAt < 0.45f || ball.Body.linearVelocity.sqrMagnitude > 0.025f)
                return false;

            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude < 0.001f) return false;
            direction.Normalize();

            ball.Body.WakeUp();
            ball.Body.AddForce(direction * Mathf.Lerp(0.12f, 0.65f, Mathf.Clamp01(power)), ForceMode.Impulse);
            lastStrokeAt = Time.time;
            strokesByHole[currentHole, currentPlayer]++;
            totalStrokes[currentPlayer]++;
            UpdateScoreboard();
            return true;
        }

        public void BallEnteredCup(HoleCup cup, GolfBall target)
        {
            if (totalStrokes == null || endingTurn || matchEnded || target != ball ||
                cup != holes[currentHole].cup) return;
            StartCoroutine(AdvanceAfterDelay());
        }

        private IEnumerator AdvanceAfterDelay()
        {
            endingTurn = true;
            ball.Body.linearVelocity = Vector3.zero;
            ball.Body.angularVelocity = Vector3.zero;
            yield return new WaitForSeconds(0.8f);

            if (currentPlayer + 1 < playerCount)
            {
                currentPlayer++;
                endingTurn = false;
                BeginTurn();
                yield break;
            }

            if (currentHole + 1 < holes.Length)
            {
                currentHole++;
                endingTurn = false;
                BeginHole();
                yield break;
            }

            FinishMatch();
        }

        private void FinishMatch()
        {
            matchEnded = true;
            ball.gameObject.SetActive(false);
            int best = int.MaxValue;
            for (int i = 0; i < playerCount; i++) best = Mathf.Min(best, totalStrokes[i]);

            StringBuilder winners = new StringBuilder();
            for (int i = 0; i < playerCount; i++)
            {
                if (totalStrokes[i] != best) continue;
                if (winners.Length > 0) winners.Append(" y ");
                winners.Append("Jugador ").Append(i + 1);
            }
            finalResult = (winners.ToString().Contains(" y ") ? "Empate: " : "Ganó: ") + winners;
            UpdateScoreboard();
        }

        private string ScoreText()
        {
            if (totalStrokes == null) return "Cargando minigolf...";
            StringBuilder result = new StringBuilder();
            if (matchEnded) result.AppendLine(finalResult);
            else result.Append("Hoyo ").Append(currentHole + 1).Append('/').Append(holes.Length)
                .Append("   Turno: Jugador ").Append(currentPlayer + 1).AppendLine();

            for (int i = 0; i < playerCount; i++)
                result.Append("Jugador ").Append(i + 1).Append(": ")
                    .Append(totalStrokes[i]).Append(" golpes").AppendLine();

            if (!matchEnded && timeRemaining >= 0f)
                result.Append("Tiempo último jugador: ")
                    .Append(Mathf.CeilToInt(timeRemaining)).Append('s').AppendLine();
            return result.ToString();
        }

        private void UpdateScoreboard()
        {
            if (vrScoreboard == null) return;
            vrScoreboard.gameObject.SetActive(rig != null && rig.VrActive);
            if (rig != null && rig.VrActive)
                vrScoreboard.text = ScoreText() + "Grip: palo | Botón primario izq.: jugadores\n" +
                    "Botón secundario izq.: reiniciar";
        }

        private void OnGUI()
        {
            if (rig != null && rig.VrActive) return;
            GUI.Box(new Rect(12, 12, 355, 145 + playerCount * 22),
                "MINIGOLF VR\n" + ScoreText() +
                (matchEnded ? "R: jugar otra vez" :
                "A/D: apuntar   W/S: fuerza   Espacio: golpear\n" +
                (CanChoosePlayers ? "1 a 4: cantidad de jugadores   " : "") + "R: reiniciar"));
        }
    }
}
