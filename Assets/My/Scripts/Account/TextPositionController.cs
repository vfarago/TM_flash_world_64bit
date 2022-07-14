using UnityEngine;
using UnityEngine.UI;

public class TextPositionController : MonoBehaviour
{
    float aa;

    public void SetTextPosition()
    {
        aa = transform.parent.GetComponent<Text>().preferredWidth + 10f;

        GetComponent<RectTransform>().anchoredPosition = new Vector3(aa, 0, 0);
    }

}
