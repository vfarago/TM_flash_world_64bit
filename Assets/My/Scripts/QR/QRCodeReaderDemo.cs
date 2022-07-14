using UnityEngine;
using UnityEngine.UI;

public class QRCodeReaderDemo : MonoBehaviour
{
    public AccountManager accountManager;
    private IReader QRReader;
    //public Text resultText;
    public RawImage image;

    private bool firstRun = false;

    void Awake()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    // Use this for initialization
    public void Start()
    {
        QRReader = new QRCodeReader();
        QRReader.Camera.Play();

        QRReader.OnReady += StartReadingQR;

        QRReader.StatusChanged += QRReader_StatusChanged;
    }

    private void OnDisable()
    {
        if (QRReader != null)
            QRReader.Camera.Stop();
        //print("                          꺼!");
    }

    private void QRReader_StatusChanged(object sender, System.EventArgs e)
    {
        //resultText.text = "Status: " + QRReader.Status;

        if (!firstRun)
        {
            StartScanning();
            firstRun = true;
        }

        //Debug.Log(QRReader.Status);
    }

    private void StartReadingQR(object sender, System.EventArgs e)
    {
        image.transform.localEulerAngles = QRReader.Camera.GetEulerAngles();
        image.transform.localScale = QRReader.Camera.GetScale();
        image.texture = QRReader.Camera.Texture;

        //RectTransform rectTransform = image.GetComponent<RectTransform>();
        //float height = rectTransform.sizeDelta.x * (QRReader.Camera.Height / QRReader.Camera.Width);
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
    }

    // Update is called once per frame
    void Update()
    {
        if (QRReader == null)
        {
            return;
        }
        else
        {
            QRReader.Update();
        }
    }

    public void StartScanning()
    {
        if (QRReader == null)
        {
            Debug.LogWarning("No valid camera - Click Start");
            return;
        }

        if (!QRReader.Camera.IsPlaying())
            QRReader.Camera.Play();

        // Start Scanning
        QRReader.Scan((barCodeType, barCodeValue) =>
        {
            //Debug.Log("Found: [" + barCodeType + "] " + "<b>" + barCodeValue +"</b>");

            QRReader.Destroy();

            QRReader = new QRCodeReader();
            QRReader.Camera.Play();

            QRReader.OnReady += StartReadingQR;

            QRReader.StatusChanged += QRReader_StatusChanged;

            accountManager.QRButtonController(null, barCodeValue);

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        });
    }


    void OnApplicationFocus(bool pauseStatus)
    {
        if (pauseStatus)
        {
            QRReader.Camera.Stop();
        }
    }
}
