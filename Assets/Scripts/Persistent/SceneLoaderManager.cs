using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : MonoBehaviour
{
    [System.Serializable]
    public enum ScenesEnum
    {
        Menu = 0,
        MainLevel = 1,
        DeathScene = 2,
        WinScene = 3,
    }

    private static SceneLoaderManager sceneLoaderManager;

    public static SceneLoaderManager instance
    {
        get
        {
            if (!sceneLoaderManager)
            {
                sceneLoaderManager = FindObjectOfType(typeof(SceneLoaderManager)) as SceneLoaderManager;

                if (!sceneLoaderManager)
                {
                    Debug.LogError("There needs to be one active SceneLoaderManager script on a GameObject in your scene.");
                }
                else
                {
                    sceneLoaderManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(sceneLoaderManager);
                }
            }
            return sceneLoaderManager;
        }
    }

    void Init()
    {
    }

    public static Scene GetCurrentScene()
    {
        return SceneManager.GetActiveScene();
    }

    public static void LoadBuildIndexed(int index)
    {
        if (SceneManager.sceneCountInBuildSettings < index)
        {
            Debug.LogError("Not enough scenes");
            return;
        }
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }

    public static void LoadBuildName(string name) {
        LoadName(name);
    }

    public static void ForceLoadEnum(ScenesEnum enumScene, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(enumScene.ToString(), sceneMode);
    }

    public static void LoadEnum(ScenesEnum enumScene, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        if (SceneLoaderManager.GetCurrentScene() != SceneManager.GetSceneByName(enumScene.ToString()))
            SceneManager.LoadScene(enumScene.ToString(), sceneMode);
    }

    public static void LoadName(string nameScene, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        if(SceneLoaderManager.GetCurrentScene() != SceneManager.GetSceneByName(nameScene))
            SceneManager.LoadScene(nameScene, sceneMode);
    }

    public static void QuitApplication() {
        Application.Quit();
    }
}