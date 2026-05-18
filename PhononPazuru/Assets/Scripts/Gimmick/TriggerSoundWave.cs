using UnityEngine;

public class TriggerSoundWave : MonoBehaviour
{
    [Header("音波が当たった時に破壊するオブジェクト")]
    [SerializeField] private GameObject targetToDestroy;

    [Header("自分自身も一緒に破壊するかどうか")]
    [SerializeField] private bool destroySelf = false;

    // Unityの物理衝突イベント（相手がSoundWaveだった場合に実行する）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突した相手に SoundWave コンポーネントがついているか確認
        SoundWave soundWave = collision.gameObject.GetComponent<SoundWave>();

        if (soundWave != null)
        {
            // 音波が当たったので、破壊処理を実行
            TriggerDestroy();

            // 【お好みで調整】もし音波側をここで消滅させたい場合
            // Destroy(collision.gameObject);
        }
    }

    // 音波が当たった時に呼ばれる関数
    public void TriggerDestroy()
    {
        if (targetToDestroy != null)
        {
            Destroy(targetToDestroy);
            Debug.Log($"{gameObject.name} に音波が衝突: {targetToDestroy.name} を破壊しました。");
        }

        if (destroySelf)
        {
            Destroy(gameObject);
        }
    }
}