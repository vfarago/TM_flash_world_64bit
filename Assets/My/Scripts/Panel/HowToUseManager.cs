using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class HowToUseManager : MonoBehaviour
{
    public GameObject linkButton;
    public Image localimg1, localimg2;

    Text[] howToUse;

    private void Awake()
    {
        howToUse = GetComponentsInChildren<Text>();
        Button[] link = linkButton.GetComponentsInChildren<Button>();
        foreach (Button btn in link)
        {
            btn.onClick.AddListener(() => LinkUrl(btn));
        }
    }

    void OnEnable()
    {
        Font changefont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        foreach (Text txt in howToUse)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = changefont;

            localimg1.sprite = Resources.Load<Sprite>(string.Format("Sprites/howtouse/image_stander_{0}(800x880)", LocalizationManager.CurrentLanguage));
            localimg2.sprite = Resources.Load<Sprite>(string.Format("Sprites/howtouse/image_howtouse_{0}(800x1380)", LocalizationManager.CurrentLanguage));
        }
    }

    void OnDisable()
    {
        Button[] link = linkButton.GetComponentsInChildren<Button>();
        foreach (Button btn in link)
        {
            btn.onClick.RemoveListener(() => LinkUrl(btn));
        }
    }

    private void LinkUrl(Button btn)
    {
        switch (btn.name)
        {
            case "link_Youtube":
                Application.OpenURL(LocalizationManager.GetTermTranslation("UI_youtube_card"));

                break;
            case "link_Youku":
                Application.OpenURL("https://v.youku.com/v_show/id_XNDQxNjM2NzU3Ng==.html?spm=a2hzp.8244740.0.0&debug=flv");

                break;
        }
    }

}
