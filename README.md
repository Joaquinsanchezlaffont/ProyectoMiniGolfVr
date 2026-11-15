# MiniGolf VR

Proyecto de Joaquín Sánchez Laffont (Unity) y Sebastián Ángel Warman (Blender).

## Cómo abrirlo

1. Si tenés un ZIP viejo, cerrá Unity y descargá la versión nueva del repositorio. No copies solamente `Assets`: también hacen falta `Packages` y `ProjectSettings`.
2. En Unity Hub abrí la carpeta que contiene esas tres carpetas, con Unity **6000.5.1f1**.
3. Esperá a que Unity importe los paquetes y Starter Assets. La primera vez puede tardar.
4. Abrí `Assets/Scenes/Inicio.unity` y tocá **Play**.
5. **JUGAR** abre el menú. **JUGAR SOLO** abre la pista y **VOLVER** regresa al inicio.

Para usar Quest Link en Windows, conectá el visor y activá Meta Horizon Link como runtime OpenXR en la app de la PC. Después reiniciá Unity. En VR, apuntá al botón y apretá el gatillo. En la pista, acercá la mano al palo y apretá el grip para agarrarlo.

## Escenas y archivos

- `Assets/Scenes/Inicio.unity`: pantalla inicial.
- `Assets/Scenes/MenuPrincipal.unity`: menú de juego.
- `Assets/Scenes/MinigolfVR.unity`: pista gris, pelota, palo y un hoyo para probar VR.
- `Assets/Scripts`: movimiento de pelota, golpes, hoyo y botones.
- `Assets/Models`: prefabs del palo, pelota, hoyo y manos.
- `Assets/Materials`: colores de la pista y los objetos.

Las tres escenas y sus prefabs están guardados en GitHub. El proyecto incluye XR Interaction Toolkit; sus **Starter Assets** se importan desde el paquete la primera vez que Unity abre el proyecto. Las manos azules vienen de los modelos del [tutorial de Valem](https://youtu.be/H8dWVlZKQu4).

Por ahora hay un solo hoyo. **JUGAR CON AMIGOS** muestra el botón, pero el modo multijugador todavía no está programado.
