using UnityEngine;

/// <summary>
/// 드래그 가능한 도형 오브젝트
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Shape : MonoBehaviour
{
    [SerializeField] private ShapeType shapeType;
    private Cell currentCell;
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;

    public ShapeType ShapeType => shapeType;
    public Cell CurrentCell => currentCell;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindObjectOfType<GridManager>();
    }

    /// <summary>
    /// 도형 타입을 설정합니다.
    /// </summary>
    public void SetShapeType(ShapeType type)
    {
        shapeType = type;
        // TODO: 스프라이트 변경 로직 추가 (나중에 구현)
    }

    /// <summary>
    /// 현재 셀을 설정합니다.
    /// </summary>
    public void SetCell(Cell cell)
    {
        currentCell = cell;
        if (cell != null)
        {
            // 셀의 월드 위치로 이동
            if (gridManager != null)
            {
                transform.position = gridManager.GetWorldPosition(cell.GetPosition());
            }
        }
    }

    /// <summary>
    /// 도형의 단계를 반환합니다.
    /// </summary>
    public int GetStage()
    {
        return shapeType.GetStage();
    }

    /// <summary>
    /// 이 도형이 다른 도형과 합성 가능한지 확인합니다.
    /// </summary>
    public bool CanMergeWith(Shape other)
    {
        if (other == null) return false;
        return shapeType.CanMerge(other.shapeType);
    }

    /// <summary>
    /// 도형의 색상을 설정합니다 (레어도 시스템용, 나중에 확장).
    /// </summary>
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
