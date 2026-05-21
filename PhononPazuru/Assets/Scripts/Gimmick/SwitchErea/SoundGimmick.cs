using UnityEngine;

public class SoundGimmick : MonoBehaviour
{
    [Header("このギミックが設置されているエリア")]
    [SerializeField] private AreaController myArea;

    [Header("発射する音波のプレハブ")]
    [SerializeField] private GameObject soundWavePrefab;

    // 音波を発射する関数（自動タイマーや、何かを当てた時のCollisionからこれを呼び出す）
    public void EmitSoundWave()
    {
        // もし設置されているエリアが現在「赤（消音）」なら、処理を無視して音波を出さない
        if (myArea != null && myArea.CurrentColor == AreaController.AreaColor.Red)
        {
            Debug.Log("赤エリアのため、ギミックの音がミュートされました。");
            return;
        }
    }

    // 例：何かオブジェクト（弾など）がこのギミックに当たったら音波を出す場合
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 当たった相手の判定（必要に応じてタグなどで絞り込んでください）
        EmitSoundWave();
    }
}