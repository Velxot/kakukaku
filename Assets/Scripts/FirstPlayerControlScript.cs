using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FirstPlayerControlScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ScoreTMP;
    [SerializeField]
    private TextMeshProUGUI ChainTMP;
    [SerializeField]
    private TextMeshProUGUI GameOverTMP;
    [SerializeField]
    private GameObject blockPrefab; // ブロックのプレハブ

    private const int HEIGHT = 12;
    private const int WIDTH = 6;

    private int[,] field = new int[HEIGHT, WIDTH];

    private List<GameObject> blockObjects = new List<GameObject>(); // ブロックオブジェクトを管理
    private int colorNum;
    private int posNum;
    private int score = 0; // スコア
    private int chainCount = 0; // 連鎖カウント

    void Start()
    {
        colorNum = Random.Range(1, 4);
        posNum = 2; // 初期位置（中央付近）
        ScoreTMP.text = score.ToString();
        ChainTMP.text = chainCount.ToString();
        SpawnBlock();
    }

    // Update is called once per frame
    void Update()
    {
        ScoreTMP.text = score.ToString();
        ChainTMP.text = chainCount.ToString();
        HandleInput();
        DeathCheck();
    }

    void DeathCheck()
    {
        for(int i = 0; i < WIDTH; i++)
        {
            if (field[0, i] != 0)
            {
                //ゲームオーバー
                GameOverTMP.text = "GAME OVER";
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) && posNum > 0)
        {
            posNum--;
            UpdateBlockPosition();
        }
        if (Input.GetKeyDown(KeyCode.D) && posNum < WIDTH - 1)
        {
            posNum++;
            UpdateBlockPosition();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            chainCount = 0;
            PlaceBlock();
            CheckAndEraseField();
        }
    }

    void SpawnBlock()
    {
        GameObject newBlock = Instantiate(blockPrefab, new Vector3(-3f + posNum * 0.5f, -2f, 0), Quaternion.identity);
        SpriteRenderer sr = newBlock.GetComponent<SpriteRenderer>();

        colorNum = Random.Range(1, 4);

        if (sr != null)
        {
            switch (colorNum)
            {
                case 1: sr.color = Color.red; break;
                case 2: sr.color = Color.blue; break;
                case 3: sr.color = Color.green; break;
                default: sr.color = Color.white; break;
            }
        }

        blockObjects.Add(newBlock);
    }

    void UpdateBlockPosition()
    {
        if (blockObjects.Count > 0)
        {
            GameObject currentBlock = blockObjects[blockObjects.Count - 1];

            // ブロックが破棄されている場合は新しいブロックを生成
            if (currentBlock == null)
            {
                SpawnBlock();
            }
            else
            {
                currentBlock.transform.position = new Vector3(-3f + posNum * 0.5f, -2f, 0);
            }
        }
    }


    void PlaceBlock()
    {
        if (field[HEIGHT - 1, posNum] == 0)
        {
            field[HEIGHT - 1, posNum] = colorNum;
        }
        else
        {
            for (int i = 0; i < HEIGHT - 1; i++)
            {
                field[i, posNum] = field[i + 1, posNum];
            }
            field[HEIGHT - 1, posNum] = colorNum;
        }

        UpdateFieldDisplay();
    }

    void UpdateFieldDisplay()
    {
        foreach (GameObject obj in blockObjects)
        {
            Destroy(obj);
        }
        blockObjects.Clear();

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (field[y, x] != 0)
                {
                    GameObject newBlock = Instantiate(blockPrefab, new Vector3(-3f + x * 0.5f, -y * 0.5f + 4, 0), Quaternion.identity);
                    SpriteRenderer sr = newBlock.GetComponent<SpriteRenderer>();

                    if (sr != null)
                    {
                        switch (field[y, x])
                        {
                            case 1: sr.color = Color.red; break;
                            case 2: sr.color = Color.blue; break;
                            case 3: sr.color = Color.green; break;
                            default: sr.color = Color.white; break;
                        }
                    }

                    blockObjects.Add(newBlock);
                }
            }
        }
    }

    void CheckAndEraseField()
    {
        bool[,] visited = new bool[HEIGHT, WIDTH];
        List<Vector2Int> toErase = new List<Vector2Int>();

        // すべてのブロックをチェック
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (field[y, x] != 0 && !visited[y, x])
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    int color = field[y, x];
                    FindConnectedBlocks(x, y, color, visited, group);

                    // 4つ以上なら消去リストに追加
                    if (group.Count >= 4)
                    {
                        toErase.AddRange(group);
                    }
                }
            }
        }

        // 消去処理開始
        if (toErase.Count > 0)
        {
            chainCount++;
            StartCoroutine(HandleBlockErase(toErase)); // 連鎖処理
            score += chainCount * 100;
        }
        else
        {
            // 消去が起きなかった場合 → 新しいブロック生成
            SpawnNewBlock();
        }
    }

    // 連鎖終了後のブロック生成
    void SpawnNewBlock()
    {
        colorNum = Random.Range(1, 4);
        posNum = 2;
        SpawnBlock();
    }






    void FindConnectedBlocks(int x, int y, int color, bool[,] visited, List<Vector2Int> group)
    {
        if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT || visited[y, x] || field[y, x] != color)
        {
            return;
        }

        visited[y, x] = true;
        group.Add(new Vector2Int(x, y));

        FindConnectedBlocks(x + 1, y, color, visited, group);
        FindConnectedBlocks(x - 1, y, color, visited, group);
        FindConnectedBlocks(x, y + 1, color, visited, group);
        FindConnectedBlocks(x, y - 1, color, visited, group);
    }

    void ApplyGravity()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            int emptyRow = -1; // 空の行位置
            for (int y = HEIGHT - 1; y >= 0; y--) // 上から下へ落とす
            {
                if (field[y, x] == 0 && emptyRow == -1)
                {
                    emptyRow = y; // 空行検出
                }
                else if (field[y, x] != 0 && emptyRow != -1)
                {
                    // ブロック落下
                    field[emptyRow, x] = field[y, x];
                    field[y, x] = 0;
                    emptyRow--;
                }
            }
        }

        UpdateFieldDisplay(); // 盤面更新
    }




    IEnumerator FadeAndDestroy(GameObject block, float duration)
    {
        SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float startAlpha = sr.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }

        blockObjects.Remove(block);  // リストから削除
        Destroy(block);  // ブロックを削除
    }


    GameObject GetBlockAtPosition(int x, int y)
    {
        for (int i = blockObjects.Count - 1; i >= 0; i--)
        {
            GameObject block = blockObjects[i];
            if (block == null)
            {
                blockObjects.RemoveAt(i);
                continue;
            }

            // 位置が一致するブロックを取得
            if (Vector2.Distance(block.transform.position, new Vector2(x, -y + 10)) < 0.1f)
            {
                return block;
            }
        }
        return null;
    }



    IEnumerator WaitAndApplyGravity(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyGravity(); // 落下処理
        yield return new WaitForSeconds(0.1f); // 連鎖終了後の少しの待機

        // 連鎖終了後、新しいブロック生成
        colorNum = Random.Range(1, 4);
        posNum = 2;
        SpawnBlock();
    }

    IEnumerator HandleBlockErase(List<Vector2Int> toErase)
    {
        // フェードアウトで消去
        foreach (Vector2Int pos in toErase)
        {
            GameObject block = GetBlockAtPosition(pos.x, pos.y);
            if (block != null)
            {
                StartCoroutine(FadeAndDestroy(block, 0.5f)); // 0.5秒でフェード
            }
        }

        yield return new WaitForSeconds(0.5f); // フェードアウト待機

        // 盤面更新
        foreach (Vector2Int pos in toErase)
        {
            field[pos.y, pos.x] = 0;
        }

        ApplyGravity(); // 落下後の盤面更新
        yield return new WaitForSeconds(0.1f); // 落下待機

        // 連鎖続行 or 新ブロック生成
        if (CheckForMoreChains())
        {
            CheckAndEraseField(); // 再帰的にチェック
        }
        else
        {
            SpawnNewBlock(); // 連鎖終了後、新しいブロックを生成
        }
    }


    bool CheckForMoreChains()
    {
        bool[,] visited = new bool[HEIGHT, WIDTH];
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (field[y, x] != 0 && !visited[y, x])
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    int color = field[y, x];
                    FindConnectedBlocks(x, y, color, visited, group);

                    // 4つ以上繋がっていたら連鎖継続
                    if (group.Count >= 4)
                    {
                        return true;
                    }
                }
            }
        }
        return false;  // 連鎖が終了
    }
}
