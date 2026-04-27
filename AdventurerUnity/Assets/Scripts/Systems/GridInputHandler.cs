using UnityEngine;

/// <summary>
/// 그리드 기준으로 드래그 인풋을 처리하는 핸들러
/// </summary>
public class GridInputHandler : MonoBehaviour
{
    [SerializeField] private float dragZOffset = -1f;

    private GridManager gridManager;
    private MergeSystem mergeSystem;
    private Camera mainCamera;

    private Shape draggingShape;
    private Cell originalCell;
    private Vector3 offset;

    void Start()
    {
        gridManager = GridManager.Instance;
        mergeSystem = MergeSystem.Instance;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryBeginDrag();
        }
        else if (Input.GetMouseButton(0) && draggingShape != null)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && draggingShape != null)
        {
            EndDrag();
        }
    }

    private void TryBeginDrag()
    {
        if (gridManager == null || mainCamera == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int gridPos = gridManager.WorldToGridPosition(mouseWorld);

        if (!gridManager.IsValidPosition(gridPos))
        {
            return;
        }

        Cell cell = gridManager.GetCell(gridPos);
        if (cell == null || cell.IsEmpty())
        {
        return;
        }

        draggingShape = cell.CurrentShape;
        originalCell = cell;

        Vector3 shapePos = draggingShape.transform.position;
        offset = shapePos - mouseWorld;

        shapePos.z = dragZOffset;
        draggingShape.transform.position = shapePos;
    }

    private void UpdateDrag()
    {
        if (draggingShape == null || mainCamera == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = dragZOffset;
        draggingShape.transform.position = mouseWorld + offset;
    }

    private void EndDrag()
    {
        if (draggingShape == null || gridManager == null || mainCamera == null)
        {
            ResetDrag();
            return;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int gridPos = gridManager.WorldToGridPosition(mouseWorld);

        if (!gridManager.IsValidPosition(gridPos))
        {
            ReturnToOriginal();
            ResetDrag();
            return;
        }

        Cell targetCell = gridManager.GetCell(gridPos);
        if (targetCell == null)
        {
            ReturnToOriginal();
            ResetDrag();
            return;
        }

        if (targetCell == originalCell)
        {
            ReturnToOriginal();
            ResetDrag();
            return;
        }

        if (targetCell.IsEmpty())
        {
            MoveToCell(targetCell);
            ResetDrag();
            return;
        }

        Shape targetShape = targetCell.CurrentShape;
        if (targetShape != null && mergeSystem != null && mergeSystem.CanMerge(draggingShape, targetShape))
        {
            mergeSystem.Merge(draggingShape, targetShape);
        }
        else
        {
            ReturnToOriginal();
        }

        ResetDrag();
    }

    private void ReturnToOriginal()
    {
        if (originalCell == null || gridManager == null || draggingShape == null) return;

        Vector3 worldPos = gridManager.GetWorldPosition(originalCell.GetPosition());
        worldPos.z = 0f;
        draggingShape.transform.position = worldPos;
    }

    private void MoveToCell(Cell targetCell)
    {
        if (targetCell == null || gridManager == null || draggingShape == null) return;

        if (originalCell != null)
        {
            originalCell.RemoveShape();
        }

        targetCell.PlaceShape(draggingShape);
        draggingShape.SetCell(targetCell);

        Vector3 worldPos = gridManager.GetWorldPosition(targetCell.GetPosition());
        worldPos.z = 0f;
        draggingShape.transform.position = worldPos;
    }

    private void ResetDrag()
    {
        draggingShape = null;
        originalCell = null;
        offset = Vector3.zero;
    }
}

