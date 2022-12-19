using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using ConnectinnoGames.Utils;

public class SceneLoadManager : MonoBehaviour
{
    /// <summary>
    /// Loads the scene according to the given scene name
    /// </summary>
    /// <param name="sceneName"></param>
    public static async Task LoadScene(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName);

        var scene = SceneManager.GetSceneByName(sceneName);

        SceneManager.SetActiveScene(scene);
    }
}
