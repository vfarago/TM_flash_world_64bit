using UnityEngine;
using UnityEngine.EventSystems;

public class DragThresholdSetting : MonoBehaviour
{
    private const float inchToCm = 2.54f;

    [SerializeField]
    private EventSystem eventSystem = null;

    [SerializeField]
    private float dragThresholdCM = 0.5f;
    //For drag Threshold

    private void SetDragThreshold()
    {
        if (eventSystem != null)
        {
            eventSystem.pixelDragThreshold = (int)(dragThresholdCM * Screen.dpi / inchToCm);
        }
    }


    void Awake()
    {
        SetDragThreshold();
    }
}
