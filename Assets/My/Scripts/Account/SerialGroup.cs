using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialGroup : MonoBehaviour
{
    public bool multiSerial;

    public GameObject serialPrefab;

    public RectTransform backBox;
    public RectTransform contentBox;
    public float objectDistance;
    public float noneSize;

    private List<GameObject> objects = new List<GameObject>();
    private float currentPos = 0;
    private float currentSize = 0;
    private float startSize;
    private float backStartSize;
    private float minimumHeight;

    private void Awake()
    {
        currentSize = Mathf.Abs(GetComponent<RectTransform>().anchoredPosition.y);

        startSize = contentBox.sizeDelta.y;
        backStartSize = backBox.sizeDelta.y;

        RectTransform rootRect = contentBox.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        minimumHeight = rootRect.sizeDelta.y - 118;
    }

    public void ResetAll()
    {
        currentSize = Mathf.Abs(GetComponent<RectTransform>().anchoredPosition.y);

        for (int i = 0; i < objects.Count; i++)
        {
            Destroy(objects[i]);
        }
        objects.Clear();

        contentBox.sizeDelta = new Vector2(contentBox.sizeDelta.x, startSize);
        backBox.sizeDelta = new Vector2(backBox.sizeDelta.x, backStartSize);

        currentPos = 0;
    }

    public void SizeScaler(bool some)
    {
        OrganizeList();

        for(int i = 0; i < objects.Count; i++)
        {
            CodeContainer codes = objects[i].GetComponent<CodeContainer>();

            objects[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(objects[i].GetComponent<RectTransform>().anchoredPosition.x, currentPos);
            currentSize += codes.currentHeight;
            currentPos -= codes.currentHeight;
        }

        if (some)
        {
            backBox.sizeDelta = new Vector2(backBox.sizeDelta.x, 0);

            if (currentSize < minimumHeight)
            {
                contentBox.sizeDelta = new Vector2(contentBox.sizeDelta.x, minimumHeight);
            }
            else
            {
                contentBox.sizeDelta = new Vector2(contentBox.sizeDelta.x, currentSize);
            }
        }
        else
        {
            backBox.sizeDelta = new Vector2(backBox.sizeDelta.x, noneSize);
            contentBox.sizeDelta = new Vector2(contentBox.sizeDelta.x, minimumHeight);
        }
    }

    public void Create(string book, string code, bool AExist, string deviceAIden, string deviceA, string deviceADate, bool BExist, string deviceBIden, string deviceB, string deviceBDate, bool isActive, bool isFirst)
    {
        GameObject serial = Instantiate(serialPrefab, transform);
        //RectTransform rect = serial.GetComponent<RectTransform>();

        CodeContainer codes = serial.GetComponent<CodeContainer>();

        string write = string.Empty;

        switch (book)
        {
            case "tm1":
                write = "Tagme3D Book1 ";
                break;

            case "tm2":
                write = "Tagme3D Book2 ";
                break;

            case "tm3":
                write = "Tagme3D Book3 ";
                break;

            case "tm4":
                write = "Tagme3D Book4 ";
                break;

            case "tm_full":
                write = "Tagme3D Book1~4 ";
                break;

            case "tm_flash_cn":
                write = "AR Flash Card ";
                break;

            case "member":
                write = "MemberShip ";
                break;
        }
        
        codes.codeBox.text = write + "CODE: " + code;
        codes.deviceANameBox.text = deviceA;
        codes.deviceADateBox.text = deviceADate;
        codes.deviceBNameBox.text = deviceB;
        codes.deviceBDateBox.text = deviceBDate;

        if (isActive)
        {
            codes.isActive = true;

            if (isFirst)
            {
                codes.deviceAImage.sprite = codes.imageOn;
            }
            else
            {
                codes.deviceBImage.sprite = codes.imageOn;
            }
        }
        else
        {
            codes.codeUsing.sprite = codes.dotOff;
        }

        if (!AExist)
        {
            codes.isActive = false;
            codes.deviceAContainer.SetActive(false);
        }
        else
        {
            Button btn = codes.deviceAContainer.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => Unregister(code, deviceAIden, deviceA));
        }

        if (!BExist)
        {
            codes.deviceBContainer.SetActive(false);
        }
        else
        {
            Button btn = codes.deviceBContainer.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => Unregister(code, deviceBIden, deviceB));
        }

        if (!AExist && !BExist)
        {
            codes.deviceBBackBox.SetActive(false);
            serial.GetComponent<RectTransform>().sizeDelta = new Vector2(serial.GetComponent<RectTransform>().sizeDelta.x, noneSize);
            codes.currentHeight = noneSize;
        }
        else
        {
            codes.currentHeight = objectDistance;
        }

        objects.Add(serial);
    }

    //public void Create(string code, bool AExist, string deviceAIden, string deviceA, string deviceADate, bool BExist, string deviceBIden, string deviceB, string deviceBDate, bool isFirst)
    //{
    //    GameObject serial = Instantiate(serialPrefab, transform);
    //    CodeContainer codes = serial.GetComponent<CodeContainer>();
    //
    //    codes.codeBox.text = "CODE: " + code;
    //    codes.AIdentifier = deviceAIden;
    //    codes.deviceANameBox.text = deviceA;
    //    codes.deviceADateBox.text = deviceADate;
    //    codes.BIdentifier = deviceBIden;
    //    codes.deviceBNameBox.text = deviceB;
    //    codes.deviceBDateBox.text = deviceBDate;
    //
    //    if (isFirst)
    //    {
    //        codes.deviceAImage.sprite = codes.imageOn;
    //    }
    //    else
    //    {
    //        codes.deviceBImage.sprite = codes.imageOn;
    //    }
    //
    //    if (!AExist)
    //    {
    //        codes.deviceAContainer.SetActive(false);
    //    }
    //    else
    //    {
    //        Button btn = codes.deviceAContainer.GetComponentInChildren<Button>();
    //        btn.onClick.AddListener(() => Unregister(code, deviceAIden, deviceA));
    //    }
    //
    //    if (!BExist)
    //    {
    //        codes.deviceBContainer.SetActive(false);
    //    }
    //    else
    //    {
    //        Button btn = codes.deviceBContainer.GetComponentInChildren<Button>();
    //        btn.onClick.AddListener(() => Unregister(code, deviceBIden, deviceB));
    //    }
    //
    //    if (!AExist && !BExist)
    //    {
    //        float sizeMinus = serial.GetComponent<RectTransform>().sizeDelta.y - noneSize;
    //
    //        serial.GetComponent<RectTransform>().sizeDelta = new Vector2(serial.GetComponent<RectTransform>().sizeDelta.x, noneSize);
    //        contentBox.sizeDelta -= new Vector2(0, sizeMinus);
    //        backBox.sizeDelta = new Vector2(backBox.sizeDelta.x, noneSize);
    //
    //        if (contentBox.sizeDelta.y < minimumHeight)
    //        {
    //            contentBox.sizeDelta = new Vector2(contentBox.sizeDelta.x, minimumHeight);
    //        }
    //    }
    //
    //    objects.Add(serial);
    //}

    private void Unregister(string code, string iden, string model)
    {
        AccountManager manager = GameObject.FindGameObjectWithTag("Accounting").GetComponent<AccountManager>();

        manager.receivedCode = code;
        manager.receivedIdentifier = iden;
        manager.receivedModel = model;

        manager.unregisterPanel.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(true);
        manager.unregisterPanel.GetComponentInChildren<PopUpManager>().PopupReady();
        manager.unregisterPanel.GetComponentInChildren<PopUpManager>().PanelButtonSetting(true, "unregisterText");

        manager.unregisterPanel.SetActive(true);

        //manager.checkCode.RunSaveViaData();
    }

    private void OrganizeList()
    {
        if(objects.Count > 0)
        {
            List<GameObject> tempList = new List<GameObject>();

            for (int i = 0; i < objects.Count; i++)
            {
                CodeContainer codes = objects[i].GetComponent<CodeContainer>();

                if (codes.isActive)
                {
                    tempList.Add(objects[i]);
                }
            }

            if(tempList.Count > 0)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    CodeContainer codes = objects[i].GetComponent<CodeContainer>();

                    if (!codes.isActive)
                    {
                        tempList.Add(objects[i]);
                    }
                }

                objects = tempList;
            }
        }
    }
}