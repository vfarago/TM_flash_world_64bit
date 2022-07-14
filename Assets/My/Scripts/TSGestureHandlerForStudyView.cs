using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;


public class TSGestureHandlerForStudyView : MonoBehaviour
{
    readonly float minScale = 0.05f;
    readonly float maxScale = 2.5f;
    private Vector3 initialRot;

    CanvasManager can;

    private void Start()
    {
        can = FindObjectOfType<CanvasManager>();
        initialRot = transform.localEulerAngles;
    }

    #region ENABLE_and_DISABLE
    private void OnEnable()
    {
        if (GetComponent<TapGesture>() != null)
        {
            GetComponent<TapGesture>().Tapped += TappedHandler;
        }
        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged += OnScaleStateChanged;
        }
        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged += OnPanStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GetComponent<TapGesture>() != null)
        {
            GetComponent<TapGesture>().Tapped -= TappedHandler;
        }
        if (GetComponent<ScaleGesture>() != null)
        {
            GetComponent<ScaleGesture>().StateChanged -= OnScaleStateChanged;
        }
        if (GetComponent<PanGesture>() != null)
        {
            GetComponent<PanGesture>().StateChanged -= OnPanStateChanged;
        }
    }
    #endregion

    private void TappedHandler(object sender, EventArgs e)
    {
        StartCoroutine(RepositionAugmentation(0.5f));
    }

    private void OnPanStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        if (!can.isToastOn)
        {
            switch (e.State)
            {
                case Gesture.GestureState.Began:
                case Gesture.GestureState.Changed:

                    //2nd attempt
                    var gesture = (PanGesture)sender;

                    if (gesture.WorldDeltaPosition != Vector3.zero)
                    {
                        if (Math.Abs(gesture.WorldDeltaPosition.x) > Math.Abs(gesture.WorldDeltaPosition.z))
                        {//horizontal
                            this.transform.Rotate(0, 0, -gesture.WorldDeltaPosition.x * 200, Space.World);
                        }
                        else
                        {
                            //					transform.Rotate( gesture.WorldDeltaPosition.z, 0, 0, Space.World);
                            this.transform.Rotate(gesture.WorldDeltaPosition.z * 200, 0, 0, Space.World);
                        }
                    }

                    break;
            }
        }
    }



    private void OnScaleStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:

                var gesture = (ScaleGesture)sender;
                float localDeltaScale = gesture.LocalDeltaScale;

                float objectScale = transform.localScale.x;

                //scaling
                float currentScale = transform.localScale.x;
                if (localDeltaScale >= 1f)
                    currentScale *= (1 + (objectScale * 0.05f));
                else
                    currentScale *= (1 - (objectScale * 0.05f));

                currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);


                break;
        }
    }

    private IEnumerator RepositionAugmentation(float time)
    {
        //rotation 
        Vector3 startRotation = transform.localEulerAngles;
        Vector3 newRotation = initialRot;

        //scaling
        Vector3 startScaling = transform.localScale;
        Vector3 newScaling = new Vector3(1, 1, 1);

        //lerping
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.localEulerAngles = Vector3.Lerp(startRotation, newRotation, (elapsedTime / time));
            transform.localScale = Vector3.Lerp(startScaling, newScaling, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = newRotation;

        yield return transform;
    }

}
