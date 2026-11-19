using UnityEngine;
using UnityEngine.UI;

namespace MiniGolfVR
{
    public class ResultadosHoyo : MonoBehaviour
    {
        [System.Serializable]
        private class Fila
        {
            public RectTransform panel;
            public Text jugador, puesto, golpes;
            public RectTransform trofeo;
        }

        [SerializeField] private Fila[] filas;
        [SerializeField] private Text detalle;

        public void Mostrar(PartidaHoyo partida)
        {
            int cantidad = partida.Jugadores;
            int[] orden = partida.OrdenPorGolpes();

            int minimo = partida.GolpesDe(orden[0]);
            bool empate = cantidad > 1 && partida.GolpesDe(orden[1]) == minimo;
            int puesto = 1;
            for (int i = 0; i < filas.Length; i++)
            {
                Fila fila = filas[i];
                fila.panel.gameObject.SetActive(i < cantidad);
                if (i >= cantidad) continue;
                int jugador = orden[i];
                int golpes = partida.GolpesDe(jugador);
                bool primero = golpes == minimo;
                if (i > 0 && golpes != partida.GolpesDe(orden[i - 1])) puesto = i + 1;
                float espacio = cantidad <= 2 ? 168f : 82f;
                fila.panel.anchoredPosition = new Vector2(0, (0.5f * (cantidad - 1) - i) * espacio);
                fila.jugador.text = $"JUGADOR {jugador + 1}";
                fila.golpes.text = golpes == 1 ? "1 GOLPE" : $"{golpes} GOLPES";
                fila.puesto.text = cantidad == 1 ? "EMBOCÓ" : primero ? (empate ? "EMPATE" : "GANADOR") : $"{puesto}.º PUESTO";
                fila.puesto.color = primero ? new Color32(48, 112, 20, 255) : new Color32(63, 66, 62, 255);
                fila.trofeo.gameObject.SetActive(primero);
                fila.trofeo.sizeDelta = Vector2.one * (cantidad > 2 ? 78f : 154f);
            }
            int t = partida.SegundosDeJuego(Time.realtimeSinceStartupAsDouble);
            detalle.text = $"Tiempo de partida: {t / 60}:{t % 60:00}";
            if (partida.Penalizado >= 0)
                detalle.text += $"   ·   Jugador {partida.Penalizado + 1}: +{PartidaHoyo.Penalidad} golpes por tiempo";
        }
    }
}
