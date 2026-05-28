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

        if (spriteRenderer != null)
            baseAlpha = spriteRenderer.color.a;

        // --- ここを変更・修正 ---
        rb.bodyType = RigidbodyType2D.Kinematic;

        // trueにすることで、Kinematicな音波がプレイヤー（Dynamic）を物理的に押し上げます
        rb.useFullKinematicContacts = true;
    }

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

        float decay = volumeCurve.Evaluate(progress);
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = baseAlpha * decay;
            spriteRenderer.color = color;
        }

        if (progress >= 1f) Destroy(gameObject);
    }

    public Vector2 GetVelocity()
    {
        return transform.right * speed;
    }

    void FixedUpdate()
    {
        // 速度の代入
        rb.linearVelocity = transform.right * speed;

        //Vector2 moveStep = transform.right * speed * Time.fixedDeltaTime;
        //rb.MovePosition(rb.position + moveStep);
    }

    // ★ 1. 親コライダー（見た目通り）：Playerやギミック用の判定

    private void OnTriggerEnter2D(Collider2D collider)
    {
        int layer = collider.gameObject.layer;

        // ここでは壁レイヤー（reflect, absorb）は【完全に無視】する
        if (((1 << layer) & reflectLayer) != 0 || ((1 << layer) & absorbLayer) != 0)
        {
            return;
        }

        // --- Playerに対する物理的な押し出し処理 ---
        if (collider.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collider.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // 押し出す強さ（インスペクターから調整できるようにすると便利です）
                float pushForce = 10f;

                // 音波の進行方向（transform.right）に向けてプレイヤーを弾き飛ばす
                playerRb.linearVelocity = (Vector2)transform.right * pushForce;

                // もし「音波の中心から外側へ広がるように飛ばしたい」ならこちら：
                // Vector2 pushDir = (collider.transform.position - transform.position).normalized;
                // playerRb.linearVelocity = pushDir * pushForce;
            }
        }

        // --- ここにスピーカーなどのギミックに対する処理を書く ---
    }


    // ★ 2. 子オブジェクト（小さめコライダー）から呼ばれる壁専用の判定メソッド
    public void HandleWallCollision(Collider2D collider)
    {
        int layer = collider.gameObject.layer;

        if (((1 << layer) & reflectLayer) != 0)
        {
            Vector2 hitNormal = Vector2.up;

            // 音波の中心から進行方向に向けてLinecastを飛ばし、壁の法線を取る
            // 子コライダーが小さいため、ここに来た時点ではまだ中心は壁にめり込んでおらず、綺麗にヒットします
            // 後半の transform.right も (Vector2) で囲って一括で2Dベクトルにします
            RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + (Vector2)transform.right * 0.5f, reflectLayer);
            if (hit.collider != null)
            {
                hitNormal = hit.normal;
            }
            else
            {
                hitNormal = ((Vector2)transform.position - collider.ClosestPoint(transform.position)).normalized;
            }

            // 反射
            Vector2 reflectDir = Vector2.Reflect(transform.right, hitNormal);
            transform.right = reflectDir;

            // 反射した瞬間に速度の向きも同期（めり込み防止）
            rb.linearVelocity = transform.right * speed;
        }
        else if (((1 << layer) & absorbLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}


//using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
//public class SoundWave : MonoBehaviour, IMovingPlatform
//{
//    [Header("基本設定")]
//    public float speed = 5f;
//    public float volume = 1f;
//    public float durationPerVolume = 3f;
//    public AnimationCurve volumeCurve = AnimationCurve.Linear(0, 1, 1, 0);

//    [Header("レイヤー設定")]
//    public LayerMask reflectLayer;
//    public LayerMask absorbLayer;
//    public LayerMask transparentLayer;

//    private Rigidbody2D rb;
//    private SpriteRenderer spriteRenderer;
//    private float calculatedLifeTime;
//    private float timer;
//    private Vector3 baseScale;
//    private float baseAlpha;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        baseScale = transform.localScale;
//        baseAlpha = spriteRenderer.color.a;

//        rb.bodyType = RigidbodyType2D.Kinematic;

//        // トリガーを使うため、通常は contact 設定は不要になります
//        rb.useFullKinematicContacts = false;
//    }

//    public void SetupWave()
//    {
//        calculatedLifeTime = volume * durationPerVolume;
//        transform.localScale = baseScale * volume;
//        timer = 0;
//    }

//    void Update()
//    {
//        timer += Time.deltaTime;
//        float progress = Mathf.Clamp01(timer / calculatedLifeTime);

//        float decay = volumeCurve.Evaluate(progress);
//        Color color = spriteRenderer.color;
//        color.a = baseAlpha * decay;
//        spriteRenderer.color = color;

//        if (progress >= 1f) Destroy(gameObject);
//    }

//    public Vector2 GetVelocity()
//    {
//        return transform.right * speed;
//    }

//    void FixedUpdate()
//    {
//        rb.linearVelocity = transform.right * speed;
//    }

//    // ★ OnCollisionEnter2D から OnTriggerEnter2D に変更
//    private void OnTriggerEnter2D(Collider2D collider)
//    {
//        int layer = collider.gameObject.layer;

//        if (((1 << layer) & reflectLayer) != 0)
//        {
//            // トリガーでは衝突点（contacts）が取れないため、Linecastなどで法線（Normal）を計算する
//            Vector2 hitNormal = Vector2.up; // デフォルトのフォールバック

//            // 音波の進行方向に向けてレイを飛ばし、正確な法線を取得する
//            RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + rb.linearVelocity * Time.fixedDeltaTime, reflectLayer);
//            if (hit.collider != null)
//            {
//                hitNormal = hit.normal;
//            }
//            else
//            {
//                // もし高速すぎてすり抜けていた場合、一番近い点から法線を逆算
//                hitNormal = ((Vector2)transform.position - collider.ClosestPoint(transform.position)).normalized;
//            }

//            // 反射ベクトルを計算して向きを変更
//            Vector2 reflectDir = Vector2.Reflect(transform.right, hitNormal);
//            transform.right = reflectDir;
//        }
//        else if (((1 << layer) & absorbLayer) != 0)
//        {
//            Destroy(gameObject);
//        }
//    }
//}