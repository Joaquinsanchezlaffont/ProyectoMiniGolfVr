using System;
using MiniGolfVR;

class ReglasDelHoyo
{
    static void Comprobar(bool cumple, string motivo)
    {
        if (!cumple) throw new Exception(motivo);
    }

    static void Tiro(PartidaHoyo hoyo, bool emboca, double segundo)
    {
        Comprobar(hoyo.AnotarGolpe(segundo), "Debería aceptar el golpe");
        hoyo.CerrarTiro(emboca, segundo);
    }

    static void Main()
    {
        Comprobar(FuerzaDelGolpe.Medir(0.34f, 2.5f) == 0f, "Un temblor de la mano no golpea");
        Comprobar(FuerzaDelGolpe.Medir(1.25f, 2.5f) == 0.5f, "La mitad de velocidad da 50%");
        Comprobar(FuerzaDelGolpe.Medir(2.5f, 2.5f) == 1f && FuerzaDelGolpe.Medir(5f, 2.5f) == 1f,
            "Un golpe fuerte no supera el 100%");
        var solo = new PartidaHoyo(1, 0);
        solo.CerrarTiro(true, 0);
        Comprobar(!solo.Terminada, "Un contacto sin golpe no cuenta");
        Comprobar(solo.AnotarGolpe(1) && !solo.AnotarGolpe(1.1), "Un tiro, un golpe");
        solo.CerrarTiro(false, 2);
        solo.RevisarTiempo(120);
        Comprobar(!solo.Terminada && !solo.EnUltimoMinuto, "En solo no hay límite");
        Tiro(solo, true, 125);
        Comprobar(solo.Terminada && solo.GolpesDe(0) == 2 && solo.SegundosDeJuego(160) == 125,
            "El resultado de solo debe quedar fijo");

        for (int cantidad = 2; cantidad <= 4; cantidad++)
        {
            var hoyo = new PartidaHoyo(cantidad, 0);
            Tiro(hoyo, false, 1);
            for (int i = 1; i < cantidad; i++)
            {
                Comprobar(hoyo.Turno == i && !hoyo.EnUltimoMinuto, "Orden de los turnos");
                Tiro(hoyo, true, 10 + i);
            }
            double inicioMinuto = 10 + cantidad - 1;
            Comprobar(hoyo.Turno == 0 && hoyo.SegundosRestantes(inicioMinuto) == 60, "Inicio del minuto");
            Tiro(hoyo, false, inicioMinuto + 10);
            Comprobar(hoyo.SegundosRestantes(inicioMinuto + 59.9) == 1, "El minuto no vuelve a empezar");
            hoyo.AnotarGolpe(inicioMinuto + 59.9);
            hoyo.CerrarTiro(true, inicioMinuto + 60);
            hoyo.RevisarTiempo(inicioMinuto + 100);
            Comprobar(hoyo.Terminada && hoyo.Penalizado == 0 && hoyo.GolpesDe(0) == 8,
                "Al vencer: tres golpes hechos más cinco, una sola vez");
            int[] orden = hoyo.OrdenPorGolpes();
            Comprobar(orden[0] == 1 && orden[cantidad - 1] == 0, "El resultado incluye la penalidad");
            for (int j = 0; j < cantidad - 1; j++)
                Comprobar(orden[j] == j + 1, "Los empatados conservan el orden de jugador");
        }

        var empate = new PartidaHoyo(2, 0);
        Tiro(empate, true, 1);
        Tiro(empate, true, 60.99);
        empate.RevisarTiempo(80);
        Comprobar(empate.Terminada && empate.Penalizado == -1 && empate.GolpesDe(0) == empate.GolpesDe(1),
            "Si entra antes del límite, el empate se mantiene");

        var tres = new PartidaHoyo(3, 0);
        Tiro(tres, false, 0);
        Tiro(tres, true, 1);
        Tiro(tres, false, 2);
        Tiro(tres, false, 3);
        Comprobar(tres.Turno == 2 && !tres.EnUltimoMinuto, "Se salta al que ya embocó");

        var sinTirar = new PartidaHoyo(2, 10);
        Tiro(sinTirar, true, 11);
        sinTirar.RevisarTiempo(71);
        Comprobar(sinTirar.GolpesDe(1) == 5 && !sinTirar.AnotarGolpe(72),
            "El reloj también vence si el último jugador no tira");
        Console.WriteLine("OK: fuerza, solo, turnos de 2 a 4, reloj, penalidad y empate.");
    }
}
