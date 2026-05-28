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

    [Header("音波消滅（遮断）設定")]
    [Tooltip("音波を消したいオブジェクト（ブロックや他のスピーカー）のレイヤー")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("重なりを調べる範囲の半径（spawnPointの周りどれくらいを調べるか）")]
    [SerializeField] private float checkRadius = 0.3f;

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

        // 1. まずは通常通り音波を生成する
        GameObject waveObj = Instantiate(soundWavePrefab, pos, rot);

        if (waveObj.TryGetComponent<SoundWave>(out SoundWave wave))
        {
            float calculatedVolume = transform.lossyScale.x * volumeRatio;
            wave.volume = calculatedVolume;
            wave.SetupWave();
        }

        // 2. 生成した瞬間に、放出ポイント（spawnPoint）が塞がれているかチェック
        Collider2D hitObstacle = Physics2D.OverlapCircle(pos, checkRadius, obstacleLayer);

        if (hitObstacle != null)
        {
            // ★ 修正・追加：
            // 重なったオブジェクトが「拡声器（Megaphone）」、または「音叉（TuningFork）」なら消さずにスルー
            // ※実際の音叉のスクリプト名に合わせて TuningFork の部分を書き換えてください。
            if (hitObstacle.GetComponent<Megaphone>() != null || hitObstacle.GetComponent<TuningFork>() != null)
            {
                return; // 音波を消さずに、そのまま外へ飛ばす
            }

            // 上記以外の障害物（普通のブロックや普通のスピーカー）なら、生まれた瞬間に即座に破壊する
            Destroy(waveObj);
        }
    }

    // 検知範囲をシーン画面に評価用のマゼンタ色の円で表示（調整用）
    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(spawnPoint.position, checkRadius);
    }
}