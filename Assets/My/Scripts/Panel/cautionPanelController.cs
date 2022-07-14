using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class cautionPanelController : MonoBehaviour
{
    CheckCode checkCode;
    CanvasManager canvasManager;

    Toggle toggle;
    Button close;

    void Start()
    {
        checkCode = FindObjectOfType<CheckCode>();
        canvasManager = FindObjectOfType<CanvasManager>();
        toggle = GetComponentInChildren<Toggle>();
        close = GetComponentInChildren<Button>();

        toggle.isOn = canvasManager.isNotCautionAgain;

        toggle.onValueChanged.AddListener(delegate
         {
             ToggleValueChanged(toggle.isOn);
         });

        close.onClick.RemoveAllListeners();
        close.onClick.AddListener(() => CloseButton());

        Text[] txt = GetComponentsInChildren<Text>();
        foreach (Text t in txt)
        {
            if (LocalizationManager.CurrentLanguage.Equals("eng"))
            {
                t.font = Resources.Load<Font>("fonts/baloo-regular");
                t.fontSize = 40;
                t.lineSpacing = 1.3f;
            }
            else
            {
                t.font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
                t.fontSize = 35;
                t.lineSpacing = 1.7f;
            }
            t.text = LocalizationManager.GetTermTranslation("UI_" + t.name);
        }
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveAllListeners();
        close.onClick.RemoveAllListeners();
    }

    private void ToggleValueChanged(bool isOn)
    {
        canvasManager.isNotCautionAgain = isOn;
        checkCode.NotCautionMsg(true, isOn);
    }

    private void CloseButton()
    {
        canvasManager.CautionActive(false);
        Destroy(gameObject);
    }
}
