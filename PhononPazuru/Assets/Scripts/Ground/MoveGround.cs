using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class MoveGround : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<SoundWave>(out SoundWave wave))
        {
            // 1. 音波が当たった瞬間に、X軸・Y軸の移動をフリーズ（固定）する
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

            // 2. 同フレームの物理演算が終わるタイミングでロックを解除するコルーチンを開始
            StartCoroutine(ReleaseConstraintsRoutine());

            // 念のため、これまで通り以降の衝突は無視に設定
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider, true);
        }
    }

    private IEnumerator ReleaseConstraintsRoutine()
    {
        // 次の固定フレーム（物理演算が1回処理された後）まで待つ
        yield return new WaitForFixedUpdate();

        // ロックを解除して、プレイヤーが押せる状態（回転だけ固定など、元の設定）に戻す
        // もし元々回転も固定（Freeze Rotation）にしていたなら、FreezeRotation を指定してください
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}