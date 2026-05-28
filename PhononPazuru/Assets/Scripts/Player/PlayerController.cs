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
        // 弾き飛ばしの予約がある場合、すべての移動計算の「前」に処理する
        if (hasPendingLaunch)
        {
            rb.linearVelocity = pendingLaunchForce;

            // 予約をリセット
            pendingLaunchForce = Vector2.zero;
            hasPendingLaunch = false;

            isGrounded = false;
            platformVelocity = Vector2.zero;

            // 0.1秒間、床へのめり込みによる物理ブレーキや接地再判定を完全に無効化する
            disableGroundCheckTimer = 0.1f;
            return; // 通常の移動処理（ApplyMovement）をスキップして即座に飛び上がる
        }

        CheckGrounded();
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
            platformVelocity = Vector2.zero;
            return;
        }

        float radius = footCollider.radius;
        Vector2 boxSize = new Vector2(radius * 2f * 0.9f, 0.05f);
        float castDistance = radius + groundCheckExtra;

        RaycastHit2D hit = Physics2D.BoxCast(
            footCollider.bounds.center,
            boxSize,
            0f,
            Vector2.down,
            castDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            isGrounded = true;
            IMovingPlatform platform = hit.collider.GetComponent<IMovingPlatform>();
            platformVelocity = (platform != null) ? platform.GetVelocity() : Vector2.zero;

            // ★【追加】トリガー床（音波）の上昇による沈み込みを防止する座標補正
            // 音波が上方向に動いている（platformVelocity.y > 0）かつ、相手がTriggerの場合
            if (platformVelocity.y > 0f && hit.collider.isTrigger)
            {
                // BoxCastがヒットした位置（音波の上面）を取得
                float groundY = hit.point.y;

                // プレイヤーの足元コライダーの中心から下端までの距離
                float footOffset = footCollider.offset.y - radius;

                // プレイヤーが本来あるべき正しいY座標を計算
                float targetY = groundY - footOffset;

                // 現在の位置より沈み込んでいる場合、強制的に音波の表面に引き上げる
                if (rb.position.y < targetY)
                {
                    rb.position = new Vector2(rb.position.x, targetY);
                }
            }
        }
        else
        {
            isGrounded = false;
            platformVelocity = Vector2.zero;
        }
    }

    private void ApplyMovement()
    {
        float targetX = moveInput.x * moveSpeed;

        if (isGrounded)
        {
            float finalX = targetX + platformVelocity.x;

            // ★【修正】音波の上昇速度とプレイヤーの縦速度を完全に同期させる
            float finalY = rb.linearVelocity.y;
            if (platformVelocity.y > 0)
            {
                // 独自の計算をやめ、音波の縦速度を100%そのまま代入して追従させる
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