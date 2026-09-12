using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ActionSelectable : MonoBehaviour, IPointerClickHandler
{

    private int _actionIndex;
    private bool _isSelectable;

    public bool IsSelectable { get; private set; }

    public int ActionIndex => _actionIndex;

    protected SelectableOutline outline;
    void Awake()
    {

        outline = GetComponent<SelectableOutline>();
        outline?.Initialize();
    }
    private void Start()
    {
    }

    public void Enable()
    {
        _isSelectable = true;
        outline?.SetOutline(true);
    }

    public void Disable()
    {
        _isSelectable = false;
        outline?.SetOutline(false);
    }
    public void SetSelectable(int actionIndex)
    {
        _actionIndex = actionIndex;
        Enable();

    }
    public void ClearSelectable()
    {
        _actionIndex = -1;
        Disable();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isSelectable)
            return;
        Disable();

        GameManager.Instance.GamePlayHandler.GamePlayEvents.RaiseActionSelected(_actionIndex);

    }

}