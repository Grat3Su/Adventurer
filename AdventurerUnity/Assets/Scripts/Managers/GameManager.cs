using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 게임 상태를 관리하는 매니저 클래스
/// </summary>
public enum GameState
{
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("게임 설정")]
    [SerializeField] private int initialScore = 0;
    [SerializeField] private float gameStartTime = 0f;

    [Header("판 확장 조건")]
    [SerializeField] private int scoreFor4x4 = 500;
    [SerializeField] private int scoreFor5x5 = 2000;
    [SerializeField] private float timeFor4x4 = 120f; // 2분
    [SerializeField] private float timeFor5x5 = 300f; // 5분

    private int currentScore;
    private float gameTime;
    private GameState gameState;
    private GridManager gridManager;
    private MergeSystem mergeSystem;
    private HashSet<ShapeType> unlockedShapeTypes = new HashSet<ShapeType>();
    private int maxShapeStage = 1;

    // 이벤트
    public event Action<int> OnScoreChanged;
    public event Action<float> OnTimeChanged;
    public event Action<GameState> OnGameStateChanged;
    public event Action OnBoardExpanded;

    public int CurrentScore => currentScore;
    public float GameTime => gameTime;
    public GameState CurrentGameState => gameState;
    public int MaxShapeStage => maxShapeStage;

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

    void Start()
    {
        gridManager = GridManager.Instance;
        mergeSystem = MergeSystem.Instance;
        InitializeGame();
    }

    void Update()
    {
        if (gameState == GameState.Playing)
        {
            gameTime += Time.deltaTime;
            OnTimeChanged?.Invoke(gameTime);

            // 맵 확장은 도형 타입 오픈 시점(RegisterShapeCreated)에서 처리
            CheckGameOver();
        }
    }

    /// <summary>
    /// 특정 도형 타입이 처음 생성될 때 호출되는 등록 함수
    /// </summary>
    public event Action<ShapeType> OnShapeTypeUnlocked;

    /// <summary>
    /// 게임을 초기화합니다.
    /// </summary>
    private void InitializeGame()
    {
        currentScore = initialScore;
        gameTime = gameStartTime;
        gameState = GameState.Playing;
        unlockedShapeTypes.Clear();
        maxShapeStage = 1;
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(gameTime);
        OnGameStateChanged?.Invoke(gameState);
    }

    /// <summary>
    /// 도형이 생성되었을 때 호출하여, 해당 타입이 처음 등장했는지 체크합니다.
    /// </summary>
    public void RegisterShapeCreated(ShapeType shapeType)
    {
        if (unlockedShapeTypes.Contains(shapeType))
        {
            return;
        }

        unlockedShapeTypes.Add(shapeType);
        maxShapeStage = Mathf.Max(maxShapeStage, shapeType.GetStage());
        OnShapeTypeUnlocked?.Invoke(shapeType);
        Debug.Log($"새 도형 타입 오픈: {shapeType}");

        // 새로운 타입이 열릴 때마다 판 확장 여부 체크
        CheckBoardExpansion();
    }

    /// <summary>
    /// 점수를 추가합니다.
    /// </summary>
    public void AddScore(int points)
    {
        if (gameState != GameState.Playing) return;

        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 현재까지 오픈된 도형 단계(maxShapeStage)에 따라 판 확장을 처리합니다.
    /// 네모(3단계) → 4x4, 오각형(4단계) → 5x5, 육각형(5단계) → 6x6.
    /// </summary>
    public void CheckBoardExpansion()
    {
        if (gridManager == null) return;

        int currentSize = gridManager.CurrentGridSize;
        int targetSize = currentSize;

        // 단계에 따른 목표 맵 크기 결정
        if (maxShapeStage >= 5)
        {
            targetSize = 6;
        }
        else if (maxShapeStage >= 4)
        {
            targetSize = 5;
        }
        else if (maxShapeStage >= 3)
        {
            targetSize = 4;
        }

        if (targetSize <= currentSize) return;

        // 목표 크기까지 한 칸씩 확장
        while (gridManager.CurrentGridSize < targetSize)
        {
            ExpandBoard();
        }
    }

    /// <summary>
    /// 판을 확장합니다.
    /// </summary>
    private void ExpandBoard()
    {
        if (gridManager == null) return;

        gridManager.ExpandBoard();
        OnBoardExpanded?.Invoke();
        Debug.Log($"판이 {gridManager.CurrentGridSize}x{gridManager.CurrentGridSize}로 확장되었습니다.");

        // 맵 크기에 따라 카메라 orthographic size 조절
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            //float halfBoardHeight = (gridManager.CurrentGridSize * gridManager.CellSize) * 0.5f;
            //float padding = 0.5f;
            //float minSize = 3f;
            //float targetSize = halfBoardHeight + padding;
            //cam.orthographicSize = Mathf.Max(minSize, targetSize);
            cam.orthographicSize = gridManager.CurrentGridSize+1;
        }
    }

    /// <summary>
    /// 게임오버 조건을 확인합니다.
    /// </summary>
    public void CheckGameOver()
    {
        if (gameState != GameState.Playing) return;
        if (gridManager == null) return;

        // 빈 칸이 있는지 확인
        var emptyCells = gridManager.GetEmptyCells();
        if (emptyCells.Count > 0)
        {
            return; // 빈 칸이 있으면 게임 계속
        }

        // 합성 가능한 도형이 있는지 확인
        if (!HasMergeableShapes())
        {
            OnGameOver();
        }
    }

    /// <summary>
    /// 합성 가능한 도형이 있는지 확인합니다.
    /// </summary>
    private bool HasMergeableShapes()
    {
        if (gridManager == null || mergeSystem == null) return false;

        var allCells = gridManager.GetAllCells();
        
        foreach (var cell1 in allCells.Values)
        {
            if (cell1.IsEmpty()) continue;

            Shape shape1 = cell1.CurrentShape;
            if (shape1 == null) continue;

            // 인접한 셀들을 확인
            var adjacentCells = gridManager.GetAdjacentCells(cell1.GetPosition());
            foreach (var cell2 in adjacentCells)
            {
                if (cell2.IsEmpty()) continue;

                Shape shape2 = cell2.CurrentShape;
                if (shape2 == null) continue;

                // 같은 타입인지 확인
                if (mergeSystem.CanMerge(shape1, shape2))
                {
                    return true; // 합성 가능한 도형이 있음
                }
            }
        }

        return false; // 합성 가능한 도형이 없음
    }

    /// <summary>
    /// 게임오버를 처리합니다.
    /// </summary>
    private void OnGameOver()
    {
        gameState = GameState.GameOver;
        OnGameStateChanged?.Invoke(gameState);
        Debug.Log($"게임 오버! 최종 점수: {currentScore}, 생존 시간: {gameTime:F2}초");
    }

    /// <summary>
    /// 게임을 재시작합니다.
    /// </summary>
    public void RestartGame()
    {
        // 모든 도형 제거
        if (gridManager != null)
        {
            var allCells = gridManager.GetAllCells();
            foreach (var cell in allCells.Values)
            {
                if (!cell.IsEmpty() && cell.CurrentShape != null)
                {
                    Destroy(cell.CurrentShape.gameObject);
                }
            }
        }

        // 그리드 재초기화
        if (gridManager != null)
        {
            gridManager.InitializeGrid(3);
        }

        InitializeGame();
    }
}
