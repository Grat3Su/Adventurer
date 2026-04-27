using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 보드 그리드를 관리하는 클래스
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("그리드 설정")]
    [SerializeField] private int gridSize = 3;
    [SerializeField] private float cellSize = 1f;
    [Tooltip("true면 (0,0)을 기준으로 위/아래/좌/우 대칭으로 확장됩니다.")]
    [SerializeField] private bool centerOnZero = true;

    private Vector3 gridOrigin = Vector3.zero;
    private Dictionary<Vector2Int, Cell> cells;
    private int currentGridSize;

    public GameObject Spawner;
    public int CurrentGridSize => currentGridSize;
    public float CellSize => cellSize;
    public Vector3 GridOrigin => gridOrigin;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        cells = new Dictionary<Vector2Int, Cell>();
        currentGridSize = gridSize;
        InitializeGrid(currentGridSize);
    }

    /// <summary>
    /// 그리드를 초기화합니다.
    /// </summary>
    public void InitializeGrid(int size)
    {
        cells.Clear();
        currentGridSize = size;

        // (0,0) 기준으로 위/아래/좌/우 대칭 확장
        if (centerOnZero)
        {
            float half = (currentGridSize - 1) * 0.5f;
            gridOrigin = new Vector3(-half * cellSize, -half * cellSize, 0f);
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                cells[position] = new Cell(x, y);
            }
        }
    }

    /// <summary>
    /// 특정 위치의 셀을 가져옵니다.
    /// </summary>
    public Cell GetCell(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (cells.TryGetValue(position, out Cell cell))
        {
            return cell;
        }
        return null;
    }

    /// <summary>
    /// 특정 위치의 셀을 가져옵니다 (Vector2Int 사용).
    /// </summary>
    public Cell GetCell(Vector2Int position)
    {
        return GetCell(position.x, position.y);
    }

    /// <summary>
    /// 위치가 유효한 범위 내에 있는지 확인합니다.
    /// </summary>
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < currentGridSize && y >= 0 && y < currentGridSize;
    }

    /// <summary>
    /// 위치가 유효한 범위 내에 있는지 확인합니다 (Vector2Int 사용).
    /// </summary>
    public bool IsValidPosition(Vector2Int position)
    {
        return IsValidPosition(position.x, position.y);
    }

    /// <summary>
    /// 빈 칸들의 리스트를 반환합니다.
    /// </summary>
    public List<Cell> GetEmptyCells()
    {
        List<Cell> emptyCells = new List<Cell>();
        foreach (var cell in cells.Values)
        {
            if (cell.IsEmpty())
            {
                emptyCells.Add(cell);
            }
        }
        return emptyCells;
    }

    /// <summary>
    /// 특정 위치의 인접한 셀들을 반환합니다 (상하좌우).
    /// </summary>
    public List<Cell> GetAdjacentCells(int x, int y)
    {
        List<Cell> adjacentCells = new List<Cell>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // 위
            new Vector2Int(0, -1), // 아래
            new Vector2Int(1, 0),  // 오른쪽
            new Vector2Int(-1, 0)  // 왼쪽
        };

        foreach (var direction in directions)
        {
            Vector2Int adjacentPos = new Vector2Int(x, y) + direction;
            if (IsValidPosition(adjacentPos))
            {
                Cell cell = GetCell(adjacentPos);
                if (cell != null)
                {
                    adjacentCells.Add(cell);
                }
            }
        }

        return adjacentCells;
    }

    /// <summary>
    /// 특정 위치의 인접한 셀들을 반환합니다 (Vector2Int 사용).
    /// </summary>
    public List<Cell> GetAdjacentCells(Vector2Int position)
    {
        return GetAdjacentCells(position.x, position.y);
    }

    /// <summary>
    /// 셀의 월드 위치(셀 중앙)를 반환합니다.
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        // gridOrigin 은 보드의 왼쪽-아래 모서리이므로,
        // 각 셀의 중앙은 (x + 0.5, y + 0.5) * cellSize 지점입니다.
        float worldX = gridOrigin.x + (x + 0.5f) * cellSize;
        float worldY = gridOrigin.y + (y + 0.5f) * cellSize;
        return new Vector3(worldX, worldY, 0f);
    }

    /// <summary>
    /// 셀의 월드 위치를 반환합니다 (Vector2Int 사용).
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int position)
    {
        return GetWorldPosition(position.x, position.y);
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 판을 확장합니다. 기존 배치는 유지하고 가장자리에 빈 칸을 추가합니다.
    /// 최대 6x6까지 확장 가능합니다.
    /// </summary>
    public void ExpandBoard()
    {
        if (currentGridSize >= 6)
        {
            Debug.LogWarning("판 크기는 최대 6x6입니다.");
            return;
        }

        int newSize = currentGridSize + 1;
        Dictionary<Vector2Int, Cell> oldCells = new Dictionary<Vector2Int, Cell>(cells);

        // 새로운 크기로 그리드 초기화
        InitializeGrid(newSize);

        // 기존 셀들의 도형을 새 그리드로 복사
        foreach (var kvp in oldCells)
        {
            if (!kvp.Value.IsEmpty())
            {
                Cell newCell = GetCell(kvp.Key);
                if (newCell != null)
                {
                    newCell.PlaceShape(kvp.Value.CurrentShape);
                }
            }
        }
    }

    /// <summary>
    /// 모든 셀을 반환합니다.
    /// </summary>
    public Dictionary<Vector2Int, Cell> GetAllCells()
    {
        return new Dictionary<Vector2Int, Cell>(cells);
    }
}
