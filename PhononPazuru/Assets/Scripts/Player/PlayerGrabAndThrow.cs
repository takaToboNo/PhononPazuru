using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabAndThrow : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputAction grabAction;
    [Tooltip("PlayerControllerと同じ移動入力をここにも設定してください")]
    [SerializeField] private InputAction moveAction; // ★完全な疎結合のための追加

    [Header("Layer Settings")]
    [SerializeField] private LayerMask grabbableLayer;

    [Header("BoxCast Settings (Grab Range)")]
    [SerializeField] private Vector2 boxCastOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Vector2 boxCastSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private float boxCastDistance = 0.5f;

    [Header("Hold Settings")]
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 1f);

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float downThrowForce = 10f; // ★下投げ用の専用速度（必要に応じてインスペクターで調整してね）

    private GameObject grabbedObject;
    private Rigidbody2D grabbedRb;
    private Collider2D grabbedCollider;
    private bool isHolding = false;

    // プレイヤーの現在の向きを記憶する変数 (1 = 右向き, -1 = 左向き)
    private float facingSign = 1f;

    private void OnEnable()
    {
        grabAction.Enable();
        grabAction.performed += OnGrabActionPerformed;

        moveAction.Enable(); // ★移動入力を有効化
    }

    private void OnDisable()
    {
        grabAction.Disable();
        grabAction.performed -= OnGrabActionPerformed;

        moveAction.Disable(); // ★移動入力を無効化
    }

    private void Update()
    {
        // ★移動入力をリアルタイムに読み取る
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // 入力がある時だけ向きを更新（立ち止まっても最後の向きをキープする）
        if (moveInput.x != 0f)
        {
            facingSign = Mathf.Sign(moveInput.x);
        }

        // オブジェクトを掴んでいる間、プレイヤーの位置に追従させる
        if (isHolding && grabbedObject != null)
        {
            // 記憶した向き(facingSign)に合わせて、ホールド位置の左右(X)を反転
            Vector3 targetPosition = transform.position + new Vector3(holdOffset.x * facingSign, holdOffset.y, 0f);
            grabbedObject.transform.position = targetPosition;
        }
    }

    private void OnGrabActionPerformed(InputAction.CallbackContext context)
    {
        if (isHolding)
        {
            ThrowItem();
        }
        else
        {
            TryGrabItem();
        }
    }

    private Vector2 GetFacingDirection()
    {
        return facingSign >= 0f ? Vector2.right : Vector2.left;
    }

    private void TryGrabItem()
    {
        Vector2 facingDir = GetFacingDirection();

        // 記憶した向きに合わせてBoxの発生位置(X)を反転
        Vector2 origin = (Vector2)transform.position + new Vector2(boxCastOffset.x * facingSign, boxCastOffset.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxCastSize, 0f, facingDir, boxCastDistance, grabbableLayer);

        if (hit.collider != null)
        {
            grabbedObject = hit.collider.gameObject;
            grabbedRb = grabbedObject.GetComponent<Rigidbody2D>();
            grabbedCollider = hit.collider;

            if (grabbedRb != null)
            {
                isHolding = true;

                grabbedRb.bodyType = RigidbodyType2D.Kinematic;
                grabbedRb.linearVelocity = Vector2.zero;
                grabbedRb.angularVelocity = 0f;

                if (grabbedCollider != null)
                {
                    grabbedCollider.isTrigger = true;
                }
            }
        }
    }

    private void ThrowItem()
    {
        if (grabbedObject == null) return;

        isHolding = false;

        if (grabbedCollider != null)
        {
            grabbedCollider.isTrigger = false;
        }

        grabbedRb.bodyType = RigidbodyType2D.Dynamic;

        // 現在の入力を取得
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        // 【修正ポイント】
        // 左右に微小な入力があっても、Sキー（下）が押されていれば（Yがマイナス値なら）下投げを優先する
        // キーボードの同時押し対策として、判定を -0.1f（少しでも下に入っていればOK）に広げます
        if (moveInput.y < -0.1f)
        {
            grabbedRb.linearVelocity = Vector2.down * downThrowForce;
        }
        else
        {
            // それ以外は通常通り横に投げる
            Vector2 throwDir = GetFacingDirection();
            grabbedRb.linearVelocity = throwDir * throwForce;
        }

        grabbedObject = null;
        grabbedRb = null;
        grabbedCollider = null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 facingDir = GetFacingDirection();
        Vector2 origin = (Vector2)transform.position + new Vector2(boxCastOffset.x * facingSign, boxCastOffset.y);

        Gizmos.color = isHolding ? Color.green : Color.red;

        Gizmos.DrawWireCube(origin, boxCastSize);
        Vector2 endPoint = origin + facingDir * boxCastDistance;
        Gizmos.DrawWireCube(endPoint, boxCastSize);
        Gizmos.DrawLine(origin, endPoint);
    }
}