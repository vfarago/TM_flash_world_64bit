using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public GameObject yesButton, noButton, okButton;
    public Text failText;
    public GameObject localizationGo, resendBtn;

    Font changefont;

    private void OnDisable()
    {
        if (resendBtn != null)
            resendBtn.SetActive(false);
    }

    public void PopupReady()
    {
        Localization(localizationGo);
    }

    private void Localization(GameObject go)
    {
        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

        Text[] goText = go.GetComponentsInChildren<Text>();
        foreach (Text txt in goText)
        {
            if (!txt.name.Contains("noLocal_"))
            {
                txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
                txt.font = changefont;
            }
        }

        if (failText != null)
        {
            failText.text = LocalizationManager.GetTermTranslation("UI_" + failText.name);
        }

        if (resendBtn != null)
            resendBtn.SetActive(false);
    }


    //로그아웃-기기해제시작 = true, 로그아웃-기기해제성공 = false
    public void PanelButtonSetting(bool on, string localKey)
    {
        yesButton.SetActive(on);
        noButton.SetActive(on);
        okButton.SetActive(!on);

        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        failText.text = LocalizationManager.GetTermTranslation(string.Format("UI_{0}", localKey));

        if (on)
        {
            yesButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_yes");
            yesButton.GetComponentInChildren<Text>().font = changefont;
            noButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_no");
            noButton.GetComponentInChildren<Text>().font = changefont;
        }
        else
        {
            okButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_ok");
            okButton.GetComponentInChildren<Text>().font = changefont;
        }
    }


    //ToastPanel Method
    public void ToastMessage(bool connectOn, string localKey)
    {
        localizationGo.transform.GetChild(1).gameObject.SetActive(connectOn);

        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

        Text[] goText = GetComponentsInChildren<Text>();
        foreach (Text txt in goText)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;
        }

        PanelButtonSetting(connectOn, localKey);
    }
}
