using System.Collections;
using UnityEngine;
using System.IO;
public class PrefabLoader : MonoBehaviour
{
    ARManager arManager;
    PrefabShelter prefabShelter;

    GameObject phoModel;
    GameObject m3dModel;
    GameObject phonics;
    public GameObject dataSet;
    public Camera arCamera;
    public bool isEndAR;
    public bool isTargetoff = false;

    float backRotY;
    float m3dModelDepth = 700f;
    Vector3 m3dModelRot;

    private void Awake()
    {
        arManager = ARManager.Instance;
        prefabShelter = FindObjectOfType<PrefabShelter>();
        dataSet = FindObjectOfType<AnimalDataSetLoader>().gameObject;
    }

    public void ChangePrefab(string name, bool isFreeModel)
    {
        isEndAR = true;
        StartCoroutine(RoadPrefab(name, isFreeModel));
        DestroyObj();
    }

    IEnumerator RoadPrefab(string targetName, bool isFreeModel)
    {
        GameObject objectHolder = GameObject.Find("ObjectHolder");
        objectHolder.transform.rotation = new Quaternion(0, 0, 0, 0);
        objectHolder.transform.localScale = new Vector3(1, 1, 1);

        if (isFreeModel)
        {
            //GameObject go = Resources.Load<GameObject>(string.Format("objects/{0}", targetName));
            //phoModel = Instantiate(go, objectHolder.transform, false);
            GameObject go = null;
            for (int i = 0; i < 50; i++)
            {
                if (targetName.Equals(FindObjectOfType<AnimalDataSetLoader>().freeModels[i].name))
                {
                    go = FindObjectOfType<AnimalDataSetLoader>().freeModels[i];
                    break;
                }
            }
            phoModel = Instantiate(go, objectHolder.transform, false);
        }
        else
        {
            for (int i = 0; i < prefabShelter.tmModel.Length; i++)
            {
                if (prefabShelter.tmModel[i] != null && prefabShelter.tmModel[i].model != null)
                {
                    if (prefabShelter.tmModel[i].model.name.Equals(targetName))
                    {
                        phoModel = Instantiate(prefabShelter.tmModel[i].model, objectHolder.transform, false);
                        break;
                    }
                }
            }
        }

        phoModel.name = targetName;
        phoModel.tag = "Phonics";

        if (phoModel.GetComponentInChildren<MeshRenderer>())
        {
            phoModel.transform.Rotate(0, -90, -90, Space.World); // 일반사물
        }
        else
        {
            phoModel.transform.Rotate(-50, -90, -90, Space.World); // 동물
        }
        phoModel.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        RendererSet(phoModel);

        yield return phoModel;

        Resources.UnloadUnusedAssets();

        phonics = Instantiate(Resources.Load<GameObject>("prefabs/Phonics"), GameObject.Find("ARPanel").transform, false);
        phonics.GetComponentInChildren<Phonics>().targetName = targetName;
        phonics.GetComponentInChildren<Phonics>().isFreeModel = isFreeModel;
    }
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

    public void TargetOffMoving(GameObject go)
    {
        m3dModel = go;
        m3dModel.tag = "targetOff";
        StartCoroutine(RepositionAugmentation(0.5f));
        AllKill();

        Resources.UnloadUnusedAssets();
    }

    //터치후 포지션변경
    private IEnumerator RepositionAugmentation(float time)
    {
        m3dModel.transform.parent = arCamera.transform;
        isTargetoff = true;

        m3dModel.GetComponent<BoxCollider>().enabled = true;

        if (m3dModel.GetComponentInChildren<MeshRenderer>())
        {
            m3dModel.GetComponentInChildren<MeshRenderer>().enabled = true;
            backRotY = -90f;
        }
        else
        {
            m3dModel.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            backRotY = -140f;
        }

        //set the initialScale
        //float initialScale = m3dModel.transform.localScale.x;
        float initialScale = 100f;

        //position
        Vector3 startPosition = m3dModel.transform.localPosition;
        Vector3 newPosition = new Vector3(0, 0, m3dModelDepth);

        //rotation 
        Vector3 startRotation = m3dModel.transform.localEulerAngles;
        Vector3 newRotation = RotValue();

        //scaling
        Vector3 startScaling = m3dModel.transform.localScale;
        Vector3 newScaling = new Vector3(initialScale, initialScale, initialScale);

        //Object Reflect
        if (arManager.isFrontCamera)
            newScaling = new Vector3(initialScale, initialScale, initialScale * -1f);

        //lerping
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            m3dModel.transform.localPosition = Vector3.Lerp(startPosition, newPosition, (elapsedTime / time));
            m3dModel.transform.localEulerAngles = Vector3.Lerp(startRotation, newRotation, (elapsedTime / time));
            m3dModel.transform.localScale = Vector3.Lerp(startScaling, newScaling, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        m3dModel.transform.localPosition = newPosition;
        m3dModel.transform.localEulerAngles = newRotation;

        yield return m3dModel;
    }

    //카메라 전/후 전환에 따라 오브젝트 180도 회전
    private Vector3 RotValue()
    {
        if (arManager.isFrontCamera)
        {
            m3dModelRot = new Vector3(180, backRotY, 0);
        }
        else
        {
            m3dModelRot = new Vector3(0, backRotY, 0);
        }
        return m3dModelRot;
    }

    //카메라 전/후 전환에 따라 오브젝트 좌우반전
    public void ModelChangePos()
    {
        //Object Reflect
        m3dModel.transform.localScale = new Vector3(m3dModel.transform.localScale.x, m3dModel.transform.localScale.y, m3dModel.transform.localScale.z * -1f);

        m3dModel.transform.localEulerAngles = RotValue();
    }

    public void ModelFalse()
    {
        DynamicTrackableEventHandler[] dteh = dataSet.GetComponentsInChildren<DynamicTrackableEventHandler>();
        foreach (DynamicTrackableEventHandler go in dteh)
        {
            go.isModelLoading = false;
        }
    }

    public bool DestroyObj()
    {
        Destroy(m3dModel);

        return isTargetoff = false;
    }

    private void AllKill()
    {
        GameObject[] gobj = GameObject.FindGameObjectsWithTag("augmentation");
        foreach (GameObject go in gobj)
            Destroy(go);
    }

    public void DestroyPrefab()
    {
        StopAllCoroutines();
        Destroy(phoModel);
        phoModel = null;
        Destroy(phonics);
        phonics = null;
    }

}