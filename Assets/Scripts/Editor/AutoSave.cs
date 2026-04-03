using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSave
{
    // Esse código roda sozinho toda vez que a Unity abre
    static AutoSave()
    {
        EditorApplication.playModeStateChanged += SalvarAoDarPlay;
    }

    private static void SalvarAoDarPlay(PlayModeStateChange state)
    {
        // Se a Unity detectar que você vai entrar no Modo Play...
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("💾 Auto-Save: Salvando a cena e os arquivos antes de testar...");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }
    }
}