# Minigolf VR: inicio y prueba de un hoyo

Proyecto para **Unity 6000.5.1f1**, PC con **Meta Horizon Link / Quest Link** y gafas Quest. Al comenzar aparece la imagen `Inicio.png` con un botón **JUGAR** debajo. La pantalla de inicio se crea con el mismo XR Origin y las manos azules del juego: se puede apuntar al botón con los controles y pulsarlo. El destino `MenuPrincipal` se agregará cuando esté diseñado; hasta entonces, pulsar el botón muestra un aviso en la consola y permanece en Inicio.

La escena de prueba sigue teniendo una sola pista rectangular gris con cuatro bordes, pelota, palo agarrable y un hoyo. Las dos manos azules animadas son los prefabs **Left Hand Model** y **Right Hand Model** del [paquete original de Valem](https://drive.google.com/file/d/1cZn67HwxYs0z_pur8vamxQaK_BRtJp7h/view). Al embocar, la pelota vuelve a la salida después de un segundo.

El proyecto usa los pasos de VR que aparecen en la transcripción del [tutorial de Valem](https://youtu.be/H8dWVlZKQu4): OpenXR para PC, XR Interaction Toolkit, Starter Assets, XR Origin en modo Floor, controladores rastreados, prefabs de manos azules animadas con `Grip`/`Trigger`, locomoción y un XR Grab Interactable en el palo. El script `AnimateHandOnInput` lee las acciones **XRI Left/Right Interaction / Select Value** (grip) y **Activate Value** (gatillo), tal como muestra el video. La muestra Starter Assets trae el rig con interactores y controles configurados. No se agregaron los ejemplos de puertas, armas, cajones ni menús.

## Paso a paso en tu PC

1. Descomprimí el ZIP con **Extraer todo**. Dentro de la carpeta extraída tienen que aparecer **directamente** `Assets`, `Packages` y `ProjectSettings`. En Unity Hub elegí **Add > Add project from disk** y seleccioná esa carpeta, no el ZIP ni la carpeta `Assets`. Usá Unity **6000.5.1f1**. Si Unity te pide activar el nuevo Input System, aceptá y dejá que reinicie. Esperá a que termine de importar los paquetes.
2. El proyecto importa automáticamente la muestra **Starter Assets** de XR Interaction Toolkit 3.5.1. Si no aparece, abrí **Window > Package Manager > XR Interaction Toolkit > Samples > Starter Assets > Import** y esperá a que termine la compilación.
3. Esperá a que se creen automáticamente `Assets/Scenes/MinigolfVR.unity` y `Assets/Scenes/Inicio.unity`. Elegí **Minigolf VR > Abrir pantalla de inicio** para ver la imagen y el botón. Si todavía no se crearon, elegí **Minigolf VR > Crear escena inicial** para generar primero la pista, y después **Minigolf VR > Abrir pantalla de inicio**. Para abrir la pista directamente usá **Minigolf VR > Abrir escena inicial**. Si ya tenías una escena del proyecto anterior, elegí **Minigolf VR > Dejar solo pista y manos** para reemplazar los cubos anteriores por las manos azules animadas.
4. En la PC abrí **Meta Horizon Link**, establecelo como **OpenXR Runtime** y conectá el visor mediante Quest Link. En Unity elegí **Minigolf VR > Jugar con Quest Link**. El menú configura el cargador OpenXR de Windows, el perfil Oculus Touch y Direct3D11. Si aparece el aviso de Direct3D11, cerrá y volvé a abrir Unity antes de jugar.
5. Dentro del visor se ve la pantalla Inicio y las manos azules cuando los controles están rastreados. Apuntá al botón **JUGAR** y pulsá el gatillo. La pantalla de menú principal se conectará en el siguiente paso. Para probar el hoyo ahora, detené Play, abrí **Minigolf VR > Abrir escena inicial** y pulsá Play. En la pista, **grip** cierra la mano y permite agarrar el palo; **gatillo** mueve el índice. Acercá una mano al mango del palo y mantené apretado grip para agarrarlo.

Si el jugador aparece flotando, antes de Play elegí **Minigolf VR > Corregir altura VR**. El XR Origin usa seguimiento de piso y el script ya no mueve la cámara a una posición aérea. Comprobá también que el suelo configurado en Quest Link coincida con el suelo real.

Si en las gafas ves una pantalla flotante con el escritorio de Windows, volvé al menú de Quest Link y comprobá que Unity realmente esté en modo Play (triángulo azul/cuadrado). Si Unity se cierra al iniciar VR, asegurate de haber reiniciado después de configurar Direct3D11.

## Estructura

| Carpeta | Contenido |
| --- | --- |
| `Assets/Scenes` | Inicio y escena de prueba, creadas al abrir Unity. Inicio es la primera escena del build. |
| `Assets/Sprites` | Imagen original de la pantalla de inicio. |
| `Assets/Models` | Prefabs de pelota, palo y hoyo creados al abrir; modelos, animaciones y prefabs originales de manos de Valem ya incluidos. |
| `Assets/Materials` | Colores del piso, bordes, palo, pelota, hoyo y material azul original de Valem. |
| `Assets/Scripts` | Física de la pelota, golpe, detección del hoyo, animación de manos y acción del botón JUGAR. |
| `Assets/Editor` | Generación de ambas escenas y configuración de Quest Link. |

Este paquete **no se compila como aplicación Android**; se ejecuta desde Unity en Windows con el visor conectado por Link. Las manos se animan al pulsar los controles; esto no es seguimiento real de cada dedo sin controles. Todavía hay que comprobar la interacción y la comodidad dentro de tus gafas. El hoyo es un disco oscuro con detector de embocada, no una perforación física del piso.
