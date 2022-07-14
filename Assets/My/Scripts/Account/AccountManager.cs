using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class AccountManager : MonoBehaviour
{
    public GameObject accountPanel, loginAndSignUp, loginPanel, signUpPanel, qrPanel, qrInfoPanel, logoutPanel, unregisterPanel, registToastPanel;

    [HideInInspector]
    public CheckCode checkCode;
    PrefabShelter prefabShelter;
    CanvasManager canvasManager;
    PopUpManager popUpManager;
    FileDownloader fileDownloader;

    public Button resendButton;
    public bool isQRInfoPanel;
    Toggle qrToggle;

    [Header("Account page Localized objects")]
    public Text deviceNumbering;

    [Header("Account page Unlocalized objects")]
    public Text names;
    public Text id;
    public Text email;
    public Text date;
    public Image type;
    public SerialGroup group2;

    [HideInInspector]
    public string receivedCode;
    [HideInInspector]
    public string receivedIdentifier;
    [HideInInspector]
    public string receivedModel;


    #region AWAKE_and_QUIT
    private void Awake()
    {
        Button[] accountButton = accountPanel.GetComponentsInChildren<Button>();
        foreach (Button button in accountButton)
        {
            button.onClick.AddListener(() => AccountButton(button));
        }

        Button[] signPanelButton = loginAndSignUp.GetComponentsInChildren<Button>();
        foreach (Button button in signPanelButton)
        {
            button.onClick.AddListener(() => LoginAndSignUpButton(button));
        }

        Button[] loginButton = loginPanel.GetComponentsInChildren<Button>();
        foreach (Button button in loginButton)
        {
            button.onClick.AddListener(() => LoginPanelButton(button));
        }

        Button[] signUpButton = signUpPanel.GetComponentsInChildren<Button>();
        foreach (Button button in signUpButton)
        {
            button.onClick.AddListener(() => SignUpPanelButton(button));
        }

        Button[] serialButton = qrPanel.GetComponentsInChildren<Button>();
        foreach (Button button in serialButton)
        {
            button.onClick.AddListener(() => QRButtonController(button, null));
        }

        Button[] logoutButton = logoutPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in logoutButton)
        {
            btn.onClick.AddListener(() => LogoutController(btn));
        }

        Button[] unregisterButton = unregisterPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in unregisterButton)
        {
            btn.onClick.AddListener(() => UnregisterController(btn));
        }

        Button[] registToastButton = registToastPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in registToastButton)
        {
            btn.onClick.AddListener(() => RegistToastController(btn));
        }

        qrToggle = qrInfoPanel.GetComponentInChildren<Toggle>();
        qrToggle.onValueChanged.AddListener(delegate
        {
            QRInfoPanelValueChanged(qrToggle.isOn);
        });

        AwakeSetting();
    }

    private void OnApplicationQuit()
    {
        Button[] accountButton = accountPanel.GetComponentsInChildren<Button>();
        foreach (Button button in accountButton)
        {
            button.onClick.RemoveListener(() => AccountButton(button));
        }

        Button[] signPanelButton = loginAndSignUp.GetComponentsInChildren<Button>();
        foreach (Button button in signPanelButton)
        {
            button.onClick.RemoveListener(() => LoginAndSignUpButton(button));
        }

        Button[] loginButton = loginPanel.GetComponentsInChildren<Button>();
        foreach (Button button in loginButton)
        {
            button.onClick.RemoveListener(() => LoginPanelButton(button));
        }

        Button[] signUpButton = signUpPanel.GetComponentsInChildren<Button>();
        foreach (Button button in signUpButton)
        {
            button.onClick.RemoveListener(() => SignUpPanelButton(button));
        }

        Button[] serialButton = qrPanel.GetComponentsInChildren<Button>();
        foreach (Button button in serialButton)
        {
            button.onClick.RemoveListener(() => QRButtonController(button, null));
        }

        Button[] logoutButton = logoutPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in logoutButton)
        {
            btn.onClick.RemoveListener(() => LogoutController(btn));
        }

        Button[] unregisterButton = unregisterPanel.GetComponentsInChildren<Button>();
        foreach (Button btn in unregisterButton)
        {
            btn.onClick.RemoveListener(() => UnregisterController(btn));
        }

        if (registToastPanel.activeSelf)
        {
            Button[] registToastButton = registToastPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in registToastButton)
            {
                btn.onClick.RemoveListener(() => RegistToastController(btn));
            }
        }
    }

    #endregion

    private void AwakeSetting()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        checkCode = FindObjectOfType<CheckCode>();
        prefabShelter = FindObjectOfType<PrefabShelter>();
        fileDownloader = FindObjectOfType<FileDownloader>();

        accountPanel.SetActive(false);
        loginAndSignUp.SetActive(false);
        signUpPanel.SetActive(false);
        qrPanel.SetActive(false);
        logoutPanel.SetActive(false);
    }

    //Home버튼이나 Menu버튼을 눌렀을때
    void OnApplicationFocus(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (qrPanel.activeSelf)
                qrPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            loginAndSignUp.SetActive(false);
            signUpPanel.SetActive(false);
            qrPanel.SetActive(false);
            logoutPanel.SetActive(false);
            registToastPanel.SetActive(false);
        }
    }


    public void SerialCheck(bool scanAfter)
    {
        if (checkCode.loginConfirm)
        {
            string modelPath = string.Format("{0}/assets/tagme3d_card_asset_unfree", Application.persistentDataPath);
            if (!File.Exists(modelPath))
            {
                fileDownloader.CheckFile();
            }


            accountPanel.GetComponentInChildren<PopUpManager>().PopupReady();
            accountPanel.SetActive(true);
            canvasManager.BottomBarOpen();
            DeviceNumbering();

            qrToggle.isOn = isQRInfoPanel;
            qrInfoPanel.GetComponent<QRPanelManager>().qrToggle = qrToggle;
            if (checkCode.storedType.Equals("admin"))
            {
                registToastPanel.GetComponent<PopUpManager>().ToastMessage(false, "loginAdmin");
                registToastPanel.SetActive(true);
            }
            else if (checkCode.codeConfirms.Equals(string.Empty))
            {
                if (checkCode.storedType.Equals("normal"))
                {
                    if (scanAfter)
                        QRScanStart();

                    if (!isQRInfoPanel)
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(true);
                }
                else
                {
                    if (!isQRInfoPanel)
                        qrInfoPanel.GetComponent<QRPanelManager>().DeviceInfoLocal(true);
                }
            }

            checkCode.WriteAccount(names, id, email, date, type, group2);
        }
        else
        {
            loginPanel.GetComponentInChildren<PopUpManager>().PopupReady();

#if UNITY_IOS
            loginAndSignUp.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
#endif

            loginAndSignUp.SetActive(true);
            loginPanel.SetActive(true);
            signUpPanel.SetActive(false);
            loginAndSignUp.GetComponentInChildren<PanelSize>().Resizer();
        }
    }

    private void DeviceNumbering()
    {
        deviceNumbering.text = deviceNumbering.text.Replace("*", checkCode.storedSerial.Count.ToString());
    }


    #region REGISTER_CONTROLLER

    //AccountPanel: 비밀번호변경,ok,cancel,로그아웃,QR 버튼
    private void AccountButton(Button temp)
    {
        popUpManager = accountPanel.GetComponentInChildren<PopUpManager>();

        switch (temp.name)
        {
            case "btn_logout":
                logoutPanel.GetComponentInChildren<PopUpManager>().PopupReady();
                logoutPanel.GetComponentInChildren<PopUpManager>().PanelButtonSetting(true, "logoutText");
                logoutPanel.SetActive(true);

                break;
            case "btn_QR":
                //My Page - QR 버튼 클릭
                if (checkCode.storedType.Equals("admin"))
                {
                    fileDownloader.CheckFile();

                    //registToastPanel.GetComponent<PopUpManager>().ToastMessage(false, "loginAdmin");
                    //registToastPanel.SetActive(true);
                }
                else if (checkCode.storedType.Equals("normal"))
                {
                    temp.interactable = false;
                    //다른기기에 QR등록되어 있는지 체크 → QR스캔없이 등록
                    bool finCheck = false;
                    int codeIndex = -1;
                    for (int i = 0; i < checkCode.storedSerial.Count; i++)
                    {
                        if (checkCode.storedSerial[i].book.Contains("flash_cn"))
                        {
                            if (checkCode.storedSerial[i].isUsing)
                            {
                                finCheck = true;
                                break;
                            }
                            if (!(checkCode.storedSerial[i].deviceAExist && checkCode.storedSerial[i].deviceBExist))
                            {
                                codeIndex = i;
                            }
                        }
                    }

                    if (!finCheck)
                    {
                        if (codeIndex.Equals(-1))
                        {
                            QRScanStart();
                            qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(!isQRInfoPanel);

                            temp.interactable = true;
                        }
                        else
                        {
                            QRButtonController(temp, checkCode.storedSerial[codeIndex].code);
                        }
                    }
                    else
                    {
                        registToastPanel.GetComponent<PopUpManager>().ToastMessage(false, "DeviceAlready");
                        registToastPanel.SetActive(true);

                        temp.interactable = true;
                    }
                }
                else
                {
                    if (!checkCode.codeConfirms.Equals(string.Empty))
                    {
                        registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "DeviceAlready");
                        registToastPanel.SetActive(true);
                    }
                    else
                    {
                        temp.interactable = false;
                        //멤버십 사용자 기기 등록이 안된경우 → 기기등록(서버)
                        StartCoroutine(checkCode.AuthorizationMembership(returned =>
                        {
                            //print("           " + returned);
                            switch (returned.Split('\n')[0])
                            {
                                case "Successfully registered":
                                case "The Device is already registered":
                                    //기기등록 웹 동기화해주고 잠긴 페이지 풀어주기
                                    registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "DeviceRegiComplete");

                                    fileDownloader.CheckFile();

                                    break;
                                case "error":
                                    registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "connectFail");

                                    break;
                                case "Unvaild Serial code":
                                    registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "serialFailUnvalid");

                                    break;
                                case "exceeded the maximum number of registratoin":
                                    registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "serialFailExceed");

                                    break;
                                case "Another user has already been registered":
                                    registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "serialFailAnother");

                                    break;
                            }
                            registToastPanel.SetActive(true);

                            temp.interactable = true;
                        }));
                    }
                }
                break;
        }
    }

    //로그인, 회원가입 패널 닫기
    private void LoginAndSignUpButton(Button button)
    {
        switch (button.name)
        {
            case "BGButton":
                button.transform.parent.gameObject.SetActive(false);

                break;
        }
    }


    //로그인
    private void LoginPanelButton(Button temp)
    {
        popUpManager = loginPanel.GetComponentInChildren<PopUpManager>();

        switch (temp.transform.name)
        {
            case "findAccount":
                Application.OpenURL("http://bookplusapp.com/reset-pwd-req.php");

                break;
            case "resend":
                Application.OpenURL("http://bookplusapp.com/resend_confirm.php");
                //popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_resendText");

                break;
            case "btn_signUp":
                loginPanel.gameObject.SetActive(false);

                signUpPanel.GetComponentInChildren<PopUpManager>().PopupReady();
                signUpPanel.SetActive(true);
                loginAndSignUp.GetComponent<PanelSize>().Resizer();

                //GameObject clone = Instantiate(Resources.Load<GameObject>("prefabs/User Agreement"), canvasManager.transform, false);
                //clone.GetComponent<UserAgreement>().StartSet(false);

                break;
            case "btn_proceed":
                //================================================================================= DB체크 해서 시리얼패스!!
                InputFieldGroup input = loginPanel.GetComponentInChildren<InputFieldGroup>();

                if (input.inputs[0].text.Equals(string.Empty) || input.inputs[1].text.Equals(string.Empty))
                {
                    popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_Empty");
                }
                else
                {
                    temp.interactable = false;
                    StartCoroutine(checkCode.StartLogin(input.inputs[0].text, input.inputs[1].text, "tm_flash_cn", returned =>
                    {
                        switch (returned)
                        {
                            case "error":
                                popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_connectFail");

                                break;
                            case "Wrong UserID":
                                popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_loginFailID");

                                break;
                            case "Wrong Password":
                                popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_loginFailPass");

                                break;
                            case "Email has not been verified":
                                popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_emailVerified");

                                resendButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("UI_resend");
                                resendButton.GetComponentInChildren<Text>().font = Resources.Load<Font>(LocalizationManager.GetTermTranslation("UI_font"));

                                resendButton.gameObject.SetActive(true);

                                break;
                            default:
                                if (returned.Contains("Login Successfully "))
                                {
                                    temp.interactable = true;
                                    loginAndSignUp.SetActive(false);

                                    List<string> wwwList = returned.Split('\n').ToList();

                                    for (int i = 0; i < wwwList.Count; i++)
                                    {
                                        if (wwwList[i].Contains("Admin account verified"))
                                        {
                                            registToastPanel.GetComponent<PopUpManager>().ToastMessage(false, "loginAdmin");
                                            registToastPanel.SetActive(true);

                                            break;
                                        }
                                    }

                                    SerialCheck(false);

                                    if (checkCode.codeConfirms.Equals(string.Empty))
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        fileDownloader.CheckFile();
                                    }
                                }
                                break;
                        }
                        temp.interactable = true;
                    }));
                }

                break;
        }
    }

    private void InputReset(GameObject go)
    {
        InputFieldGroup input = go.GetComponentInChildren<InputFieldGroup>();
        for (int i = 0; i < input.inputs.Length; i++)
        {
            input.inputs[i].text = string.Empty;
        }
    }


    //회원가입
    private void SignUpPanelButton(Button temp)
    {
        popUpManager = signUpPanel.GetComponentInChildren<PopUpManager>();
        InputFieldGroup input = signUpPanel.GetComponentInChildren<InputFieldGroup>();

        switch (temp.name)
        {
            case "btn_login":
                signUpPanel.SetActive(false);

                loginPanel.GetComponentInChildren<PopUpManager>().PopupReady();
                loginPanel.SetActive(true);

                loginAndSignUp.GetComponent<PanelSize>().Resizer();

                break;
            case "btn_proceed":
                string email = string.Format("{0}@{1}", input.inputs[3].text, input.inputs[4].text);
                if (email.Equals("@"))
                    email = string.Empty;

                if (input.inputs[0].text.Equals(string.Empty) || input.inputs[1].text.Equals(string.Empty) || input.inputs[2].text.Equals(string.Empty)
                    || email.Equals(string.Empty))
                {
                    popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_Empty");
                }
                else
                {
                    temp.interactable = false;
                    //id,  pw,  name, email
                    StartCoroutine(checkCode.StartSignUp(input.inputs[0].text, input.inputs[1].text, input.inputs[2].text, email, returned =>
                    {
                        if (returned.Equals("Success_Regist"))
                        {
                            InputReset(signUpPanel);
                            signUpPanel.SetActive(false);

                            loginPanel.GetComponentInChildren<PopUpManager>().PopupReady();
                            loginPanel.SetActive(true);
                            InputReset(loginPanel);

                            loginAndSignUp.GetComponent<PanelSize>().Resizer();

                            loginPanel.GetComponentInChildren<PopUpManager>().failText.text = string.Format("{0}\n{1}",
                                LocalizationManager.GetTermTranslation(string.Format("UI_{0}_mail", returned)), email);
                        }
                        else if (returned.Equals("error"))
                        {
                            popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_connectFail");
                        }
                        else
                        {
                            popUpManager.failText.text = LocalizationManager.GetTermTranslation(string.Format("UI_{0}", returned));
                        }
                        temp.interactable = true;
                    }));
                }
                break;
        }
    }

    //QR 스캔
    public void QRButtonController(Button temp, string code)
    {
        popUpManager = qrPanel.GetComponentInChildren<PopUpManager>();

        if (code == null)
        {
            switch (temp.transform.name)
            {
                case "BGButton":
                    temp.transform.parent.gameObject.SetActive(false);

                    return;
                case "btn_InfoOn":
                    if (checkCode.storedType.Equals("normal"))
                    {
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(true);
                    }

                    return;
                case "btn_close":
                    qrPanel.SetActive(false);

                    return;
            }
        }
        else
        {
            QRCodeReaderDemo demos = qrPanel.GetComponentInChildren<QRCodeReaderDemo>();

            StartCoroutine(checkCode.StartAuthorization(checkCode.storedID, checkCode.storedPW, code, checkCode.storedType, returned =>
            {
                string splitted = returned.Split('\n')[0];
                //print("     뭐오냐 " + returned);

                switch (splitted)
                {
                    case "Successfully registered":
                    case "The Device is already registered":
                        qrPanel.SetActive(false);
                        qrInfoPanel.SetActive(false);

                        //QR인증완료 후 다운로드시작
                        fileDownloader.CheckFile();

                        //성공메시지
                        registToastPanel.GetComponentInChildren<PopUpManager>().ToastMessage(false, "successQR");
                        registToastPanel.SetActive(true);

                        break;
                    case "error":
                        QRScanStart();
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(!isQRInfoPanel);
                        popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_connectFail");
                        demos.StartScanning();

                        break;
                    case "Unvaild Serial code":
                        QRScanStart();
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(!isQRInfoPanel);
                        popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_serialFailUnvalid");
                        demos.StartScanning();

                        break;
                    case "exceeded the maximum number of registratoin":
                        QRScanStart();
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(!isQRInfoPanel);
                        popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_serialFailExceed");
                        demos.StartScanning();

                        break;
                    case "Another user has already been registered":
                        QRScanStart();
                        qrInfoPanel.GetComponent<QRPanelManager>().QRInfoLocal(!isQRInfoPanel);
                        popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_serialFailAnother");
                        demos.StartScanning();

                        break;
                    default:

                        break;
                }
                temp.interactable = true;
            }));
        }
    }
    #endregion


    // 기기해제
    private void UnregisterController(Button btn)
    {
        popUpManager = unregisterPanel.GetComponentInChildren<PopUpManager>();

        switch (btn.name)
        {
            case "btn_yes":
                StartCoroutine(checkCode.UnregisterDevice(receivedCode, receivedIdentifier, receivedModel, returned =>
                {
                    if (returned.Equals("error"))
                    {
                        popUpManager.failText.text = LocalizationManager.GetTermTranslation("UI_connectFail");
                    }
                    else
                    {
                        popUpManager.PanelButtonSetting(false, "unregisterSuccess");
                        unregisterPanel.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
                        SerialCheck(false);

                        foreach (TMModel tm in prefabShelter.tmModel)
                        {
                            tm.isConfirm = false;
                        }
                    }
                }));

                break;
            case "btn_no":
            case "btn_ok":
                unregisterPanel.SetActive(false);

                break;
        }
    }

    //기기등록관련 토스트메시지
    private void RegistToastController(Button btn)
    {
        popUpManager = registToastPanel.GetComponentInChildren<PopUpManager>();

        switch (btn.name)
        {
            case "btn_yes":

                break;
            case "btn_no":
            case "btn_ok":
                registToastPanel.SetActive(false);

                break;
        }
    }

    //로그아웃
    private void LogoutController(Button btn)
    {
        popUpManager = logoutPanel.GetComponentInChildren<PopUpManager>();

        switch (btn.name)
        {
            case "btn_yes":
                isQRInfoPanel = false;
                checkCode.Logout();

                for (int i = 0; i < prefabShelter.tmModel.Length; i++)
                {
                    if (prefabShelter.tmModel[i] != null)
                        prefabShelter.tmModel[i].isConfirm = false;
                }

                popUpManager.PanelButtonSetting(false, "logoutSuccess");

                break;
            case "btn_no":
                logoutPanel.SetActive(false);

                break;
            case "btn_ok":
                logoutPanel.SetActive(false);
                canvasManager.PanelManager(false);
                AllDisable();

                break;
        }
    }


    //QR스캔 시작
    private void QRScanStart()
    {
        qrPanel.SetActive(true);
        QRCodeReaderDemo demos = qrPanel.GetComponentInChildren<QRCodeReaderDemo>();
        demos.StartScanning();
        qrPanel.GetComponent<PopUpManager>().PopupReady();
        qrPanel.GetComponent<PopUpManager>().failText.text = LocalizationManager.GetTermTranslation("UI_connectText");
    }

    public void CameraSwap(bool serialTrue)
    {
        if (serialTrue)
        {
            qrPanel.SetActive(serialTrue);
            QRCodeReaderDemo demos = qrPanel.GetComponentInChildren<QRCodeReaderDemo>();
            demos.StartScanning();
            qrPanel.GetComponent<PopUpManager>().failText.text = string.Empty;
        }
        else
        {
            qrPanel.SetActive(!serialTrue);
        }

    }

    //QR안내창 토글: true=앞으로 안띄움
    private void QRInfoPanelValueChanged(bool isOn)
    {
        isQRInfoPanel = isOn;
        checkCode.SaveCurrentUiLanguage();
    }

    public void AllDisable()
    {
        accountPanel.SetActive(false);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
        logoutPanel.SetActive(false);
        qrPanel.SetActive(false);
        unregisterPanel.SetActive(false);
    }
}