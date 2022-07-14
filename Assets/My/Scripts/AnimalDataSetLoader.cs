using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;

public class AnimalDataSetLoader : MonoBehaviour
{
    public UnityEngine.UI.Image progressBar;
    public Text progressText;
    public GameObject arCam;
    public GameObject[] freeModels;
    public bool isFirst;

    public bool fileExist; //file Exist Check
    public List<string> tagmeTargets; //전체 타겟이름(All 소문자)
    public List<string> freeTargets; //무료 타겟이름(All 소문자)
    public List<bool> cntDataSet;

    public int child = 0;
    public string dataSetName;

    private readonly string[] freePage = { "alligator", "ant", "bat_animal", "bear", "beaver", "bird", "butterfly", "cat", "chicken", "cow",
            "camera", "computer", "desk", "envelope", "folder", "laptop", "marker", "tape", "baby_powder", "bathtub",
            "flamingo", "worm", "zebra", "moose", "mouse", "octopus", "owl", "hippopotamus", "squirrel", "stingray",
            "briefcase", "flashlight", "magnifying_glass", "mug", "paper_clip", "printer", "staple_remover", "stapler", "faucet", "floss",
             "Jam", "Juice", "Kale", "Noodle", "Nuts", "Rice", "Vinegar", "Yam", "Yogurt", "Zucchini"};

    public readonly string[] audioFolder = { "kor", "eng", "chn", "esp", "jpn", "deu", "fra", "are", "ita", "pol", "hin", "heb", "rus", "tha", "word", "sentence" };

    private readonly string url = "http://bookplusapp.co.kr/fileStorage/tm_flashcards_cn/new/";
    CanvasManager canvasManager;
    PrefabShelter prefabShelter;

    void Awake()
    {
        freeModels = new GameObject[50];
        isFirst = false;
        VuforiaAbstractConfiguration.Instance.Vuforia.DelayedInitialization = false;
        arCam.GetComponent<VuforiaBehaviour>().enabled = true;
        VuforiaRuntime.Instance.InitVuforia();

        canvasManager = FindObjectOfType<CanvasManager>();
        prefabShelter = FindObjectOfType<PrefabShelter>();

        tagmeTargets = new List<string>();
        freeTargets = new List<string>();
        cntDataSet = new List<bool>();

        //Vuforia ver.6.2
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
    }

    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaStartedCallback(LoadDataSet);
    }

    //Non Fliped ImageTarget
    void LoadDataSet()
    {
        child += 1;

        if (child < 6)
            dataSetName = string.Format("TM_flashcards_cn_0{0}", child); //1~5
        else
            dataSetName = string.Format("TM_flashcards_cn_fliped_0{0}", child - 5); //6~10


        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

        DataSet dataSet = objectTracker.CreateDataSet();
        if (dataSet.Load(dataSetName))
        {
            if (!objectTracker.ActivateDataSet(dataSet))
                Debug.Log("<color=yellow>Failed to Activate DataSet: " + (dataSetName) + "</color>");

            if (!objectTracker.Start())
                Debug.Log("<color=yellow>Tracker Failed to Start.</color>");

            objectTracker.Stop();
            StartCoroutine(DataSetLoading());
        }
        else
        {
            Debug.LogError("<color=yellow>Failed to load dataset: '" + (dataSetName) + "'</color>");
        }
    }

    public IEnumerator FreeModelSetting()
    {
        string path = string.Format("{0}/assets/tagme3d_card_asset_free", Application.persistentDataPath);
        if (File.Exists(path))
        {
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(path);
            AssetBundle freeBundle = req.assetBundle;
            freeModels = freeBundle.LoadAllAssets<GameObject>();
            freeBundle.Unload(false);
            isFirst = true;
        }
        yield return null;
    }

    IEnumerator DataSetLoading()
    {
        if (child < 6)
            cntDataSet.Add(true);

        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();

        foreach (TrackableBehaviour tb in tbs)
        {
            if (tb.name.Equals("New Game Object"))
            {
                tb.gameObject.name = tb.TrackableName;

                bool checkfree = false;

                tb.gameObject.transform.parent = transform.GetChild(child - 1);
                DynamicTrackableEventHandler dteh = tb.gameObject.AddComponent<DynamicTrackableEventHandler>();

                for (int i = 0; i < freePage.Length; i++)
                {
                    if (freePage[i].ToLower().Equals(tb.TrackableName))
                    {
                        checkfree = true;
                        break;
                    }
                }
                dteh.isFreeModel = checkfree;

                if (child < 6)
                {
                    if (checkfree)
                        //freeTargets.Add(tb.TrackableName);
                        tagmeTargets.Add(string.Empty);
                    else
                        tagmeTargets.Add(tb.TrackableName);
                }
            }
        }
        yield return new WaitForEndOfFrame();

        if (child == 10)
            StartCoroutine(TargetPrefabSetting());
        else
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);

        yield return null;
    }


    //파일체크 후 AssetBundle 컴포넌트에 셋팅 → 로딩끝
    private IEnumerator TargetPrefabSetting()
    {
        int cntCheck = cntDataSet.Count;
        fileExist = true;

        //AssetBundle, Audio, Video 파일 유무 체크
        bool assetExist = File.Exists(string.Format("{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath));

        for (int k = 0; k < cntCheck; k++)
        {
            for (int i = 0; i < 100; i++)
            {
                int index = (k * 100) + i;
                if (!tagmeTargets[index].Equals(string.Empty))
                {
                    string videoPath = string.Format("{0}/videos/tagme3d_card_video_unfree", Application.persistentDataPath);
                    string audioPath = string.Format("{0}/audios/tagme3d_card_audio_unfree", Application.persistentDataPath);

                    if (!File.Exists(videoPath))
                    {
                        cntDataSet[k] = false;
                        break;
                    }
                    if (!File.Exists(audioPath))
                    {
                        cntDataSet[k] = false;
                        break;
                    }

                    //if (!File.Exists(videoPath) || !assetExist)
                    //{
                    //    cntDataSet[k] = false;
                    //    break;
                    //}

                    //for (int j = 0; j < audioFolder.Length; j++)
                    //{
                    //    string audioPath = string.Format("{0}/audio/{1}/{2}.mp3", Application.persistentDataPath, audioFolder[j], tagmeTargets[index]);

                    //    if (!File.Exists(audioPath))
                    //    {
                    //        cntDataSet[k] = false;
                    //        break;
                    //    }
                    //}
                }
                progressBar.fillAmount = 0.5f + ((index + 1f) / (cntCheck * 100) * 0.5f);
                yield return progressBar.fillAmount;
            }
        }

        for (int i = 0; i < cntCheck; i++)
        {
            if (!cntDataSet[i])
            {
                fileExist = false;
                break;
            }
        }
        yield return fileExist;

        if (!fileExist)
        {
            prefabShelter.nothingModel = true;

            yield return new WaitForEndOfFrame();
            canvasManager.OnLoadingDone();
        }
        else
        {
            progressBar.fillAmount = 0;

#if UNITY_EDITOR
            string path = string.Format("file:///{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
#elif UNITY_ANDROID
            string path = string.Format("file://{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
#elif UNITY_IOS
            string path = string.Format("{0}/assets/tm_flashcard_cn_ios", Application.persistentDataPath);
#endif

            UnityWebRequest webr = UnityWebRequestAssetBundle.GetAssetBundle(path);
            webr.Send();

            while (!webr.isDone)
                yield return new WaitForEndOfFrame();

            AssetBundle bundles = DownloadHandlerAssetBundle.GetContent(webr);
            for (int j = 0; j < cntDataSet.Count; j++)
            {
                for (int i = 0; i < 100; i++)
                {
                    int index = (j * 100) + i;

                    if (!tagmeTargets[index].Equals(string.Empty))
                    {
                        AssetBundleRequest req = bundles.LoadAssetAsync<GameObject>(tagmeTargets[index]);
                        prefabShelter.tmModel[index] = new TMModel((GameObject)req.asset, false);
                    }
                    else
                        prefabShelter.tmModel[index] = new TMModel(null, false);

                    progressText.text = string.Format("Preparing Tagme AR Cards   {0}/{1}", index + 1, cntDataSet.Count * 100);
                    progressBar.fillAmount = (index + 1f) / (cntDataSet.Count * 100f);
                    yield return progressBar.fillAmount;
                }
            }
            bundles.Unload(false);

            Resources.UnloadUnusedAssets();
            prefabShelter.nothingModel = false;

            yield return new WaitForEndOfFrame();
            canvasManager.OnLoadingDone();
        }
        yield return null;
    }



    public IEnumerator FreeModelCheck(bool b)
    {
        bool isCheck = true;
        string fileName = "";
        string path = "";
        for (int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0:
                    fileName = "tagme3d_card_audio_free";
                    path = string.Format("{0}/audios/{1}", Application.persistentDataPath, fileName);
                    break;
                case 1:
                    fileName = "tagme3d_card_video_free";
                    path = string.Format("{0}/videos/{1}", Application.persistentDataPath, fileName);
                    break;
                case 3:
                    fileName = "tagme3d_card_asset_free";
                    path = string.Format("{0}/assets/{1}", Application.persistentDataPath, fileName);
                    break;
            }
            if (File.Exists(path))
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    isCheck = true;
                    break;
                }
                else
                {
                    FileInfo inf = new FileInfo(path);
                    long fileSize = inf.Length;

                    UnityWebRequest reqs = UnityWebRequest.Head(url + fileName);
                    reqs.SendWebRequest();

                    while (reqs.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    long checkSize = long.Parse(reqs.GetResponseHeader("Content-Length"));
                    Debug.Log(checkSize);
                    if (checkSize == 0)
                    {
                        isCheck = true;
                    }
                    else if (fileSize == checkSize)
                    {
                        isCheck = true;
                    }
                    else
                    {
                        isCheck = false;
                        break;
                    }

                }
            }
            else
            {
                isCheck = false;
                break;
            }
        }
        if (!isCheck)
        {
            StartCoroutine(FindObjectOfType<FileDownloader>().Download("tagme3d_asset_free"));
        }
        yield return null;
    }
}