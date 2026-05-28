using UnityEngine;

public class TuningFork : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private GameObject soundWavePrefab; // 発射する音波のプレハブ
    [SerializeField] private Transform spawnPoint;       // 音波を生成する位置（空のオブジェクトをアタッチ）

    // ★ OnCollisionEnter2D から OnTriggerEnter2D に変更
    // これにより、音波がトリガーでも地面（実体）としての機能を保ったまま検知できます
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 接触したオブジェクトから SoundWave コンポーネントを取得
        SoundWave incomingWave = collider.gameObject.GetComponent<SoundWave>();

        if (incomingWave != null)
        {
            float incomingVolume = incomingWave.volume;

            // 1. 先に当たった音波を削除（collider.gameObject を破壊）
            Destroy(collider.gameObject);

            // 2. 生成位置の決定（未設定なら自身の位置）
            Vector3 targetPosition = spawnPoint != null ? spawnPoint.position : transform.position;

            // 3. 音叉の傾き（transform.rotation）に、上を向くための90度回転を掛け合わせる
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, 90f);

            GameObject newWaveObj = Instantiate(soundWavePrefab, targetPosition, targetRotation);

            // 4. 設定を引き継ぐ
            SoundWave newWave = newWaveObj.GetComponent<SoundWave>();
            if (newWave != null)
            {
                newWave.volume = incomingVolume;
                newWave.SetupWave();
            }
        }
    }
}