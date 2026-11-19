using System;

namespace MiniGolfVR
{
    public static class FuerzaDelGolpe
    {
        public const float VelocidadMinima = 0.35f;

        // 0 es reposo. 1 equivale al impulso máximo de la pelota.
        public static float Medir(float rapidez, float maxima)
        {
            if (rapidez < VelocidadMinima || maxima <= 0f) return 0f;
            return Math.Min(1f, rapidez / maxima);
        }
    }
}
