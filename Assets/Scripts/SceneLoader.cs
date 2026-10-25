using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates itself automatically when the game starts (no scene setup needed),
/// survives scene loads, and fades to black between levels.
/// Use: SceneLoader.LoadNext();  SceneLoader.Reload();  SceneLoader.Load(index);
/// </summary>
public class SceneLoader : MonoBehaviour
{
    const float FadeTime = 0.4f;

    static SceneLoader instance;

    CanvasGroup fade;
    bool loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("SceneLoader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneLoader>();
        instance.BuildFadeOverlay();
    }

    void BuildFadeOverlay()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        fade = gameObject.AddComponent<CanvasGroup>();
        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        fade.interactable = false;

        var imageObj = new GameObject("Fade");
        imageObj.transform.SetParent(transform, false);

        var image = imageObj.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void LoadNext()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0;   // after the last level, back to the first
        Load(next);
    }

    public static void Reload()
    {
        Load(SceneManager.GetActiveScene().buildIndex);
    }

    public static void Load(int buildIndex)
    {
        if (instance == null || instance.loading) return;
        instance.StartCoroutine(instance.LoadRoutine(buildIndex));
    }

    IEnumerator LoadRoutine(int buildIndex)
    {
        loading = true;

        yield return FadeTo(1f);
        yield return SceneManager.LoadSceneAsync(buildIndex);
        yield return FadeTo(0f);

        loading = false;
    }

    IEnumerator FadeTo(float target)
    {
        float start = fade.alpha;
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            fade.alpha = Mathf.Lerp(start, target, t / FadeTime);
            yield return null;
        }
        fade.alpha = target;
    }
    

}

