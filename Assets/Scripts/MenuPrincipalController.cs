using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolfVR
{
    public sealed class MenuPrincipalController : MonoBehaviour
    {
        public void JugarSolo()
        {
            if (Application.CanStreamedLevelBeLoaded("MinigolfVR"))
                SceneManager.LoadScene("MinigolfVR");
            else
                Debug.LogError("Minigolf VR: falta la escena del hoyo en Build Settings.");
        }

        public void JugarConAmigos()
        {
            Debug.Log("Minigolf VR: el modo con amigos todavía no está conectado.");
        }

        public void Volver()
        {
            SceneManager.LoadScene("Inicio");
        }
    }
}
