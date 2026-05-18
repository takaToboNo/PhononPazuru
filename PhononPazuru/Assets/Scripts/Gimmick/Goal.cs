using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    // インスペクターからプレイヤーのレイヤーを選択できるようにする
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string nextSceneName = "";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトのレイヤーが、指定したPlayerLayerに含まれているか判定
        // (1 << collision.gameObject.layer) でビットシフトしてマスクと比較します
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("プレイヤーがゴールに到達しました！");
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("次のシーンがBuild Settingsに登録されていないか、最後のシーンです。");
            }
        }
    }
}