using UnityEngine;

public class StoneCollisionHandler : MonoBehaviour
{
    [Header("生成する音波の設定")]
    public GameObject soundWavePrefab;
    public LayerMask targetLayer;

    [Header("生成位置のオフセット")]
    [Tooltip("地面にめり込んで消えないように、衝突面から少し浮かす距離")]
    public float surfaceOffset = 0.1f;

    [Header("速度と音量の設定")]
    [Tooltip("音波が発生し始める最小速度")]
    public float minVelocityThreshold = 2f;

    [Tooltip("最大音量に達する速度")]
    public float maxVelocityThreshold = 15f;

    [Tooltip("速度に対する音量の倍率")]
    public float volumeMultiplier = 1f;

    [Tooltip("出力される音量の最大上限値")]
    public float maxVolumeCap = 2f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // レイヤーチェック
        if (((1 << collision.gameObject.layer) & targetLayer) == 0) return;

        // 衝突速度（相対速度）を取得
        float impactSpeed = collision.relativeVelocity.magnitude;

        // 下限速度チェック
        if (impactSpeed < minVelocityThreshold) return;

        // 音量の計算（速度を下限〜上限で0.0〜1.0に正規化して倍率を掛ける）
        float normalizedSpeed = Mathf.InverseLerp(minVelocityThreshold, maxVelocityThreshold, impactSpeed);
        float finalVolume = Mathf.Min(normalizedSpeed * volumeMultiplier, maxVolumeCap);

        // 衝突情報から位置と向きを計算
        ContactPoint2D contact = collision.contacts[0];

        // 衝突地点から法線方向（面の外側）へ少しずらした位置を生成場所にする
        Vector2 spawnPosition = contact.point + (contact.normal * surfaceOffset);

        // 飛んでいく方向を衝突面の法線方向（跳ね返る方向）に設定
        Vector2 spawnDirection = contact.normal;
        float angle = Mathf.Atan2(spawnDirection.y, spawnDirection.x) * Mathf.Rad2Deg;

        SpawnSoundWave(spawnPosition, finalVolume, angle);
    }

    private void SpawnSoundWave(Vector2 position, float volume, float angle)
    {
        if (soundWavePrefab == null) return;

        GameObject waveObj = Instantiate(soundWavePrefab, position, Quaternion.Euler(0, 0, angle));

        // 石ころ自身のコライダーと、生成した音波のコライダーを物理的に無視させる ---
        Collider2D stoneCollider = GetComponent<Collider2D>();
        Collider2D waveCollider = waveObj.GetComponent<Collider2D>();
        if (stoneCollider != null && waveCollider != null)
        {
            Physics2D.IgnoreCollision(stoneCollider, waveCollider);
        }

        SoundWave waveScript = waveObj.GetComponent<SoundWave>();
        if (waveScript != null)
        {
            waveScript.Initialize(volume);
        }
    }
}