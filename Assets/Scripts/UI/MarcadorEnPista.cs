using UnityEngine;
using UnityEngine.UI;

namespace MiniGolfVR
{
    public class MarcadorEnPista : MonoBehaviour
    {
        [SerializeField] private MiniGolfGame juego;
        [SerializeField] private Transform visor;
        [SerializeField] private Text turno, golpes, tituloReloj, reloj, tituloFuerza, porcentaje;
        [SerializeField] private RectTransform relleno;

        private int jugadorAnterior = -1, golpesAnteriores = -1, segundoAnterior = -1, fuerzaAnterior = -1;
        private bool cuentaAtras, ultimoGolpe;
        private static readonly Color ColorDelMinuto = new Color32(255, 205, 106, 255);
        private static readonly Color ColorDeAviso = new Color32(255, 162, 143, 255);

        private void LateUpdate()
        {
            PartidaHoyo partida = juego.Partida;
            if (partida == null || partida.Terminada) return;

            // Se queda junto al lugar del tiro mientras rueda la pelota.
            transform.position = juego.InicioDelTiro + new Vector3(-0.64f, 0.47f, 0.35f);
            Vector3 frente = transform.position - visor.position;
            if (frente.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(frente);

            if (partida.Turno != jugadorAnterior)
            {
                jugadorAnterior = partida.Turno;
                turno.text = partida.Jugadores == 1 ? "Individual" :
                    $"Turno: jugador {partida.Turno + 1} de {partida.Jugadores}";
            }
            int cantidadGolpes = partida.GolpesDe(partida.Turno);
            if (cantidadGolpes != golpesAnteriores)
            {
                golpesAnteriores = cantidadGolpes;
                golpes.text = $"Golpes: {cantidadGolpes}";
            }

            double ahora = Time.realtimeSinceStartupAsDouble;
            int segundos = partida.EnUltimoMinuto ? partida.SegundosRestantes(ahora) : partida.SegundosDeJuego(ahora);
            if (segundos != segundoAnterior || cuentaAtras != partida.EnUltimoMinuto)
            {
                segundoAnterior = segundos;
                cuentaAtras = partida.EnUltimoMinuto;
                bool quedanDiez = cuentaAtras && segundos <= 10;
                tituloReloj.text = quedanDiez ? "ÚLTIMOS 10 S" : cuentaAtras ? "ÚLTIMO MINUTO" : "TIEMPO";
                reloj.text = $"{segundos / 60}:{segundos % 60:00}";
                reloj.color = quedanDiez ? ColorDeAviso : cuentaAtras ? ColorDelMinuto : Color.white;
            }

            int fuerza = Mathf.RoundToInt(juego.Fuerza * 100f);
            if (fuerza != fuerzaAnterior)
            {
                fuerzaAnterior = fuerza;
                porcentaje.text = $"{fuerza}%";
                relleno.anchorMax = new Vector2(fuerza / 100f, 1f);
            }
            if (ultimoGolpe != juego.MuestraUltimoGolpe)
            {
                ultimoGolpe = juego.MuestraUltimoGolpe;
                tituloFuerza.text = ultimoGolpe ? "Último golpe" : "Fuerza del palo";
            }
        }
    }
}
