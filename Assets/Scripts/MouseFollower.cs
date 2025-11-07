using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] CellBlockType type;

    Player player;

    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnMouseDrag()
    {
        Vector3 objPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }
}
