using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class QRPanelManager : MonoBehaviour
{
    public GameObject qrInfoP, deviceInfoP;
    public Toggle qrToggle;
    public bool isInfo;

    Font changefont;

    private void OnEnable()
    {
        Button[] qrInfoButton = GetComponentsInChildren<Button>();
        foreach (Button btn in qrInfoButton)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => QRInfoButtonController(btn));
        }
    }

    private void OnDisable()
    {
        isInfo = false;
        gameObject.SetActive(false);
        qrInfoP.SetActive(true);
        deviceInfoP.SetActive(true);

        Button[] qrInfoButton = GetComponentsInChildren<Button>();
        foreach (Button btn in qrInfoButton)
            btn.onClick.RemoveAllListeners();
    }

    private void QRInfoButtonController(Button btn)
    {
        switch (btn.name)
        {
            case "BGButton":
                gameObject.SetActive(false);

                break;
            case "btn_close":
                if (GetComponent<QRPanelManager>().qrInfoP.activeSelf)
                {
                    QRInfoLocal(false);
                }
                else
                    gameObject.SetActive(false);

                break;
        }
    }

    //사용자 등급 Normal
    public void QRInfoLocal(bool on)
    {
        if (on)
            Localization(qrInfoP);

        gameObject.SetActive(on);
        qrInfoP.SetActive(on);

        deviceInfoP.SetActive(false);
        isInfo = on;
    }

    //Membership가입자
    public void DeviceInfoLocal(bool on)
    {
        if (on)
            Localization(deviceInfoP);

        gameObject.SetActive(on);
        deviceInfoP.SetActive(on);

        qrInfoP.SetActive(false);
        isInfo = on;
    }

    private void Localization(GameObject go)
    {
        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

        Text[] goText = go.GetComponentsInChildren<Text>();
        foreach (Text txt in goText)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;
        }
        qrToggle.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_QRToggleText");
        qrToggle.GetComponentInChildren<Text>().font = changefont;
    }
}
