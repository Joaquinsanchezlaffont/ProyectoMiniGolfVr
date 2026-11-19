using UnityEngine;

namespace MiniGolfVR
{
    public class FichaDePractica : MonoBehaviour
    {
        [SerializeField] private Renderer ficha;
        [SerializeField] private Material claro, oscuro;
        private bool marcada;

        public void CambiarColor()
        {
            marcada = !marcada;
            ficha.sharedMaterial = marcada ? oscuro : claro;
        }
    }
}
