using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 빈 칸에 도형을 소환하는 시스템
/// </summary>
public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem Instance;
    [Header("소환 설정")]
    [SerializeField] private float spawnIntervalEarly = 1f;  // 초반 (0-2분): 1초
    [SerializeField] private float spawnIntervalMid = 0.5f;    // 중반 (2-5분): 0.5초
    [SerializeField] private float spawnIntervalLate = 0.1f;   // 후반 (5분+): 0.1초

    private GridManager gridManager;
    private GameManager gameManager;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[SpawnSystem] Instance 생성");
        }
        else
        {
            Debug.LogWarning("[SpawnSystem] 중복 인스턴스 감지, 제거");
            Destroy(gameObject);
            return;
        }

    }

    void Start()
    {
        gridManager = GridManager.Instance;
        gameManager = GameManager.Instance;

        if (gridManager == null)
        {
            Debug.LogWarning("[SpawnSystem] GridManager.Instance 가 null 입니다");
        }
        if (gameManager == null)
        {
            Debug.LogWarning("[SpawnSystem] GameManager.Instance 가 null 입니다");
        }
        Debug.Log("[SpawnSystem] Start 호출, 스폰 시작");
        StartSpawning();
    }

    void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }

    void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    /// <summary>
    /// 소환을 시작합니다.
    /// </summary>
    public void StartSpawning()
    {
        Debug.Log("[SpawnSystem] StartSpawning 호출");
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    /// <summary>
    /// 소환을 중지합니다.
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 주기적으로 도형을 소환하는 코루틴
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            if (gameManager != null && gameManager.CurrentGameState == GameState.Playing)
            {
                Debug.Log("[SpawnSystem] SpawnCoroutine tick, SpawnShape 시도");
                SpawnShape();
            }

            float interval = GetSpawnInterval();
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// 현재 맵 크기(그리드 사이즈)에 따른 소환 간격을 반환합니다.
    /// </summary>
    private float GetSpawnInterval()
    {
        if (gridManager == null) return spawnIntervalEarly;

        int size = gridManager.CurrentGridSize;

        // 3x3: 느리게, 4x4: 중간, 5x5 이상: 빠르게
        if (size <= 3)
        {
            return spawnIntervalEarly;
        }
        else if (size == 4)
        {
            return spawnIntervalMid;
        }
        else
        {
            return spawnIntervalLate;
        }
    }

    /// <summary>
    /// 빈 칸에 도형을 소환합니다.
    /// </summary>
    public void SpawnShape()
    {
        if (gridManager == null)
        {
            Debug.LogWarning("[SpawnSystem] gridManager 가 null 이어서 스폰 중단");
            return;
        }
        if (gameManager == null)
        {
            Debug.LogWarning("[SpawnSystem] gameManager 가 null 이어서 스폰 중단");
            return;
        }
        if (gameManager.CurrentGameState != GameState.Playing)
        {
            Debug.Log($"[SpawnSystem] GameState 가 {gameManager.CurrentGameState} 여서 스폰 생략");
            return;
        }

        Debug.Log("[SpawnSystem] SpawnShape 진입");

        // 빈 칸 찾기
        List<Cell> emptyCells = gridManager.GetEmptyCells();
        if (emptyCells.Count == 0)
        {
            Debug.Log("[SpawnSystem] 빈 칸이 없어 스폰 불가");
            return; // 빈 칸이 없으면 소환하지 않음
        }

        // 랜덤 빈 칸 선택
        Cell targetCell = emptyCells[Random.Range(0, emptyCells.Count)];

        // 소환할 도형 타입 결정
        ShapeType shapeType = GetRandomShapeType();

        // 도형 생성
        CreateShapeAtCell(targetCell, shapeType);
    }

    /// <summary>
    /// 현재까지 오픈된 최고 단계보다 낮은 도형 타입을 랜덤으로 반환합니다.
    /// </summary>
    private ShapeType GetRandomShapeType()
    {
        if (gameManager == null) return ShapeType.Circle;

        int maxStage = gameManager.MaxShapeStage;

        // 아직 아무 타입도 오픈 안 되었으면 원만 소환
        if (maxStage <= 1)
        {
            return ShapeType.Circle;
        }

        // 최소 1단계(원)부터, 최대 (maxStage - 1)단계까지 랜덤 선택
        int randomStage = Random.Range(1, maxStage);
        return (ShapeType)randomStage;
    }

    /// <summary>
    /// 특정 셀에 도형을 생성합니다.
    /// </summary>
    private void CreateShapeAtCell(Cell cell, ShapeType shapeType)
    {
        if (cell == null || gridManager == null) return;

        // 도형 GameObject 생성
        GameObject shapeObject = new GameObject($"Shape_{shapeType}");
        Shape shape = shapeObject.AddComponent<Shape>();
        shape.SetShapeType(shapeType);

        // SpriteRenderer 설정
        SpriteRenderer sr = shapeObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = shapeObject.AddComponent<SpriteRenderer>();
        }
        sr.sprite = CreateDefaultSprite(shapeType);
        sr.color = Color.white;
        sr.sortingOrder = 1;

        // 셀에 배치
        cell.PlaceShape(shape);
        shape.SetCell(cell);
    }

    /// <summary>
    /// 기본 스프라이트를 생성합니다 (임시, 나중에 실제 스프라이트로 교체).
    /// </summary>
    private Sprite CreateDefaultSprite(ShapeType shapeType)
    {
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
    /// 게임 상태 변경 시 호출됩니다.
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            StartSpawning();
        }
        else if (newState == GameState.GameOver)
        {
            StopSpawning();
        }
    }
}
