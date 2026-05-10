using UnityEngine;

public class StoneCollisionHandler : MonoBehaviour
{
    [Header("生成する音波の設定")]
    public GameObject soundWavePrefab;
    public LayerMask targetLayer;
    public float surfaceOffset = 0.1f;

    [Header("速度と音量の設定")]
    [Tooltip("音波が発生し始める最小速度。これ以下の衝撃では何も起きません。")]
    public float minVelocityThreshold = 2f;

    [Tooltip("音波の強さを決めるグラフ。\n横軸：衝突時の速さ (Speed)\n縦軸：音量の倍率 (Volume)")]
    public AnimationCurve velocityCurve = AnimationCurve.Linear(2f, 0.5f, 15f, 2.0f);

    [Tooltip("どれだけ激しくぶつかっても、音波のサイズはこの倍率で止まります（安全装置）。")]
    public float maxVolumeCap = 3f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minVelocityThreshold) return;

        float finalVolume = Mathf.Min(velocityCurve.Evaluate(impactSpeed), maxVolumeCap);

        ContactPoint2D contact = collision.contacts[0];
        Vector2 spawnPosition = contact.point + (contact.normal * surfaceOffset);
        float angle = Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg;

        // 生成と同時に初期化
        GameObject waveObj = Instantiate(soundWavePrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
        if (waveObj.TryGetComponent<SoundWave>(out var wave))
        {
            wave.volume = finalVolume;
            wave.SetupWave();
        }
    }
}