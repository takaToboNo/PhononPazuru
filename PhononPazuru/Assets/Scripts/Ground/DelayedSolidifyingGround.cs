using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DelayedSolidifyingGround : MonoBehaviour
{
    [Header("設定パラメータ")]
    [Tooltip("通過してから実体化するまでの時間（秒）")]
    [SerializeField] private float solidifyDelay = 2.0f;

    [Tooltip("ターゲットにするレイヤー（Playerなど）")]
    [SerializeField] private LayerMask targetLayer;

    [Header("透明度設定")]
    [Range(0f, 1f)]
    [Tooltip("初期状態（半透明時）の不透明度（0:完全に透明 〜 1:完全に不透明）")]
    [SerializeField] private float translucentAlpha = 0.3f;

    private Collider2D myCollider;
    private SpriteRenderer myRenderer;
    private bool isTriggered = false;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        myRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 初期状態は「通り抜け可能」にする
        myCollider.isTrigger = true;

        // 元のスプライトの色（RGB）を維持したまま、透明度（A）だけを初期値にする
        if (myRenderer != null)
        {
            Color currentColor = myRenderer.color;
            currentColor.a = translucentAlpha;
            myRenderer.color = currentColor;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 接触したオブジェクトのレイヤーが、targetLayer に含まれているか判定
        if (!isTriggered && (targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            isTriggered = true;
            StartCoroutine(SolidifyRoutine());
        }
    }

    private IEnumerator SolidifyRoutine()
    {
        // 指定された秒数だけ待機
        yield return new WaitForSeconds(solidifyDelay);

        // 物理的な床（壁）にする
        myCollider.isTrigger = false;

        // 元のスプライトの色（RGB）を維持したまま、完全に不透明（1.0）にする
        if (myRenderer != null)
        {
            Color currentColor = myRenderer.color;
            currentColor.a = 1.0f;
            myRenderer.color = currentColor;
        }

        Debug.Log($"{gameObject.name} が実体化しました。");
    }

    // エディタのインスペクターで値をいじった時に、リアルタイムで半透明度をプレビューする処理
    private void OnValidate()
    {
        if (myRenderer == null) myRenderer = GetComponent<SpriteRenderer>();
        if (myRenderer != null && !Application.isPlaying)
        {
            Color currentColor = myRenderer.color;
            currentColor.a = translucentAlpha;
            myRenderer.color = currentColor;
        }
    }
}