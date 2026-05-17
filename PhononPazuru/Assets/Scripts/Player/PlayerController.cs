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
    [SerializeField] private CapsuleCollider2D bodyCollider; // カプセルに変更
    [SerializeField] private CircleCollider2D footCollider;   // 足元用を追加

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 platformVelocity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 自動取得のロジックを更新
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
        CheckGrounded();
        ApplyMovement();
    }

    private void CheckGrounded()
    {
        // 足元のサークルコライダーの半径と中心を基準にする
        float radius = footCollider.radius;
        // 判定の幅をサークルの直径よりわずかに狭くする
        Vector2 boxSize = new Vector2(radius * 2f * 0.9f, 0.05f);

        // 足元サークルの底面から下方向にレイを飛ばす
        // castDistanceはサークルの中心から底面までの距離 + 余裕分
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
        }
        else
        {
            isGrounded = false;
            platformVelocity = Vector2.zero;
        }
    }

    // ApplyMovementは変更なしでOK
    private void ApplyMovement()
    {
        float targetX = moveInput.x * moveSpeed;

        if (isGrounded)
        {
            float finalX = targetX + platformVelocity.x;
            float finalY = rb.linearVelocity.y;
            if (platformVelocity.y > 0 && finalY < platformVelocity.y)
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