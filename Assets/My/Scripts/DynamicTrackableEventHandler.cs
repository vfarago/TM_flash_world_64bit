/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/
using System.Collections;
using TouchScript.Gestures;
using UnityEngine;
using System.IO;
public class DynamicTrackableEventHandler : TrackableEventHandler
{
    public bool isFreeModel = false;
    public string targetName;
    public bool isModelLoading = false;

    private bool isEndAR;
    GameObject m3dModel = null;
    protected override void Awake()
    {
        base.Awake();
        targetName = mTrackableBehaviour.TrackableName.ToLower();
    }

    #region PRIVATE_METHODS

    protected override void OnTrackingFound()
    {
        if (isFreeModel)
        {
            isEndAR = prefabLoader.isEndAR;

            canvasManager.OnTrackingFound(false);

            if (!isModelLoading && !isEndAR)
            {
                //Debug.Log("        found " + targetName);

                StartCoroutine(loadModelAsync());
                isModelLoading = true;
            }
        }
        else if (prefabShelter.nothingModel)
        {
            canvasManager.OnInfoSerial();
        }
        else
        {
            bool isExist = false;
            for (int i = 0; i < prefabShelter.tmModel.Length; i++)
            {
                if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                {
                    if (prefabShelter.tmModel[i].model.name.Equals(targetName))
                    {
                        isExist = prefabShelter.tmModel[i].isConfirm;
                        break;
                    }
                }
            }

            if (isExist)
            {
                isEndAR = prefabLoader.isEndAR;

                canvasManager.OnTrackingFound(false);

                if (!isModelLoading && !isEndAR)
                {
                    //Debug.Log("        found " + targetName);

                    StartCoroutine(loadModelAsync());
                    isModelLoading = true;
                }
            }
            else
            {
                canvasManager.OnInfoSerial();
            }
        }
    }


    protected override void OnTrackingLost()
    {
        if (isModelLoading && !prefabLoader.isTargetoff)
        {
            //Debug.Log("        lost " + targetName);

            Destroy(m3dModel);
            m3dModel = null;

            isModelLoading = false;
        }
    }

    private IEnumerator loadModelAsync()
    {
        if (prefabLoader.isTargetoff)
            yield return prefabLoader.DestroyObj();

        if (m3dModel == null && !isEndAR)
        {
            if (isFreeModel)
            {
                //GameObject go = Resources.Load<GameObject>(string.Format("objects/{0}", targetName));
                //m3dModel = Instantiate(go, transform, false);
                GameObject go = null;
                for (int i = 0; i < 50; i++)
                {
                    if (targetName.Equals(FindObjectOfType<AnimalDataSetLoader>().freeModels[i].name))
                    {
                        go = FindObjectOfType<AnimalDataSetLoader>().freeModels[i];
                        break;
                    }
                }
                m3dModel = Instantiate(go, transform, false);

            }
            else
            {
                for (int i = 0; i < prefabShelter.tmModel.Length; i++)
                {
                    if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                    {
                        if (prefabShelter.tmModel[i].model.name.Equals(targetName))
                        {
                            m3dModel = Instantiate(prefabShelter.tmModel[i].model, transform, false);
                            break;
                        }
                    }
                }
            }
            RendererSet(m3dModel);

            if (m3dModel != null)
            {
                m3dModel.tag = "augmentation";

                m3dModel.transform.Rotate(0, 270, -90, Space.Self); // side  10.18.2017

                StartCoroutine(RepositionAugmentation(0.3f));

                yield return m3dModel;

                if (m3dModel != null)
                {
                    //gestures  [start]
                    TapGesture tagGesture = m3dModel.AddComponent<TapGesture>();
                    tagGesture.NumberOfTapsRequired = 1;
                    tagGesture.TimeLimit = 1;

                    ScaleGesture scaleGesture = m3dModel.AddComponent<ScaleGesture>();
                    PanGesture panGesture = m3dModel.AddComponent<PanGesture>();

                    scaleGesture.AddFriendlyGesture(panGesture);
                    panGesture.AddFriendlyGesture(scaleGesture);

                    TSGestureHandler gestureHandler = m3dModel.AddComponent<TSGestureHandler>();
                    gestureHandler.mTrackableBehaviour = mTrackableBehaviour;
                    gestureHandler.targetName = targetName;
                    gestureHandler.isFreeModel = isFreeModel;
                    gestureHandler.enabled = true;
                    //gesture [end]
                }
            }
        }
    }


    private IEnumerator RepositionAugmentation(float time)
    {
        float initialScale;
        if (m3dModel.GetComponentInChildren<MeshRenderer>())
        {
            m3dModel.GetComponentInChildren<MeshRenderer>().enabled = true;
            initialScale = 0.7f;
        }
        else
        {
            m3dModel.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            initialScale = 0.5f;
        }

        m3dModel.AddComponent<BoxCollider>().size = new Vector3(2, 2, 2);

        Vector3 startScaling = new Vector3(0.01f, 0.01f, 0.01f);
        Vector3 newScaling = new Vector3(initialScale, initialScale, initialScale);

        //Object Reflect
        if (arManager.isFrontCamera)
            newScaling = new Vector3(initialScale, initialScale, initialScale * -1f);

        //lerping
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            if (m3dModel == null)
                break;

            m3dModel.transform.localScale = Vector3.Lerp(startScaling, newScaling, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (m3dModel != null)
            m3dModel.transform.localScale = newScaling;

        yield return m3dModel;
    }
    #endregion
    void RendererSet(GameObject obj)
    {
        Renderer[] renderer = obj.transform.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer item in renderer)
        {
            if (item.materials != null)
            {
                foreach (Material mat in item.materials)
                {
                    Shader sha = mat.shader;
                    Debug.Log(sha.name);
                    mat.shader = Shader.Find(sha.name);
                }
            }
        }
    }
}
