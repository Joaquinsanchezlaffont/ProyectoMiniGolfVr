using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolfVR
{
    // Assigned to the persistent On Click event of the JUGAR button.
    public sealed class InicioMenuController : MonoBehaviour
    {
        private const string MainMenuScene = "MenuPrincipal";

        public void Jugar()
        {
            if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
            {
                Debug.Log("Minigolf VR: botón JUGAR listo. Falta crear la escena MenuPrincipal.");
                return;
            }

            SceneManager.LoadScene(MainMenuScene);
        }
    }
}
