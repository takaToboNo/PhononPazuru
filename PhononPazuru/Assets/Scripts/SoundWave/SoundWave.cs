using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SoundWave : MonoBehaviour, IMovingPlatform
{
    [Header("基本設定")]
    public float speed = 5f;

    [Header("音量と寿命の連動設定")]
    public float volume = 1f;
    [Tooltip("音量1に対する寿命(秒)の倍率")]
    public float durationPerVolume = 3f;

    [Tooltip("横軸0〜1(時間割合)、縦軸0〜1(倍率)")]
    public AnimationCurve volumeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("レイヤー設定")]
    public LayerMask reflectLayer;
    public LayerMask absorbLayer;
    public LayerMask transparentLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;
    private float calculatedLifeTime;
    private Vector3 baseScale;
    private float baseAlpha;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        baseScale = transform.localScale;
        baseAlpha = spriteRenderer.color.a;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        SetupWave();
    }

    public void Initialize(float newVolume)
    {
        volume = newVolume;
        SetupWave();
    }

    public void SetupWave()
    {
        calculatedLifeTime = volume * durationPerVolume;
        timer = 0f;

        // 生成時の初期サイズを音量（速度）に合わせる
        transform.localScale = baseScale * volume;
    }

    void Update()
    {
        UpdateLifeCycle();
    }

    void UpdateLifeCycle()
    {
        if (calculatedLifeTime <= 0)
        {
            OnAbsorb();
            return;
        }

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / calculatedLifeTime);

        // AnimationCurveから現在の減衰率を取得
        float decay = volumeCurve.Evaluate(progress);

        // 【修正】スケールの変更を無効化（baseScaleを維持）
        // transform.localScale = baseScale * (volume * decay); 

        // 透明度のみを更新
        Color color = spriteRenderer.color;
        color.a = baseAlpha * decay;
        spriteRenderer.color = color;

        if (progress >= 1f)
        {
            OnAbsorb();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = transform.right * speed;
    }

    public Vector2 GetVelocity()
    {
        return transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layer = collision.gameObject.layer;
        if (((1 << layer) & reflectLayer) != 0)
        {
            Vector2 incoming = transform.right;
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(incoming, normal);

            float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
            rb.linearVelocity = reflectDir * speed;
        }
        else if (((1 << layer) & absorbLayer) != 0)
        {
            OnAbsorb();
        }
        else if (((1 << layer) & transparentLayer) != 0)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
        }
    }

    private void OnAbsorb()
    {
        transform.DetachChildren();
        Destroy(gameObject);
    }
}