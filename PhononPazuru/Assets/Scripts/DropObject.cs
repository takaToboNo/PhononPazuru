using UnityEngine;

public class DropObject : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private bool isFalling = false;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // プレイヤーに押されて、下方向に落ち始めた瞬間を検知
#if UNITY_2023_1_OR_NEWER
        float currentYVelocity = rb2d.linearVelocity.y;
#else
        float currentYVelocity = rb2d.velocity.y;
#endif

        if (!isFalling && currentYVelocity < -0.1f)
        {
            isFalling = true;

            // 1. 横方向の速度（慣性）を完全にゼロにする
#if UNITY_2023_1_OR_NEWER
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
#else
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
#endif

            // 2. インスペクターのConstraintsをスクリプトから書き換える
            // 物理演算を生かしたまま、X軸の移動（FreezePositionX）と回転（FreezeRotation）をロック！
            rb2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    // 床や地面に着地した時
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling)
        {
            // プレイヤー以外のもの（床など）にぶつかったらロックを解除する
            if (!collision.gameObject.CompareTag("Player"))
            {
                isFalling = false;

                // 次にまた押せるように、X軸のロックを解除して元の状態（Z軸回転のみロック）に戻す
                rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}