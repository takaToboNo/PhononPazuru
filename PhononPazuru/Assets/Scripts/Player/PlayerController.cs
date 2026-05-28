using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("空中操作の設定")]
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.2f;

    [Header("接地判定の設定")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckExtra = 0.1f;

    [Header("コライダー設定")]
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private CircleCollider2D footCollider;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 platformVelocity;
    private bool isGrounded;
    private bool isRidingOnTop;

    // 振動床からの弾き飛ばし力を予約する変数
    private Vector2 pendingLaunchForce = Vector2.zero;
    private bool hasPendingLaunch = false;
    private float disableGroundCheckTimer = 0f; // 接地判定を一時停止するタイマー

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bodyCollider == null) bodyCollider = GetComponent<CapsuleCollider2D>();
        if (footCollider == null) footCollider = GetComponent<CircleCollider2D>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    private void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            // 音波の上昇速度を考慮したジャンプ力を計算
            float newJumpV = jumpForce + (platformVelocity.y > 0 ? platformVelocity.y : 0);

            // ★【修正ポイント】
            // 速度を直接代入するのではなく、床からの「弾き飛ばし予約システム」と同じ原理を使う
            // ジャンプした瞬間に「弾き飛ばし予約」をONにして、次のFixedUpdateの先頭で最優先で飛び上がらせる
            QueueLaunchForce(new Vector2(rb.linearVelocity.x, newJumpV));
        }
    }

    void FixedUpdate()
    {
        if (hasPendingLaunch)
        {
            rb.linearVelocity = pendingLaunchForce;
            pendingLaunchForce = Vector2.zero;
            hasPendingLaunch = false;
            isGrounded = false;
            platformVelocity = Vector2.zero;
            disableGroundCheckTimer = 0.1f;
            return;
        }

        CheckGrounded();

        // ★【修正】「横から押されている、かつ上に乗っていないとき」だけ入力をロックする
        if (!isRidingOnTop)
        {
            if (platformVelocity.x > 0f && moveInput.x < 0f)
            {
                moveInput.x = 0f;
            }
            else if (platformVelocity.x < 0f && moveInput.x > 0f)
            {
                moveInput.x = 0f;
            }
        }

        ApplyMovement();
    }

    // 床側から呼び出されて、弾き飛ばし力を予約するためのメソッド
    public void QueueLaunchForce(Vector2 force)
    {
        pendingLaunchForce = force;
        hasPendingLaunch = true;
    }

    private void CheckGrounded()
    {
        if (disableGroundCheckTimer > 0f)
        {
            disableGroundCheckTimer -= Time.fixedDeltaTime;
            isGrounded = false;
            isRidingOnTop = false;
            platformVelocity = Vector2.zero;
            return;
        }

        // すべてのフラグを一瞬リセット
        isGrounded = false;
        isRidingOnTop = false;
        platformVelocity = Vector2.zero;

        if (bodyCollider == null || footCollider == null) return;

        // ★【超重要】体全体（少し下まで広げたカプセル）で、今触れている地面や音波を「1つだけ」確実に捉える
        float radius = footCollider.radius;
        float castDist = 0.2f;

        // プレイヤーの足元より少し下まで届くように下方向にカプセルキャスト
        RaycastHit2D hit = Physics2D.CapsuleCast(
            bodyCollider.bounds.center,
            bodyCollider.size,
            bodyCollider.direction,
            0f,
            Vector2.down,
            castDist,
            groundLayer
        );

        // 下方向で見つからなければ、念のため左右もまとめて探す（空中での横衝突用）
        if (hit.collider == null)
        {
            RaycastHit2D hitRight = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0f, Vector2.right, castDist, groundLayer);
            RaycastHit2D hitLeft = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0f, Vector2.left, castDist, groundLayer);

            if (hitRight.collider != null) hit = hitRight;
            else if (hitLeft.collider != null) hit = hitLeft;
        }

        // 何かしらの地形・音波に触れている場合
        if (hit.collider != null)
        {
            isGrounded = true;
            IMovingPlatform platform = hit.collider.GetComponent<IMovingPlatform>();
            platformVelocity = (platform != null) ? platform.GetVelocity() : Vector2.zero;

            // 触れた相手が「音波（Trigger）」だった場合の仕分け処理
            if (hit.collider.isTrigger && platform != null)
            {
                // プレイヤーの足元の実際のY座標
                float footOffset = footCollider.offset.y - radius;
                float currentFootY = rb.position.y + footOffset;

                // 音波の「中心の高さ」を取得
                float waveCenterY = hit.collider.bounds.center.y;
                // 音波の「上面の高さ」を取得（極小サイズでもboundsから正確に取れます）
                float waveTopY = hit.collider.bounds.max.y;

                // ----------------------------------------------------
                // パターンA：【音波の上にいるとき】
                // プレイヤーの足元が、音波の中心よりも上にあるなら「上に乗っている」とみなす
                // ----------------------------------------------------
                if (currentFootY >= waveCenterY - 0.05f)
                {
                    isRidingOnTop = true;

                    // 上から着地、または乗って沈み込もうとしたらY座標を表面に固定
                    float targetY = waveTopY - footOffset;
                    if (rb.position.y <= targetY)
                    {
                        rb.position = new Vector2(rb.position.x, targetY);
                        if (rb.linearVelocity.y < 0f)
                        {
                            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                        }
                    }
                }
                // ----------------------------------------------------
                // パターンB：【音波の横（側面）にいるとき】
                // 足元が音波の中心より下 ＝ 完全に横から突っ込まれている状態
                // ----------------------------------------------------
                else
                {
                    isGrounded = false;
                    isRidingOnTop = false;

                    // ★【修正】波の速度ではなく、プレイヤーと音波の相対位置で押し出し方向を決める
                    float playerRadius = bodyCollider.size.x * 0.5f;
                    float waveCenterX = hit.collider.bounds.center.x;

                    if (rb.position.x > waveCenterX)
                    {
                        // プレイヤーが音波の「右側」にいるなら、音波の右端のフチへ押し出す
                        float waveRightX = hit.collider.bounds.max.x;
                        float targetX = waveRightX + playerRadius;
                        if (rb.position.x < targetX)
                        {
                            rb.position = new Vector2(targetX, rb.position.y);
                        }
                    }
                    else
                    {
                        // プレイヤーが音波の「左側」にいるなら、音波の左端のフチへ押し出す
                        float waveLeftX = hit.collider.bounds.min.x;
                        float targetX = waveLeftX - playerRadius;
                        if (rb.position.x > targetX)
                        {
                            rb.position = new Vector2(targetX, rb.position.y);
                        }
                    }
                }
            }
        }
    }

    private void ApplyMovement()
    {
        float targetX = moveInput.x * moveSpeed;

        if (isGrounded)
        {
            float finalX = targetX + platformVelocity.x;
            float finalY = rb.linearVelocity.y;

            // ★【修正】「接地していて、かつ上に乗っているとき」だけ上方向の速度を同期する
            if (isRidingOnTop && platformVelocity.y > 0)
            {
                finalY = platformVelocity.y;
            }

            rb.linearVelocity = new Vector2(finalX, finalY);
        }
        else
        {
            // 空中制御
            float currentX = rb.linearVelocity.x;
            float newX = Mathf.Lerp(currentX, targetX, airControl);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }
}