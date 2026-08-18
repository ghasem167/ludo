using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Cell : ActionSelectable
{
    [HideInInspector]
    public Vector3 centerPosition=new Vector3(0,0,0);

    private void Start()
    {
        centerPosition=gameObject.transform.position;
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