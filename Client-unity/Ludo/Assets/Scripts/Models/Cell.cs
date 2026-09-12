using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Cell : ActionSelectable
{
    [HideInInspector]

    public Vector3 Position;

    [HideInInspector]
    public int CellIndex;
    public SpecialCellVisual Visual;

    private void Start()
    {
        Position = transform.position;
    }
    public void SetVisual(SpecialCellVisual visual)
    {
        Visual = visual;
        Visual.transform.SetParent(transform, true);
        outline?.Initialize();
    }
    public async Task ActiveAsSafe()
    {
        Visual?.Highlight();

    }
    public async Task ActiveAsPenalty()
    {
        Visual?.Highlight();

    }
}