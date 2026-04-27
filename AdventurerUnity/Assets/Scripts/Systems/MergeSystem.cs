using UnityEngine;

/// <summary>
/// 도형 합성 로직을 처리하는 시스템
/// </summary>
public class MergeSystem : MonoBehaviour
{
    public static MergeSystem Instance;
    private GridManager gridManager;
    private GameManager gameManager;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        gameManager = FindObjectOfType<GameManager>();
        
    }

    /// <summary>
    /// 두 도형이 합성 가능한지 확인합니다.
    /// </summary>
    public bool CanMerge(Shape shape1, Shape shape2)
    {
        if (shape1 == null || shape2 == null) return false;
        return shape1.CanMergeWith(shape2);
    }

    /// <summary>
    /// 두 도형을 합성하여 다음 단계의 도형을 생성합니다.
    /// </summary>
    public Shape Merge(Shape shape1, Shape shape2)
    {
        if (!CanMerge(shape1, shape2))
        {
            Debug.LogWarning("합성할 수 없는 도형입니다.");
            return null;
        }

        ShapeType? nextType = shape1.ShapeType.GetNextShapeType();
        if (nextType == null)
        {
            // 육각형 2개를 합성하면 소멸
            DestroyShapes(shape1, shape2);
            if (gameManager != null)
            {
                // 육각형 합성 점수 계산 (기초 시스템에서는 단순화)
                int score = CalculateScore(shape1.ShapeType, 2); // 2배 점수
                gameManager.AddScore(score);
            }
            return null;
        }

        // 다음 단계 도형 생성
        Cell targetCell = shape1.CurrentCell ?? shape2.CurrentCell;
        if (targetCell == null)
        {
            Debug.LogError("도형의 셀 정보가 없습니다.");
            return null;
        }

        // 기존 도형 제거
        DestroyShapes(shape1, shape2);

        // 새 도형 생성
        Shape newShape = CreateShapeAtCell(targetCell, nextType.Value);
        
        // 점수 계산
        if (gameManager != null)
        {
            int score = CalculateScore(shape1.ShapeType, 1);
            gameManager.AddScore(score);
        }

        return newShape;
    }

    /// <summary>
    /// 특정 셀에 도형을 생성합니다.
    /// </summary>
    private Shape CreateShapeAtCell(Cell cell, ShapeType shapeType)
    {
        if (cell == null || gridManager == null) return null;

        // 도형 GameObject 생성 (나중에 프리팹 시스템으로 교체 가능)
        GameObject shapeObject = new GameObject($"Shape_{shapeType}");
        Shape shape = shapeObject.AddComponent<Shape>();
        shape.SetShapeType(shapeType);
        
        // SpriteRenderer 설정 (기본 스프라이트 사용)
        SpriteRenderer sr = shapeObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = shapeObject.AddComponent<SpriteRenderer>();
        }
        sr.sprite = CreateDefaultSprite(shapeType);
        sr.color = Color.white;

        // 셀에 배치
        cell.PlaceShape(shape);

        // 도형 타입 최초 오픈 로직
        if (gameManager != null)
        {
            gameManager.RegisterShapeCreated(shapeType);
        }

        return shape;
    }

    /// <summary>
    /// 기본 스프라이트를 생성합니다 (임시, 나중에 실제 스프라이트로 교체).
    /// </summary>
    private Sprite CreateDefaultSprite(ShapeType shapeType)
    {
        // Unity의 기본 스프라이트 생성 (나중에 실제 스프라이트 에셋으로 교체)
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Color shapeColor = GetShapeColor(shapeType);
        
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = shapeColor;
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 도형 타입에 따른 색상을 반환합니다 (임시).
    /// </summary>
    private Color GetShapeColor(ShapeType shapeType)
    {
        switch (shapeType)
        {
            case ShapeType.Circle:
                return Color.red;
            case ShapeType.Triangle:
                return Color.yellow;
            case ShapeType.Square:
                return Color.green;
            case ShapeType.Pentagon:
                return Color.blue;
            case ShapeType.Hexagon:
                return Color.magenta;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 두 도형을 제거합니다.
    /// </summary>
    private void DestroyShapes(Shape shape1, Shape shape2)
    {
        if (shape1 != null && shape1.CurrentCell != null)
        {
            shape1.CurrentCell.RemoveShape();
        }
        if (shape2 != null && shape2.CurrentCell != null)
        {
            shape2.CurrentCell.RemoveShape();
        }

        if (shape1 != null) Destroy(shape1.gameObject);
        if (shape2 != null) Destroy(shape2.gameObject);
    }

    /// <summary>
    /// 합성 점수를 계산합니다 (기초 시스템에서는 단순화).
    /// </summary>
    private int CalculateScore(ShapeType shapeType, int multiplier)
    {
        int baseScore = shapeType.GetStage() * 10;
        return baseScore * multiplier;
    }

    /// <summary>
    /// 다음 단계의 도형 타입을 반환합니다.
    /// </summary>
    public ShapeType? GetNextShapeType(ShapeType current)
    {
        return current.GetNextShapeType();
    }
}
