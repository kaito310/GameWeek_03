using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// キャラクターをゴールまで導くプレイヤーを定義したクラス
/// </summary>
public class Player : MonoBehaviour
{

    bool isBlockPlaced = false; // ブロックを設置したかどうか
    bool[] placedBlock = new bool[(int)PlaceBlockType.AMOUNT]; // ブロックを設置したらtrueにする

    [Header("設置できるオブジェクト(imagesと合わせる)"), SerializeField]
    GameObject[] prefabs;
    [Header("設置できるオブジェクトの色(prefabsと合わせる)"), SerializeField]
    Image[] images;

    Color originalColor = Color.white; // 画像の元の色
    Color otherSelectColor = new Color32(0x7D, 0x7D, 0x7D, 0xFF); // もう一方が選択中の画像の色
    GameObject placeBlock; // 実際に設置するブロック
    int blockIdx = (int)PlaceBlockType.NO_PLACE;
   
    const float MAX_COOL_TIMER = 1.0f;
    float[] coolTimer = new float[(int)PlaceBlockType.AMOUNT];

    GameManager gameManager;
    MapManager mapManager;

    public bool GetIsBlockPlaced() { return isBlockPlaced; }
    public void ResetBlockPlaced() {  isBlockPlaced = false; }

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        for (int i = 0; i < (int)PlaceBlockType.AMOUNT; i++)
        {
            placedBlock[i] = false;
            coolTimer[i] = MAX_COOL_TIMER;
        }        
    }

    void Start()
    {
    }

    void Update()
    {
        if (gameManager.GetIsGameClear() || gameManager.GetIsGameOver()) return;

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log(transform.position);

        if (placeBlock != null && Input.GetMouseButtonDown(0))
        {
            Vector3 limitPos = PosChanger.ChangeToGamePos(new Vector2Int(mapManager.GetMapWidth(), mapManager.GetMapHeight()));
            // セル外でクリックしたらキャンセル動作
            if (transform.position.x < 0 || transform.position.x > limitPos.x || transform.position.y > 0 || transform.position.y < limitPos.y)
            {
                ResetBlockSelecting();
            }
            // セル内でクリックしたら、そのセルに置く判定
            else
            {
                Vector3 roundPos = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
                Vector2Int cellPos = PosChanger.ChangeToCellPos(roundPos);
                isBlockPlaced = mapManager.SetBlockOfCell((PlaceBlockType)blockIdx, placeBlock, cellPos.x, cellPos.y);
                // 置けなくてもブロックの選択はリセットする
                ResetBlockSelecting();
            }
        }
    }

    /// <summary>
    /// 橋ブロックをクリックしたら呼ぶ
    /// </summary>
    public void ClickBridgeBlock()
    {
        ResetBlockSelecting();

        gameManager.SetGuideText("↓ 選択中の\r\nブロック");
        blockIdx = (int)PlaceBlockType.BRIDGE;
        placeBlock = prefabs[blockIdx];
        // 選択していないブロックは暗くしておく
        images[(int)PlaceBlockType.CRATE].color = otherSelectColor;
    }

    /// <summary>
    /// 木箱ブロックをクリックしたら呼ぶ
    /// </summary>
    public void ClickCrateBlock()
    {
        ResetBlockSelecting();

        gameManager.SetGuideText("↓ 選択中の\r\nブロック");
        blockIdx = (int)PlaceBlockType.CRATE;
        placeBlock = prefabs[blockIdx];
        // 選択していないブロックは暗くしておく
        images[(int)PlaceBlockType.BRIDGE].color = otherSelectColor;
    }

    /// <summary>
    /// ブロックの選択状態をリセットする関数
    /// </summary>
    void ResetBlockSelecting()
    {
        if (blockIdx == (int)PlaceBlockType.NO_PLACE) return;

        gameManager.SetGuideText("↓ 配置可能な\r\nブロック");
        placeBlock = null;
        for (int i = 0; i < (int)PlaceBlockType.AMOUNT; i++)
        {
            images[i].color = originalColor;
        }
        blockIdx = (int)PlaceBlockType.NO_PLACE;
    }
}
