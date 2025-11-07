using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゴールを目指すキャラクターを定義したクラス
/// </summary>
public class Character : MonoBehaviour
{
    /// <summary>
    /// 経路探索に使用
    /// </summary>
    struct RouteNode
    {
        public int x, y;    // マップ上の座標
        public int actCost; // 開始マスから現在マスまでの実コスト
        public int estCost; // 現在マスからゴールまでの推定コスト
    }

    Vector2Int cellPos = Vector2Int.zero; // 現在地点のマス座標
    int goalX = 0, goalY = 0; // ゴール地点のマス座標
    const float ARRIVAL_TIME = 1.3f; // 次のマスへの到達時間
    List<Vector2Int> routes = new List<Vector2Int>(); // ゴールに向かう経路
    float moveRate = 0;

    GameManager gameManager;
    MapManager mapManager;
    Player player; // プレイヤーが設置物を置いているかどうかを取得するために使用
    PathVisualizer pathVisualizer; // 経路を描画するために使用

    public void SetStartPos(Vector2Int _pos) {  cellPos = _pos; }

    void Awake()
    {
        cellPos = PosChanger.ChangeToCellPos(transform.position);
        Vector2 pos = transform.position;
        // ゲーム内座標からcsv座標へ変換
        cellPos = new Vector2Int((int)pos.x / MapManager.CELL_SIZE, (int)pos.y / -MapManager.CELL_SIZE);
        
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
        pathVisualizer = GameObject.Find("Path").GetComponent<PathVisualizer>();

        /* ゴール地点を見つけておく（他の処理との兼ね合いのため、ここで行う） */
        bool goalFinded = false;
        for(int y = 0; y < mapManager.GetAllBlockType().Count; y++)
        {
            if (goalFinded) break;

            for(int x = 0; x < mapManager.GetAllBlockType()[y].Count; x++)
            {
                if (mapManager.GetBlockTypeOfCell(x, y) == CellBlockType.GOAL)
                {
                    goalX = x;
                    goalY = y;
                    goalFinded = true;
                    break;
                }
            }
        }

        ShortestPathSearch(cellPos.x, cellPos.y, ref routes);
    }

    void Update()
    {
        if (gameManager.GetIsGameClear() || gameManager.GetIsGameOver())
        {
            pathVisualizer.ResetLine();
            return;
        }

            // ブロック設置中は動かない
        if (player.GetIsBlockPlaced())
        {
            ShortestPathSearch(cellPos.x, cellPos.y, ref routes);
            player.ResetBlockPlaced();
            // ゴールまでの経路が無ければ、ゲームオーバーとする
            if (routes.Count == 0)
            {
                gameManager.GameOver();
                return;
            }
        }
        MoveToNextCell();
        pathVisualizer.DrawPath(routes, transform.position);
    }

    /// <summary>
    /// 次のセルまで移動する関数
    /// </summary>
    void MoveToNextCell()
    {
        // 現在いるマスから次のマスに向かう
        Vector2Int next = routes[1];
        Vector2 pos = Vector2.Lerp(cellPos, next, moveRate);
        transform.position = PosChanger.ChangeToGamePos(pos);

        moveRate += Time.deltaTime / ARRIVAL_TIME;
        if (moveRate >= 1)
        {
            moveRate = 0;
            CollideBlocks(next.x, next.y); // ここで障害物判定を取る
            mapManager.UpdateCharaCell(next.x, next.y, cellPos.x, cellPos.y);
            routes.RemoveAt(0); // 移動し終わったら、先頭の要素を削除
            cellPos = routes[0];
        }
    }

    /// <summary>
    /// 最短経路探索を行う関数
    /// </summary>
    /// <param name="_startX">探索開始マスのX座標</param>
    /// <param name="_startY">探索開始マスのY座標</param>
    /// <param name="_route">経路を格納する配列</param>
    void ShortestPathSearch(int _startX, int _startY, ref List<Vector2Int> _route)
    {
        _route.Clear();
        int mapWidth = mapManager.GetMapWidth();
        int mapHeight = mapManager.GetMapHeight();

        // オープンリスト
        PriorityQueue<RouteNode> open = new PriorityQueue<RouteNode>();
        // クローズドリスト
        bool[,] closed = new bool[mapHeight, mapWidth];
        // 親ノードのリスト
        Vector2Int[,] parents = new Vector2Int[mapHeight, mapWidth];
        // 「現在マスまでの実コスト」を比較するための配列
        int[,] actCosts = new int[mapHeight, mapWidth];

        // 各リストの初期化
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                closed[y, x] = false;
                parents[y, x] = new Vector2Int(-1, -1);
                actCosts[y, x] = 99;
            }
        }

        RouteNode start = new RouteNode();
        start.x = _startX;
        start.y = _startY;
        start.actCost = 0;
        start.estCost = CalcEstDis(_startX, _startY);
        // 開始地点の要素を入れる
        open.Enqueue(start, start.estCost);

        while(open.Count > 0)
        {
            RouteNode curr = open.Dequeue();

            // ゴールに到達した
            if (curr.x == goalX && curr.y == goalY)
            {
                // 経路復元
                int currX = goalX;
                int currY = goalY;

                // 開始地点に戻るまで繰り返す
                while (!(currX == _startX && currY == _startY))
                {
                    _route.Add(new Vector2Int(currX, currY));

                    // 現在のノードの親ノードを辿る
                    var p = parents[currY, currX];
                    currX = p.x;
                    currY = p.y;
                }

                // 最後に開始マスを追加し、逆順に並べる
                _route.Add(new Vector2Int(_startX, _startY));
                _route.Reverse();
                return;
            }

            // 既に探索済みならスキップ
            if (closed[curr.y, curr.x]) continue;
            closed[curr.y, curr.x] = true;

            // 隣接マス4方向を表す
            Vector2Int[] dirVec =
            {
                new Vector2Int( 1,  0),
                new Vector2Int(-1,  0),
                new Vector2Int( 0, -1),
                new Vector2Int( 0,  1)
            };

            // 現在マスから4方向を探索
            for (int dir = 0; dir < dirVec.Length; dir++)
            {
                int nextX = curr.x + dirVec[dir].x;
                int nextY = curr.y + dirVec[dir].y;

                // 開始マスは計算しない
                if (nextX == _startX && nextY == _startY) continue;

                // マップ外判定
                if (nextX < 0 || nextX > mapWidth -1 || nextY < 0 || nextY > mapHeight - 1)
                    continue;

                // 移動できるかどうか
                CellBlockType type = mapManager.GetBlockTypeOfCell(nextX, nextY);
                if (type == CellBlockType.NULL || type == CellBlockType.WALL || type == CellBlockType.CRATE) continue;

                // 探索済みかどうか
                if (closed[nextY, nextX]) continue;

                int actC = curr.actCost + 1; // 次のマスの実コスト

                // 今回探索した実コストが、前回探索時よりも小さい時のみ更新する
                if (actC >= actCosts[nextY, nextX]) continue;
                actCosts[nextY, nextX] = actC;

                // 現在マスから目的マスまでの推定距離を計算し、そこから推定コストを算出する
                int nToGDis = CalcEstDis(nextX, nextY);
                int nToGCost = actC + nToGDis;

                // 次に探索する「候補となるノード」を設定
                Vector2Int point = new Vector2Int(nextX, nextY);
                RouteNode nextNode = new RouteNode();
                nextNode.x = nextX;
                nextNode.y = nextY;
                nextNode.actCost = actC;
                nextNode.estCost = nToGCost;
                open.Enqueue(nextNode, nextNode.estCost);
                
                // 現在ノード座標を、探索候補ノードの親ノードとして記録
                parents[nextY, nextX] = new Vector2Int(curr.x, curr.y);
            }
        }
    }

    /// <summary>
    /// 現在マスからゴールまでの推定距離を計算する関数
    /// </summary>
    /// <param name="_currX">現在マスのX座標</param>
    /// <param name="_currY">現在マスのY座標</param>
    /// <returns>推定距離</returns>
    int CalcEstDis(int _currX, int _currY)
    {
        // 距離は絶対値で算出
        int costX = _currX - goalX;
        int costY = _currY - goalY;
        if (costX < 0) costX *= -1;
        if (costY < 0) costY *= -1;

        return costX + costY;
    }

    /// <summary>
    /// 障害物と衝突した際の処理を行う関数
    /// </summary>
    void CollideBlocks(int _cellPosX, int _cellPosY)
    {
        // 障害物となるのは敵か川ブロック
        if (mapManager.GetBlockTypeOfCell(_cellPosX, _cellPosY) == CellBlockType.ENEMY
            || mapManager.GetBlockTypeOfCell(_cellPosX, _cellPosY) == CellBlockType.RIVER)
        {
            gameManager.GameOver();
            return;
        }

        // ゴール
        if (mapManager.GetBlockTypeOfCell(_cellPosX, _cellPosY) == CellBlockType.GOAL)
        {
            gameManager.GameClear();
        }
    }
}
