using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class AboutUsManager : MonoBehaviour
{
    Text[] aboutUs;

    private void Awake()
    {
        aboutUs = GetComponentsInChildren<Text>();
        Button[] btnLink = GetComponentsInChildren<Button>();
        foreach (Button btn in btnLink)
            btn.onClick.AddListener(() => LinkUrl(btn));
    }

    void OnEnable()
    {
        Font changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        foreach (Text txt in aboutUs)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;
        }
    }

    private void LinkUrl(Button btn)
    {
        switch (btn.name)
        {
            case "aboutLinkComp":
                Application.OpenURL("http://www.vproductions.net/");
                break;
            case "aboutLinkProduct":
                Application.OpenURL("http://www.bookplusapp.co.kr/");
                break;
        }
    }
}