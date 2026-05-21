using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class VibratingObject : MonoBehaviour
{
    [Header("振動設定")]
    [SerializeField] private float vibrateDuration = 0.5f;
    [SerializeField] private float vibrateMagnitude = 0.05f;

    [Header("プレイヤーへの反発設定")]
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private Vector2 launchDirection = Vector2.up; // Vector2.upで真上（大ジャンプ）、Vector2.rightで右など

    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private bool isVibrating = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        originalPosition = transform.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突相手が「音波」だった場合、振動を開始
        if (collision.gameObject.TryGetComponent<SoundWave>(out var wave))
        {
            if (!isVibrating) StartCoroutine(VibrateCoroutine());
        }

        // 振動中にプレイヤーが接触したら弾む力を送る
        if (isVibrating && collision.gameObject.CompareTag("Player"))
        {
            SendLaunchForce(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // すでに乗っている状態で音波が当たって振動が始まった場合も弾む力を送る
        if (isVibrating && collision.gameObject.CompareTag("Player"))
        {
            SendLaunchForce(collision.gameObject);
        }
    }

    // プレイヤーに「弾むベクトル」を予約させる
    private void SendLaunchForce(GameObject playerObj)
    {
        if (playerObj.TryGetComponent<PlayerController>(out var player))
        {
            Vector2 force = launchDirection.normalized * launchForce;
            player.QueueLaunchForce(force);
        }
    }

    private IEnumerator VibrateCoroutine()
    {
        isVibrating = true;
        float elapsed = 0f;

        while (elapsed < vibrateDuration)
        {
            // ランダムに位置をずらしてガタガタさせる
            float offsetX = Random.Range(-1f, 1f) * vibrateMagnitude;
            float offsetY = Random.Range(-1f, 1f) * vibrateMagnitude;
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 振動時間が終わったら元の位置に戻す
        transform.localPosition = originalPosition;
        isVibrating = false;
    }
}