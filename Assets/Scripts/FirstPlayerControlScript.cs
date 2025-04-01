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
    private GameObject blockPrefab; // �u���b�N�̃v���n�u

    private const int HEIGHT = 12;
    private const int WIDTH = 6;

    private int[,] field = new int[HEIGHT, WIDTH];

    private List<GameObject> blockObjects = new List<GameObject>(); // �u���b�N�I�u�W�F�N�g���Ǘ�
    private int colorNum;
    private int posNum;
    private int score = 0; // �X�R�A
    private int chainCount = 0; // �A���J�E���g

    void Start()
    {
        colorNum = Random.Range(1, 4);
        posNum = 2; // �����ʒu�i�����t�߁j
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
                //�Q�[���I�[�o�[
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

            // �u���b�N���j������Ă���ꍇ�͐V�����u���b�N�𐶐�
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

        // ���ׂẴu���b�N���`�F�b�N
        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                if (field[y, x] != 0 && !visited[y, x])
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    int color = field[y, x];
                    FindConnectedBlocks(x, y, color, visited, group);

                    // 4�ȏ�Ȃ�������X�g�ɒǉ�
                    if (group.Count >= 4)
                    {
                        toErase.AddRange(group);
                    }
                }
            }
        }

        // ���������J�n
        if (toErase.Count > 0)
        {
            chainCount++;
            StartCoroutine(HandleBlockErase(toErase)); // �A������
            score += chainCount * 100;
        }
        else
        {
            // �������N���Ȃ������ꍇ �� �V�����u���b�N����
            SpawnNewBlock();
        }
    }

    // �A���I����̃u���b�N����
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
            int emptyRow = -1; // ��̍s�ʒu
            for (int y = HEIGHT - 1; y >= 0; y--) // �ォ�牺�֗��Ƃ�
            {
                if (field[y, x] == 0 && emptyRow == -1)
                {
                    emptyRow = y; // ��s���o
                }
                else if (field[y, x] != 0 && emptyRow != -1)
                {
                    // �u���b�N����
                    field[emptyRow, x] = field[y, x];
                    field[y, x] = 0;
                    emptyRow--;
                }
            }
        }

        UpdateFieldDisplay(); // �ՖʍX�V
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

        blockObjects.Remove(block);  // ���X�g����폜
        Destroy(block);  // �u���b�N���폜
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

            // �ʒu����v����u���b�N���擾
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
        ApplyGravity(); // ��������
        yield return new WaitForSeconds(0.1f); // �A���I����̏����̑ҋ@

        // �A���I����A�V�����u���b�N����
        colorNum = Random.Range(1, 4);
        posNum = 2;
        SpawnBlock();
    }

    IEnumerator HandleBlockErase(List<Vector2Int> toErase)
    {
        // �t�F�[�h�A�E�g�ŏ���
        foreach (Vector2Int pos in toErase)
        {
            GameObject block = GetBlockAtPosition(pos.x, pos.y);
            if (block != null)
            {
                StartCoroutine(FadeAndDestroy(block, 0.5f)); // 0.5�b�Ńt�F�[�h
            }
        }

        yield return new WaitForSeconds(0.5f); // �t�F�[�h�A�E�g�ҋ@

        // �ՖʍX�V
        foreach (Vector2Int pos in toErase)
        {
            field[pos.y, pos.x] = 0;
        }

        ApplyGravity(); // ������̔ՖʍX�V
        yield return new WaitForSeconds(0.1f); // �����ҋ@

        // �A�����s or �V�u���b�N����
        if (CheckForMoreChains())
        {
            CheckAndEraseField(); // �ċA�I�Ƀ`�F�b�N
        }
        else
        {
            SpawnNewBlock(); // �A���I����A�V�����u���b�N�𐶐�
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

                    // 4�ȏ�q�����Ă�����A���p��
                    if (group.Count >= 4)
                    {
                        return true;
                    }
                }
            }
        }
        return false;  // �A�����I��
    }
}
