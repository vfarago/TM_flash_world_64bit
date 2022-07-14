using System.IO;

using UnityEngine;

namespace SJ.MathFun
{
    public class TableViewCellController : MonoBehaviour
    {
        public bool isLocked = true;
        public GameObject lockIcon;

        public void CheckUnlock(bool free)
        {
            if (free)
            {
                isLocked = false;
            }
            else
            {
                string path = string.Format("{0}/Data/TrackingObj", Application.persistentDataPath);
                if (File.Exists(path))
                {
                    StreamReader read = new StreamReader(path);
                    string readAll = read.ReadToEnd();
                    read.Close();
                    string[] readSplit = readAll.Split('\n');

                    for (int i = 0; i < readSplit.Length; i++)
                    {
                        if (readSplit[i].Contains(gameObject.name))
                        {
                            isLocked = false;
                            break;
                        }
                    }
                }
            }

            lockIcon.SetActive(isLocked);
        }
    }
}