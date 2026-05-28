using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoSequence : MonoBehaviour
{
    [Header("フェード設定")]
    [SerializeField] private CanvasGroup logoCanvasGroup;
    [SerializeField] private float startDelayDuration = 0.5f; // 追加: 開始前の待ち時間
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float waitDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float endDelayDuration = 0.5f;   // 追加: 終了後の待ち時間

    [Header("イージング設定")]
    [Tooltip("左から右へ上がっていくカーブ（0から1へ変化）を設定してください")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("遷移先シーン名")]
    [SerializeField] private string nextSceneName = "TitleScene";

    private void Start()
    {
        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.alpha = 0f;
            StartCoroutine(DoLogoSequence());
        }
        else
        {
            Debug.LogError("CanvasGroupがアタッチされていません！");
        }
    }

    private IEnumerator DoLogoSequence()
    {
        // 0. フェードイン開始前の待ち時間 (追加)
        if (startDelayDuration > 0f)
        {
            yield return new WaitForSeconds(startDelayDuration);
        }

        // 1. フェードイン (カーブの値をそのまま使う: 0 → 1)
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration; // 0 ～ 1 に正規化
            logoCanvasGroup.alpha = easeCurve.Evaluate(progress);
            yield return null;
        }
        logoCanvasGroup.alpha = 1f;

        // 2. 表示キープの待機
        yield return new WaitForSeconds(waitDuration);

        // 3. フェードアウト (カーブの値を反転して使う: 1 → 0)
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration; // 0 ～ 1 に正規化
            logoCanvasGroup.alpha = 1f - easeCurve.Evaluate(progress);
            yield return null;
        }
        logoCanvasGroup.alpha = 0f;

        // 3.5. フェードアウト終了後、シーン遷移するまでの待ち時間 (追加)
        if (endDelayDuration > 0f)
        {
            yield return new WaitForSeconds(endDelayDuration);
        }

        // 4. タイトルシーンへ遷移
        SceneManager.LoadScene(nextSceneName);
    }
}