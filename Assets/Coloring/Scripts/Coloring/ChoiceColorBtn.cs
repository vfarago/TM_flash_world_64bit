using UnityEngine;
using UnityEngine.UI;

public class ChoiceColorBtn : MonoBehaviour
{
    public ColorPicker picker;

    private Button[] colorBtns;

    private void OnEnable()
    {
        colorBtns = GetComponentsInChildren<Button>();
        for (int i = 0; i < colorBtns.Length; i++)
        {
            Button colorBtn = colorBtns[i];
            colorBtn.onClick.AddListener(() => ChoiceColor(colorBtn));
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < colorBtns.Length; i++)
        {
            Button colorBtn = colorBtns[i];
            colorBtn.onClick.RemoveAllListeners();
        }
    }

    private void ChoiceColor(Button colorBtn)
    {
        Color clr = colorBtn.GetComponent<Image>().color;
        picker.CurrentColor = clr;
    }
}
