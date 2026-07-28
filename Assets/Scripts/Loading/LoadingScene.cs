using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;

    private void Start()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        // Start loading the Game scene quietly in the background.
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync("Game");

        // Keep the Loading scene visible until we say it can change.
        loadOperation.allowSceneActivation = false;

        float progress = 0f;

        // Fill the bar over about 3 seconds.
        while (progress < 1f || loadOperation.progress < 0.9f)
        {
            progress += Time.deltaTime / 3f;

            if (loadingSlider != null)
            {
                loadingSlider.value = Mathf.Clamp01(progress);
            }

            yield return null;
        }

        // Wait half a second when the bar reaches the end.
        yield return new WaitForSeconds(0.5f);

        // Now open the Game scene.
        loadOperation.allowSceneActivation = true;
    }
}