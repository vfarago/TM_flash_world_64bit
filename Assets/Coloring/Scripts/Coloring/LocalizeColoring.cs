using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

namespace SJ.MathFun
{
    public class LocalizeColoring : MonoBehaviour
    {
        ColoringController coloringController;
        public GameObject coloringCanvas, toastPanel;
        private Font localizeFont;

        private void Awake()
        {
            coloringController = GetComponent<ColoringController>();
            LocalizationColoring();
        }

        private void LocalizationColoring()
        {
            GameObjectActive(true);
            localizeFont = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

            Text[] colorText = coloringCanvas.GetComponentsInChildren<Text>();
            foreach(Text txt in colorText)
            {
                txt.text = LocalizationManager.GetTermTranslation("UI_" + txt.name);
                txt.font = localizeFont;
            }

            GameObjectActive(false);
        }

        private void GameObjectActive(bool on)
        {
            coloringController.eraserWrapper.SetActive(on);
            coloringController.sharePalette.SetActive(on);
            coloringController.resetPopWrapper.SetActive(on);
            coloringController.popupWrapper.SetActive(on);
            toastPanel.SetActive(on);
            coloringController.sketchSelector.SetActive(true);
        }

    }
}
