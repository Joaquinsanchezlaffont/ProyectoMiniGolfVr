# Minigolf VR

Prototipo de un hoyo para Unity **6000.5.1f1**, la versión instalada en la PC del proyecto, realizado a partir del [tutorial de VR en Unity 6 de Valem](https://youtu.be/H8dWVlZKQu4). Joaquín Sánchez Laffont trabaja en Unity y Sebastián Ángel Warman en los modelos de Blender.

## Abrir el proyecto

1. Descargá o cloná este repositorio **ProyectoMiniGolfVr**, descomprimilo y abrí la **carpeta raíz** en Unity Hub. Debe contener `Assets`, `Packages` y `ProjectSettings`. Si arriba de Unity dice `MiniGolfVROriginal`, tenés abierto el proyecto anterior: cerralo y abrí esta carpeta desde Unity Hub.
2. Esperá a que Unity instale los paquetes y compile. Un script de Editor importa automáticamente **Starter Assets** de XR Interaction Toolkit 3.5.1, crea `Assets/Scenes/MinigolfVR.unity` y los prefabs en `Assets/Models`, y prepara los materiales `.mat` ya incluidos en `Assets/Materials`. Unity abre la escena si estabas en una escena nueva o en `SampleScene` sin modificar.
3. Si no se abre, elegí **Minigolf VR > Abrir escena inicial**. Si la escena todavía no existe, elegí **Minigolf VR > Crear escena inicial**. Si Starter Assets solicita importar una muestra, aceptá y esperá a que termine la compilación; luego repetí esa opción del menú.
4. Abrí **Edit > Project Settings > XR Plug-in Management** y activá **OpenXR** en la pestaña **Windows, Mac, Linux**. En OpenXR, habilitá el perfil del control de tu visor y ejecutá **Project Validation > Fix All**. Asegurate de que en tu computadora haya un runtime OpenXR activo y el visor conectado.
5. Abrí la escena `MinigolfVR` y presioná **Play**. Es un proyecto para jugar desde la PC con el visor conectado; no tiene compilación para Android.

Si abriste una descarga anterior y la consola dice `Cannot create a new scene additively with an untitled scene unsaved`, guardá la escena vacía con **File > Save As...** como `Assets/Scenes/Borrador.unity`. Después elegí **Minigolf VR > Crear escena inicial** y **Minigolf VR > Abrir escena inicial**. La versión actual ya permite crear el mapa directamente desde la escena `Untitled` sin modificar.

Si ya tenés un proyecto de Unity abierto en tu computadora, incorporá `Assets` y las dependencias de `Packages/manifest.json` a ese proyecto. La escena y los prefabs se generan al abrirlo en el Editor. Tras revisar la escena generada, subí también sus `.unity`, `.prefab` y `.meta` a GitHub para conservar las modificaciones que hagas en Unity.

### Si en las gafas se ve el mapa pero no podés jugar

1. En la PC, abrí **Meta Horizon Link > Settings > General** y comprobá que **OpenXR Runtime** indique Meta Horizon Link como activo. En Unity, activá **OpenXR** para **Windows, Mac, Linux** y agregá **Oculus Touch Controller Profile** en **Enabled Interaction Profiles**.
2. Abrí `Assets/Scenes/MinigolfVR.unity` y pulsá el triángulo **Play** en la barra superior de Unity: debe quedar azul. Si dentro de las gafas ves el escritorio y los menús de Unity como una pantalla flotante, estás mirando el escritorio de Link; el juego debe aparecer como escena inmersiva.
3. Mové la cabeza para comprobar que cambia la vista. Acercá una mano al mango del palo y apretá **grip** (botón lateral). Si la vista sigue fija o no aparecen los controles, revisá la consola de Unity y la conexión de Quest Link antes de intentar golpear la pelota.

Si abriste el proyecto desde un ZIP anterior, podés actualizar solo `Assets/Scripts/MiniGolfRig.cs` desde este repositorio y conservar la escena que Unity ya creó.

## Cómo jugar

- **En VR:** acercate al palo y agarralo con **grip**. Mové el control como un palo de minigolf para golpear la pelota. El XR Origin de Starter Assets tiene controles de mano, desplazamiento suave y teletransporte sobre el pasto de la pista. El puntaje se ve delante del visor. Antes del primer tiro, el **botón primario izquierdo** cambia entre 1 y 4 jugadores; el **botón secundario izquierdo** reinicia la partida. Al cambiar de turno, se puede pasar el mismo visor al siguiente jugador.
- **Sin visor, para probar desde Unity:** `A/D` apuntan; `W/S` ajustan fuerza; `Espacio` golpea; `1` a `4` eligen la cantidad de jugadores antes del primer tiro; `R` reinicia.

El tutorial muestra la instalación de OpenXR, XR Interaction Toolkit y Starter Assets, el XR Origin, el movimiento, los controles y el agarre con XR Direct Interactor / XR Grab Interactable. Esas partes se aplicaron al minigolf. Sus ejemplos de armas, puertas y cajones no forman parte de este juego. El tutorial usa XR Interaction Toolkit **3.2.1**; este proyecto usa **3.5.1**, junto con Input System **1.20.0** y OpenXR **1.17.1**, porque son versiones publicadas para Unity 6.5 y evitan errores de compilación al abrirlo con esa versión del Editor.

## Carpetas

| Carpeta | Contenido |
| --- | --- |
| `Assets/Scripts` | Golpes, física, puntaje, turnos, timer y controles de PC y VR. |
| `Assets/Models` | Un obstáculo `.obj` editable en Blender. Unity también crea ahí los prefabs de pelota, palo y bandera. |
| `Assets/Materials` | Archivos `.mat` para pasto verde, bordes azules, obstáculo celeste, pelota blanca, bandera roja e interior del hoyo. |
| `Assets/Scenes` | Un solo mapa: `MinigolfVR.unity`, creado al abrir el proyecto en Unity. |
| `Assets/Editor` | Generador de la escena, prefabs y materiales editables. |

## Reglas implementadas

Cada jugador hace sus tiros por turno; se suman los golpes. Cuando los demás ya terminaron y empieza el turno del último jugador, tiene **60 segundos**: si no emboca, conserva sus golpes y recibe **5 adicionales**. Tras el único hoyo gana el menor puntaje, o se muestra un empate. La pelota vuelve al punto de salida si cae fuera de la pista.

El modelo y los colores son provisionales. Falta reemplazarlos por el trabajo definitivo de Blender y ajustar la fuerza del golpe mediante pruebas con el visor y los controles físicos. Este entorno no tiene instalado el Editor de Unity ni un visor, así que la escena generada y los golpes VR necesitan esa prueba en tu computadora.
