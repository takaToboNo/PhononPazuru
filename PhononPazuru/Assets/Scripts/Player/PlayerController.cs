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
            float newJumpV = jumpForce + (platformVelocity.y > 0 ? platformVelocity.y : 0);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newJumpV);
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
            isRidingOnTop = false; // ★追加
            platformVelocity = Vector2.zero;
            return;
        }

        // --- 1. 縦方向（足元）の接地判定 ---
        float radius = footCollider.radius;
        Vector2 boxSize = new Vector2(radius * 2f * 0.9f, 0.05f);
        float castDistance = radius + groundCheckExtra;

        RaycastHit2D verticalHit = Physics2D.BoxCast(
            footCollider.bounds.center,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            groundLayer
        );

        isGrounded = false;
        isRidingOnTop = false; // ★まずは毎フレームリセット
        platformVelocity = Vector2.zero;

        if (verticalHit.collider != null)
        {
            isGrounded = true;
            IMovingPlatform platform = verticalHit.collider.GetComponent<IMovingPlatform>();
            platformVelocity = (platform != null) ? platform.GetVelocity() : Vector2.zero;

            if (verticalHit.collider.isTrigger && platform != null)
            {
                // ★音波を足元で検知している ＝ 上に乗っている状態
                isRidingOnTop = true;

                float groundY = verticalHit.point.y;
                float footOffset = footCollider.offset.y - radius;
                float targetY = groundY - footOffset;

                if (rb.position.y < targetY)
                {
                    rb.position = new Vector2(rb.position.x, targetY);
                    if (rb.linearVelocity.y < 0f)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    }
                }
            }
        }

        // --- 2. 横・体全体方向の音波検知 ---
        // 足元で音波の上に乗っていない（isRidingOnTop が false）場合のみ、横の壁としての判定を行う
        if (!isRidingOnTop && bodyCollider != null)
        {
            float castDist = 0.2f;

            RaycastHit2D hitRight = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0f, Vector2.right, castDist, groundLayer);
            RaycastHit2D hitLeft = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0f, Vector2.left, castDist, groundLayer);

            RaycastHit2D activeHit = new RaycastHit2D();
            if (hitRight.collider != null && hitRight.collider.isTrigger) activeHit = hitRight;
            else if (hitLeft.collider != null && hitLeft.collider.isTrigger) activeHit = hitLeft;

            if (activeHit.collider != null)
            {
                IMovingPlatform platform = activeHit.collider.GetComponent<IMovingPlatform>();
                if (platform != null)
                {
                    Vector2 waveVel = platform.GetVelocity();

                    if (Mathf.Abs(waveVel.x) > 0f)
                    {
                        isGrounded = true;
                        platformVelocity = waveVel;

                        float playerRadius = bodyCollider.size.x * 0.5f;

                        // ★【修正ポイント】
                        // すでに上に乗っている（isRidingOnTop）なら、横からの位置強制補正は絶対にスキップする！
                        // これにより、上を歩いている最中に側面センサーが一瞬かすめても引き戻されなくなります。
                        if (!isRidingOnTop)
                        {
                            if (waveVel.x > 0f)
                            {
                                float targetX = activeHit.point.x + playerRadius;
                                if (rb.position.x < targetX)
                                {
                                    rb.position = new Vector2(targetX, rb.position.y);
                                }
                            }
                            else if (waveVel.x < 0f)
                            {
                                float targetX = activeHit.point.x - playerRadius;
                                if (rb.position.x > targetX)
                                {
                                    rb.position = new Vector2(targetX, rb.position.y);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void ApplyMovement()
    {
        // FixedUpdateでmoveInputが安全な値に書き換わっているため、そのまま計算してOK
        float targetX = moveInput.x * moveSpeed;

        if (isGrounded)
        {
            // 入力が事前に制限されているため、単純な足し算でも絶対に逆走・めり込みが起きない
            float finalX = targetX + platformVelocity.x;

            float finalY = rb.linearVelocity.y;
            if (platformVelocity.y > 0)
            {
                finalY = platformVelocity.y;
            }

            rb.linearVelocity = new Vector2(finalX, finalY);
        }
        else
        {
            float currentX = rb.linearVelocity.x;
            float newX = Mathf.Lerp(currentX, targetX, airControl);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }
}