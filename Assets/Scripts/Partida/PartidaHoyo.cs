using System;

namespace MiniGolfVR
{
    public class PartidaHoyo
    {
        public const int SegundosUltimoJugador = 60;
        public const int Penalidad = 5;

        private readonly int[] golpes;
        private readonly bool[] embocaron;
        private readonly double inicio;
        private double limite = -1;
        private double fin;
        private int pendientes;

        public int Jugadores => golpes.Length;
        public int Turno { get; private set; }
        public bool TiroEnCurso { get; private set; }
        public bool Terminada => pendientes == 0;
        public bool EnUltimoMinuto => limite >= 0 && !Terminada;
        public int Penalizado { get; private set; } = -1;

        public PartidaHoyo(int jugadores, double ahora)
        {
            if (jugadores < 1 || jugadores > 4)
                throw new ArgumentOutOfRangeException(nameof(jugadores));

            golpes = new int[jugadores];
            embocaron = new bool[jugadores];
            pendientes = jugadores;
            inicio = ahora;
        }

        public int GolpesDe(int jugador) => golpes[jugador];

        public int SegundosDeJuego(double ahora)
        {
            return (int)((Terminada ? fin : ahora) - inicio);
        }

        public int SegundosRestantes(double ahora)
        {
            if (!EnUltimoMinuto) return 0;
            return Math.Max(0, (int)Math.Ceiling(limite - ahora));
        }

        public bool AnotarGolpe(double ahora)
        {
            RevisarTiempo(ahora);
            if (Terminada || TiroEnCurso) return false;

            golpes[Turno]++;
            TiroEnCurso = true;
            return true;
        }

        public void CerrarTiro(bool entro, double ahora)
        {
            RevisarTiempo(ahora);
            if (Terminada || !TiroEnCurso) return;
            TiroEnCurso = false;
            if (entro)
            {
                embocaron[Turno] = true;
                pendientes--;
                if (Terminada)
                {
                    fin = ahora;
                    return;
                }
                if (pendientes == 1 && Jugadores > 1)
                    limite = ahora + SegundosUltimoJugador;
            }

            do
            {
                Turno = (Turno + 1) % Jugadores;
            }
            while (embocaron[Turno]);
        }

        public void RevisarTiempo(double ahora)
        {
            if (!EnUltimoMinuto || ahora < limite) return;
            Penalizado = Turno;
            golpes[Turno] += Penalidad;
            pendientes = 0;
            TiroEnCurso = false;
            fin = limite;
        }
    }
}
