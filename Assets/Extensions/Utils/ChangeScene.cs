#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class ChangeScene : Editor
{
    [MenuItem("Open Scene/Loading #1")]
    public static void OpenLoading()
    {
        OpenScene("Loading");
    }
    [MenuItem("Open Scene/Main #2")]
    public static void OpenMain()
    {
        OpenScene("Main");
    }
    
    [MenuItem("Open Scene/Game #3")]
    public static void OpenGame()
    {
        OpenScene("Game");
    }
    
    private static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Game/Scenes/" + sceneName + ".unity");
        }
    }
}
#endif