using System.Threading.Tasks;
using UnityEngine;

public class Cell : ActionSelectable
{
    public int index;
    public Vector3 centerPosition=new Vector3(0,0,0);

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