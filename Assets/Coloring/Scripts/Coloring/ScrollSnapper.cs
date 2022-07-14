using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollSnapper : MonoBehaviour
{
    public int colNum;

    private RectTransform rect;

    private float contentHeight;
    private float clipLoc;
    private float movingPos;

    private float lerpTime = 0.25f;
    private float lerpEllap;

    private bool onDraging;

    public void SettingStart()
    {
        rect = GetComponent<RectTransform>();
        contentHeight = rect.sizeDelta.y;
        clipLoc = contentHeight / colNum;

        rect.localPosition = new Vector2(rect.localPosition.x, 0);
    }

    private void Update()
    {
        if (onDraging)
        {
            if (Input.GetButtonUp("Fire1"))
            {
                onDraging = false;
                if (rect.anchoredPosition.y % clipLoc != 0)
                {
                    lerpEllap = lerpTime;

                    if (rect.anchoredPosition.y % clipLoc >= clipLoc / 2)
                    {
                        movingPos = Mathf.Ceil(rect.anchoredPosition.y / clipLoc) * clipLoc;
                        float ySize = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>().sizeDelta.y;
                        if (movingPos > contentHeight - ySize)
                        {
                            movingPos = contentHeight - ySize;
                        }
                    }
                    else
                    {
                        movingPos = Mathf.Floor(rect.anchoredPosition.y / clipLoc) * clipLoc;
                        if (movingPos < 0)
                        {
                            movingPos = 0;
                        }
                    }

                    movingPos = Mathf.Round(movingPos);
                }
            }
        }
        else
        {
            if (lerpEllap > 0)
            {
                lerpEllap -= Time.deltaTime;
                rect.anchoredPosition = Vector2.Lerp(new Vector2(rect.anchoredPosition.x, movingPos), rect.anchoredPosition, lerpEllap / lerpTime);
            }
            else if (lerpEllap < 0)
            {
                lerpEllap = 0;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, movingPos);
            }
        }
    }

    public void Changed()
    {
        if (Input.GetButton("Fire1"))
        {
            onDraging = true;
        }
    }
}