using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public TileBase tileA;
    public TileBase tileB;
    public Vector2Int size;         //行列(x, y)

    void Start()
    {
        Vector3Int[] positions = new Vector3Int[size.x * size.y];
        TileBase[] tileArray = new TileBase[positions.Length];

        for (int index = 0; index < positions.Length; index++)
        {
            positions[index] = ToCustomPos(index / size.y, index % size.y);
            tileArray[index] = index % 2 == 0 ? tileA : tileB;
        }

        Tilemap tilemap = GetComponent<Tilemap>();
        tilemap.SetTiles(positions, tileArray);
    }

    //转换为自定义的排列格式
    private Vector3Int ToCustomPos(int x, int y)
    {
        int basex = -(x / 2);
        int basey = -(x + 1) / 2;
        int tox = basex + y;
        int toy = basey - y;
        return new Vector3Int(tox, toy, 0);
    }
}