using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ActionSelectable : MonoBehaviour
{
    private int _actionIndex;

    public bool IsSelectable { get; private set; }

    public int ActionIndex => _actionIndex;

    private GamePlayEvents gamePlayEvents;

    private void Start()
    {
        if (GameManager.Instance.GamePlayHandler != null)
            gamePlayEvents = GameManager.Instance.GamePlayHandler.GamePlayEvents;
    }

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

        gamePlayEvents.RaiseActionSelected(_actionIndex);

        ClearSelectable();
    }

    protected abstract void OnSelectableChanged(bool selectable);
}