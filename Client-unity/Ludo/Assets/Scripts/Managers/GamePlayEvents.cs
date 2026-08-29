using System;

public class GamePlayEvents
{
    public event Action<int> ActionSelected;

    public event Action DiceSelected;

    public void RaiseActionSelected(int actionIndex)
    {
        ActionSelected?.Invoke(actionIndex);
    }
      public void RaiseDiceSelected()
    {
        DiceSelected?.Invoke();
    }
    
}