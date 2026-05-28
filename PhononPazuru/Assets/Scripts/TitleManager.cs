using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("画面全体を覆うフェード用画像（黒）")]
    [SerializeField] private Image fadeImage;
    [Header("フェードさせるボタングループ")]
    [SerializeField] private CanvasGroup buttonGroup;

    [Header("フェード速度の設定")]
    [SerializeField] private float sceneFadeInSpeed = 1.5f; // シーン全体のフェードイン（じわーっと遅め）
    [SerializeField] private float buttonFadeSpeed = 4.0f;  // ボタンのフェード（サッと速め）

    [Header("ボタンの登録（上から順番に）")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button endButton;

    [Header("ゲーム終了の確認画面（Panel）")]
    [SerializeField] private GameObject confirmPanel;

    [Header("確認画面のボタン")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private enum TitleState
    {
        SceneFadingIn,    // 1. 黒画面がじわーっと透明になり、シーン全体が出現中
        WaitingForEnter,  // 2. シーンが出きって、Enter入力を待っている状態
        ButtonsFadingIn,  // 3. Enterが押されて、ボタンがフェードイン中
        ActiveMenu        // 4. ボタンも出きって、メニュー操作ができる状態
    }
    private TitleState currentState = TitleState.SceneFadingIn;

    private int selectedIndex = 0;
    private bool isInConfirmMenu = false;

    void Start()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // ボタンは最初は完全に透明にしておく
        if (buttonGroup != null)
        {
            buttonGroup.alpha = 0;
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        // フェード画像（黒）を確実に表示・有効化しておく
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1.0f; // 完全に真っ黒
            fadeImage.color = c;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            // ==========================================
            // ステップ1: 黒画面を透明にしていき、シーン全体を出す
            // ==========================================
            case TitleState.SceneFadingIn:
                if (fadeImage != null)
                {
                    Color c = fadeImage.color;
                    c.a -= Time.deltaTime * sceneFadeInSpeed; // アルファ値を減らす（透明にする）

                    if (c.a <= 0.0f)
                    {
                        c.a = 0.0f;
                        fadeImage.gameObject.SetActive(false); // 完全に透明になったら邪魔なので非アクティブに
                        currentState = TitleState.WaitingForEnter; // Enter待ちへ
                    }
                    fadeImage.color = c;
                }
                else
                {
                    currentState = TitleState.WaitingForEnter;
                }
                break;

            // ==========================================
            // ステップ2: ボタンなし（背景とタイトルのみ）の状態でEnterを待つ
            // ==========================================
            case TitleState.WaitingForEnter:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    currentState = TitleState.ButtonsFadingIn;
                }
                break;

            // ==========================================
            // ステップ3: ボタンをサッとフェードイン
            // ==========================================
            case TitleState.ButtonsFadingIn:
                if (buttonGroup != null)
                {
                    buttonGroup.alpha += Time.deltaTime * buttonFadeSpeed;
                    if (buttonGroup.alpha >= 1.0f)
                    {
                        buttonGroup.alpha = 1.0f;
                        buttonGroup.interactable = true;
                        buttonGroup.blocksRaycasts = true;
                        currentState = TitleState.ActiveMenu;

                        SelectButton(); // 最初のボタンを選択状態に
                    }
                }
                break;

            // ==========================================
            // ステップ4: メニュー操作
            // ==========================================
            case TitleState.ActiveMenu:
                HandleMenuInput();
                break;
        }
    }

    void HandleMenuInput()
    {
        if (isInConfirmMenu)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                selectedIndex = 0;
                SelectConfirmButton();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                selectedIndex = 1;
                SelectConfirmButton();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (selectedIndex == 0) OnConfirmYes();
                else OnConfirmNo();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % 3;
            SelectButton();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex + 2) % 3;
            SelectButton();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedIndex == 0) OnStart();
            else if (selectedIndex == 1) OnSetting();
            else if (selectedIndex == 2) OnEnd();
        }
    }

    void SelectButton()
    {
        if (startButton != null && selectedIndex == 0) startButton.Select();
        if (settingButton != null && selectedIndex == 1) settingButton.Select();
        if (endButton != null && selectedIndex == 2) endButton.Select();
    }

    void SelectConfirmButton()
    {
        if (yesButton != null && selectedIndex == 0) yesButton.Select();
        if (noButton != null && selectedIndex == 1) noButton.Select();
    }

    void OnStart() { SceneManager.LoadScene("Stage1"); }
    void OnSetting() { Debug.Log("設定未実装"); }

    void OnEnd()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
            isInConfirmMenu = true;
            selectedIndex = 1;
            SelectConfirmButton();
        }
    }

    void OnConfirmYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnConfirmNo()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
            isInConfirmMenu = false;
            selectedIndex = 2;
            SelectButton();
        }
    }
}