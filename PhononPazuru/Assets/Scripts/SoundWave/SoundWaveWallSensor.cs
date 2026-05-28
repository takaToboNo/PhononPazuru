using UnityEngine;

public class SoundWaveWallSensor : MonoBehaviour
{
    private SoundWave parentWave;

    void Awake()
    {
        // 親の SoundWave スクリプトを取得
        parentWave = GetComponentInParent<SoundWave>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (parentWave != null)
        {
            // 衝突を親の壁専用メソッドに丸投げする
            parentWave.HandleWallCollision(collider);
        }
    }
}