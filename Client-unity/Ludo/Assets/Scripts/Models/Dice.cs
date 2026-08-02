using UnityEngine;

public class Dice : MonoBehaviour
{
    public bool IsSelectable { get; private set; }

    public void SetSelectable(bool selectable)
    {
        IsSelectable = selectable;
        OnSelectableChanged(selectable);
    }

    public void ClearSelectable()
    {
        SetSelectable(false);
    }

    public void Select()
    {
        if (!IsSelectable)
            return;

       // GameNetworkService.Instance.SendDiceTouched();
    }

    private void OnSelectableChanged(bool selectable)
    {
        // Dice animation
    }
}