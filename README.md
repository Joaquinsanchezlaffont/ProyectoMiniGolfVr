# MiniGolf VR

Proyecto de Joaquín Sánchez Laffont (Unity) y Sebastián Ángel Warman (Blender).

## Cómo abrirlo

1. Si tenés un ZIP viejo, cerrá Unity y descargá la versión nueva del repositorio. No copies solamente `Assets`: también hacen falta `Packages` y `ProjectSettings`.
2. En Unity Hub abrí la carpeta que contiene esas tres carpetas, con Unity **6000.5.1f1**.
3. Esperá a que Unity importe los paquetes, los Starter Assets y el simulador. La primera vez puede tardar.
4. Abrí `Assets/Scenes/Inicio.unity` y tocá **Play**.
5. Tocá **JUGAR**. Podés entrar solo o elegir **JUGAR CON AMIGOS**, marcar **2, 3 o 4 jugadores** y tocar **COMENZAR**. Se comparte un visor.

Para usar Quest Link en Windows, conectá el visor y activá Meta Horizon Link como runtime OpenXR en la app de la PC. Después reiniciá Unity. En VR, apuntá a **JUGAR** y apretá el gatillo, o acercá el índice y tocá el botón. En la pista, agarrá el mango con el grip (botón lateral); podés apoyar la otra mano un poco más abajo.

## En la pista

El cartel al lado de la pelota muestra el turno, los golpes y el tiempo. La barra mide el movimiento horizontal del palo: al pegarle a la pelota queda visible el porcentaje que se usó para ese golpe.

En solo, el reloj cuenta cuánto tardás en embocar. Con amigos, se turnan después de cada tiro y cada jugador conserva la posición de su pelota. Cuando queda uno sin embocar, el reloj cambia a una cuenta de **1:00**. Si se vence, suma **5 golpes** a los que ya hizo. Al terminar el hoyo aparecen los resultados ordenados de menor a mayor; se puede empatar, volver al menú o jugar de vuelta.

[Vista de las pantallas con puntajes de ejemplo](docs/Interfaz.png)

## Probar los controles del tutorial

1. Mové el stick izquierdo para caminar. Con el derecho hacia los costados girás; con el derecho hacia adelante apuntás el arco y al soltarlo te teletransportás. El botón **A** salta. Para preferir giros por pasos, desmarcá **Smooth Turn Enabled** en el **Right Controller** de la escena de la pista.
2. Embocá usando una o dos manos. Para guardar el palo, acercá el mango al soporte de la salida y soltá ambos grips. La marca con una línea en el piso te trae al tee mirando al hoyo.
3. A la derecha de la salida hay un mueble. Teletransportate al piso a su lado: podés tirar de la manija de la puerta y abrir el cajón hasta el tope. La ficha de arriba se desliza sobre la mesa; agarrala con ambas manos para cambiarle el tamaño o apretá el gatillo mientras la tenés agarrada para cambiarle el color. Es de práctica y no suma golpes.

Los capítulos de [Valem](https://www.youtube.com/watch?v=H8dWVlZKQu4) también comparan formas de agarrar objetos y muestran un panel con botones y un deslizador. Esos ejemplos ya vienen en `Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/DemoScene.unity`. Podés abrirla desde el editor; la partida sigue teniendo un solo hoyo. Para ver qué está leyendo cada botón en **Play**, abrí **Window → Analysis → XR Interaction Debugger** y elegí **Input Devices**.

**Sin visor:** en **Edit → Project Settings → XR Plug-in Management → XR Interaction Toolkit**, marcá **Use XR Interaction Simulator in scenes** y dejá **Instantiate In Editor Only** activado. Abrí Inicio y tocá Play. Desmarcá el simulador antes de volver a Quest Link.

**Para jugar directamente en Quest:** agregá **Android Build Support** con SDK, NDK y OpenJDK desde Unity Hub. Con el visor en modo desarrollador y la depuración USB autorizada, abrí **File → Build Profiles → Android**, cambiá de plataforma, elegí tu visor en **Run Device** y tocá **Build And Run**. Para jugar con Quest Link desde la PC usá el perfil de Windows.

## Escenas y archivos

- `Assets/Scenes/Inicio.unity`: pantalla inicial.
- `Assets/Scenes/MenuPrincipal.unity`: menú, selección de jugadores y resultados.
- `Assets/Scenes/MinigolfVR.unity`: pista gris, pelota, palo y un hoyo para probar VR.
- `Assets/Scripts/Partida`: pelota, palo, reglas y turnos.
- `Assets/Scripts/UI`: botones, marcador y resultados.
- `Assets/Scripts/VR`: seguimiento del visor y animación de manos.
- `Assets/Models`: prefabs del palo, pelota, hoyo y manos.
- `Assets/Materials`: colores de la pista y los objetos.

Las tres escenas y sus prefabs están guardados en GitHub. El proyecto incluye XR Interaction Toolkit; sus **Starter Assets** se importan desde el paquete la primera vez que Unity abre el proyecto. Las manos azules vienen de los modelos del [tutorial de Valem](https://youtu.be/H8dWVlZKQu4).

Las fuentes Irish Grover y Luckiest Guy están en `Assets/Fuentes`; el trofeo de Ant Design Icons está en `Assets/Sprites/Interfaz`. Para probar las reglas sin abrir Unity: `dotnet run --project Pruebas/Pruebas.csproj` (requiere .NET 8).
