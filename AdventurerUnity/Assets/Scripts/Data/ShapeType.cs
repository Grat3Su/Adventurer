using UnityEngine;

/// <summary>
/// 도형의 타입을 정의하는 열거형
/// </summary>
public enum ShapeType
{
    Circle = 1,      // 원, 1단계
    Triangle = 2,    // 삼각형, 2단계
    Square = 3,      // 사각형, 3단계
    Pentagon = 4,    // 오각형, 4단계
    Hexagon = 5      // 육각형, 5단계
}

/// <summary>
/// ShapeType 확장 메서드
/// </summary>
public static class ShapeTypeExtensions
{
    /// <summary>
    /// 현재 도형의 단계를 반환합니다.
    /// </summary>
    public static int GetStage(this ShapeType shapeType)
    {
        return (int)shapeType;
    }

    /// <summary>
    /// 다음 단계의 도형 타입을 반환합니다. 육각형의 경우 null을 반환합니다.
    /// </summary>
    public static ShapeType? GetNextShapeType(this ShapeType current)
    {
        int nextStage = (int)current + 1;
        if (nextStage > (int)ShapeType.Hexagon)
        {
            return null; // 육각형이 최고 단계
        }
        return (ShapeType)nextStage;
    }

    /// <summary>
    /// 두 도형이 합성 가능한지 확인합니다 (같은 타입인지).
    /// </summary>
    public static bool CanMerge(this ShapeType shape1, ShapeType shape2)
    {
        return shape1 == shape2;
    }
}
