using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Piece : ActionSelectable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 0.3f;
    public int index;
    private GameObject _modelInstance;
    private Cell _currentCell;
    protected override void OnSelectableChanged(bool selectable)
    {
        // Highlight Piece
    }
    public async Task Spawn(Cell startCell)
    {

        await MoveToPosition(startCell.transform.position);
    }
    public async Task MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float traveled = 0f;

        while (traveled < distance)
        {
            traveled += moveSpeed * Time.deltaTime;

            float t = Mathf.Clamp01(traveled / distance);

            // حرکت افقی
            Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);

            // قوس حرکت (0 → 1 → 0)
            position.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = position;

            await Task.Yield();
        }

        transform.position = targetPosition;
    }
    public async Task MoveAlong(IReadOnlyList<Cell> path)
    {
        if (path == null || path.Count == 0)
            return;

        foreach (Cell cell in path)
        {
            await MoveToPosition(cell.transform.position);
            _currentCell = cell;
        }

        // اطمینان از قرار گرفتن دقیق روی آخرین سلول
        transform.position = path[^1].transform.position;
    }
    public void SetCurrentCell(Cell cell)
    {
        _currentCell = cell;
        gameObject.transform.position = cell.Position;
        Debug.Log("set current cell");
    }
    public void SetModel(GameObject modelPrefab)
    {
        if (modelPrefab == null)
        {
            Debug.LogError($"Model prefab is null for Piece {index}.");
            return;
        }

        // حذف مدل قبلی
        if (_modelInstance != null)
        {
            Destroy(_modelInstance);
        }

        // ساخت مدل جدید به عنوان فرزند Piece
        _modelInstance = Instantiate(
            modelPrefab,
            transform);

        // صفر کردن Transform نسبت به Piece
        _modelInstance.transform.localPosition = Vector3.zero;
        _modelInstance.transform.localRotation = Quaternion.identity;
        _modelInstance.transform.localScale = Vector3.one;
    }
    public void ApplyColor(PlayerColor playerColor)
    {
        //if (_modelInstance == null)
           // return;

        Color color = GetPlayerColor(playerColor);

        Renderer[] renderers =
            gameObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            foreach (Material material in materials)
            {
                if (material != null &&
                    material.name.StartsWith("PieceColor"))
                {
                    material.color = color;
                }
            }
        }
    }
    private Color GetPlayerColor(PlayerColor playerColor)
    {
        switch (playerColor)
        {
            case PlayerColor.Red:
                return Color.red;

            case PlayerColor.Green:
                return Color.green;

            case PlayerColor.Blue:
                return Color.blue;

            case PlayerColor.Yellow:
                return Color.yellow;

            default:
                return Color.white;
        }
    }

}