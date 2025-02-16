using UnityEngine;

public class SpawnHitBoxes : MonoBehaviour
{
    [SerializeField]
    GameObject emptyTile;
    void Start()
    {
        for(int i = 0; i < 8; i++) {
            for(int j = 0; j < 8; j++) {
                GameObject e = Instantiate(emptyTile, Board.IdxToPos(i,j), Quaternion.Euler(90, 0, 0));
                e.GetComponent<ChessPiece>().idx = new Vector2Int(i,j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
