using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Megaphone : MonoBehaviour
{
    [Header("基本設定")]
    [Tooltip("生成する音波のプレハブ")]
    public GameObject soundWavePrefab;

    [Tooltip("音波を生成する子オブジェクトのTransform")]
    public Transform firePoint;

    [Tooltip("通過した音波の音量を何倍にするか")]
    public float volumeMultiplier = 1.5f;

    [Header("角度制限設定")]
    [Tooltip("チェックを入れると、特定の角度から侵入した音波のみを増幅します")]
    public bool useAngleLimit = true;

    [Tooltip("メガホンの正面方向から、どれだけの傾き（度数法）まで受け付けるか")]
    [Range(0f, 180f)]
    public float allowableAngle = 45f;

    [Header("クールタイム設定")]
    [Tooltip("音波を生成後、次の音波を受け付けるまでの時間（秒）")]
    public float coolTime = 0.5f;

    [Header("レイヤー設定")]
    [Tooltip("音波オブジェクトが属するレイヤー")]
    public LayerMask soundWaveLayer;

    private float coolTimeTimer = 0f;
    private bool isReady = true;

    void Update()
    {
        // クールタイムのカウントダウン処理
        if (!isReady)
        {
            coolTimeTimer -= Time.deltaTime;
            if (coolTimeTimer <= 0f)
            {
                isReady = true;
            }
        }
    }

    // ★ OnCollisionEnter2D から OnTriggerEnter2D に変更
    // これにより、音波がトリガー（Is Trigger=ON）でも地面としての実体を保ったまま検知できます。
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // クールタイム中なら衝突を完全に無視する
        if (!isReady) return;

        // 接触したオブジェクトが音波レイヤーかどうか判定
        if (((1 << collider.gameObject.layer) & soundWaveLayer) != 0)
        {
            SoundWave originalWave = collider.gameObject.GetComponent<SoundWave>();

            if (originalWave != null)
            {
                // 角度制限が有効な場合、向きのチェックを行う
                if (useAngleLimit)
                {
                    // メガホンの正面方向（+90度補正）
                    Vector2 megaphoneForward = Quaternion.Euler(0, 0, 90f) * transform.right;

                    // 侵入してきた音波の進行方向（colliderから直接transformを取得）
                    Vector2 waveDirection = collider.transform.right;

                    // 補正した正面と、音波の進行方向のなす角を計算
                    float angleDifference = Vector2.Angle(megaphoneForward, waveDirection);

                    // 許容範囲外の角度であれば、メガホンを作動させずに処理を抜ける
                    if (angleDifference > allowableAngle)
                    {
                        return;
                    }
                }

                float incomingVolume = originalWave.volume;

                // 1. 先に古い音波を削除
                Destroy(collider.gameObject);

                // 2. 新しい音波を生成
                Vector3 targetPosition = firePoint != null ? firePoint.position : transform.position;
                Quaternion correctedRotation = transform.rotation * Quaternion.Euler(0, 0, 90f);
                GameObject newWaveObj = Instantiate(soundWavePrefab, targetPosition, correctedRotation);

                SoundWave newWave = newWaveObj.GetComponent<SoundWave>();
                if (newWave != null)
                {
                    newWave.volume = incomingVolume * volumeMultiplier;
                    newWave.SetupWave();
                }

                // 3. クールタイムを開始（判定を一定時間オフにする）
                isReady = false;
                coolTimeTimer = coolTime;
            }
        }
    }
}