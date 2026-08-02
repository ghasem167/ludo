using UnityEngine;

public abstract class ActionSelectable : MonoBehaviour
{
    private int _actionIndex;

    public bool IsSelectable { get; private set; }

    public void SetSelectable(int actionIndex)
    {
        _actionIndex = actionIndex;
        SetSelectable(true);
    }

    public virtual void SetSelectable(bool selectable)
    {
        IsSelectable = selectable;
        OnSelectableChanged(selectable);
    }

    public virtual void ClearSelectable()
    {
        SetSelectable(false);
    }

    public virtual void Select()
    {
        if (!IsSelectable)
            return;

       // GameNetworkService.Instance.SendActionSelected(_actionIndex);
    }

    protected abstract void OnSelectableChanged(bool selectable);
}