using UnityEngine;

public class BreakableGround : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float requiredVolume = 0.5f; // 必要な音量（これ以下なら壊れない）

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 当たった相手が SoundWave かどうかを確認
        if (collision.gameObject.TryGetComponent<SoundWave>(out SoundWave wave))
        {
            // 音波の強さ（volume）が足りているかチェック
            if (wave.volume >= requiredVolume)
            {
                Break();
            }
        }
    }

    private void Break()
    {
        // オブジェクトを削除
        Destroy(gameObject);
    }
}