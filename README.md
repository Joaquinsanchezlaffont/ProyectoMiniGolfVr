# Minigolf VR

Prototipo de un hoyo para Unity **6000.5.1f1**, la versión instalada en la PC del proyecto, realizado a partir del [tutorial de VR en Unity 6 de Valem](https://youtu.be/H8dWVlZKQu4). Joaquín Sánchez Laffont trabaja en Unity y Sebastián Ángel Warman en los modelos de Blender.

## Abrir el proyecto

1. Descargá o cloná este repositorio **ProyectoMiniGolfVr**, descomprimilo y abrí la **carpeta raíz** en Unity Hub. Debe contener `Assets`, `Packages` y `ProjectSettings`. Si arriba de Unity dice `MiniGolfVROriginal`, tenés abierto el proyecto anterior: cerralo y abrí esta carpeta desde Unity Hub.
2. Esperá a que Unity instale los paquetes y compile. Un script de Editor importa automáticamente **Starter Assets** de XR Interaction Toolkit 3.5.1, crea `Assets/Scenes/MinigolfVR.unity` y los prefabs en `Assets/Models`. Después cambia el circuito de prueba por la pista, pelota y palo de `protecto29sep.blend` y crea sus materiales en `Assets/Materials/Blender`. Unity abre la escena si estabas en una escena nueva o en `SampleScene` sin modificar.
3. Si no se abre, elegí **Minigolf VR > Abrir escena inicial**. Si la escena todavía no existe, elegí **Minigolf VR > Crear escena inicial**. Si Starter Assets solicita importar una muestra, aceptá y esperá a que termine la compilación; luego repetí esa opción del menú.
4. En Windows, el proyecto prepara automáticamente **OpenXR** para PC, la inicialización XR, el perfil **Oculus Touch** y **Direct3D11**. La PC de prueba sufrió un cierre del Editor al crear las imágenes del visor con Direct3D12. Si Unity avisa que cambió la API gráfica, **cerrá y volvé a abrir el mismo proyecto** antes de tocar Play. En **Meta Horizon Link > Settings > General**, comprobá que **OpenXR Runtime** tenga Meta Horizon Link activo. Conectá las gafas por Quest Link.
5. En Unity elegí **Minigolf VR > Jugar con Quest Link**. Esta opción abre `MinigolfVR.unity` y activa el Play del Editor. También podés abrir la escena y pulsar el triángulo de la barra superior. Es un proyecto para jugar desde la PC con el visor conectado; no tiene compilación para Android.

### Primer nivel de Blender

El primer y único nivel usa la pista diseñada por Sebastián: salida, fairway, green, bunkers, estanque, bandera y hoyo. La pelota y el palo son los modelos del mismo archivo de Blender. Sus modelos `.obj` exportados están en `Assets/Models/Blender`; Unity puede cargarlos aunque no abras Blender. El original está en `Assets/Models/protecto29sep.blend`.

Si ya tenías `MinigolfVR.unity` creada, abrila sin cambios pendientes de guardar: el Editor reemplaza el hoyo de prueba una sola vez y conserva los turnos y el timer. Si seguís viendo el circuito rectangular, guardá la escena y elegí **Minigolf VR > Usar primer nivel de Blender**. El nuevo nivel aparece en la jerarquía como `HOYO 1 - Blender`.

Si abriste una descarga anterior y la consola dice `Cannot create a new scene additively with an untitled scene unsaved`, guardá la escena vacía con **File > Save As...** como `Assets/Scenes/Borrador.unity`. Después elegí **Minigolf VR > Crear escena inicial** y **Minigolf VR > Abrir escena inicial**. La versión actual ya permite crear el mapa directamente desde la escena `Untitled` sin modificar.

Si ya tenés un proyecto de Unity abierto en tu computadora, incorporá `Assets` y las dependencias de `Packages/manifest.json` a ese proyecto. La escena y los prefabs se generan al abrirlo en el Editor. Tras revisar la escena generada, subí también sus `.unity`, `.prefab` y `.meta` a GitHub para conservar las modificaciones que hagas en Unity.

### Si en las gafas se ve el mapa pero no podés jugar

1. En la PC, abrí **Meta Horizon Link > Settings > General** y comprobá que **OpenXR Runtime** indique Meta Horizon Link como activo. Conectá Quest Link y elegí **Minigolf VR > Jugar con Quest Link**. Si el proyecto venía de una descarga anterior, reemplazá `Assets/Editor/MiniGolfVrPcSetup.cs`, `Assets/Scripts/MiniGolfRig.cs` y `Packages/manifest.json` por los de este repositorio o descargá el ZIP actualizado.
2. El triángulo **Play** de la barra superior de Unity debe transformarse en un cuadrado azul. El texto `Play` del panel Game no inicia el juego. Si el triángulo vuelve a quedar gris, abrí **Window > General > Console** y revisá el primer error rojo. Si dentro de las gafas ves el escritorio y los menús de Unity como una pantalla flotante, estás mirando el escritorio de Link; el juego debe aparecer como escena inmersiva.
3. Mové la cabeza para comprobar que cambia la vista. Acercá una mano al mango del palo y apretá **grip** (botón lateral). Si la vista sigue fija o no aparecen los controles, revisá la consola de Unity y la conexión de Quest Link antes de intentar golpear la pelota.

La escena creada dentro de Unity se puede conservar al actualizar los scripts. No borres `Assets/Scenes/MinigolfVR.unity` de tu computadora.

### Si Unity se cierra al entrar en VR

En la PC de prueba, `Logs/Editor.log` registró `d3d12: Unrecoverable GPU device error` mientras OpenXR creaba las texturas de los ojos. El script de preparación configura Direct3D11 para Windows y el menú **Jugar con Quest Link** impide iniciar desde un Editor que siga ejecutándose con Direct3D12. Cerrá y abrí Unity una vez para aplicar el cambio. Si estás usando una copia anterior del proyecto, en **Edit > Project Settings > Player > Other Settings > Rendering** desmarcá **Auto Graphics API for Windows/Mac/Linux**, dejá **Direct3D11** como única API de Windows y reiniciá Unity. No hace falta volver a generar el mapa.

## Cómo jugar

- **En VR:** acercate al palo y agarralo con **grip**. Mové el control como un palo de minigolf para golpear la pelota. El XR Origin de Starter Assets tiene controles de mano, desplazamiento suave y teletransporte sobre el pasto de la pista. El puntaje se ve delante del visor. Antes del primer tiro, el **botón primario izquierdo** cambia entre 1 y 4 jugadores; el **botón secundario izquierdo** reinicia la partida. Al cambiar de turno, se puede pasar el mismo visor al siguiente jugador.
- **Sin visor, para probar desde Unity:** `A/D` apuntan; `W/S` ajustan fuerza; `Espacio` golpea; `1` a `4` eligen la cantidad de jugadores antes del primer tiro; `R` reinicia.

El tutorial muestra la instalación de OpenXR, XR Interaction Toolkit y Starter Assets, el XR Origin, el movimiento, los controles y el agarre con XR Direct Interactor / XR Grab Interactable. Esas partes se aplicaron al minigolf. Sus ejemplos de armas, puertas y cajones no forman parte de este juego. El tutorial usa XR Interaction Toolkit **3.2.1**; este proyecto usa **3.5.1**, junto con Input System **1.20.0** y OpenXR **1.17.1**, porque son versiones publicadas para Unity 6.5 y evitan errores de compilación al abrirlo con esa versión del Editor.

## Carpetas

| Carpeta | Contenido |
| --- | --- |
| `Assets/Scripts` | Golpes, física, puntaje, turnos, timer y controles de PC y VR. |
| `Assets/Models` | Archivo original `.blend` y, en `Blender/`, pista, pelota, palo y paleta para Unity; los prefabs se crean al abrir el proyecto. |
| `Assets/Materials` | Materiales `.mat` del juego; al abrirlo se crean los colores del modelo en `Blender/`. |
| `Assets/Scenes` | Un solo mapa: `MinigolfVR.unity`, creado al abrir el proyecto en Unity. |
| `Assets/Editor` | Generador de la escena, prefabs y materiales editables. |

## Reglas implementadas

Cada jugador hace sus tiros por turno; se suman los golpes. Cuando los demás ya terminaron y empieza el turno del último jugador, tiene **60 segundos**: si no emboca, conserva sus golpes y recibe **5 adicionales**. Tras el único hoyo gana el menor puntaje, o se muestra un empate. La pelota vuelve al punto de salida si cae fuera de la pista.

La pista, la pelota y el palo salen de `protecto29sep.blend`. El montaje de la escena se hace al abrir Unity. Todavía falta ajustar la fuerza del golpe y comprobar el movimiento y el agarre con el visor y los controles reales en tu computadora.
