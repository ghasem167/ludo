using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CommandHandler
{

    private readonly Queue<GameCommand> queue;
    private bool isProcessing;
    public CommandHandler()
    {
        queue = new Queue<GameCommand>();
    }
    public void Enqueue(GameCommand command)
    {
        queue.Enqueue(command);

        if (!isProcessing)
        {
            _ = ProcessQueue();
        }
    }

    private async Task ProcessQueue()
    {
        isProcessing = true;

        while (queue.Count > 0)
        {
            GameCommand command = queue.Dequeue();

            await command.Execute();
        }

        isProcessing = false;
    }


}
