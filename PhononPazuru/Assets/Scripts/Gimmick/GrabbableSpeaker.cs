using UnityEngine;

// Speakerコンポーネントと一緒に使うことを前提にします
[RequireComponent(typeof(Speaker))]
public class GrabbableSpeaker : MonoBehaviour
{
    private Speaker targetSpeaker;
    private Rigidbody2D rb;

    void Awake()
    {
        // 同じオブジェクトについているコンポーネントを自動取得
        targetSpeaker = GetComponent<Speaker>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb == null || targetSpeaker == null) return;

        // ★プレイヤーの PlayerGrabAndThrow.cs が、掴んだ瞬間に Rigidbody2D を Kinematic にします。
        // それをここで検知して、Speaker のオン/オフを自動で切り替えます。
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // プレイヤーが持っている間は、Speakerスクリプト自体の動きを完全に止める
            targetSpeaker.enabled = false;
        }
        else
        {
            // 地面にある時や、投げられて Dynamic に戻った時は、Speakerを動かす
            targetSpeaker.enabled = true;
        }
    }
}