using UnityEngine;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager instance;

    public CanvasManager canvasManager;
    GameObject mainScene, coloringScene;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        mainScene = canvasManager.gameObject;
    }

    //true → coloringScene
    public void ChangeScene(bool goColor, bool goScan)
    {
        Resources.UnloadUnusedAssets();
        if (goColor)
        {
            coloringScene = Instantiate(Resources.Load<GameObject>("prefabs/ColoringScene"));
            mainScene.SetActive(false);
        }
        else
        {
            Destroy(coloringScene);
            mainScene.SetActive(true);
            canvasManager.PanelManager(goScan);
        }
    }




    //public void LoadScene(string levelID)
    //{
    //    if (isAction)
    //    {
    //        isAction = false;
    //        StartCoroutine(LoadSceneCoroutine(levelID));
    //    }
    //}

    //private IEnumerator LoadSceneCoroutine(string sceneName)
    //{
    //    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

    //    while (true)
    //    {
    //        if (op.isDone)
    //        {
    //            this.addedScene = SceneManager.GetSceneByName(sceneName);
    //            isAction = true;

    //            FindObjectOfType<MainGroup>().gameObject.SetActive(false);

    //            break;
    //        }
    //        yield return 0;
    //    }
    //}


    //public void UnloadScene(bool scanStart)
    //{
    //    StartCoroutine(UnloadScene(addedScene, scanStart));
    //}

    //private IEnumerator UnloadScene(Scene scene, bool scanStart)
    //{
    //    AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
    //    Resources.UnloadUnusedAssets();

    //    while (true)
    //    {
    //        if (op.isDone)
    //        {
    //            Resources.FindObjectsOfTypeAll<MainGroup>()[0].gameObject.SetActive(true);

    //            if (scanStart)
    //            {
    //                yield return canvasManager.isActiveAndEnabled;
    //                canvasManager.PanelManager(true);
    //            }

    //            break;
    //        }
    //        yield return 0;
    //    }
    //}

}
