using UnityEngine;
using Vuforia;

public class ARManager : MonoBehaviour
{
    private static ARManager instance;

    public enum State { IDLE, ARState, StudyViewState };
    public enum HintState { ZERO, SINGLE, MULTI };

    public State state;
    public Camera arCam;
    public Camera StudyViewCamera;

    public bool isFrontCamera;
    public HintState hintState = HintState.ZERO;

    public static ARManager Instance
    {
        get
        {
            if (instance == null) //Finds the instance if it doesn't exist
                instance = FindObjectOfType<ARManager>();

            return instance;
        }
    }


    void Start()
    {
        //ARCamera.enabled = true;
        StudyViewCamera.enabled = false;
        state = State.IDLE;
        isFrontCamera = false;
        //TurnOnAR(false, false);
        setHintZero();
    }


    // ARRenderHint [start]
    public void setHintMulti()
    {
        //first stop ObjectTracker
        bool needsObjectTrackerRestart = stopRunningObjectTracker();

        VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 10);
        hintState = HintState.MULTI;

        //finally restart ObjectTracker
        if (needsObjectTrackerRestart)
            restartRunningObjectTracker();
    }


    public void setHintSingle()
    {
        //first stop ObjectTracker
        bool needsObjectTrackerRestart = stopRunningObjectTracker();

        hintState = HintState.SINGLE;
        VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 1);

        if (needsObjectTrackerRestart)
            restartRunningObjectTracker();
    }


    public void setHintZero()
    {
        //first stop ObjectTracker
        //bool needsObjectTrackerRestart = stopRunningObjectTracker();
        stopRunningObjectTracker();

        hintState = HintState.ZERO;
        VuforiaUnity.SetHint(VuforiaUnity.VuforiaHint.HINT_MAX_SIMULTANEOUS_IMAGE_TARGETS, 0); //it doesn't seem to work

        ////finally restart ObjectTracker
        //if (needsObjectTrackerRestart)
        //    restartRunningObjectTracker();
    }



    private bool stopRunningObjectTracker()
    {
        bool needsObjectTrackerRestart = false;

        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objectTracker != null)
        {
            if (objectTracker.IsActive)
            {
                objectTracker.Stop();
                needsObjectTrackerRestart = true;
            }
        }
        return needsObjectTrackerRestart;
    }



    private bool restartRunningObjectTracker()
    {
        bool hasObjectTrackerRestarted = false;

        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objectTracker != null)
        {
            if (!objectTracker.IsActive)
            {
                hasObjectTrackerRestarted = objectTracker.Start();
            }
        }
        return hasObjectTrackerRestarted;
    }

    // ARRenderHint [end]



    public void ChangeCamera(string destCamera)
    {
        if (destCamera.Equals("ARCamera"))
        {
            TurnOnAR(true, true);
            StudyViewCamera.enabled = false;
            arCam.enabled = true;
            state = State.ARState;
        }
        else
        { //MainCamera
            TurnOnAR(false, false);
            arCam.enabled = false;
            StudyViewCamera.enabled = true;
            state = State.StudyViewState;
        }

    }


    public void changeCameraStateToIdle()
    {
        state = State.IDLE;
        setHintZero();
    }


    //https://developer.vuforia.com/forum/faq/unity-how-select-camera-and-mirroring
    public void UseFrontCamera(bool ready)
    {

        if (ready) //Front Camera = true
        {
            isFrontCamera = true;

            // turn off the curent camera : the back camera
            Vuforia.CameraDevice.Instance.Stop();
            Vuforia.CameraDevice.Instance.Deinit();

            // turn on the front camera
            Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_FRONT);
            Vuforia.CameraDevice.Instance.Start();

            //turn on the mirroring
            Vuforia.VuforiaRenderer.VideoBGCfgData config = Vuforia.VuforiaRenderer.Instance.GetVideoBackgroundConfig();
            config.reflection = Vuforia.VuforiaRenderer.VideoBackgroundReflection.ON;
            Vuforia.VuforiaRenderer.Instance.SetVideoBackgroundConfig(config);

            //Debug.Log("UseFrontCamera");
            return;
        }
        else //Back Camera = false
        {
            isFrontCamera = false;

            // turn off the curent camera : the front camera
            Vuforia.CameraDevice.Instance.Stop();
            Vuforia.CameraDevice.Instance.Deinit();

            // turn on the back camera
            Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_BACK);
            Vuforia.CameraDevice.Instance.Start();

            //turn off the mirroring
            Vuforia.VuforiaRenderer.VideoBGCfgData config = Vuforia.VuforiaRenderer.Instance.GetVideoBackgroundConfig();
            config.reflection = Vuforia.VuforiaRenderer.VideoBackgroundReflection.OFF;
            Vuforia.VuforiaRenderer.Instance.SetVideoBackgroundConfig(config);

            //Debug.Log("UseBackCamera");
            return;
        }
    }

    public void TurnOnAR(bool use, bool isStop)
    {
        if (use)
        {
            ActivateDataSet(isStop);
            // turn on the front camera
            if (isFrontCamera)
                Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_FRONT);
            else
                Vuforia.CameraDevice.Instance.Init(Vuforia.CameraDevice.CameraDirection.CAMERA_BACK);
            Vuforia.CameraDevice.Instance.Start();
        }
        else
        {
            ActivateDataSet(false);
            // turn off the curent camera 
            Vuforia.CameraDevice.Instance.Stop();
            Vuforia.CameraDevice.Instance.Deinit();
        }
    }


    public void ActivateDataSet(bool activate)
    {
        Vuforia.ObjectTracker objectTracker = Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>();

        if (activate)
        {
            objectTracker.Start();
        }
        else
        {
            objectTracker.Stop();
        }
        //print("               Tracking  " + objectTracker.IsActive);
    }

}
