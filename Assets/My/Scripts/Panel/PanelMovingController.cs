using I2.Loc;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelMovingController : MonoBehaviour
{
    public GameObject movePanel;
    public Text InfoText;
    RectTransform rt;
    float posY;

    private void Awake()
    {
        posY = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.y;
        rt = movePanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, posY);
    }

    public void PanelOff()
    {
        StartCoroutine(RectMoving(rt, posY * 0.5f));
    }

    public void TouchOn()
    {
        Text[] downText = movePanel.GetComponentsInChildren<Text>();
        foreach (Text txt in downText)
        {
            txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
            txt.font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
        }

        rt.anchoredPosition = new Vector2(0, posY * 0.5f);
        StartCoroutine(RectMoving(rt, -posY * 0.5f));
    }

    IEnumerator RectMoving(RectTransform rt, float posY)
    {
        float step = 0;
        while (step < 1)
        {
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, new Vector2(rt.anchoredPosition.x, posY), step += Time.deltaTime);
            yield return new WaitForEndOfFrame();

            if (rt.anchoredPosition.y > (this.posY * 0.5f) - 1)
                gameObject.SetActive(false);
        }
    }
}
