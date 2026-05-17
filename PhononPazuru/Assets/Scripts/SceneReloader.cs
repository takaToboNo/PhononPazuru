using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // これを追加！

public class SceneReloader : MonoBehaviour
{
    void Update()
    {
        // 新しいInput Systemでのキー判定
        if (Keyboard.current.rKey.wasPressedThisFrame)
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