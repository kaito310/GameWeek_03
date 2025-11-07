using UnityEngine;
using System.Collections.Generic;

public class PathVisualizer : MonoBehaviour
{
    LineRenderer line;

    void Awake()
    {
        line= GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Œ»İƒ}ƒX‚©‚çƒS[ƒ‹‚Ü‚Å‚ÌŒo˜H‚ğ•`‰æ‚·‚éŠÖ”
    /// </summary>
    /// <param name="_path">Œo˜H</param>
    public void DrawPath(List<Vector2Int> _path, Vector2 _pos)
    {
        if (_path.Count == 0) return;

        // Œo˜H‚Ì”‚¾‚¯“_‚ğw’è‚·‚é
        line.positionCount = _path.Count;
        for (int i = 0; i < _path.Count; i++)
        {
            if (i == 0)
            {
                line.SetPosition(i, _pos);
                continue;
            }
            
            line.SetPosition(i, PosChanger.ChangeToGamePos(new Vector2Int(_path[i].x, _path[i].y)));
        }
    }

    /// <summary>
    /// •`‰æ‚ğ~‚ß‚½‚¢‚ÉŒÄ‚Ô
    /// </summary>
    public void ResetLine()
    {
        line.positionCount = 0;
    }
}
