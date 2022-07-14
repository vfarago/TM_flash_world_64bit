using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ToastMsgManager : MonoBehaviour
{
    public Button yesButton, noButton, okButton;
    public GameObject failText;

    FileDownloader fileDownloader;
    Font changefont;
    string dataSetName;

    #region ENABLE_DISABLE
    private void Awake()
    {
        fileDownloader = FindObjectOfType<FileDownloader>();
    }

    private void OnEnable()
    {
        yesButton.onClick.AddListener(() => ClickBtn(yesButton));
        noButton.onClick.AddListener(() => ClickBtn(noButton));
        okButton.onClick.AddListener(() => ClickBtn(okButton));
    }

    private void OnDisable()
    {
        yesButton.onClick.RemoveListener(() => ClickBtn(yesButton));
        noButton.onClick.RemoveListener(() => ClickBtn(noButton));
        okButton.onClick.RemoveListener(() => ClickBtn(okButton));
    }
    #endregion

    private void ClickBtn(Button bb)
    {
        switch (bb.name)
        {
            case "btn_yes":
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    ToastMessage("connectFail", "1", false);
                }
                else
                {
                    fileDownloader.OnClickYes(dataSetName);
                    gameObject.SetActive(false);
                }

                break;
            case "btn_ok":
            case "btn_no":
                gameObject.SetActive(false);

                break;
        }
    }

    public void ToastMessage(string localKey, string dataSetName, bool on)
    {
        changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        string bookNumber = "";

        if (!dataSetName.Equals(string.Empty))
        {
            bookNumber = dataSetName.Substring(dataSetName.Length - 1);
            this.dataSetName = dataSetName;
        }

        PanelButtonSetting(on);

        Text[] goText = GetComponentsInChildren<Text>();
        foreach (Text txt in goText)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;

            if (txt.name.Equals("downloadFile"))
            {
                if (localKey.Equals("downWait"))
                {
                    txt.text = LocalizationManager.GetTermTranslation("UI_" + localKey);
                }
                else
                {
                    if (bookNumber.Equals("l"))
                        txt.text = LocalizationManager.GetTermTranslation("UI_downAllStart");
                    else
                        txt.text = LocalizationManager.GetTermTranslation("UI_" + localKey).Replace("*", bookNumber);
                }
            }
        }
    }


    public void PopUpMsg(string msg, bool btn)
    {
        changefont = Resources.Load<Font>("fonts/baloo-regular");

        PanelButtonSetting(btn);
        okButton.GetComponentInChildren<Text>().text = "OK";

        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = msg;
        transform.GetChild(0).GetChild(0).GetComponent<Text>().font = changefont;
    }

    public void PanelButtonSetting(bool on)
    {
        yesButton.gameObject.SetActive(on);
        noButton.gameObject.SetActive(on);
        okButton.gameObject.SetActive(!on);
        failText.SetActive(on);

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
}
