using UnityEngine;
using System.Collections;

public class PlayerOutOfBounds : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Collider2D boundsCollider;

    [Header("リスポーン設定")]
    [SerializeField] private float deathDelay = 3.0f;

    private Vector2 startPosition;
    private Coroutine deathCoroutine;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (boundsCollider == null)
        {
            Debug.LogWarning($"{gameObject.name}: boundsCollider が設定されていません。インスペクターで CameraBounds を割り当ててください。");
        }
    }

    void Start()
    {
        startPosition = transform.position;
    }

    // オブジェクトが非アクティブ、または破棄される時にコルーチンを確実に止める
    void OnDisable()
    {
        StopDeathTimer();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 1. すでにオブジェクトが破棄されている場合は何もしない
        if (this == null) return;

        // 2. 衝突したのが指定した境界線かチェック
        if (other == boundsCollider)
        {
            if (deathCoroutine == null)
            {
                deathCoroutine = StartCoroutine(DeathTimer());
                Debug.Log("<color=orange>エリア外：カウントダウン開始</color>");
            }
        }
    }

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
            Debug.Log("<color=cyan>エリア復帰：カウントダウン停止</color>");
        }
    }

    private IEnumerator DeathTimer()
    {
        // 猶予時間を待機
        yield return new WaitForSeconds(deathDelay);

        // 3. 待機が終わった瞬間に自分がまだ存在するか最終確認
        if (this != null && gameObject.activeInHierarchy)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        // 念押しでの存在チェック
        if (this == null) return;

        transform.position = startPosition;
        deathCoroutine = null;

        if (rb != null)
        {
            // Unity 6 最新プロパティ
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.totalForce = Vector2.zero;
        }

        Debug.Log("<color=red>リスポーン完了</color>");
    }
}