/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using System.Collections;
using UnityEngine;
using Vuforia;

public class ImageTargetUIEventHandler : ISampleAppUIEventHandler
{
    #region PUBLIC_METHODS

    public override void Bind() { }

    public override void UnBind() { }

    public override void TriggerAutoFocus()
    {
        StartCoroutine(TriggerAutoFocusAndEnableContinuousFocusIfSet());
    }

    #endregion PUBLIC_METHODS


    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
    }

    #region PRIVATE_METHODS

    /// <summary>
    /// Activating trigger autofocus mode unsets continuous focus mode 
    /// So, we wait for a second and turn continuous focus back on 
    /// </returns>

    private void OnVuforiaStarted()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }


    private IEnumerator TriggerAutoFocusAndEnableContinuousFocusIfSet()
    {
        //triggers a single autofocus operation 
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
        yield return new WaitForSeconds(1.0f);

        //continuous focus mode is turned back on 
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

    }

    private void OnPaused(bool pause)
    {
        if (!pause)
        {
            // set to continous autofocus
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }


    //We want autofocus to be enabled when the app starts
    private void EnableContinuousAutoFocus()
    {

        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        Debug.Log("EnableContinuousAutoFocus");
    }



    #endregion PRIVATE_METHODS
}

