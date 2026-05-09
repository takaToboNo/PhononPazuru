using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("空中操作の設定")]
    [Tooltip("0:完全な慣性ジャンプ / 1:地面と同じ操作感")]
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.2f;

    [Header("接地判定の設定")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        // キャラクターが倒れないように回転を固定
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Unity 6推奨の連続衝突検知（壁抜け防止）
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Input System: 移動入力
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input System: ジャンプ入力
    private void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        float targetX = moveInput.x * moveSpeed;
        float currentX = rb.linearVelocity.x;

        if (IsGrounded())
        {
            // 地面にいる時はキビキビ動く
            rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
        }
        else
        {
            // 空中にいる時はLerpを使って「慣性」と「操作」を混ぜる
            // airControlが低いほど、飛んだ瞬間の勢いが維持される
            float newX = Mathf.Lerp(currentX, targetX, airControl);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        // Colliderのサイズに合わせて接地判定を飛ばす
        return Physics2D.BoxCast(
            coll.bounds.center,
            coll.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
    }

    // デバッグ用：接地判定の範囲をScene画面に表示
    private void OnDrawGizmosSelected()
    {
        if (coll == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            coll.bounds.center + Vector3.down * groundCheckDistance,
            coll.bounds.size
        );
    }
}