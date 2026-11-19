# MiniGolf VR

Proyecto de Joaquín Sánchez Laffont (Unity) y Sebastián Ángel Warman (Blender).

## Cómo abrirlo

1. Si tenés un ZIP viejo, cerrá Unity y descargá la versión nueva del repositorio. No copies solamente `Assets`: también hacen falta `Packages` y `ProjectSettings`.
2. En Unity Hub abrí la carpeta que contiene esas tres carpetas, con Unity **6000.5.1f1**.
3. Esperá a que Unity importe los paquetes y Starter Assets. La primera vez puede tardar.
4. Abrí `Assets/Scenes/Inicio.unity` y tocá **Play**.
5. Tocá **JUGAR**. Podés entrar solo o elegir **JUGAR CON AMIGOS**, marcar **2, 3 o 4 jugadores** y tocar **COMENZAR**. Se comparte un visor.

Para usar Quest Link en Windows, conectá el visor y activá Meta Horizon Link como runtime OpenXR en la app de la PC. Después reiniciá Unity. En VR, apuntá al botón y apretá el gatillo. En la pista, acercá la mano al palo y apretá el grip para agarrarlo.

## En la pista

El cartel al lado de la pelota muestra el turno, los golpes y el tiempo. La barra mide el movimiento horizontal del palo: al pegarle a la pelota queda visible el porcentaje que se usó para ese golpe.

En solo, el reloj cuenta cuánto tardás en embocar. Con amigos, se turnan después de cada tiro y cada jugador conserva la posición de su pelota. Cuando queda uno sin embocar, el reloj cambia a una cuenta de **1:00**. Si se vence, suma **5 golpes** a los que ya hizo. Al terminar el hoyo aparecen los resultados ordenados de menor a mayor; se puede empatar, volver al menú o jugar de vuelta.

[Vista de las pantallas con puntajes de ejemplo](docs/Interfaz.png)

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
