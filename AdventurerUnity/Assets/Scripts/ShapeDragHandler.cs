using UnityEngine;

/// <summary>
/// 도형의 드래그 앤 드롭을 처리하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Shape))]
public class ShapeDragHandler : MonoBehaviour
{
    private Shape shape;
    private GridManager gridManager;
    private MergeSystem mergeSystem;
    private Camera mainCamera;
    private Vector3 offset;
    private Cell originalCell;
    private bool isDragging = false;
    private float dragZOffset = -1f; // 드래그 중일 때 Z 오프셋

    void Awake()
    {
        shape = GetComponent<Shape>();
        gridManager = FindObjectOfType<GridManager>();
        mergeSystem = FindObjectOfType<MergeSystem>();
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        if (shape == null || gridManager == null) return;

        originalCell = shape.CurrentCell;
        isDragging = true;

        // 마우스 위치와 오브젝트 위치의 오프셋 계산
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        offset = transform.position - mouseWorldPos;

        // 드래그 중일 때 Z 오프셋 적용
        Vector3 pos = transform.position;
        pos.z = dragZOffset;
        transform.position = pos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        // 마우스 위치로 오브젝트 이동
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = dragZOffset;
        transform.position = mouseWorldPos + offset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (gridManager == null || shape == null) return;

        // 마우스 위치를 그리드 좌표로 변환
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2Int gridPos = gridManager.WorldToGridPosition(mouseWorldPos);

        // 유효한 위치인지 확인
        if (!gridManager.IsValidPosition(gridPos))
        {
            // 원래 위치로 복귀
            ReturnToOriginalPosition();
            return;
        }

        Cell targetCell = gridManager.GetCell(gridPos);

        if (targetCell == null)
        {
            ReturnToOriginalPosition();
            return;
        }

        // 같은 셀에 드롭한 경우
        if (targetCell == originalCell)
        {
            ReturnToOriginalPosition();
            return;
        }

        // 빈 칸에 드롭한 경우
        if (targetCell.IsEmpty())
        {
            MoveToCell(targetCell);
            return;
        }

        // 다른 도형이 있는 칸에 드롭한 경우 - 합성 시도
        Shape targetShape = targetCell.CurrentShape;
        if (targetShape != null && mergeSystem != null)
        {
            if (mergeSystem.CanMerge(shape, targetShape))
            {
                // 합성 실행
                mergeSystem.Merge(shape, targetShape);
                return;
            }
        }

        // 합성 불가능한 경우 원래 위치로 복귀
        ReturnToOriginalPosition();
    }

    /// <summary>
    /// 도형을 원래 위치로 되돌립니다.
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        if (originalCell != null && gridManager != null)
        {
            Vector3 worldPos = gridManager.GetWorldPosition(originalCell.GetPosition());
            worldPos.z = 0;
            transform.position = worldPos;
        }
    }

    /// <summary>
    /// 도형을 특정 셀로 이동시킵니다.
    /// </summary>
    private void MoveToCell(Cell targetCell)
    {
        if (targetCell == null || gridManager == null) return;

        // 원래 셀에서 제거
        if (originalCell != null)
        {
            originalCell.RemoveShape();
        }

        // 새 셀에 배치
        targetCell.PlaceShape(shape);
        shape.SetCell(targetCell);

        // 위치 업데이트
        Vector3 worldPos = gridManager.GetWorldPosition(targetCell.GetPosition());
        worldPos.z = 0;
        transform.position = worldPos;
    }
}
