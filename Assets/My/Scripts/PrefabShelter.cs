using System.Collections.Generic;
using UnityEngine;

public class PrefabShelter : MonoBehaviour
{
    public TMModel[] tmModel;
    public bool nothingModel = false;
    //public List<GameObject> model;

    private void Awake()
    {
        tmModel = new TMModel[500];
    }
}

[System.Serializable]
public class TMModel
{
    public GameObject model;
    public bool isConfirm;

    public TMModel(GameObject model, bool isConfirm)
    {
        this.model = model;
        this.isConfirm = isConfirm;
    }
}