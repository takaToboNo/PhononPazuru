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
    // 判定の厚み。足場から一瞬浮いても追従を維持するために少し余裕を持たせる
    [SerializeField] private float groundCheckExtra = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Vector2 moveInput;
    private Vector2 platformVelocity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    private void OnJump(InputValue value)
    {
        // 接地時のみジャンプ。足場の垂直速度があれば加算する
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
        // 判定範囲：コライダーの幅より少し狭く(0.9f)して壁への引っ掛かりを防ぐ
        Vector2 boxSize = new Vector2(coll.bounds.size.x * 0.9f, 0.05f);
        // 判定開始位置：コライダーの底面
        float castDistance = (coll.bounds.size.y / 2f) + groundCheckExtra;

        RaycastHit2D hit = Physics2D.BoxCast(
            coll.bounds.center,
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

    private void ApplyMovement()
    {
        float targetX = moveInput.x * moveSpeed;

        if (isGrounded)
        {
            // 足場の速度をベースに、入力分の速度を加える
            // 垂直方向(y)も、足場が上下に動くことを想定して足場の速度を最低限保証する
            float finalX = targetX + platformVelocity.x;
            float finalY = rb.linearVelocity.y;

            // 足場が上昇している場合、Y速度が負（落下）にならないように補正
            if (platformVelocity.y > 0 && finalY < platformVelocity.y)
            {
                finalY = platformVelocity.y;
            }

            rb.linearVelocity = new Vector2(finalX, finalY);
        }
        else
        {
            // 空中制御：現在の速度から目標速度へ補完
            float currentX = rb.linearVelocity.x;
            float newX = Mathf.Lerp(currentX, targetX, airControl);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }
}