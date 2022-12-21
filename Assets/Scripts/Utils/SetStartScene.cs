using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SetStartScene
{
    static SetStartScene()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Preload.unity");
    }
}