using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);

    // Ensure we start fully transparent
    if (fadeImage != null)
    {
        fadeImage.color = new Color(0, 0, 0, 0);
    }
}

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(Fade(1, onComplete));
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(Fade(0, onComplete));
    }

    private IEnumerator Fade(float targetAlpha, System.Action onComplete)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
        onComplete?.Invoke();
    }
}
