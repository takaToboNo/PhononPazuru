using UnityEngine;

public class SwitchButton : MonoBehaviour
{
    [Header("エリアマネージャーへの参照")]
    [SerializeField] private AreaGimmickManager gimmickManager;

    [Header("クールタイム（秒）")]
    [SerializeField] private float coolTime = 5f;

    private float timer = 0f;
    private bool isReady = true;

    void Start()
    {
        // 複数人開発用にマネージャーを自動取得
        if (gimmickManager == null)
        {
            gimmickManager = Object.FindFirstObjectByType<AreaGimmickManager>();
        }
    }

    void Update()
    {
        // クールタイム中の場合はタイマーを進める
        if (!isReady)
        {
            timer += Time.deltaTime;
            if (timer >= coolTime)
            {
                isReady = true;
                timer = 0f;
                Debug.Log("ボタンが再度押せるようになりました。");
            }
        }
    }

    // プレイヤーがボタンを踏んだ（またはクリックした）ときに実行
    public void PushButton()
    {
        // クールタイム中なら何も処理しない
        if (!isReady) return;

        if (gimmickManager != null)
        {
            gimmickManager.SwapAllAreas();

            // クールタイム開始
            isReady = false;
            timer = 0f;
            Debug.Log($"エリアを切り替えました！ これから {coolTime} 秒間はボタンを押せません。");
        }
    }

    // 2Dの衝突判定
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // クールタイム中なら、衝突判定の計算自体をスキップして負荷を下げる
        if (!isReady) return;

        // 自分自身、または親オブジェクトに Player タグがあるか確認
        if (collision.CompareTag("Player") ||
           (collision.transform.parent != null && collision.transform.parent.CompareTag("Player")))
        {
            PushButton();
        }
    }
}