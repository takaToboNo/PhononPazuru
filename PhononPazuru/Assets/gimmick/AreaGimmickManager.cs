using UnityEngine;

public class AreaGimmickManager : MonoBehaviour
{
    [Header("真ん中で分かれている2つのエリア")]
    [SerializeField] private AreaController areaLeft;
    [SerializeField] private AreaController areaRight;

    // ボタン等から呼び出されるエリア入れ替え関数
    public void SwapAllAreas()
    {
        if (areaLeft != null && areaRight != null)
        {
            areaLeft.SwapColor();
            areaRight.SwapColor();
            Debug.Log("左右のエリアの状態が入れ替わりました。");
        }
    }
}