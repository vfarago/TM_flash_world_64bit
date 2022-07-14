using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class UnregisterLocalization : MonoBehaviour
{
    private void OnEnable()
    {
        Text[] txt = GetComponentsInChildren<Text>();
        foreach (Text tt in txt)
        {
            if (tt.name.Equals("unregister"))
            {
                tt.text = LocalizationManager.GetTermTranslation("UI_unregister");
                tt.font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));
            }
        }
    }
}