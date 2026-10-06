# Minigolf VR

Un solo hoyo de minigolf para Unity **6000.5.1f1** y Meta Quest por **Quest Link**. La pista es un rectángulo gris, plano y con cuatro bordes. Cerca de la salida hay una pelota blanca y un palo que se puede agarrar con los controles VR. El hoyo oscuro está al otro extremo. No hay obstáculos ni mapas adicionales.

El proyecto parte del [tutorial de VR en Unity 6 de Valem](https://youtu.be/H8dWVlZKQu4): OpenXR, XR Interaction Toolkit, Starter Assets, XR Origin y agarre con los controles. Joaquín Sánchez Laffont trabaja en Unity y Sebastián Ángel Warman en diseño 3D.

## Abrir y jugar

1. Descargá o actualizá este repositorio y abrí **ProyectoMiniGolfVr** desde Unity Hub. Elegí Unity **6000.5.1f1** y esperá a que termine la importación. La carpeta del proyecto debe contener `Assets`, `Packages` y `ProjectSettings`.
2. Elegí **Minigolf VR > Abrir escena inicial**. Si todavía no existe `Assets/Scenes/MinigolfVR.unity`, elegí **Minigolf VR > Crear escena inicial**. Al primer inicio Unity importa los Starter Assets de XR Interaction Toolkit y genera la escena y los prefabs. Esperá a que termine si se vuelve a compilar.
3. Si la escena local todavía tiene el mapa anterior, elegí **Minigolf VR > Dejar pista gris**. Esto reemplaza la pista, pelota y palo anteriores, conservando el XR Origin, el puntaje, los turnos y el timer. El menú **Jugar con Quest Link** también hace el cambio antes de Play.
4. En Windows, conectá las Quest mediante Quest Link y activá **Meta Horizon Link** como OpenXR Runtime de la PC. Elegí **Minigolf VR > Jugar con Quest Link**. El proyecto prepara OpenXR, Oculus Touch y Direct3D11. Si Unity pide reiniciar por el cambio de Direct3D11, cerralo y abrilo de nuevo antes de jugar.
5. Acercá el control al mango del palo, mantené **grip** (botón lateral) y mové la mano para golpear la pelota. Podés teletransportarte sobre el piso gris. El puntaje aparece frente al visor.

En VR, antes del primer golpe, el botón primario izquierdo cambia la cantidad de jugadores (1 a 4) y el secundario izquierdo reinicia la partida. Cada jugador hace sus golpes por turno y se pasa el visor al siguiente jugador. El último jugador dispone de **60 segundos** después de que terminaron los demás; si no emboca, recibe **5 golpes adicionales**. Al terminar el hoyo gana quien tenga menos golpes.

También podés probar sin gafas: `A/D` para apuntar, `W/S` para regular fuerza, `Espacio` para golpear, `1` a `4` para elegir jugadores antes del primer tiro y `R` para reiniciar.

## Carpetas

| Carpeta | Contenido |
| --- | --- |
| `Assets/Scripts` | Pelota, palo, turnos, puntaje, timer y controles. |
| `Assets/Models` | Prefabs de pelota, palo y hoyo, generados dentro de Unity. |
| `Assets/Materials` | Materiales del piso gris, bordes, pelota y hoyo. |
| `Assets/Scenes` | `MinigolfVR.unity`, un solo nivel creado en Unity. |
| `Assets/Editor` | Generador de escena y preparación de Quest Link. |

Si el visor muestra únicamente el escritorio flotante, revisá que el triángulo Play de Unity esté activo y que Meta Horizon Link sea el OpenXR Runtime. Si Unity se cierra al entrar en VR, el menú **Jugar con Quest Link** evita iniciar sobre Direct3D12; después de configurar Direct3D11 hay que reiniciar el Editor. El movimiento con gafas y control real debe comprobarse en tu computadora; este repositorio no incluye una compilación Android.
