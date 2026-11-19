using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolfVR
{
    // Botón de la pantalla de inicio.
    public sealed class InicioMenuController : MonoBehaviour
    {
        private const string MainMenuScene = "MenuPrincipal";

        public void Jugar()
        {
            if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
            {
                Debug.LogError("Falta agregar MenuPrincipal a las escenas del proyecto.");
                return;
            }

            SceneManager.LoadScene(MainMenuScene);
        }
    }
}
