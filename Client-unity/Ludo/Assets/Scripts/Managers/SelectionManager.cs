using System.Collections.Generic;
using UnityEngine;

public class SelectionManager 
{
    private List<ActionSelectable> selectables = new();

    public void AddToList(ActionSelectable selectable)
    {
        if (!selectables.Contains(selectable))
            selectables.Add(selectable);
    }
    public void ClearAll()
    {
        foreach (var item in selectables)
            item.ClearSelectable();
    }
}