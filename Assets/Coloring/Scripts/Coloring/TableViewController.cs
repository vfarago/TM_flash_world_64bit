using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SJ.MathFun
{
    public class TableViewController : MonoBehaviour
    {
        public string[] drawings;  //썸네일, 이미지이름
        public GameObject contentPanel;
        public GameObject tableViewCellPrefab;
        public GameObject toastPanel;

        ColoringController coloringController;

        private void Awake()
        {
            coloringController = FindObjectOfType<ColoringController>();

            Button[] toastbuttons = toastPanel.GetComponentsInChildren<Button>();
            foreach (Button button in toastbuttons)
            {
                button.onClick.AddListener(() => LockPressedController(button));
            }
        }

        void Start()
        {
            for (int i = 0; i < drawings.Length; i++)
            {
                GameObject go = Instantiate(tableViewCellPrefab, contentPanel.transform, false);
                go.name = drawings[i];

                string path = string.Format("{0}/drawImage/saved-{1}.png", Application.persistentDataPath, drawings[i]);
                if (File.Exists(path))
                {
                    byte[] data = File.ReadAllBytes(path);
                    Texture2D texture = new Texture2D(300, 200, TextureFormat.ARGB32, false);
                    texture.LoadImage(data);
                    go.transform.GetChild(0).GetComponent<Image>().sprite =
                        Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)); //Texture2D , Rect, Pivot
                }
                else
                {
                    go.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(string.Format("coloring/button/{0}", drawings[i]));
                }

                if (i < 9)
                    go.GetComponent<TableViewCellController>().CheckUnlock(true);
                else
                    go.GetComponent<TableViewCellController>().CheckUnlock(false);

                go.GetComponent<Button>().onClick.AddListener(() => OnCickCell(go));

                if (i % 3 == 0)
                {
                    RectTransform rt = contentPanel.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 340f);
                }
            }
        }

        private void OnDestroy()
        {
            Button[] toastbuttons = toastPanel.GetComponentsInChildren<Button>();
            foreach (Button button in toastbuttons)
            {
                button.onClick.RemoveListener(() => LockPressedController(button));
            }
        }


        private void LockPressedController(Button button)
        {
            switch (button.name)
            {
                case "btn_Scan":
                    LoadSceneManager.instance.ChangeScene(false, true);

                    break;
                case "btn_Cancel":
                    toastPanel.SetActive(false);

                    break;
            }
        }

        public void UpdateThumbnails()
        {
            GameObject go = null;
            TableViewCellController[] tvc = contentPanel.GetComponentsInChildren<TableViewCellController>();
            for (int i = 0; i < tvc.Length; i++)
            {
                if (tvc[i].name.Equals(coloringController.currentImageName))
                {
                    go = tvc[i].gameObject;
                    break;
                }
            }

            if (go != null)
            {
                string path = string.Format("{0}/drawImage/saved-{1}.png", Application.persistentDataPath, go.name);
                if (File.Exists(path))
                {
                    byte[] data = File.ReadAllBytes(path);
                    Texture2D texture = new Texture2D(300, 200, TextureFormat.ARGB32, false);
                    texture.LoadImage(data);
                    go.transform.GetChild(0).GetComponent<Image>().sprite =
                        Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)); //Texture2D , Rect, Pivot
                }
                else
                {
                    go.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(string.Format("coloring/button/{0}", go.name));
                }
            }
        }

        private void OnCickCell(GameObject go)
        {
            //print("OnCickCell drawingID:" + controller.name.text);
            if (go.GetComponent<TableViewCellController>().isLocked)
            {
                toastPanel.SetActive(true);
            }
            else
            {
                coloringController.OnClickTableViewCell(go.name);
            }
        }
    }
}