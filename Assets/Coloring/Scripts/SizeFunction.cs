using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SizeFunction
{
    public static Vector2 SizeToParent(this RawImage image, float widthDecreasement = 1, float heightDecreasement = 1, float padding = 0)
    {
        var parent = image.transform.parent.GetComponentInParent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
        if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
        padding = 1 - padding;
        float w = 0, h = 0;
        float ratio = image.texture.width / (float)image.texture.height;
        var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
        if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
        {
            //Invert the bounds if the image is rotated
            bounds.size = new Vector2(bounds.height, bounds.width);
        }
        //Size by height first
        h = bounds.height * padding;
        w = h * ratio;
        if (w > bounds.width * padding)
        { //If it doesn't fit, fallback to width;
            w = bounds.width * padding;
            h = w / ratio;
        }

        w *= widthDecreasement;
        h *= heightDecreasement;

        imageTransform.sizeDelta = new Vector2(w, h);
        return imageTransform.sizeDelta;
    }
}
