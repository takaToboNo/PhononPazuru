using System.Collections.Generic;
using UnityEngine;

// エリアの侵入検知のため、Collider2D（Is Trigger）が必要です
[RequireComponent(typeof(Collider2D))]
public class AreaController : MonoBehaviour
{
    public enum AreaColor { Red, Blue }

    [Header("現在のエリアの色")]
    [SerializeField] private AreaColor currentType;
    public AreaColor CurrentColor => currentType;

    [Header("色ごとの設定")]
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color blueColor = Color.blue;

    private SpriteRenderer spriteRenderer;

    // 現在このエリア内に存在している音波のリスト
    private List<SoundWave> wavesInArea = new List<SoundWave>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateAreaState();
    }

    public void SwapColor()
    {
        currentType = (currentType == AreaColor.Red) ? AreaColor.Blue : AreaColor.Red;
        UpdateAreaState();
    }

    private void UpdateAreaState()
    {
        if (spriteRenderer == null) return;

        if (currentType == AreaColor.Red)
        {
            spriteRenderer.color = redColor;

            // 赤エリアになった瞬間、エリア内にある音波をすべて消滅（ミュート）させる
            ClearAllWavesInArea();
        }
        else
        {
            spriteRenderer.color = blueColor;
        }
    }

    // エリア内の音波をすべて削除する処理
    private void ClearAllWavesInArea()
    {
        // リストの後ろからループして安全に削除
        for (int i = wavesInArea.Count - 1; i >= 0; i--)
        {
            if (wavesInArea[i] != null)
            {
                Destroy(wavesInArea[i].gameObject);
            }
        }
        wavesInArea.Clear();
    }

    // --- エリア内に入ってきた音波の監視用コライダー判定 ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SoundWave wave = collision.GetComponent<SoundWave>();
        if (wave != null)
        {
            // もしここが赤エリアなら、入ってきた瞬間に消滅（ミュート）させる
            if (currentType == AreaColor.Red)
            {
                Destroy(wave.gameObject);
                return;
            }

            // 青エリアならリストに追加して監視
            if (!wavesInArea.Contains(wave))
            {
                wavesInArea.Add(wave);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        SoundWave wave = collision.GetComponent<SoundWave>();
        if (wave != null)
        {
            wavesInArea.Remove(wave);
        }
    }
}


//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(Collider2D))]
//public class AreaController : MonoBehaviour
//{
//    public enum AreaColor { Red, Blue }

//    [Header("現在のエリアの色")]
//    [SerializeField] private AreaColor currentType;
//    public AreaColor CurrentColor => currentType;

//    [Header("色ごとの設定")]
//    [SerializeField] private Color redColor = Color.red;
//    [SerializeField] private Color blueColor = Color.blue;

//    [Header("発射する音波のプレハブ")]
//    [SerializeField] private GameObject soundWavePrefab;

//    private SpriteRenderer spriteRenderer;
//    private List<SoundWave> wavesInArea = new List<SoundWave>();

//    void Awake()
//    {
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        UpdateAreaState(isInitialSetup: true);
//    }

//    public void SwapColor()
//    {
//        currentType = (currentType == AreaColor.Red) ? AreaColor.Blue : AreaColor.Red;
//        UpdateAreaState(isInitialSetup: false);
//    }

//    private void UpdateAreaState(bool isInitialSetup = false)
//    {
//        if (spriteRenderer == null) return;

//        if (currentType == AreaColor.Red)
//        {
//            spriteRenderer.color = redColor;
//            if (!isInitialSetup) ClearAllWavesInArea();
//        }
//        else
//        {
//            spriteRenderer.color = blueColor;
//        }
//    }

//    private void ClearAllWavesInArea()
//    {
//        for (int i = wavesInArea.Count - 1; i >= 0; i--)
//        {
//            if (wavesInArea[i] != null) Destroy(wavesInArea[i].gameObject);
//        }
//        wavesInArea.Clear();
//    }

//    // ★★★【ここが新しいロジック】★★★
//    // エリアのトリガーコライダー内で「何かと何かがぶつかった瞬間」を検知する
//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        // 1. まず侵入してきたのが音波オブジェクト自体なら、これまで通りリスト管理/赤なら消滅
//        SoundWave wave = collision.GetComponent<SoundWave>();
//        if (wave != null)
//        {
//            if (currentType == AreaColor.Red)
//            {
//                Destroy(wave.gameObject);
//                return;
//            }
//            if (!wavesInArea.Contains(wave)) wavesInArea.Add(wave);
//            return;
//        }

//        // 2.【ギミック対策】もし現在「青エリア」で、エリア内で「衝突（何かを当てた等）」が発生した場合
//        // ぶつかったオブジェクトがギミック（例: "Gimmick"タグ）等だった場合、ここから直接音波を生成する
//        if (currentType == AreaColor.Blue)
//        {
//            // ここで「オブジェクトに何かが当たったか」を判定
//            // チームの仕様に合わせて、タグ判定（例: "Gimmick" や "Interactable"）を入れるとより安全です
//            if (collision.CompareTag("Gimmick"))
//            {
//                SpawnSoundWave(collision.transform.position, collision.transform.rotation);
//            }
//        }
//        else
//        {
//            Debug.Log("赤エリア内での衝突のため、音波の発生をミュートしました。");
//        }
//    }

//    // 音波を生成して飛ばす共通関数
//    private void SpawnSoundWave(Vector3 position, Quaternion rotation)
//    {
//        if (soundWavePrefab != null)
//        {
//            GameObject waveObj = Instantiate(soundWavePrefab, position, rotation);
//            SoundWave wave = waveObj.GetComponent<SoundWave>();
//            if (wave != null)
//            {
//                wave.SetupWave();
//                wavesInArea.Add(wave); // 生成した音波を即座に監視リストに入れる
//            }
//        }
//    }

//    private void OnTriggerExit2D(Collider2D collision)
//    {
//        SoundWave wave = collision.GetComponent<SoundWave>();
//        if (wave != null) wavesInArea.Remove(wave);
//    }
//}