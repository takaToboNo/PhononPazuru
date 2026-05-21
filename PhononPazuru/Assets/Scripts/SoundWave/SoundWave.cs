using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class SoundWave : MonoBehaviour, IMovingPlatform
{
    [Header("基本設定")]
    public float speed = 5f;
    public float volume = 1f;
    public float durationPerVolume = 3f;
    public AnimationCurve volumeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("レイヤー設定")]
    public LayerMask reflectLayer;
    public LayerMask absorbLayer;
    public LayerMask transparentLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float calculatedLifeTime;
    private float timer;
    private Vector3 baseScale;
    private float baseAlpha;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseAlpha = spriteRenderer.color.a;

        // ※ Rigidbody2Dの設定はインスペクターで行う前提でコードからは削除
    }

    // 生成側から確実に一度だけ呼ばれる
    public void SetupWave()
    {
        calculatedLifeTime = volume * durationPerVolume;
        transform.localScale = baseScale * volume;
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / calculatedLifeTime);

        // 透明度の更新
        float decay = volumeCurve.Evaluate(progress);
        Color color = spriteRenderer.color;
        color.a = baseAlpha * decay;
        spriteRenderer.color = color;

        if (progress >= 1f) Destroy(gameObject);
    }

    // IMovingPlatform の実装
    public Vector2 GetVelocity()
    {
        return transform.right * speed;  // FixedUpdate と同じ速度を返す
    }

    void FixedUpdate()
    {
        rb.linearVelocity = transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;

        if (((1 << layer) & reflectLayer) != 0)
        {
            Vector2 reflectDir = Vector2.Reflect(transform.right, collision.contacts[0].normal);
            transform.right = reflectDir; // 向きを変えるだけでFixedUpdateが移動を処理する
        }
        else if (((1 << layer) & absorbLayer) != 0)
        {
            Destroy(gameObject);
        }
        else if (((1 << layer) & transparentLayer) != 0)
        {
            // レイヤー設定で解決できない場合のみPhysics2D.IgnoreCollisionを使用
        }
    }
}