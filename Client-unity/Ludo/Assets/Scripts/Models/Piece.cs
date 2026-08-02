using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Piece : ActionSelectable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 0.3f;
    public int index;
    private Cell _currentCell;
    protected override void OnSelectableChanged(bool selectable)
    {
        // Highlight Piece
    }
    private async Task MoveToPosition(Vector3 targetPosition)
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
        foreach (Cell cell in path)
        {
            await MoveToPosition(cell.centerPosition);
            _currentCell = cell;
        }
    }
}