using I2.Loc;
using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
//using System.Runtime.InteropServices;

public class FileDownloader : MonoBehaviour
{
    public Text assetLoaderText, downSpeedText;
    public GameObject bookDownloadGO;
    public Image mainProgress;

    AnimalDataSetLoader aDSL;
    CanvasManager canvasManager;

    private string savedDataSet;
    //private int bookNumber;
    [SerializeField]
    private List<FileList> fileList = new List<FileList>();

    private WWW ping;
    private WWW downPing;
    private GameObject blocker;
    private bool downloadInProgress;
    private float pingWaitEllap;
    private readonly float pingWait = 7.5f;
    private readonly float downPingWait = 10f;
    private float downPingWaitEllap;
    private float downServerWait = 15f;
    private float downServerWaitEllap;

    private long totalSize;
    private long lastSize;
    private readonly float timerSpeed = 0.75f;
    private float timerSpeedEllap = 0;
    private double speed = 0;
    //private string url = "http://vproductions.mobi/api/assets/tm_flashcards_cn/";
    //private readonly string url = "https://tm-flashcards-cn.oss-cn-beijing.aliyuncs.com/assets/";
    private readonly string pingUrl = "http://bookplusapp.co.kr/fileStorage/tm_flashcards_cn/new/";
    private readonly string url = "http://bookplusapp.co.kr/";
    private readonly string iosUrl = "https://tm-flashcards-cn.oss-cn-beijing.aliyuncs.com/assets/ios/";

    public int coroutineNumber;
    private List<IEnumerator> coroutines;
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void unzip (string zipFilePath, string location);
 
    [DllImport("__Internal")]
    private static extern void zip (string zipFilePath);
 
    [DllImport("__Internal")]
    private static extern void addZipFile (string addFile);
 
#endif

    private void Awake()
    {
        aDSL = GetComponent<AnimalDataSetLoader>();
        canvasManager = FindObjectOfType<CanvasManager>();
    }


    private void Update()
    {
        if (ping != null)
        {
            if (ping.isDone && string.IsNullOrEmpty(ping.error))
            {
                //#if UNITY_IOS
                //                StartCoroutine(DownloadIOS(savedDataSet));
                //#else
                StartCoroutine(Download(savedDataSet));
                //#endif

                //Debug.Log("Connected successfully");
                ping = null;
            }
            else if (pingWaitEllap < pingWait)
            {
                pingWaitEllap += Time.deltaTime;
            }
            else
            {
                //Debug.Log("Connection check failed - No ping from server");
                Destroy(blocker);
                ActiveWindow("connectFail");
                savedDataSet = string.Empty;
                ping = null;

                canvasManager.bookPanel.GetComponent<PanelMovingController>().PanelOff();
            }
        }

        if (downloadInProgress)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                downServerWaitEllap = 0;
            }
            else if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (downServerWaitEllap < downServerWait)
                {
                    downServerWaitEllap += Time.deltaTime;
                }
                else
                {
                    downServerWaitEllap = 0;
                    ForceQuitDownload(true);
                    //Debug.Log("Download force quitted - Internet connection lost");
                }
            }
            else
            {
                if (downPing != null)
                {
                    if (downPing.isDone && string.IsNullOrEmpty(downPing.error))
                    {
                        //#if UNITY_IOS
                        //                        downPing = new WWW(iosUrl);
                        //#else
                        downPing = new WWW(pingUrl);
                        //#endif
                    }
                    else if (downPingWaitEllap < downPingWait)
                    {
                        downPingWaitEllap += Time.deltaTime;
                    }
                    else
                    {
                        ForceQuitDownload(true);
                        //Debug.Log("Download force quitted - Server connection lost");
                    }
                }
            }
        }
    }

    //파일 체크 → 있으면 프리펩셋팅 / 없으면 다운로드후 프리펩셋팅
    public void CheckFile()
    {
        bool checkBook = true;
        string modelPath = string.Format("{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
        string audioPath = string.Format("{0}/audios/tagme3d_card_audio_unfree", Application.persistentDataPath);
        string videoPath = string.Format("{0}/videos/tagme3d_card_video_unfree", Application.persistentDataPath);


        for(int i = 0; i < 3; i++)
        {
            string path = string.Empty;
            string format = string.Empty;
            switch (i)
            {
                case 0:
                    path = modelPath;
                    format = "tagme3d_card_asset_unfree";
                    break;
                case 1:
                    path = audioPath;
                    format = "tagme3d_card_audio_unfree";
                    break;
                case 2:
                    path = videoPath;
                    format = "tagme3d_card_video_unfree";
                    break;
            }
            if (File.Exists(path))
            {
                if (Application.internetReachability.Equals(NetworkReachability.NotReachable))
                {
                    checkBook = true;
                    break;
                }
                else
                {
                    FileInfo inf = new FileInfo(path);
                    long fileSize = inf.Length;

                    UnityWebRequest reqs = UnityWebRequest.Head(pingUrl + format);
                    reqs.SendWebRequest();

                    long checkSize = long.Parse(reqs.GetResponseHeader("Content-Length"));

                    if (checkSize == 0)
                    {
                        checkBook = true;
                    }
                    else if (fileSize == checkSize)
                    {
                        checkBook = true;
                    }
                    else
                    {
                        checkBook = false;
                        break;
                    }
                }
            }
            else
            {
                aDSL.cntDataSet[0] = false;
                aDSL.cntDataSet[1] = false;
                aDSL.cntDataSet[2] = false;
                aDSL.cntDataSet[3] = false;
                aDSL.cntDataSet[4] = false;
                checkBook = false;
                break;
            }
        }

        if (checkBook)
        {
            if (!aDSL.fileExist)
            {
                canvasManager.bookPanel.SetActive(true);
                canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();

                CreateBlocker();
                StartCoroutine(TargetDataSetting());
            }
        }
        else
        {
            canvasManager.bookPanel.SetActive(true);
            canvasManager.bookPanel.GetComponentInChildren<PanelMovingController>().TouchOn();
            OnClickYes("tagme3d_asset_unfree");
        }
    }


    public void OnClickYes(string dataSetName)
    {
        if (ping == null)
        {
            Font localFont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

            assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downCheckPing");
            assetLoaderText.font = localFont;

            pingWaitEllap = 0;

            //#if UNITY_IOS
            //            ping = new WWW(iosUrl);
            //#else
            ping = new WWW(url);
            //#endif
            savedDataSet = dataSetName;
            mainProgress.fillAmount = 0;

            CreateBlocker();
        }
    }

    private void ActiveWindow(string st)
    {
        canvasManager.toastMsgPanel.SetActive(false);
        canvasManager.toastMsgPanel.SetActive(true);
        canvasManager.toastMsgPanel.GetComponent<ToastMsgManager>().ToastMessage(st, "1", false);
    }

    private void CreateBlocker()
    {
        if (blocker != null)
            Destroy(blocker);
        blocker = new GameObject("blocker", typeof(Image), typeof(Button));
        blocker.transform.SetParent(canvasManager.bookPanel.transform);
        RectTransform rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition3D = new Vector3(0, 0, 0);
        rect.localScale = new Vector3(1, 1, 1);
        Image image = blocker.GetComponent<Image>();
        image.color = Color.clear;
        Button button = blocker.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ActiveWindow("downWait"));
    }

    private void ForceQuitDownload(bool window)
    {
        StopAllCoroutines();
        assetLoaderText.text = string.Empty;

        if (blocker != null)
        {
            Destroy(blocker);
        }

        if (downPing != null)
        {
            downPing = null;
        }

        if (window)
            ActiveWindow("connectFail");
        else
            ActiveWindow("downFileError");

        mainProgress.fillAmount = 0;
        downSpeedText.transform.parent.gameObject.SetActive(false);
        savedDataSet = string.Empty;

        downloadInProgress = false;

        canvasManager.bookPanel.GetComponent<PanelMovingController>().PanelOff();
    }

    public IEnumerator Download(string dataSetName)
    {
        if (Directory.Exists(Application.persistentDataPath + "/asset"))
        {
            Directory.Delete(Application.persistentDataPath + "/asset", true);
        }
        if (Directory.Exists(Application.persistentDataPath + "/audio"))
        {
            Directory.Delete(Application.persistentDataPath + "/audio", true);
        }
        if (Directory.Exists(Application.persistentDataPath + "/video"))
        {
            Directory.Delete(Application.persistentDataPath + "/video", true);
        }
        downloadInProgress = true;
        fileList.Clear();

        string progressString;
        savedDataSet = dataSetName;

        downPing = new WWW(pingUrl + dataSetName + ".zip");

        downSpeedText.transform.parent.gameObject.SetActive(true);
        downSpeedText.gameObject.SetActive(false);


        //____________ Check Server Files
        assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downFileCheck");

        totalSize = 0;

        bool checkInfo = false;

        StartCoroutine(Download_Information(dataSetName, output =>
        {
            checkInfo = true;
        }));

        while (!checkInfo)
        {
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < fileList.Count; i++)
        {
            totalSize += fileList[i].size;
        }


        //____________ Download Start
        progressString = LocalizationManager.GetTermTranslation("UI_downloading");
        assetLoaderText.text = string.Format("{0}...", progressString);

        downSpeedText.gameObject.SetActive(true);

        bool checkDown = false;

        StartCoroutine(Download_Data(dataSetName, output =>
        {
            checkDown = true;
        }));

        while (!checkDown)
        {
            long currentSize = 0;

            for (int i = 0; i < fileList.Count; i++)
            {
                currentSize += fileList[i].size;
            }

            if (timerSpeedEllap < timerSpeed)
            {
                timerSpeedEllap += Time.deltaTime;
                speed += (lastSize - currentSize);
            }
            else
            {
                float speedMultiplier = 1 / timerSpeed;
                speed *= speedMultiplier;

                timerSpeedEllap = 0;
                if (speed <= 0)
                {
                    downSpeedText.text = "Unpredictable";
                }
                else if (speed < 1024)
                {
                    downSpeedText.text = string.Format("{0}/Bps", speed.ToString("####"));
                }
                else if (speed < 1048576)
                {
                    downSpeedText.text = string.Format("{0}/Kbps", (speed / 1024).ToString("####.#"));
                }
                else
                {
                    downSpeedText.text = string.Format("{0}/Mbps", (speed / 1048576).ToString("####.#"));
                }
                speed = 0;
            }

            lastSize = currentSize;

            mainProgress.fillAmount = (float)(totalSize - currentSize) / totalSize;
            assetLoaderText.text = string.Format("{0}...   {1:00.0}%", progressString, ((float)(totalSize - currentSize) / totalSize) * 100);

            yield return new WaitForEndOfFrame();
        }
        downSpeedText.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();


        //____________ Download File Extraction
        bool checkExtract = false;

        StartCoroutine(Download_Extract(dataSetName, output =>
        {
            checkExtract = true;
        }));

        while (!checkExtract)
        {
            yield return new WaitForEndOfFrame();
        }


        downSpeedText.transform.parent.gameObject.SetActive(false);
        assetLoaderText.text = string.Empty;

        //Debug.Log("Total downloaded file Size : " + totalSize);

        if (!totalSize.Equals(0))
        {
            StartCoroutine(TargetDataSetting());
        }
        else
        {
            ForceQuitDownload(false);
        }

        savedDataSet = string.Empty;
        downloadInProgress = false;

        yield return null;
    }

    // IOS
    IEnumerator DownloadIOS(string dataSetName)
    {
        downloadInProgress = true;

        string progressString;
        savedDataSet = dataSetName;

        downPing = new WWW(pingUrl);

        downSpeedText.transform.parent.gameObject.SetActive(true);
        downSpeedText.gameObject.SetActive(false);

        for (int k = 0; k < aDSL.cntDataSet.Count; k++)
        {
            fileList.Clear();
            mainProgress.fillAmount = 0;

            //____________ Check Server Files
            assetLoaderText.text = string.Format("{0} {1:00}", LocalizationManager.GetTermTranslation("UI_downFileCheck"), k + 1);

            totalSize = 0;

            bool checkInfo = false;

            StartCoroutine(Download_Information(string.Format("tm_flashcards_cn_0{0}", k + 1), output =>
            {
                checkInfo = true;
            }));

            while (!checkInfo)
            {
                yield return new WaitForEndOfFrame();
            }

            for (int i = 0; i < fileList.Count; i++)
            {
                totalSize += fileList[i].size;
            }


            //____________ Download Start
            progressString = string.Format("{0} {1:00}", LocalizationManager.GetTermTranslation("UI_downloading"), k + 1);
            assetLoaderText.text = progressString;

            downSpeedText.gameObject.SetActive(true);

            bool checkDown = false;

            StartCoroutine(Download_Data(string.Format("tm_flashcards_cn_0{0}", k + 1), output =>
            {
                checkDown = true;
            }));

            while (!checkDown)
            {
                long currentSize = 0;

                for (int i = 0; i < fileList.Count; i++)
                {
                    currentSize += fileList[i].size;
                }

                if (timerSpeedEllap < timerSpeed)
                {
                    timerSpeedEllap += Time.deltaTime;
                    speed += (lastSize - currentSize);
                }
                else
                {
                    float speedMultiplier = 1 / timerSpeed;
                    speed *= speedMultiplier;

                    timerSpeedEllap = 0;
                    if (speed <= 0)
                    {
                        downSpeedText.text = "Unpredictable";
                    }
                    else if (speed < 1024)
                    {
                        downSpeedText.text = string.Format("{0}/Bps", speed.ToString("####"));
                    }
                    else if (speed < 1048576)
                    {
                        downSpeedText.text = string.Format("{0}/Kbps", (speed / 1024).ToString("####.#"));
                    }
                    else
                    {
                        downSpeedText.text = string.Format("{0}/Mbps", (speed / 1048576).ToString("####.#"));
                    }
                    speed = 0;
                }

                lastSize = currentSize;

                mainProgress.fillAmount = (float)(totalSize - currentSize) / totalSize;
                assetLoaderText.text = string.Format("{0} :   {1:00.0}%", progressString, ((float)(totalSize - currentSize) / totalSize) * 100);

                yield return new WaitForEndOfFrame();
            }
            downSpeedText.gameObject.SetActive(false);

            yield return new WaitForEndOfFrame();


            //____________ Download File Extraction
            bool checkExtract = false;

            StartCoroutine(Download_Extract(string.Format("tm_flashcards_cn_0{0}", k + 1), output =>
            {
                checkExtract = true;
            }));

            while (!checkExtract)
            {
                yield return new WaitForEndOfFrame();
            }

            assetLoaderText.text = string.Empty;

            if (totalSize.Equals(0))
                ForceQuitDownload(false);
        }

        downSpeedText.transform.parent.gameObject.SetActive(false);
        //Debug.Log("Total downloaded file Size : " + totalSize);

        StartCoroutine(TargetDataSetting());

        savedDataSet = string.Empty;
        downloadInProgress = false;

        yield return null;
    }



    private IEnumerator TargetDataSetting()
    {
        PrefabShelter prefabShelter = FindObjectOfType<PrefabShelter>();

        Image prefabProgress = bookDownloadGO.transform.GetChild(0).GetComponent<Image>();
        prefabProgress.gameObject.SetActive(true);
        prefabProgress.fillAmount = 1;

        assetLoaderText.text = LocalizationManager.GetTermTranslation("UI_downSetPrefab");

#if UNITY_EDITOR
        string path = string.Format("file:///{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
#elif UNITY_ANDROID
        string path = string.Format("file://{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
#elif UNITY_IOS
        string path = string.Format("{0}/asset/TM_Flashcards_cn", Application.persistentDataPath);
#endif

        UnityWebRequest webr = UnityWebRequestAssetBundle.GetAssetBundle(path);
        webr.Send();

        while (!webr.isDone)
            yield return new WaitForEndOfFrame();

#if UNITY_ANDROID
        AssetBundle bundles = DownloadHandlerAssetBundle.GetContent(webr);
#else
        AssetBundle bundles = AssetBundle.LoadFromFile(path);
#endif
        for (int j = 0; j < aDSL.cntDataSet.Count; j++)
        {
            for (int i = 0; i < 100; i++)
            {
                int index = (j * 100) + i;

                if (!aDSL.tagmeTargets[index].Equals(string.Empty))
                {
                    AssetBundleRequest req = bundles.LoadAssetAsync<GameObject>(aDSL.tagmeTargets[index]);
                    prefabShelter.tmModel[index] = new TMModel((GameObject)req.asset, false);
                }
                else
                    prefabShelter.tmModel[index] = new TMModel(null, false);

                prefabProgress.fillAmount = 1 - ((index + 1f) / (aDSL.cntDataSet.Count * 100));
                yield return prefabProgress.fillAmount;
            }
        }
        bundles.Unload(false);

        Resources.UnloadUnusedAssets();

        aDSL.fileExist = true;


        Destroy(blocker);
        Resources.UnloadUnusedAssets();
        prefabShelter.nothingModel = false;

        yield return new WaitForEndOfFrame();

        canvasManager.PanelManager(true);
        assetLoaderText.text = string.Empty;
    }


    #region DOWNLOAD_DATA
    private IEnumerator Download_Information(string name, Action<bool> output)
    {
        //#if UNITY_IOS
        //        string path = string.Format("{0}{1}.zip", iosUrl, name);
        //#else
        string path = string.Format("{0}{1}.zip", pingUrl, name);
        //#endif

        UnityWebRequest req = UnityWebRequest.Get(path);
        req.method = "HEAD";
        req.Send();

        while (!req.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        while (downServerWaitEllap != 0)
        {
            req.Abort();

            req = UnityWebRequest.Get(req.url);
            req.Send();

            while (!req.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        long fileSize = 0;
        if (long.TryParse(req.GetResponseHeader("Content-Length"), out fileSize))
        {
            //Debug.Log("download file size : " + fileSize);
            fileList.Add(new FileList(name, fileSize));
        }

        output(true);

        yield return null;
    }

    private IEnumerator Download_Data(string name, Action<bool> output)
    {
        //#if UNITY_IOS
        //        string path = string.Format("{0}{1}.zip", iosUrl, name);
        //#else
        string path = string.Format("{0}{1}.zip", pingUrl, name);
        //#endif
        int index = fileList.FindIndex(find => find.name == name);
        long originalSize = fileList[index].size;

        UnityWebRequest req = new UnityWebRequest(path)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.Send();

        while (!req.isDone)
        {
            fileList[index].size = originalSize - (long)(originalSize * req.downloadProgress);

            yield return new WaitForEndOfFrame();
        }

        while (downServerWaitEllap != 0)
        {
            req.Abort();

            fileList[index].size = originalSize;

            req = UnityWebRequest.Get(req.url);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.Send();

            while (!req.isDone)
            {
                fileList[index].size = originalSize - (long)(originalSize * req.downloadProgress);

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }

        fileList[index].size = 0;

        string downloadPath = string.Format("{0}/{1}.zip", Application.persistentDataPath, name);
        File.WriteAllBytes(downloadPath, req.downloadHandler.data);

        output(true);

        yield return null;
    }

    #endregion     //DOWNLOAD_DATA


    private IEnumerator Download_Extract(string name, Action<bool> output)
    {
        string zipFile = string.Format("{0}/{1}.zip", Application.persistentDataPath, name);
        string location = Application.persistentDataPath;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        Directory.CreateDirectory(location);

        using (ZipFile zip = ZipFile.Read(zipFile))
        {
            zip.ExtractAll(location, ExtractExistingFileAction.OverwriteSilently);
        }

#elif UNITY_ANDROID
		using (AndroidJavaClass zipper = new AndroidJavaClass ("com.tsw.zipper")) {
			zipper.CallStatic ("unzip", zipFile, location);
		}
#elif UNITY_IOS
		unzip (zipFile, location);
#endif

        output(true);
        File.Delete(zipFile);

        yield return null;
    }


    public IEnumerator StartCoroutine_Queue(IEnumerator coroutine)
    {
        while (coroutines.Count > coroutineNumber)
        {
            for (int i = 0; i < coroutines.Count; i++)
            {
                if (coroutines[i] == null)
                {
                    coroutines.Remove(coroutines[i]);
                }

                if (!coroutines[i].MoveNext())
                {
                    coroutines.Remove(coroutines[i]);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        coroutines.Add(coroutine);
        StartCoroutine(coroutine);

        yield return null;
    }

    //private void ClearLog()
    //{
    //    Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
    //    Type type = assembly.GetType("UnityEditorInternal.LogEntries");
    //    MethodInfo method = type.GetMethod("Clear");
    //    method.Invoke(new object(), null);
    //}
}

[Serializable]
public class FileList
{
    public string name;
    public long size;

    public FileList(string _name, long _size)
    {
        name = _name;
        size = _size;
    }
}

[Serializable]
public class Exceptions
{
    public string name;
    public string type;
    public int book;

    public Exceptions(string _name, string _type, int _book)
    {
        name = _name;
        type = _type;
        book = _book;
    }
}