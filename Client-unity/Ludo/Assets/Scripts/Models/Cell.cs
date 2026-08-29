using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Cell : ActionSelectable
{
    [HideInInspector]

    public Vector3 Position;

    private void Start()
    {
        Position = transform.position;
    }   
    protected override void OnSelectableChanged(bool selectable)
    {
        // Highlight Cell
    }
    public async Task ActiveAsSafe()
    {

    }
    public async Task ActiveAsPenalty()
    {

    }
}