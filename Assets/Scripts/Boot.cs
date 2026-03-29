using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour
{
    [SerializeField] private string uiSceneName = "UI_Scene";
    [SerializeField] private string gameSceneName = "Game_Scene";

    private void Start()
    {
        SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
