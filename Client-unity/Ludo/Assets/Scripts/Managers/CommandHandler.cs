using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class CommandHandler : MonoBehaviour
{
    private readonly Queue<GameCommand> _queue = new();

    private bool _isProcessing;

    public void Enqueue(GameCommand command)
    {
        if (command == null)
            return;

        _queue.Enqueue(command);

        UnityEngine.Debug.Log("commands in queue: " + _queue.Count);
    }

    private void Update()
    {
        if (_isProcessing)
            return;

        if (_queue.Count == 0)
            return;

        _ = ProcessQueue();
    }

    private async Task ProcessQueue()
    {
        _isProcessing = true;

        try
        {
            while (_queue.Count > 0)
            {
                GameCommand command = _queue.Dequeue();

                UnityEngine.Debug.Log("execution");

                await command.Execute();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            _isProcessing = false;
        }
    }
}