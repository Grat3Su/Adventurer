using UnityEngine;

/// <summary>
/// 그리드의 각 칸을 나타내는 클래스
/// </summary>
public class Cell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Shape CurrentShape { get; set; }

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
        CurrentShape = null;
    }

    /// <summary>
    /// 이 칸이 비어있는지 확인합니다.
    /// </summary>
    public bool IsEmpty()
    {
        return CurrentShape == null;
    }

    /// <summary>
    /// 이 칸에 도형을 배치합니다.
    /// </summary>
    public void PlaceShape(Shape shape)
    {
        CurrentShape = shape;
        if (shape != null)
        {
            shape.SetCell(this);
        }
    }

    /// <summary>
    /// 이 칸에서 도형을 제거합니다.
    /// </summary>
    public void RemoveShape()
    {
        if (CurrentShape != null)
        {
            CurrentShape.SetCell(null);
        }
        CurrentShape = null;
    }

    /// <summary>
    /// 셀의 위치를 Vector2Int로 반환합니다.
    /// </summary>
    public Vector2Int GetPosition()
    {
        return new Vector2Int(X, Y);
    }
}
