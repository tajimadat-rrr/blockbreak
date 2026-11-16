
using System.Collections;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;

    [SerializeField] private float rowInterval = 15f;

    // 時間経過による難易度上昇
    [SerializeField] private float difficultyInterval = 30f;
    [SerializeField] private float intervalDecrease = 2f;
    [SerializeField] private float minRowInterval = 7f;

    private float elapsedTime = 0f;

    [SerializeField] private float rowHeight = 0.7f;
    [SerializeField] private float topY = 4f;
    [SerializeField] private Transform gameOverArea;

    [SerializeField] private int columns = 6;
    [SerializeField] private float startX = -3f;
    [SerializeField] private float columnSpacing = 1.1f;

    // ブロック補充
    [SerializeField] private int minBlockCount = 12;
    [SerializeField] private float refillDelay = 2f;

    // ランダムサイズ
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1f;

    // ========================================
    // 回復ブロック出現率
    // ========================================

    [Header("Life Block Rate")]
    [SerializeField, Range(0f, 100f)]
    private float initialLifeBlockRate = 10f;

    [SerializeField, Range(0f, 100f)]
    private float lifeRateDecreasePerRevive = 5f;

    private float currentLifeBlockRate;

    private int reviveCount = 0;

    private int blockCount = 0;

    private bool isRunning = false;
    private bool isRefilling = false;

    // ========================================
    // 初期化
    // ========================================

    private void Start()
    {
        currentLifeBlockRate =
            Mathf.Clamp(initialLifeBlockRate, 0f, 100f);

        // 最初は6行生成
        for (int row = 0; row < 6; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float x = startX + column * columnSpacing;
                float y = topY - row * rowHeight;

                CreateBlock(new Vector3(x, y, 0f));
            }
        }
    }

    // ========================================
    // 復活時の回復ブロック確率変更
    // ========================================

    public void ReduceLifeBlockRate()
    {
        reviveCount++;

        currentLifeBlockRate = Mathf.Max(
            0f,
            currentLifeBlockRate -
                lifeRateDecreasePerRevive
        );

        Debug.Log(
            "復活回数 : " + reviveCount +
            " / 回復ブロック出現率 : " +
            currentLifeBlockRate + "%"
        );
    }

    // ========================================
    // ブロック生成
    // ========================================

    private void CreateBlock(Vector3 position)
    {
        GameObject newBlock = Instantiate(
            blockPrefab,
            position,
            Quaternion.identity
        );

        blockCount++;

        RandomBlock(newBlock);
    }

    // ========================================
    // ブロック破壊
    // ========================================

    public void BlockDestroyed(Vector3 destroyedPosition)
    {
        blockCount = Mathf.Max(0, blockCount - 1);

        Debug.Log("残りBlock : " + blockCount);

        if (isRunning &&
            blockCount <= minBlockCount &&
            !isRefilling)
        {
            StartCoroutine(RefillRows());
        }
    }

    private IEnumerator RefillRows()
    {
        isRefilling = true;

        yield return new WaitForSeconds(refillDelay);

        if (isRunning && blockCount <= minBlockCount)
        {
            Debug.Log("ブロック不足のため補充");

            AddNewRow();
        }

        isRefilling = false;
    }

    // ========================================
    // ブロックの種類・色・サイズ
    // ========================================

    private void RandomBlock(GameObject block)
    {
        SpriteRenderer renderer =
            block.GetComponent<SpriteRenderer>();

        Block blockScript =
            block.GetComponent<Block>();

        // 回復ブロックの現在確率
        float lifeRate = currentLifeBlockRate;

        // 回復以外のブロックに割り当てる確率
        float otherRate = 100f - lifeRate;

        // 初期確率で回復以外が占める割合
        float initialOtherRate =
            100f - initialLifeBlockRate;

        // 回復以外の元の割合を維持して再配分
        float multiplier =
            initialOtherRate > 0f
                ? otherRate / initialOtherRate
                : 1f;

        float normalRate = 50f * multiplier;
        float bombRate = 10f * multiplier;
        float bigBallRate = 10f * multiplier;
        float multiBallRate = 10f * multiplier;
        float speedUpRate = 5f * multiplier;

        float randomColor = Random.Range(0f, 100f);

        float threshold = normalRate;

        if (randomColor < threshold)
        {
            renderer.color = Color.white;
            blockScript.SetBlockType(BlockType.Normal);
        }
        else
        {
            threshold += lifeRate;

            if (randomColor < threshold)
            {
                renderer.color = Color.yellow;
                blockScript.SetBlockType(BlockType.Life);
            }
            else
            {
                threshold += bombRate;

                if (randomColor < threshold)
                {
                    renderer.color = Color.red;
                    blockScript.SetBlockType(BlockType.Bomb);
                }
                else
                {
                    threshold += bigBallRate;

                    if (randomColor < threshold)
                    {
                        renderer.color =
                            new Color(0.3f, 0.8f, 1f);

                        blockScript.SetBlockType(
                            BlockType.BigBall
                        );
                    }
                    else
                    {
                        threshold += multiBallRate;

                        if (randomColor < threshold)
                        {
                            renderer.color = Color.green;

                            blockScript.SetBlockType(
                                BlockType.MultiBall
                            );
                        }
                        else
                        {
                            threshold += speedUpRate;

                            if (randomColor < threshold)
                            {
                                renderer.color =
                                    new Color(0.65f, 0.3f, 1f);

                                blockScript.SetBlockType(
                                    BlockType.SpeedUp
                                );
                            }
                            else
                            {
                                renderer.color =
                                    new Color(1f, 0.5f, 0f);

                                blockScript.SetBlockType(
                                    BlockType.HighScore
                                );
                            }
                        }
                    }
                }
            }
        }

        // サイズをランダムに変更
        float randomScale =
            Random.Range(minScale, maxScale);

        block.transform.localScale =
            new Vector3(
                randomScale,
                randomScale * 0.5f,
                1f
            );
    }

    // ========================================
    // ゲーム開始
    // ========================================

    public void StartRows()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;

        elapsedTime = 0f;

        Debug.Log("行生成スタート");

        StartCoroutine(AddRowLoop());
        StartCoroutine(DifficultyLoop());
    }

    // ========================================
    // 行生成
    // ========================================

    private IEnumerator AddRowLoop()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(rowInterval);

            if (!isRunning)
            {
                yield break;
            }

            Debug.Log("時間経過による行生成");

            AddNewRow();
        }
    }

    // ========================================
    // 時間経過による難易度上昇
    // ========================================

    private IEnumerator DifficultyLoop()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(difficultyInterval);

            if (!isRunning)
            {
                yield break;
            }

            elapsedTime += difficultyInterval;

            rowInterval = Mathf.Max(
                minRowInterval,
                rowInterval - intervalDecrease
            );

            Debug.Log(
                "難易度UP！ 経過時間 : " + elapsedTime +
                "秒 / ブロック生成間隔 : " +
                rowInterval + "秒"
            );
        }
    }

    // ========================================
    // 新しい行を生成
    // ========================================

    private void AddNewRow()
    {
        if (!isRunning)
        {
            return;
        }

        Block[] blocks = FindObjectsByType<Block>(
            FindObjectsSortMode.None
        );

        foreach (Block block in blocks)
        {
            block.transform.position +=
                Vector3.down * rowHeight;
        }

        int createdCount = 0;

        for (int column = 0; column < columns; column++)
        {
            float x = startX + column * columnSpacing;

            Vector3 position =
                new Vector3(x, topY, 0f);

            CreateBlock(position);

            createdCount++;
        }

        Debug.Log(
            "新しい行を生成 : " + createdCount +
            "個 / 設定列数 : " + columns
        );

        CheckDangerLine();
    }

    // ========================================
    // 危険ライン判定
    // ========================================

    private void CheckDangerLine()
    {
        Block[] blocks = FindObjectsByType<Block>(
            FindObjectsSortMode.None
        );

        bool reachedDanger = false;

        foreach (Block block in blocks)
        {
            if (block.transform.position.y <=
                gameOverArea.position.y)
            {
                reachedDanger = true;

                Destroy(block.gameObject);

                blockCount = Mathf.Max(
                    0,
                    blockCount - 1
                );
            }
        }

        if (reachedDanger)
        {
            GameManager manager =
                FindFirstObjectByType<GameManager>();

            if (manager != null)
            {
                manager.BlockGameOver();
            }
        }
    }

    // ========================================
    // ゲームオーバー
    // ========================================

    public void StopRows()
    {
        isRunning = false;

        StopAllCoroutines();

        isRefilling = false;
    }

    // ========================================
    // コンボによる生成間隔変更
    // ========================================

    public void UpdateComboInterval(int combo)
    {
        int level = combo / 5;

        rowInterval = Mathf.Max(
            minRowInterval,
            15f - level * 2f
        );
    }

    // ========================================
    // LIFE減少時のブロックリセット
    // ========================================

    public void ResetBlocks()
    {
        StopAllCoroutines();

        isRefilling = false;
        rowInterval = 15f;

        Block[] blocks = FindObjectsByType<Block>(
            FindObjectsSortMode.None
        );

        foreach (Block block in blocks)
        {
            Destroy(block.gameObject);
        }

        blockCount = 0;

        // 初期36個を再生成
        for (int row = 0; row < 6; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float x = startX + column * columnSpacing;
                float y = topY - row * rowHeight;

                CreateBlock(new Vector3(x, y, 0f));
            }
        }

        if (isRunning)
        {
            StartCoroutine(AddRowLoop());

            // リセット後も時間経過による
            // 難易度上昇を再開
            StartCoroutine(DifficultyLoop());
        }
    }
}
