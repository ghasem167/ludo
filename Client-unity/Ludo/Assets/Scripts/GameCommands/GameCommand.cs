using System.Threading.Tasks;

public abstract class GameCommand
{
   
    public virtual bool CanExecute() => true;

    public virtual string Name => GetType().Name;

    public abstract Task Execute();
}