namespace Ludo.Offline
{
    public class DiceState
    {
        public bool WaitingForInput;
        public bool WaitingForAnimation;
        public int DiceValue;
        public bool WaitingForActionSelect;

        public DiceState()
        {
            WaitingForInput = false;
            WaitingForAnimation = false;
            DiceValue = 0;
            WaitingForActionSelect = false;
        }
    }
}