using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class loadingScript : MonoBehaviour {
    public GameObject loadingScreen;
    public Slider slider;
    public Image fadeOverlay;
    public Text textStatus;
    public float fadingTime;
    
    public void loadlevel (int sceneIndex)
    {
        StartCoroutine(loadScene(sceneIndex));
    }

    IEnumerator loadScene (int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        loadingScreen.SetActive(true);
        fadeOverlay.gameObject.SetActive(true);
        yield return null;
        fadeOverlay.CrossFadeAlpha(0, fadingTime, true);
        operation.allowSceneActivation = false;
        while (operation.isDone == false)
        {
            float progress = Mathf.Clamp01(operation.progress/0.91f);
            slider.value = progress;
            textStatus.text = (progress*100f).ToString("F0")  + "%";
            yield return null;

            if (operation.progress == 0.9f)
            {
                fadeOverlay.CrossFadeAlpha(1, fadingTime, true);
                //yield return new WaitForSeconds(fadingTime);
                operation.allowSceneActivation = true;
            }
        }
        
    }
}
