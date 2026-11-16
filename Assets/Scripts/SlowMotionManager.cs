
using UnityEngine;
using System.Collections;

public class SlowMotionManager : MonoBehaviour
{
    public static SlowMotionManager Instance { get; private set; }

    [Header("Slow Motion")]
    [SerializeField, Range(0.05f, 1f)]
    private float slowScale = 0.35f;

    [SerializeField]
    private float slowDuration = 0.15f;

    [SerializeField]
    private float cooldown = 1f;

    private Coroutine slowCoroutine;
    private float lastSlowTime = -999f;

    private bool isSlowMotion = false;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySlowMotion()
    {
        // ポーズ中は発動しない
        if (Time.timeScale == 0f)
        {
            return;
        }

        // 連続発動防止
        if (Time.unscaledTime - lastSlowTime < cooldown)
        {
            return;
        }

        if (isSlowMotion)
        {
            return;
        }

        lastSlowTime = Time.unscaledTime;

        slowCoroutine = StartCoroutine(
            SlowMotionRoutine()
        );
    }

    private IEnumerator SlowMotionRoutine()
    {
        isSlowMotion = true;

        Time.timeScale = slowScale;

        // スローモーション中の物理更新間隔を調整
        float originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.fixedDeltaTime =
            originalFixedDeltaTime * slowScale;

        float elapsed = 0f;

        while (elapsed < slowDuration)
        {
            // ポーズが発生した場合は解除
            if (Time.timeScale == 0f)
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        // ポーズ中なら速度を戻さない
        if (Time.timeScale != 0f)
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = originalFixedDeltaTime;

        isSlowMotion = false;
        slowCoroutine = null;
    }

    private void OnDisable()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
            slowCoroutine = null;
        }

        if (Time.timeScale != 0f)
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = 0.02f;
        isSlowMotion = false;
    }
}
