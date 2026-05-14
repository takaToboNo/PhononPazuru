using UnityEngine;

public class Speaker : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField] private GameObject soundWavePrefab;
    [SerializeField, Min(0.1f)] private float interval = 1.0f;

    // スピーカーのスケールに対する音波の大きさの比率
    [Header("音波サイズ設定")]
    [SerializeField, Range(0.1f, 2.0f)] private float volumeRatio = 1.0f;

    [Header("放出ポイント")]
    [SerializeField] private Transform spawnPoint;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            SpawnWave();
            timer = 0;
        }
    }

    private void SpawnWave()
    {
        if (soundWavePrefab == null) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject waveObj = Instantiate(soundWavePrefab, pos, rot);

        if (waveObj.TryGetComponent<SoundWave>(out SoundWave wave))
        {
            // スピーカーのXスケール（または平均値）に比率を掛けて、音波のVolumeを決定
            // transform.lossyScale を使うことで、親オブジェクトのスケールの影響も考慮できます
            float calculatedVolume = transform.lossyScale.x * volumeRatio;

            wave.volume = calculatedVolume;
            wave.SetupWave();
        }
    }
}