using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashManager : MonoBehaviour
{
    public Image progressBar;
    public Text verText;

    IEnumerator Start()
    {
        yield return null;
        verText.text = "v."+Application.version;
        AsyncOperation op = SceneManager.LoadSceneAsync("TMPeriscope");
        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if (op.progress >= 0.9f)
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer) * 0.5f;

                if (progressBar.fillAmount.Equals(0.5f))
                    op.allowSceneActivation = true;
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer) * 0.5f;
                if (progressBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
        }
    }
}
