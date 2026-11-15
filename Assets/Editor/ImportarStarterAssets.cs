using UnityEditor;
using UnityEditor.PackageManager.UI;

// El jugador VR de las escenas viene del ejemplo Starter Assets de Unity.
[InitializeOnLoad]
public static class ImportarStarterAssets
{
    static ImportarStarterAssets()
    {
        EditorApplication.delayCall += Importar;
    }

    private static void Importar()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Importar;
            return;
        }

        foreach (Sample ejemplo in Sample.FindByPackage("com.unity.xr.interaction.toolkit", "3.5.1"))
        {
            if (ejemplo.displayName == "Starter Assets" && !ejemplo.isImported)
            {
                ejemplo.Import(Sample.ImportOptions.None);
                return;
            }
        }
    }
}
