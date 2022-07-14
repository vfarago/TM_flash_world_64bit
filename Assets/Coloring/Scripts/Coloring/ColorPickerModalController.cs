using UnityEngine;
using UnityEngine.EventSystems;

namespace SJ.MathFun
{
    public class ColorPickerModalController : MonoBehaviour, IPointerClickHandler
    {

        ColoringController coloringController;

        void Awake()
        {
            coloringController = FindObjectOfType<ColoringController>();
        }

        #region IPointerClickHandler implementation

        public void OnPointerClick(PointerEventData eventData)
        {
            //print ("OnPointerClick");
            coloringController.TogglePalette();

        }

        #endregion
    }
}