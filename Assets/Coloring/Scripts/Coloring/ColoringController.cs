using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace SJ.MathFun
{
    [Serializable]
    class ColoringData
    {
        public Byte r;
        public Byte g;
        public Byte b;
        public Byte a;
    }

    public class ColoringController : MonoBehaviour
    {
        public enum AppState
        {
            COLORING,
            PAGE_PICKER_ENABLED,
            PALETTE_OPENED
        }
        public List<Color32> ColorPalette;
        public RawImage rawImage;

        public Image whiteFade;
        public RectTransform screenshotFade;
        private readonly float flashView = 0.45f;
        private readonly float fadeView = 1f;
        private readonly float fadeDissapear = 0.25f;
        private readonly float fadeDecreasementMulti = 0.8f;
        private float fadeEllap;

        public Transform CrayonsContainerContent;

        public GameObject sketchSelector, colorPickerWrapper;
        public GameObject eraserWrapper, sharePalette, resetPopWrapper, popupWrapper;

        public TableViewController tbController;
        public ColorSelectionButton ColorButtonPrefab;
        public SelectColoringPageButton ColoringPageButonPrefab;
        public Camera MyCamera;


        private DrawableTextureContainer imageContainer;
        private Color32 selectedColor;
        private Color32 realColor;

        private bool doubleTap = false;
        private bool paletteOpen = false;
        private bool eraserOpen = false;
        private bool shareOpen = false;
        private bool popupOpen = false;

        public AppState state = AppState.COLORING;

        private float initialScale;
        public const float MIN_SCALE = 0.75f;
        public const float MAX_SCALE = 3.5f;

        public string currentImageName = "";
        private ColorPicker picker;

        public RawImage screenRaw;
        public Camera screenCamera;

        private Vector2 rawPositionRecord;
        private Vector2 rawSizeRecord;
        private Vector3 rawScaleRecord;
        private GameObject zoomActive;


        void OnApplicationQuit()
        {
            Serialize();
        }

        void Awake()
        {
            picker = colorPickerWrapper.GetComponentInChildren<ColorPicker>();

            selectedColor = Color.yellow;
            realColor = Color.yellow;
            Deserialize();

            if (selectedColor == Color.white) //if the selected color is white, people can get confused.
                selectedColor = Color.yellow;

            picker.CurrentColor = selectedColor;
        }


        void Start()
        {
            colorPickerWrapper.SetActive(false);
            InitDefaultColorSelector();

            Input.simulateMouseWithTouches = true;

            picker.onValueChanged.AddListener(color =>
            {
                SetColor(color);
            });

            //show sketchSelector first

            state = AppState.PAGE_PICKER_ENABLED;

        }

        private void OnEnable()
        {
            initialScale = rawImage.transform.localScale.x;
            rawPositionRecord = rawImage.transform.localPosition;
            rawSizeRecord = rawImage.GetComponent<RectTransform>().sizeDelta;
            rawScaleRecord = rawImage.transform.localScale;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnClicHomeButton();
            }

            eraserOpen = eraserWrapper.activeSelf;
            shareOpen = sharePalette.activeSelf;
            popupOpen = (popupWrapper.activeSelf || resetPopWrapper.activeSelf);

            //if(Input.GetButtonDown("Fire1"))
            if (Input.touchCount == 2)
            {
                if (zoomActive == null)
                {
                    doubleTap = true;

                    zoomActive = new GameObject("sizeCanvas", typeof(RectTransform));
                    RectTransform rect = zoomActive.GetComponent<RectTransform>();
                    rect.parent = rawImage.transform.parent;
                    rect.localScale = rawImage.transform.localScale;
                    rect.SetSiblingIndex(0);

                    Vector2 pos = (Input.touches[0].position + Input.touches[1].position) / 2;

                    Ray hit = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitted;

                    if (Physics.Raycast(hit, out hitted, 1000f, LayerMask.GetMask("UI")))
                    {
                        rect.localPosition = hitted.point;
                    }

                    rawImage.transform.parent = rect;
                }

                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
                Vector2 prevPos2 = touch2.position - touch2.deltaPosition;

                Vector2 touch = (touch1.position + touch2.position) / 2;
                Vector2 prevPos = (prevPos1 + prevPos2) / 2;

                float distance = (touch1.position - touch2.position).magnitude;
                float preDistance = (prevPos1 - prevPos2).magnitude;

                float magnitudeScale = distance - preDistance;

                float currentScale = zoomActive.transform.localScale.x;

                if (magnitudeScale < -5)
                {
                    if (currentScale + magnitudeScale * 0.0025f >= MIN_SCALE)
                    {
                        currentScale += magnitudeScale * 0.0025f;
                    }
                    else
                    {
                        currentScale = MIN_SCALE;
                    }

                }
                else if (magnitudeScale > 5)
                {
                    if (currentScale + magnitudeScale * 0.0025f <= MAX_SCALE)
                    {
                        currentScale += magnitudeScale * 0.0025f;
                    }
                    else
                    {
                        currentScale = MAX_SCALE;
                    }
                }
                else
                {
                    zoomActive.GetComponent<RectTransform>().offsetMin += (touch - prevPos);
                    zoomActive.GetComponent<RectTransform>().offsetMax += (touch - prevPos);
                }

                zoomActive.GetComponent<RectTransform>().localScale = new Vector3(currentScale, currentScale, 1);
            }
            else
            {
                if (zoomActive != null)
                {
                    rawImage.transform.parent = zoomActive.transform.parent;
                    rawImage.transform.SetSiblingIndex(0);

                    Destroy(zoomActive);
                }

                if (Input.touchCount == 0)
                {
                    doubleTap = false;
                }
            }
        }

        public void OnClickCanvas()
        {
            OnClick(Input.mousePosition);
        }

        private void OnClick(Vector3 pos)
        {
            if (paletteOpen || eraserOpen || shareOpen || popupOpen || doubleTap)
            {
                return;
            }

            if (state == AppState.COLORING)
                Click(pos);
        }


        public void InitDefaultColorSelector()
        {
            foreach (Color32 c in ColorPalette)
            {
                ColorSelectionButton obj = Instantiate(ColorButtonPrefab, CrayonsContainerContent, false);
                obj.Init(this, c);
            }
        }


        public void InitWithNamedColorPage(string name)
        {
            currentImageName = name;
            //print("InitWithNamedColorPage currentImageName:" + currentImageName);

            string savedImagePath = string.Format("{0}/drawImage/saved-{1}.png", Application.persistentDataPath, name);
            if (File.Exists(savedImagePath))
            {
                LoadSavedDrawing(savedImagePath);
            }
            else
            {
                //20200206 직접로드로 변경
                Texture2D img = Resources.Load<Texture2D>(string.Format("coloring/{0}", name));
                imageContainer = new DrawableTextureContainer(img, true, false);
                rawImage.texture = imageContainer.getTexture();
            }

            rawImage.transform.localPosition = rawPositionRecord;
            rawImage.GetComponent<RectTransform>().sizeDelta = rawSizeRecord;
            rawImage.transform.localScale = rawScaleRecord;
        }


        private IEnumerator Share_Save(string path, bool isShare)
        {
            RenderTexture tex = new RenderTexture(rawImage.texture.width, rawImage.texture.height, 16);
            Graphics.Blit(rawImage.texture, tex);
            screenRaw.texture = tex;

            RawImage[] raws = screenCamera.GetComponentsInChildren<RawImage>();
            foreach (RawImage raw in raws)
            {
                switch (raw.name)
                {
                    case "TopCell":
                        raw.SizeToParent(1f, 1f);
                        break;

                    case "BotCell":
                        raw.SizeToParent(1.2f, 1.2f);
                        break;

                    case "MiddleCell":
                        raw.SizeToParent(0.95f, 0.95f);
                        break;
                }
            }

            RenderTexture screen = new RenderTexture(Screen.width, Screen.height, 16);
            screenCamera.targetTexture = screen;
            Texture2D screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            screenCamera.Render();
            RenderTexture.active = screen;
            screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(screen);
            byte[] screenByte = screenTex.EncodeToPNG();

            if (isShare)
            {
                File.WriteAllBytes(path, screenByte);
            }
            else
            {
                NativeGallery.SaveImageToGallery(screenByte, "TagMe", path + ".png", null);
            }

            screenshotFade.gameObject.SetActive(true);
            whiteFade.gameObject.SetActive(true);

            fadeEllap = fadeDissapear + fadeView;
            Texture2D load = new Texture2D(Screen.width, Screen.height);
            load.LoadImage(screenByte);
            screenshotFade.GetComponent<Image>().sprite = Sprite.Create(load, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
            screenshotFade.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            screenshotFade.GetComponent<Image>().preserveAspect = true;
            screenshotFade.localScale = Vector2.one;

            while (fadeEllap != 0)
            {
                if (fadeEllap > 0)
                {
                    fadeEllap -= Time.deltaTime;
                    if (fadeEllap > fadeDissapear)
                    {
                        if (fadeView > (fadeDissapear + fadeView - flashView))
                        {
                            whiteFade.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), (fadeEllap - (fadeDissapear + fadeView - flashView)) / flashView);
                        }
                        else
                        {
                            whiteFade.color = new Color(1, 1, 1, 0);
                        }
                    }
                    else
                    {
                        screenshotFade.GetComponent<Image>().color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), fadeEllap / fadeDissapear);
                        screenshotFade.localScale = Vector2.Lerp(new Vector2(fadeDecreasementMulti, fadeDecreasementMulti), Vector2.one, fadeEllap / fadeDissapear);
                    }
                }
                else if (fadeEllap < 0)
                {
                    fadeEllap = 0;
                    screenshotFade.localScale = Vector2.one;
                    screenshotFade.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    screenshotFade.GetComponent<Image>().sprite = null;
                    whiteFade.color = new Color(1, 1, 1, 0);
                }

                yield return new WaitForEndOfFrame();
            }

            screenshotFade.gameObject.SetActive(false);
            whiteFade.gameObject.SetActive(false);

            if (isShare)
            {
                new NativeShare().AddFile(path).SetTitle("Share image to...").Share();
            }
            else
            {
                sharePalette.SetActive(false);
                popupWrapper.SetActive(true);

                float waitTime = 3f;
                //float waitTimeEllap = waitTime;

                while (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }

                if (popupWrapper.activeSelf)
                {
                    SJUtility.ShowUI(this, 0, 0.2f, popupWrapper, false);
                }
            }

            yield return null;
        }

        private void SaveDrawing()
        {
            //print("SaveDrawing currentImageName: " + currentImageName);
            Texture2D texture2D = (Texture2D)rawImage.texture;
            string savedImagePath = string.Format("{0}/drawImage/saved-{1}.png", Application.persistentDataPath, currentImageName);
            File.WriteAllBytes(savedImagePath, texture2D.EncodeToPNG());
        }


        private void LoadSavedDrawing(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(1024, 768, TextureFormat.RGBA4444, false);

            if (tex.LoadImage(fileData))
            { //..this will auto-resize the texture dimensions.
                //print("well loaded");
                rawImage.texture = tex;

                imageContainer = new DrawableTextureContainer(tex, false, false);
                //rawImage.texture = imageContainer.getTexture ();

            }
            else
            {
                print("error while loading the image");
            }
        }



        public void SetColor(Color32 _color)
        {
            //Debug.Log ("Color changed to: " + _color);
            selectedColor = _color;
            picker.CurrentColor = selectedColor;

            Color floatColor = selectedColor;

            float realR = floatColor.r + ((1f - floatColor.r) * (1f - floatColor.a));
            float realG = floatColor.g + ((1f - floatColor.g) * (1f - floatColor.a));
            float realB = floatColor.b + ((1f - floatColor.b) * (1f - floatColor.a));

            realColor = new Color(realR, realG, realB, 1f);

            if (realColor == Color.black)
            {
                realColor = new Color32(1, 1, 1, 255);
            }
        }


        void Click(Vector3 position)
        {

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.GetComponent<RectTransform>(), position, MyCamera, out localCursor))
                return;
            else
            {
                localCursor /= initialScale;
                localCursor.x += imageContainer.getWidth() / 2;
                localCursor.y += imageContainer.getHeight() / 2;

                bool ret = imageContainer.PaintBucketToolWithHistory((int)localCursor.x, (int)localCursor.y, realColor);

                rawImage.texture = imageContainer.getTexture();
            }

        }

        //색칠 초기화 버튼 터치
        private void OnResetPressed()
        {
            eraserWrapper.SetActive(false);
            resetPopWrapper.SetActive(true);

            Button[] buttons = resetPopWrapper.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.onClick.RemoveAllListeners();

                switch (button.name)
                {
                    case "btn_yes":
                        button.onClick.AddListener(() => OnResetConfirm());

                        break;

                    case "btn_no":
                        button.onClick.AddListener(() => SJUtility.ShowUI(this, 0, 0.2f, resetPopWrapper, false));
                        break;
                }
            }
        }

        private void OnResetConfirm()
        {
            SJUtility.ShowUI(this, 0, 0.2f, resetPopWrapper, false);

            Texture2D img = Resources.Load<Texture2D>(string.Format("coloring/{0}", currentImageName));
            imageContainer = new DrawableTextureContainer(img, true, false);
            rawImage.texture = imageContainer.getTexture();
        }

        //공유하기 버튼 터치
        private void OnShare(Button button)
        {
            string path = string.Empty;

            switch (button.name)
            {
                case "btn_save":
                    string name = string.Format("{0}_{1}{2:00}{3:00}_{4:00}{5:00}{6:00}", currentImageName, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    StartCoroutine(Share_Save(name, false));
                    break;

                case "btn_share":
                    path = string.Format("{0}/drawImage/sharing-{1}.png", Application.persistentDataPath, currentImageName);
                    StartCoroutine(Share_Save(path, true));
                    break;
            }
        }


        #region UI element event handlers

        public void OnClickSharingButton()
        {
            if (sharePalette.activeSelf)
            {
                Button[] buttons = sharePalette.GetComponentsInChildren<Button>();
                foreach (Button button in buttons)
                {
                    button.onClick.RemoveAllListeners();
                }
                SJUtility.ShowUI(this, 0, 0.2f, sharePalette, false);
            }
            else
            {
                SJUtility.ShowUI(this, 0, 0.2f, sharePalette, true);
                Button[] buttons = sharePalette.GetComponentsInChildren<Button>();
                foreach (Button button in buttons)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnShare(button));
                }
            }
        }

        //홈버튼 터치 → 색칠하기중이면 저장 후 홈으로 이동
        public void OnClicHomeButton()
        {
            if (state == AppState.COLORING)
            {
                state = AppState.PAGE_PICKER_ENABLED;
                SaveDrawing();
            }
            OnClicRootHomeButton();
        }

        public void OnClicRootHomeButton()
        {
            // ______________________________________________________ Go back to root scene ________________________________________________________________
            LoadSceneManager.instance.ChangeScene(false, false);
        }

        public void OnClickPaletteButton()
        {
            TogglePalette();
        }

        public void TogglePalette()
        {
            if (paletteOpen)
                SJUtility.ShowUI(this, 0, 0.2f, colorPickerWrapper.gameObject, false);
            else
                SJUtility.ShowUI(this, 0, 0.2f, colorPickerWrapper.gameObject, true);
            paletteOpen = !paletteOpen;
        }

        public void OpenPagesBrowser()
        {
            if (state == AppState.COLORING)
            {

                state = AppState.PAGE_PICKER_ENABLED;
                SaveDrawing();
                SJUtility.ShowUI(this, 0, 0.2f, sketchSelector, true);

            }
        }

        public void ClosePagesBrowser()
        {
            if (state == AppState.PAGE_PICKER_ENABLED)
            {
                state = AppState.COLORING;
                SJUtility.ShowUI(this, 0, 0.2f, sketchSelector, false);
            }
        }

        public void OnClickEraserButton()
        {
            SetColor(Color.white);
            SJUtility.ShowUI(this, 0, 0.2f, eraserWrapper, true);
            Button button = eraserWrapper.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => OnResetPressed());
        }



        public void OnClickTableViewCell(string name)
        {
            InitWithNamedColorPage(name);
            ClosePagesBrowser();
        }


        public void OnClickRedoButton()
        {
            imageContainer.Redo();
            rawImage.texture = imageContainer.getTexture();
        }



        public void OnClickUndoButton()
        {
            imageContainer.Undo();
            rawImage.texture = imageContainer.getTexture();
        }



        #endregion


        #region AudioState Serialization

        private void Serialize()
        {
            ColoringData data = new ColoringData();
            data.r = selectedColor.r;
            data.g = selectedColor.g;
            data.b = selectedColor.b;
            data.a = selectedColor.a;

            FileStream fs = new FileStream(Application.persistentDataPath + "/coloring.dat", FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, data);
            }
            catch (Exception e)
            {
                print("Serialize Exception:" + e.Message);
            }
            finally
            {
                fs.Close();
            }
            print("coloring data serialized");
        }



        private void Deserialize()
        {
            ColoringData data = null;

            string path = Application.persistentDataPath + "/coloring.dat";
            if (File.Exists(path))
            {
                print("The file coloring.dat exists at " + path);
                FileStream fs = new FileStream(path, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = (ColoringData)formatter.Deserialize(fs);
                    selectedColor.r = data.r;
                    selectedColor.g = data.g;
                    selectedColor.b = data.b;
                    selectedColor.a = data.a;

                }
                catch (Exception e)
                {
                    selectedColor = Color.yellow;  // it doesn't seem to work
                    print("Deserialize Exception:" + e.Message);
                }
                finally
                {
                    fs.Close();
                }

            }
            else
            {
                //print("no file at " + path);
            }
        }


        #endregion
    }
}