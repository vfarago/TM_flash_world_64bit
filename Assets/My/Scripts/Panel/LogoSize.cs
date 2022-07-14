using UnityEngine;

public class LogoSize : MonoBehaviour
{
    public RectTransform logo, mid;

    float logoPosY;
    float midPosY;

    void Awake()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;

        if (0 < windowAspect && windowAspect < 0.49f)
        { // 412x846 갤럭시S9
            logoPosY = -400f;
            midPosY = 580f;
        }
        else if (0.49f <= windowAspect && windowAspect < 0.57f)
        { // 9:16  = 0.5625
            logoPosY = -350f;
            midPosY = 580f;
        }
        else if (0.57f <= windowAspect && windowAspect < 0.65f)
        { // 10:16 = 0.625
            logoPosY = -330f;
            midPosY = 580f;
        }
        else
        {  // 3:4 = 0.75
            logoPosY = -280f;
            midPosY = 530f;
        }
    }

    private void Start()
    {
        logo.anchoredPosition = new Vector2(logo.anchoredPosition.x, logoPosY);
        mid.anchoredPosition = new Vector2(logo.anchoredPosition.x, midPosY);
    }
}
