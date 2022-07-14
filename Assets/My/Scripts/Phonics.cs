using I2.Loc;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Phonics : MonoBehaviour
{
    public string targetName { get; set; }
    public bool isFreeModel;

    public RawImage image; //Video Play Panel
    public Button recordButton, sentenceButton, exitButton;
    public Text targetWord, targetPron, recordText, sentenceText; //Phonics English, Phonics pronunciation
    public bool recordExist;

    private float phonicsLength, globalLength, recordLength, sentenceLength;
    private int recordingLength;
    private bool recording = false;
    private bool isSentence = false;

    private IEnumerator phonicsPlayer;
    private VideoPlayer videoPlayer;
    private AudioSource audioSource, recordSource, globalAudio, engAudio;
    private AudioClip audioClip, recordClip;
    private float timeCnt = 0;
    private string sentence;

    CheckCode checkCode;
    CanvasManager canvasManager;

    AssetBundle audioBundle;
    AssetBundle videoBundle;

    #region AWAKE_and_DISABLE
    private void Awake()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        checkCode = FindObjectOfType<CheckCode>();
        canvasManager.phonics = this;

        recordButton.onClick.AddListener(() => RecordController());
        sentenceButton.onClick.AddListener(() => SentenceController());
        exitButton.onClick.AddListener(() => canvasManager.ChoiceControll());

        canvasManager.btn_scan.SetActive(false);
    }

    private void OnDisable()
    {
        if (audioBundle != null) audioBundle.Unload(true);
        if (videoBundle != null) videoBundle.Unload(true);
        LocalizationManager.CurrentLanguage = canvasManager.ui_CurrentLang;
        sentence = string.Empty;

        Resources.UnloadUnusedAssets();

        recordButton.onClick.RemoveAllListeners();
        sentenceButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();

        canvasManager.btn_scan.SetActive(true);
    }
    #endregion

    void Start()
    {
        Application.runInBackground = true;

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        globalAudio = gameObject.AddComponent<AudioSource>();
        recordSource = gameObject.AddComponent<AudioSource>();
        engAudio = gameObject.AddComponent<AudioSource>();

        videoPlayer.playOnAwake = false;
        globalAudio.playOnAwake = false;
        audioSource.playOnAwake = false;
        recordSource.playOnAwake = false;
        engAudio.playOnAwake = false;

        StartCoroutine(ChangeText(true));

        //한번 인식한 타겟 이름 저장
        bool isInclude = false;
        foreach (string st in checkCode.objName)
        {
            if (st.Equals(targetName))
                isInclude = true;
        }
        if (!isInclude)
        {
            checkCode.objName.Add(targetName);
            checkCode.SaveOnTrackingObject();
        }
    }

    //Phonics 시작시 녹음파일 있는지 체크
    private void CurrentSelectRecord()
    {
        string recordFilePath = string.Format("{0}/RecordedAudio/{1}.wav", Application.persistentDataPath, targetName);

        if (File.Exists(recordFilePath))
        {
            recordExist = true;
            canvasManager.btn_myVoice.GetComponent<Image>().color = Color.white;
        }
        else
        {
            recordExist = false;
            canvasManager.btn_myVoice.GetComponent<Image>().color = Color.gray;
        }
    }

    //__________Phonics 최초 실행 & Localize Button Click
    public IEnumerator ChangeText(bool isFirst)
    {
        LocalizeWord();

        if (LocalizationManager.CurrentLanguage.Equals("eng") || LocalizationManager.CurrentLanguage.Equals("kor")
            || LocalizationManager.CurrentLanguage.Equals("chn"))
        {
            sentence = LocalizationManager.GetTermTranslation(string.Format("sent_{0}", targetName));
        }
        yield return targetWord.text;

        if (isFirst)
        {
            CurrentSelectRecord();
            canvasManager.LocalPanelInitialSetting(LocalizationManager.CurrentLanguage);

            PlayPhonics();
        }
        else
        {
            //__________Only Global Audio
            if (audioSource.isPlaying || recordSource.isPlaying || videoPlayer.isPlaying || globalAudio.isPlaying || engAudio.isPlaying)
                StopPhonics(false);

            if (recording)
            {
                StopRecord();
                recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_rec(95x95)");
            }
            else
            {
                phonicsPlayer = LoadPhonics(false, true, false, false);
                StartCoroutine(phonicsPlayer);
            }
        }
    }

    private void LocalizeWord()
    {
        isSentence = false;

        string aa = LocalizationManager.GetTermTranslation(targetName);
        string bb = LocalizationManager.GetTermTranslation(targetName + "_pron");


        if (LocalizationManager.CurrentLanguage.Equals("are") || LocalizationManager.CurrentLanguage.Equals("heb"))
            targetWord.text = LocalizationManager.FixRTL_IfNeeded(aa);
        else
            targetWord.text = aa;

        targetPron.text = bb;

        sentenceText.text = canvasManager.phoSentenceString;
        sentenceText.font = canvasManager.localizeFont;
    }

    //__________SentenceButton Click
    private void SentenceController()
    {
        isSentence = !isSentence;
        if (audioSource.isPlaying || recordSource.isPlaying || videoPlayer.isPlaying || globalAudio.isPlaying || engAudio.isPlaying)
            StopPhonics(false);

        if (isSentence)
        {
            targetWord.text = sentence;
            targetPron.text = string.Empty;

            sentenceText.text = canvasManager.phoWordString;
            sentenceText.font = canvasManager.localizeFont;
            phonicsPlayer = LoadPhonics(false, false, false, false);
            StartCoroutine(phonicsPlayer);
        }
        else
        {
            StartCoroutine(ChangeText(false));
        }
    }

    //__________Play Phonics record audio
    public void PlayRecPhonics()
    {
        if (isSentence)
            LocalizeWord();

        if (audioSource.isPlaying || recordSource.isPlaying || videoPlayer.isPlaying || globalAudio.isPlaying || engAudio.isPlaying)
            StopPhonics(true);

        if (recording)
        {
            StopRecord();
            recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_rec(95x95)");
        }
        else
        {
            phonicsPlayer = LoadPhonics(true, false, false, false);
            StartCoroutine(phonicsPlayer);
            canvasManager.localizePhonicsImage.sprite = Resources.Load<Sprite>("Sprites/Localize/btn_language_custom(70x70)");
        }
    }

    //__________Play Phonics eng & global audio + Replay
    public void PlayPhonics()
    {
        if (isSentence)
            LocalizeWord();

        bool isEng = (LocalizationManager.CurrentLanguage.Equals("eng") ? isEng = true : isEng = false);

        if (audioSource.isPlaying || recordSource.isPlaying || videoPlayer.isPlaying || globalAudio.isPlaying || engAudio.isPlaying)
            StopPhonics(true);

        if (recording)
        {
            StopRecord();
            recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_rec(95x95)");
        }
        else
        {
            phonicsPlayer = LoadPhonics(false, false, false, isEng);
            StartCoroutine(phonicsPlayer);
        }
    }

    //__________Recording Button click
    public void RecordController() //true = Start Record
    {
        if (isSentence)
            LocalizeWord();

        if (audioSource.isPlaying || recordSource.isPlaying || videoPlayer.isPlaying || globalAudio.isPlaying || engAudio.isPlaying)
            StopPhonics(true);

        if (recording)
        {
            StopRecord();
            recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_rec(95x95)");
        }
        else
        {
            timeCnt = 0;
            phonicsPlayer = LoadPhonics(false, false, true, false);
            StartCoroutine(phonicsPlayer);
            recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_stop(95x95)");
        }
    }


    //__________Play Video, Audio and Recording
    IEnumerator LoadPhonics(bool isRecordPlay, bool onlyGlobal, bool startRecord, bool isEng)
    {
        //__________Play Sentence Audio
        if (isSentence)
        {
            if (isFreeModel)
            {
                audioSource.clip = FindFreeAudClip(3);
                sentenceLength = audioSource.clip.length;
            }
            else
            {
                audioSource.clip = FindAudClip(3);

                sentenceLength = (float)audioSource.clip.samples / audioSource.clip.frequency;
            }
            audioSource.Play();

            yield return new WaitForSeconds(sentenceLength + 0.2f);
            audioSource.Stop();
            audioSource.clip = null;

            yield break;
        }

        //__________Play Only GlobalAudio
        if (onlyGlobal)
        {
            if (isFreeModel)
            {
                globalAudio.clip = FindFreeAudClip(0);
                globalLength = globalAudio.clip.length;
            }
            else
            {
                globalAudio.clip = FindAudClip(0);

                if (globalAudio.clip.frequency > 30000)
                    globalLength = (float)globalAudio.clip.samples / globalAudio.clip.frequency;
                else
                    globalLength = (float)globalAudio.clip.samples / globalAudio.clip.frequency;
            }
            globalAudio.Play();

            yield return new WaitForSeconds(globalLength + 0.2f);
            globalAudio.Stop();
            globalAudio.clip = null;
            yield break;
        }

        if (startRecord)
        {
            yield return null;
        }
        else if (isRecordPlay)
        {
            //__________RecordAudioClip Setting
            WWW audioRecord = new WWW(string.Format("file:///{0}/RecordedAudio/{1}.wav", Application.persistentDataPath, targetName));
            yield return audioRecord;
            recordSource.clip = audioRecord.GetAudioClip();
            recordLength = recordSource.clip.length;
        }
        else
        {
            //__________AudioClip & GlobalAudioClip Setting
            if (isFreeModel)
            {
                audioSource.clip = FindFreeAudClip(1);
                phonicsLength = audioSource.clip.length;

                globalAudio.clip = FindFreeAudClip(0);
                globalLength = globalAudio.clip.length;
            }
            else
            {
                audioSource.clip = FindAudClip(1);

                //if (audioFile.GetAudioClip().frequency > 30000)
                phonicsLength = (float)audioSource.clip.samples / audioSource.clip.frequency;
                //else
                //    phonicsLength = (float)audioSource.clip.samples / audioSource.clip.frequency / audioSource.clip.channels;

                globalAudio.clip = FindAudClip(0);

                if (globalAudio.clip.frequency > 30000)
                    globalLength = (float)globalAudio.clip.samples / globalAudio.clip.frequency;
                else
                    globalLength = (float)globalAudio.clip.samples / globalAudio.clip.frequency;
            }
        }

        //__________VideoClip Setting
        if (isFreeModel)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = FindFreeVidClip();
        }
        else
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = FindVidClip();
        }
        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        image.texture = videoPlayer.texture;
        videoPlayer.Play();

        if (startRecord)
        {
            //__________Start Recording
            recording = true;
            recordingLength = Mathf.CeilToInt(phonicsLength * 2);

            recordClip = Microphone.Start(null, false, recordingLength, 44100);

            yield return new WaitForSeconds(recordingLength);
            videoPlayer.Stop();
            StopRecord();
        }
        else
        {
            //__________Play Record or Phonics Audio
            if (isRecordPlay)
            {
                recordSource.Play();
                yield return new WaitForSeconds(recordLength);
            }
            else
            {
                audioSource.Play();

                yield return new WaitForSeconds(phonicsLength + 0.2f);
                audioSource.Stop();

                float waitTime;

                if (isFreeModel)
                {
                    engAudio.clip = FindFreeAudClip(2);
                    waitTime = engAudio.clip.length;
                }
                else
                {
                    engAudio.clip = FindAudClip(2);

                    //if (engFile.GetAudioClip().frequency > 30000)
                    waitTime = (float)engAudio.clip.samples / engAudio.clip.frequency;
                    //else
                    //    waitTime = (float)engAudio.clip.samples / engAudio.clip.frequency / engAudio.clip.channels;
                }
                engAudio.Play();

                yield return new WaitForSeconds(waitTime + 0.2f);

                if (!isEng)
                {
                    engAudio.Stop();

                    globalAudio.Play();
                    yield return new WaitForSeconds(globalLength);
                }
            }
            StopPhonics(true);
        }
    }

    AudioClip FindAudClip(int isWord)
    {
        AudioClip clip = null;
        //언어
        string lang = "";
        switch (isWord)
        {
            case 0:
                lang = LocalizationManager.CurrentLanguage;
                break;
            case 1:
                lang = "word";
                break;
            case 2:
                lang = "eng";
                break;
            case 3:
                lang = "sent";
                break;
        }
        //파일이름
        string target = string.Format("{0}_{1}", lang, targetName);
        string dir = string.Format("{0}/audios/tagme3d_card_audio_unfree", Application.persistentDataPath);

        //책번호로 오디오 클립 에셋번들 찾기
        if (File.Exists(dir))
        {
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(dir);

            audioBundle = req.assetBundle;
            clip = audioBundle.LoadAsset<AudioClip>(target);
            audioBundle.Unload(false);
        }
        return clip;
    }

    //VideoClip FindVidClip(string str)
    VideoClip FindVidClip()
    {
        VideoClip clip = null;
        string dir = string.Format("{0}/videos/tagme3d_card_video_unfree", Application.persistentDataPath);

        //책번호로 오디오 클립 에셋번들 찾기
        if (File.Exists(dir))
        {
            AssetBundleCreateRequest req = null;

            if (videoBundle == null)
                req = AssetBundle.LoadFromFileAsync(dir);

            if (videoPlayer.clip == null)
            {
                if (videoBundle == null)
                    videoBundle = req.assetBundle;

                clip = videoBundle.LoadAsset<VideoClip>(string.Format("{0}", targetName));
            }
            else clip = videoPlayer.clip;
            //bundle.Unload(false);
        }
        return clip;
    }
    AudioClip FindFreeAudClip(int isWord)
    {
        AudioClip clip = null;
        string lang = "";
        switch (isWord)
        {
            case 0:
                lang = LocalizationManager.CurrentLanguage;
                break;
            case 1:
                lang = "word";
                break;
            case 2:
                lang = "eng";
                break;
            case 3:
                lang = "sent";
                break;
        }
        string target = string.Format("{0}_{1}", lang, targetName);
        string dir = string.Format("{0}/audios/tagme3d_card_audio_free", Application.persistentDataPath);

        if (File.Exists(dir))
        {
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(dir);

            audioBundle = req.assetBundle;
            clip = audioBundle.LoadAsset<AudioClip>(target);
            audioBundle.Unload(false);
        }
        return clip;
    }

    VideoClip FindFreeVidClip()
    {
        VideoClip clip = null;
        string dir = string.Format("{0}/videos/tagme3d_card_video_free", Application.persistentDataPath);

        //책번호로 오디오 클립 에셋번들 찾기
        if (File.Exists(dir))
        {
            AssetBundleCreateRequest req = null;

            if (videoBundle == null)
                req = AssetBundle.LoadFromFileAsync(dir);

            if (videoPlayer.clip == null)
            {
                if (videoBundle == null)
                    videoBundle = req.assetBundle;

                clip = videoBundle.LoadAsset<VideoClip>(string.Format("{0}", targetName));
            }
            else clip = videoPlayer.clip;
            //bundle.Unload(false);
        }
        return clip;
    }





    #region STOP_METHOD
    private void StopPhonics(bool isStopVideo)
    {
        StopAllCoroutines();
        phonicsPlayer = null;

        if (isStopVideo)
        {
            if (!videoPlayer.isPaused)
                videoPlayer.Stop();
            //videoPlayer.clip = null;
        }

        audioSource.Stop();
        audioSource.clip = null;

        globalAudio.Stop();
        globalAudio.clip = null;

        recordSource.Stop();
        recordSource.clip = null;

        engAudio.Stop();
        engAudio.clip = null;
    }

    private void StopRecord()
    {
        recording = false;
        Microphone.End(null);
        SavWav.Save(string.Format("{0}/RecordedAudio/{1}", Application.persistentDataPath, targetName), recordClip);

        StopAllCoroutines();
        recordText.text = string.Empty;
        recordExist = true;
        PlayRecPhonics();
        recordButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Scan/btn_rec(95x95)");

        CurrentSelectRecord();
        canvasManager.LocalPanelInitialSetting("custom");
    }
    #endregion   //STOP_METHOD

    private void Update()
    {
        if (recording)
        {
            timeCnt += Time.deltaTime;
            recordText.text = string.Format("{0:00.0} / {1:00.0}s", timeCnt, recordingLength);
        }
    }
}
