using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 座標を変換するためのグローバルクラス
/// </summary>
public static class PosChanger
{
    /// <summary>
    /// ゲーム内座標をマス座標に変換
    /// </summary>
    /// <param name="_gamePos">変換したいゲーム内座標</param>
    /// <returns></returns>
    public static Vector2Int ChangeToCellPos(Vector3 _gamePos)
    {
        return new Vector2Int((int)_gamePos.x / MapManager.CELL_SIZE , (int)_gamePos.y / -MapManager.CELL_SIZE);
    }

    /// <summary>
    /// マス座標をゲーム内座標に変換(マス座標がint型Ver)
    /// </summary>
    /// <param name="_cellPos">変換したいマス座標</param>
    /// <returns></returns>
    public static Vector3 ChangeToGamePos(Vector2Int _cellPos)
    {
        return new Vector3(_cellPos.x * MapManager.CELL_SIZE, _cellPos.y * -MapManager.CELL_SIZE, 0);
    }

    /// <summary>
    /// マス座標をゲーム内座標に変換(マス座標がfloat型Ver)
    /// </summary>
    /// <param name="_cellPos">変換したいマス座標</param>
    /// <returns></returns>
    public static Vector3 ChangeToGamePos(Vector2 _cellPos)
    {
        return new Vector3(_cellPos.x * MapManager.CELL_SIZE, _cellPos.y * -MapManager.CELL_SIZE, 0);
    }
}

/// <summary>
/// マス上に置かれたブロックの種類
/// </summary>
public enum CellBlockType {
    NULL = -99, // エラーチェック用
    CHARA = -9, // ゴールまで導くキャラクター
    GOAL = -10,
    NORMAL = 0, // 通常の、通過可能なブロック
    WALL,       // 壁

    /* ダメージが発生する障害物 */
    RIVER, // 川
    ENEMY, // 敵

    BRIDGE, // 橋(設置物)
    CRATE,  // 木箱(設置物)
}

public enum PlaceBlockType
{
    NO_PLACE = -1,
    /* 設置物 */
    BRIDGE,     // 橋
    CRATE,      // 木箱
    AMOUNT,     // 種類数
}

/// <summary>
/// csvデータからマップを生成、
/// マップ情報を管理するクラス
/// </summary>
public class MapManager : MonoBehaviour
{
    public const int CELL_SIZE = 2;
    // マップ情報を格納する配列
    List<List<CellBlockType>> mapData = new List<List<CellBlockType>>();

    [SerializeField] GameObject[] stagePrefabs; // 予め、ステージに配置しておくオブジェクト
    [SerializeField] GameObject[] playerPrefabs; // プレイヤーが配置できるオブジェクト（Playerクラスに移動するかも）

    /// <summary>
    /// 全てのマスのブロックの種類を得る関数
    /// </summary>
    /// <returns></returns>
    public ref readonly List<List<CellBlockType>> GetAllBlockType() { return ref mapData; }

    /// <summary>
    /// 指定したマスの、ブロックの種類を取得する関数
    /// </summary>
    /// <param name="_x">マスのX座標</param>
    /// <param name="_y">マスのY座標</param>
    /// <returns>設置するブロックの種類</returns>
    public CellBlockType GetBlockTypeOfCell(int _x, int _y)
    {
        // エラーチェック
        if (_y < 0 || _y >= mapData.Count || _x < 0 || _x >= mapData[_y].Count) return CellBlockType.NULL;
        return mapData[_y][_x];
    }

    public int GetMapWidth() { return mapData[0].Count; }
    public int GetMapHeight() { return mapData.Count; }

    /// <summary>
    /// ブロックを設置する際に呼ぶ関数
    /// </summary>
    /// <param name="_type">設置するブロックの種類</param>
    /// <returns>設置に成功したらtrue</returns>
    public bool SetBlockOfCell(PlaceBlockType _type, GameObject _block, int _cellX, int _cellY)
    {
        if (_type == PlaceBlockType.NO_PLACE) return false;
        
        bool canPlace = false;

        CellBlockType cellType = CellBlockType.NULL;
        switch (_type)
        {
            case PlaceBlockType.BRIDGE: // 橋ブロックは川ブロックの上にのみ置ける
                if (mapData[_cellY][_cellX] == CellBlockType.RIVER)
                {
                    cellType = CellBlockType.BRIDGE;
                    canPlace = true;
                }
                break;
            case PlaceBlockType.CRATE: // 木箱ブロックは通常のブロックの上にのみ置ける
                if (mapData[_cellY][_cellX] == CellBlockType.NORMAL)
                {
                    cellType = CellBlockType.CRATE;
                    canPlace = true;
                }
                break;
        }
        if (!canPlace) return false;

        if (canPlace)
        {
            mapData[_cellY][_cellX] = cellType;

            // 配置する位置にあるオブジェクトは削除し、新たなブロックを配置する
            Vector2 pos = new Vector2(_cellX * CELL_SIZE, _cellY * -CELL_SIZE);
            GameObject[] allObj = GameObject.FindGameObjectsWithTag("CanReplace");
            foreach (GameObject obj in allObj)
            {
                if ((Vector2)obj.transform.position == pos)
                {
                    Destroy(obj);
                    Instantiate(_block, pos, Quaternion.identity);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// キャラクターがいるマスを更新する関数(移動後に呼ぶ)
    /// </summary>
    /// <param name="_currCellX">現在マスのX座標</param>
    /// <param name="_currCellY">現在マスのY座標</param>
    /// <param name="_prevCellX">前回マスのX座標</param>
    /// <param name="_prevCellY">前回マスのY座標</param>
    public void UpdateCharaCell(int _currCellX, int _currCellY, int _prevCellX, int _prevCellY)
    {
        mapData[_prevCellY][_prevCellX] = CellBlockType.NORMAL;
        mapData[_currCellY][_currCellX] = CellBlockType.CHARA;
    }

    void Awake()
    {
        PlaceObjects();
    }

    void Update()
    {

    }

    /// <summary>
    /// csvファイルを読み込み、データを入れていく関数
    /// </summary>
    /// <param name="_fileName">ファイル名</param>
    void LoadCsvFile(string _fileName)
    {
        // ResourcesフォルダからCsvファイルを読み込む
        TextAsset csvFile = Resources.Load<TextAsset>(_fileName);
        if (csvFile == null)
        {
            Debug.LogError("Csvファイルが見つかりません：" + _fileName);
            return;
        }

        // 行ごとに配列に取り出していく
        // string.Trim()は、空白文字を削除するメソッド
        string[] lines = csvFile.text.Trim().Split('\n');
        
        // 行単位で取り出していく
        foreach(string line in lines)
        {
            // 「,」の位置で分割していく
            string[] values = line.Trim().Split(",");
            List<CellBlockType> row = new List<CellBlockType>();

            // セル単位で取り出していく
            foreach(string cell in values)
            {
                /* 型名.TryParse()：変換を試みるメソッドで、失敗したらfalseを返す
                 *      cell：変換元の文字列
                 *      out int num：変換結果を格納する変数（outは「出力先」）
                 */
                if (int.TryParse(cell, out int num))
                {
                    // 定義外の数値なら、NULLの値を入れる
                    if (Enum.IsDefined(typeof(CellBlockType), num))
                    {
                        row.Add((CellBlockType)num);
                    }
                    else
                    {
                        row.Add(CellBlockType.NULL);
                    }
                }
                else
                {
                    row.Add(CellBlockType.NULL);
                }
            }
            mapData.Add(row);
        }
    }

    void PlaceObjects()
    {
        LoadCsvFile("Stage");

        // 対応するブロックを生成していく
        for (int y = 0; y < mapData.Count; y++)
        {
            for (int x = 0; x < mapData[y].Count; x++)
            {
                CellBlockType type = mapData[y][x];
                int createIdx = -1;
                
                switch (type)
                {
                    case CellBlockType.CHARA:
                        createIdx = 0;
                        break;
                    case CellBlockType.GOAL:
                        createIdx = 1;
                        break;
                    case CellBlockType.NORMAL:
                        createIdx = 2;
                        break;
                    case CellBlockType.WALL:
                        createIdx = 3;
                        break;
                    case CellBlockType.RIVER:
                        createIdx = 4;
                        break;
                    case CellBlockType.ENEMY:
                        createIdx = 5;
                        break;
                }

                Vector2 pos = PosChanger.ChangeToGamePos(new Vector2Int(x, y));
                // キャラを配置する際は、先に地面を配置しておく
                if (type == CellBlockType.CHARA || type == CellBlockType.ENEMY)
                {
                    Instantiate(stagePrefabs[2], pos, Quaternion.identity);
                }
                Instantiate(stagePrefabs[createIdx], pos, Quaternion.identity);
            }
        }
    }
}
