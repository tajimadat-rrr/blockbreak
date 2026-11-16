
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private int score = 0;

    // 最高スコア
    private int highScore = 0;

    // 最高スコア表示
    [SerializeField] private TMP_Text titleHighScoreText;
    [SerializeField] private TMP_Text gameHighScoreText;
    [SerializeField] private TMP_Text gameOverHighScoreText;

    private int life = 1;
    private int combo = 0;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text lifeText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text comboEffectText;

    // ========================================
    // コンボ演出
    // ========================================

    [Header("Combo Effect")]
    [SerializeField] private float comboEffectDuration = 0.8f;
    [SerializeField] private float comboEffectMaxScale = 1.5f;

    private Coroutine comboEffectCoroutine;
    private Vector3 comboEffectOriginalScale;

    // ========================================
    // 各種参照
    // ========================================

    [SerializeField] private BlockSpawner blockSpawner;

    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultScoreText;

    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject pausePanel;

    private bool isGameStarted = false;
    private bool isPaused = false;
    private bool isGameOver = false;
    private bool howToPlayFromPause = false;

    private static bool retryAndStart = false;

    [SerializeField] private Ball ball;
    [SerializeField] private Paddle paddle;

    private int nextDifficultyScore = 3000;

    // ========================================
    // 初期化
    // ========================================

    private void Start()
    {
        gameOverPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        startPanel.SetActive(true);

        howToPlayPanel.SetActive(false);

        scoreText.text = "Score : " + score;
        lifeText.text = "Life : " + life;
        comboText.text = "Combo : " + combo;

        // コンボ演出の初期状態
        comboEffectOriginalScale =
            comboEffectText.transform.localScale;

        comboEffectText.gameObject.SetActive(false);

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        UpdateHighScoreText();

        Time.timeScale = 1f;

        pausePanel.SetActive(false);

        isGameStarted = false;
        isPaused = false;
        isGameOver = false;

        AudioManager.Instance?.PlayTitleBGM();

        if (retryAndStart)
        {
            retryAndStart = false;
            GameStart();
        }
    }

    private void Update()
    {
        if (!isGameStarted || isGameOver)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (howToPlayPanel.activeSelf &&
                howToPlayFromPause)
            {
                CloseHowToPlay();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ========================================
    // ゲーム開始
    // ========================================

    public void GameStart()
    {
        isGameStarted = true;

        startPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        ball.InitializeBallCount();

        paddle.StartMove();
        ball.Startball();

        blockSpawner.StartRows();

        AudioManager.Instance?.PlayBattleBGM();
    }

    // ========================================
    // スコア
    // ========================================

    public void AddScore()
    {
        int addScore = 100;

        if (combo >= 15)
        {
            addScore = 400;
        }
        else if (combo >= 10)
        {
            addScore = 300;
        }
        else if (combo >= 5)
        {
            addScore = 200;
        }

        score += addScore;

        UpdateHighScore();

        scoreText.text = "Score :" + score;

        CheckDifficulty();

        Debug.Log("獲得スコア:" + addScore);
        Debug.Log("Score : " + score);
    }

    public Ball GetMainBall()
    {
        return ball;
    }

    // ========================================
    // コンボ加算
    // ========================================

    public void AddCombo()
    {
        combo++;

        blockSpawner.UpdateComboInterval(combo);

        comboText.text = "Combo :" + combo;

        if (combo % 5 == 0)
        {
            // 前の演出が残っていたら停止
            if (comboEffectCoroutine != null)
            {
                StopCoroutine(comboEffectCoroutine);
                comboEffectCoroutine = null;
            }

            comboEffectCoroutine =
                StartCoroutine(ComboEffect());
        }

        Debug.Log("Combo :" + combo);
    }

    // ========================================
    // コンボ文字演出
    // ========================================

    private IEnumerator ComboEffect()
    {
        int currentCombo = combo;

        Color effectColor;
        string message;

        if (currentCombo >= 20)
        {
            effectColor = new Color(0.8f, 0.4f, 1f);
            message = "LEGENDARY!";
        }
        else if (currentCombo >= 15)
        {
            effectColor = Color.red;
            message = "AMAZING COMBO!";
        }
        else if (currentCombo >= 10)
        {
            effectColor = new Color(1f, 0.5f, 0f);
            message = "GREAT COMBO!";
        }
        else
        {
            effectColor = Color.yellow;
            message = "NICE COMBO!";
        }

        comboEffectText.text =
            currentCombo + " COMBO!\n" + message;

        comboEffectText.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < comboEffectDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / comboEffectDuration
            );

            // 大きい状態から徐々に縮小
            float scale = Mathf.Lerp(
                comboEffectMaxScale,
                1f,
                t
            );

            comboEffectText.transform.localScale =
                comboEffectOriginalScale * scale;

            // 徐々に透明にする
            Color color = effectColor;
            color.a = 1f - t;

            comboEffectText.color = color;

            yield return null;
        }

        comboEffectText.transform.localScale =
            comboEffectOriginalScale;

        comboEffectText.gameObject.SetActive(false);

        comboEffectCoroutine = null;
    }

    // ========================================
    // コンボ演出リセット
    // ========================================

    private void ResetComboEffect()
    {
        if (comboEffectCoroutine != null)
        {
            StopCoroutine(comboEffectCoroutine);
            comboEffectCoroutine = null;
        }

        comboEffectText.transform.localScale =
            comboEffectOriginalScale;

        comboEffectText.gameObject.SetActive(false);
    }

    // ========================================
    // ボールミス
    // ========================================

    public void MissBall()
    {
        life--;

        lifeText.text = "Life : " + life;

        combo = 0;

        ResetComboEffect();

        blockSpawner.UpdateComboInterval(combo);

        comboText.text = "Combo:" + combo;

        Debug.Log("LIFE: " + life);

        if (life <= 0)
        {
            GameOver();
        }
        else
        {
            // 復活時に回復ブロックの出現率を下げる
            blockSpawner.ReduceLifeBlockRate();

            // ブロックを初期配置に戻す
            blockSpawner.ResetBlocks();
        }
    }

    // ========================================
    // リトライ
    // ========================================

    public void Retry()
    {
        Time.timeScale = 1f;

        retryAndStart = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // ========================================
    // GAME OVER
    // ========================================

    private void GameOver()
    {
        isGameOver = true;
        isPaused = false;

        Time.timeScale = 1f;

        ResetComboEffect();

        pausePanel.SetActive(false);

        blockSpawner.StopRows();

        gameUIPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        resultScoreText.text = "Score :" + score;

        Ball[] balls = FindObjectsByType<Ball>(
            FindObjectsSortMode.None
        );

        foreach (Ball b in balls)
        {
            b.StopBall();
        }

        AudioManager.Instance?.PlayGameOverBGM();

        Debug.Log("Game Over");
    }

    // ========================================
    // 危険ライン到達
    // ========================================

    public void BlockGameOver()
    {
        if (life <= 0)
        {
            return;
        }

        life--;

        lifeText.text = "Life : " + life;

        combo = 0;

        ResetComboEffect();

        blockSpawner.UpdateComboInterval(combo);

        comboText.text = "Combo : " + combo;

        Debug.Log(
            "ブロック危険ライン到達 LIFE : " + life
        );

        if (life <= 0)
        {
            GameOver();
        }
        else
        {
            blockSpawner.ReduceLifeBlockRate();
            blockSpawner.ResetBlocks();
        }
    }

    // ========================================
    // LIFE増加
    // ========================================

    public void AddLife()
    {
        if (life >= 3)
        {
            return;
        }

        life++;

        lifeText.text = "Life : " + life;

        Debug.Log("Life : " + life);
    }

    public bool HasLife()
    {
        return life > 0;
    }

    // ========================================
    // 難易度上昇
    // ========================================

    public void DifficultyUp()
    {
        Debug.Log("Difficultyが呼ばれた");

        ball.AddSpeed();
        ball.ShrinkBall();
    }

    // ========================================
    // オレンジブロック
    // ========================================

    public void AddHighScore()
    {
        score += 500;

        UpdateHighScore();

        scoreText.text = "Score :" + score;

        Debug.Log("High Score Blok + 500");

        CheckDifficulty();
    }

    private void CheckDifficulty()
    {
        while (score >= nextDifficultyScore)
        {
            DifficultyUp();

            nextDifficultyScore += 3000;
        }
    }

    public void AddBombScore()
    {
        int addScore = 100;

        score += addScore;

        scoreText.text = "Score : " + score;

        Debug.Log("Bomb Score : " + score);
    }

    // ========================================
    // 操作説明
    // ========================================

    public void OpenHowToPlay()
    {
        howToPlayFromPause = false;

        startPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void OpenHowToPlayFromPause()
    {
        if (!isPaused)
        {
            return;
        }

        howToPlayFromPause = true;

        pausePanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);

        if (howToPlayFromPause)
        {
            pausePanel.SetActive(true);
            howToPlayFromPause = false;
        }
        else
        {
            startPanel.SetActive(true);
        }
    }

    // ========================================
    // ハイスコア
    // ========================================

    private void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt(
                "HighScore",
                highScore
            );

            PlayerPrefs.Save();

            UpdateHighScoreText();
        }
    }

    private void UpdateHighScoreText()
    {
        string text =
            "HIGH SCORE : " + highScore;

        titleHighScoreText.text = text;
        gameHighScoreText.text = text;
        gameOverHighScoreText.text = text;
    }

    // ========================================
    // ポーズ
    // ========================================

    public void PauseGame()
    {
        if (!isGameStarted ||
            isGameOver ||
            isPaused)
        {
            return;
        }

        isPaused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        AudioManager.Instance?.PauseBGM();
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        AudioManager.Instance?.ResumeBGM();
    }

    // ========================================
    // タイトルへ戻る
    // ========================================

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
