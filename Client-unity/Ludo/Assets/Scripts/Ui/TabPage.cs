using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TabPage : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float startScale = 0.95f;

    private CanvasGroup _canvasGroup;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        gameObject.SetActive(true);
        _transitionCoroutine = StartCoroutine(ShowCoroutine());
    }

    public void Hide()
    {
        if (_transitionCoroutine != null)
            StopCoroutine(_transitionCoroutine);

        gameObject.SetActive(false);
    }

    private IEnumerator ShowCoroutine()
    {
        float time = 0f;

        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * startScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);

            // Smooth transition
            t = Mathf.SmoothStep(0f, 1f, t);

            _canvasGroup.alpha = t;
            transform.localScale = Vector3.Lerp(
                Vector3.one * startScale,
                Vector3.one,
                t
            );

            yield return null;
        }

        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;

        _transitionCoroutine = null;
    }
}