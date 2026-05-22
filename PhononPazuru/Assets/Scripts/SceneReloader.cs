using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 必須

public class SceneReloader : MonoBehaviour
{
    // PlayerInputコンポーネントから呼び出されるメソッド
    private void OnRestart(InputValue value)
    {
        if (value.isPressed)
        {
            RestartScene();
        }
    }

    public void RestartScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}