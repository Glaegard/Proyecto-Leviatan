using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Dimensiones del Tablero")]
    public int rows = 9;    // 9 filas
    public int columns = 3; // 3 columnas
    public float spacing = 2f;

    [Header("Prefab y Cámara")]
    public GameObject cellPrefab;
    public Camera boardCamera;

    private GameObject[,] boardCells;
    public List<GameObject> freeCells = new List<GameObject>();

    private void Start()
    {
        if (boardCamera == null)
            boardCamera = Camera.main;
        GenerateBoard();
    }

    private Vector3 GetBoardCenter()
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray centerRay = boardCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (groundPlane.Raycast(centerRay, out float rayDistance))
            return centerRay.GetPoint(rayDistance);
        return Vector3.zero;
    }

    private void GenerateBoard()
    {
        Vector3 center = GetBoardCenter();
        float width = (columns - 1) * spacing;
        float height = (rows - 1) * spacing;
        Vector3 origin = center - new Vector3(width / 2f, 0f, height / 2f);

        boardCells = new GameObject[rows, columns];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 cellPos = origin + new Vector3(c * spacing, 0f, r * spacing);
                GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
                cell.name = $"Cell_{r}_{c}";

                BoardCell bc = cell.GetComponent<BoardCell>();
                if (bc == null)
                    bc = cell.AddComponent<BoardCell>();

                bc.row = r;
                bc.column = c;

                boardCells[r, c] = cell;
                freeCells.Add(cell);
            }
        }
    }

    public GameObject GetCell(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < columns)
            return boardCells[row, col];
        return null;
    }

    public void OccupyCell(int row, int col)
    {
        GameObject cell = GetCell(row, col);
        if (cell != null && freeCells.Contains(cell))
            freeCells.Remove(cell);
    }

    public void FreeCell(int row, int col)
    {
        GameObject cell = GetCell(row, col);
        if (cell != null && !freeCells.Contains(cell))
            freeCells.Add(cell);
    }
}
