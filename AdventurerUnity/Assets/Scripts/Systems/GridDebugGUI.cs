using UnityEngine;

/// <summary>
/// 현재 그리드에 맞춰 화면 위에 선을 그려주는 디버그용 GUI
/// </summary>
public class GridDebugGUI : MonoBehaviour
{
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private float lineThickness = 2f;

    private GridManager gridManager;
    private Camera mainCamera;
    private Texture2D lineTexture;

    void Start()
    {
        gridManager = GridManager.Instance;
        mainCamera = Camera.main;

        // 1x1 흰색 텍스처 생성
        lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, Color.white);
        lineTexture.Apply();
    }

    void OnGUI()
    {
        if (gridManager == null || mainCamera == null || lineTexture == null)
        {
            return;
        }

        int size = gridManager.CurrentGridSize;
        if (size <= 0) return;

        Vector3 origin = gridManager.GridOrigin;
        float cellSize = gridManager.CellSize;

        // 세로 선들 (x 방향 경계) : 왼쪽→오른쪽
        for (int x = 0; x <= size; x++)
        {
            float worldX = origin.x + x * cellSize;
            Vector3 worldStart = new Vector3(worldX, origin.y, 0f);
            Vector3 worldEnd = new Vector3(worldX, origin.y + size * cellSize, 0f);
            DrawWorldLine(worldStart, worldEnd, lineColor, gridManager.CurrentGridSize + 1);
        }

        // 가로 선들 (y 방향 경계) : 아래→위
        for (int y = 0; y <= size; y++)
        {
            float worldY = origin.y + y * cellSize;
            Vector3 worldStart = new Vector3(origin.x, worldY, 0f);
            Vector3 worldEnd = new Vector3(origin.x + size * cellSize, worldY, 0f);
            DrawWorldLine(worldStart, worldEnd, lineColor, gridManager.CurrentGridSize + 1);
        }
    }

    /// <summary>
    /// 월드 좌표 기준 두 점을 GUI 라인으로 그립니다.
    /// </summary>
    private void DrawWorldLine(Vector3 worldStart, Vector3 worldEnd, Color color, float thickness)
    {

        // 월드 좌표 → 스크린 좌표
        Vector3 screenStart = mainCamera.WorldToScreenPoint(worldStart);
        Vector3 screenEnd = mainCamera.WorldToScreenPoint(worldEnd);

        if (screenStart.z < 0f || screenEnd.z < 0f)
        {
            // 카메라 뒤에 있으면 그리지 않음
            return;
        }

        // GUI 좌표계(0,0 = 왼쪽 위)로 변환
        screenStart.y = Screen.height - screenStart.y;
        screenEnd.y = Screen.height - screenEnd.y;

        // 두 점을 포함하는 사각형 영역 계산
        float minX = Mathf.Min(screenStart.x, screenEnd.x);
        float maxX = Mathf.Max(screenStart.x, screenEnd.x);
        float minY = Mathf.Min(screenStart.y, screenEnd.y);
        float maxY = Mathf.Max(screenStart.y, screenEnd.y);

        Rect rect;
        if (Mathf.Abs(screenStart.x - screenEnd.x) < Mathf.Epsilon)
        {
            // 거의 수직선
            rect = new Rect(
                minX - thickness * 0.5f,
                minY,
                thickness,
                maxY - minY
            );
        }
        else
        {
            // 거의 수평선
            rect = new Rect(
                minX,
                minY - thickness * 0.5f,
                maxX - minX,
                thickness
            );
        }

        Color prevColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, lineTexture);
        GUI.color = prevColor;
    }
}

