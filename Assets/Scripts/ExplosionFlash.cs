
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExplosionFlash : MonoBehaviour
{
    public static ExplosionFlash Instance { get; private set; }

    [SerializeField] private Image flashImage;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float maxAlpha = 0.45f;

    private Coroutine flashCoroutine;

    private void Awake()
    {
        Instance = this;

        SetAlpha(0f);
    }

    public void Flash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(
            FlashRoutine()
        );
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / flashDuration
            );

            float alpha = Mathf.Lerp(
                maxAlpha,
                0f,
                t
            );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);

        flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (flashImage == null)
        {
            return;
        }

        Color color = flashImage.color;
        color.a = alpha;

        flashImage.color = color;
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        SetAlpha(0f);
    }
}
