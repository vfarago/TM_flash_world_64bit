using UnityEngine;

public class PanelSize : MonoBehaviour
{
    public RectTransform content;
    public RectTransform login, signUp;

    public void Resizer()
    {
        if (login.gameObject.activeSelf)
        {
            content.sizeDelta = login.sizeDelta;
        }
        else if (signUp.gameObject.activeSelf)
        {
            content.sizeDelta = signUp.sizeDelta;
        }
    }
}
