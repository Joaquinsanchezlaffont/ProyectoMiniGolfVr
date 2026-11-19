using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGolfVR
{
    public sealed class MenuPrincipalController : MonoBehaviour
    {
        [SerializeField] private GameObject menu, selector;
        [SerializeField] private ResultadosHoyo resultados;
        [SerializeField] private GameObject[] marcasDeSeleccion;
        private int cantidadElegida = 2;

        private void Start()
        {
            if (SesionMinigolf.Resultado == null) MostrarMenu();
            else
            {
                Mostrar(resultados.gameObject);
                resultados.Mostrar(SesionMinigolf.Resultado);
            }
        }

        public void JugarSolo() => AbrirPista(1);

        public void JugarConAmigos()
        {
            Mostrar(selector);
            ElegirCantidad(2);
        }

        public void ElegirCantidad(int cantidad)
        {
            cantidadElegida = cantidad;
            for (int i = 0; i < marcasDeSeleccion.Length; i++)
                marcasDeSeleccion[i].SetActive(cantidad == i + 2);
        }

        public void Comenzar() => AbrirPista(cantidadElegida);
        public void JugarDeVuelta() => AbrirPista(SesionMinigolf.Jugadores);
        public void Volver() => SceneManager.LoadScene("Inicio");

        public void MostrarMenu()
        {
            SesionMinigolf.Resultado = null;
            Mostrar(menu);
        }

        private void Mostrar(GameObject pantalla)
        {
            menu.SetActive(pantalla == menu);
            selector.SetActive(pantalla == selector);
            resultados.gameObject.SetActive(pantalla == resultados.gameObject);
        }

        private void AbrirPista(int cantidad)
        {
            SesionMinigolf.Preparar(cantidad);
            SceneManager.LoadScene("MinigolfVR");
        }
    }
}
