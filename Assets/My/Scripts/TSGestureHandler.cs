using System;
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;
using Vuforia;
//using System.Runtime.InteropServices;

public class TSGestureHandler : MonoBehaviour
{
    CanvasManager canvasManager;
    PrefabLoader prefabLoader;
    float onTargetScale;
    float objectScale;
    float MaxScale;
    float MinScale;

    public TrackableBehaviour mTrackableBehaviour;
    public string targetName;
    public bool isFreeModel;
    private int counter = 0;

    void Start()
    {
        prefabLoader = FindObjectOfType<PrefabLoader>();
        canvasManager = FindObjectOfType<CanvasManager>();
        onTargetScale = transform.localScale.x;
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
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
        counter++;

        if (counter == 1)
            StartCoroutine(DoubleTapEvent());
    }

    // 터치→ 오브젝트 타겟 분리, 더블탭→ 파닉스전환
    IEnumerator DoubleTapEvent()
    {
        yield return new WaitForSeconds(0.5f);
        if (counter > 1)
        {
            yield return new WaitForSeconds(0.02f);

            //camera changer
            ARManager.Instance.ChangeCamera("MainCamera");
            ARManager.Instance.setHintZero();

            prefabLoader.ChangePrefab(targetName, isFreeModel);

            canvasManager.OnPhonicsPanel(true);
        }
        else
        {
            prefabLoader.TargetOffMoving(gameObject);
            canvasManager.OnTargetOffObject(true);
            mTrackableBehaviour.OnTrackerUpdate(TrackableBehaviour.Status.NOT_FOUND);
        }

        yield return new WaitForSeconds(0.05f);
        prefabLoader.ModelFalse();

        counter = 0;
    }

    private void OnPanStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:
                var gesture = (PanGesture)sender;

                //2nd attempt
                if (gesture.WorldDeltaPosition != Vector3.zero)
                {
                    if (Math.Abs(gesture.WorldDeltaPosition.x) > Math.Abs(gesture.WorldDeltaPosition.z))
                    {//horizontal
                        transform.Rotate(0, 0, -gesture.WorldDeltaPosition.x * 1.3f, Space.World);
                    }
                    else
                    {
                        transform.Rotate(gesture.WorldDeltaPosition.z * 1.3f, 0, 0, Space.World);
                    }
                }

                break;
        }
    }



    private void OnScaleStateChanged(object sender, GestureStateChangeEventArgs e)
    {
        float scaleSpeed;

        if (prefabLoader.isTargetoff)
        {
            objectScale = 1;
            scaleSpeed = 0.05f;
            MinScale = 50f;
            MaxScale = 250f;
        }
        else
        {
            objectScale = onTargetScale;
            scaleSpeed = 1.5f;
            MinScale = 0.05f;
            MaxScale = 2.0f;
        }

        switch (e.State)
        {
            case Gesture.GestureState.Began:
            case Gesture.GestureState.Changed:

                var gesture = (ScaleGesture)sender;

                float localDeltaScale = gesture.LocalDeltaScale;
                //float objectScale = transform.localScale.x;

                //scaling
                float currentScale = transform.localScale.x;
                if (localDeltaScale >= 1f)
                    currentScale *= (1 + (objectScale * scaleSpeed));
                else
                    currentScale *= (1 - (objectScale * scaleSpeed));

                currentScale = Mathf.Clamp(currentScale, MinScale, MaxScale);
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);



                break;
        }
    }

}
