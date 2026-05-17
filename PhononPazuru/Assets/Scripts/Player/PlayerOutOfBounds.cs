using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // 1. これを追加

public class PlayerOutOfBounds : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Collider2D boundsCollider;

    [Header("リロード設定")]
    [SerializeField] private float deathDelay = 3.0f;

    private Coroutine deathCoroutine;

    void Awake()
    {
        if (boundsCollider == null)
        {
            Debug.LogWarning($"{gameObject.name}: boundsCollider が設定されていません。");
        }
    }

    // エリア外に出た時の処理
    private void OnTriggerExit2D(Collider2D other)
    {
        if (this == null) return;

        if (other == boundsCollider)
        {
            if (deathCoroutine == null)
            {
                deathCoroutine = StartCoroutine(DeathTimer());
                Debug.Log("<color=orange>エリア外：リロードカウント開始</color>");
            }
        }
    }

    // エリア内に戻った時の処理
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (this == null) return;

        if (other == boundsCollider)
        {
            StopDeathTimer();
        }
    }

    private void StopDeathTimer()
    {
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
            Debug.Log("<color=cyan>エリア復帰：リロード中止</color>");
        }
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(deathDelay);

        if (this != null && gameObject.activeInHierarchy)
        {
            RestartScene(); // 2. リスタートを実行
        }
    }

    // 3. シーンを最初からやり直すメソッド
    public void RestartScene()
    {
        Debug.Log("<color=red>エリア外によるシーンリロード</color>");
        // 現在のシーン名を取得してロード（最初からになる）
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDisable()
    {
        StopDeathTimer();
    }
}