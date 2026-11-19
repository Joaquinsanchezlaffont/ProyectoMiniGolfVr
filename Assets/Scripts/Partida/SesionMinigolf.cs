using UnityEngine;

namespace MiniGolfVR
{
    // Estos dos datos pasan del menú a la pista y vuelven a la pantalla de resultados.
    public static class SesionMinigolf
    {
        public static int Jugadores { get; private set; } = 1;
        public static PartidaHoyo Resultado { get; set; }

        public static void Preparar(int cantidad)
        {
            Jugadores = cantidad;
            Resultado = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reiniciar() => Preparar(1);
    }
}
