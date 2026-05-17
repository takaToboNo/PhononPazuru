using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 質量を取得するためにRigidbody2Dを必須にします
public class StoneCollisionHandler : MonoBehaviour
{
    [Header("生成する音波の設定")]
    public GameObject soundWavePrefab;
    public LayerMask targetLayer;
    public float surfaceOffset = 0.1f;

    [Header("物理設定（質量 × 速度）")]
    [Tooltip("音波が発生し始める最小の衝撃値(自分の質量 × 衝突速度)。")]
    public float minImpactThreshold = 1.0f;

    [Tooltip("音波の強さを決めるグラフ。\n横軸：衝撃の強さ (Mass * Speed)\n縦軸：音量の倍率 (Volume)")]
    public AnimationCurve impactCurve = AnimationCurve.Linear(1f, 0.5f, 20f, 2.0f);

    public float maxVolumeCap = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        // 自分のRigidbodyをキャッシュ
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ターゲット層以外なら無視
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;

        // 【修正ポイント】速さだけでなく、自身の質量を掛け合わせて「衝撃の強さ」にする
        float impactForce = rb.mass * collision.relativeVelocity.magnitude;

        // しきい値判定（衝撃の強さで判定）
        if (impactForce < minImpactThreshold) return;

        // グラフから音量を決定
        float finalVolume = Mathf.Min(impactCurve.Evaluate(impactForce), maxVolumeCap);

        //  デバッグログの表示
        // F2は小数点以下2桁までの表示を意味します
        Debug.Log($"<color=cyan>[Collision]</color> 相手: {collision.gameObject.name} | " +
                  $"速度: {collision.relativeVelocity.magnitude:F2} | 衝撃: {impactForce:F2} | 音量: {finalVolume:F2}");

        // --- 生成処理 ---
        ContactPoint2D contact = collision.contacts[0];
        Vector2 spawnPosition = contact.point + (contact.normal * surfaceOffset);
        float angle = Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg;

        GameObject waveObj = Instantiate(soundWavePrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
        if (waveObj.TryGetComponent<SoundWave>(out var wave))
        {
            wave.volume = finalVolume;
            wave.SetupWave();
        }
    }
}