using System;
using System.Threading.Tasks;
using UnityEngine;

public class GamePlayHandler : IDisposable
{

    public GamePlayEvents GamePlayEvents { get; set; }

    public GamePlayContext LastContext { get; private set; }
    public CommandHandler CommandHandler;
    private GameNetworkServices NetworkServices;
    public GamePlayHandler(GameNetworkServices networkServices)
    {
        LastContext = new GamePlayContext();
        GamePlayEvents = new GamePlayEvents();

        GameObject commandHandlerObject =
            new GameObject("CommandHandler");

        CommandHandler =
            commandHandlerObject.AddComponent<CommandHandler>();
        NetworkServices = networkServices;
        CreateOnlineMatch();
        SubscribeGamePlayEvents();
    }

    private async void CreateOnlineMatch()
    {

        NetworkServices.SetCommandHandler(CommandHandler);
        await Task.Delay(1000);
        var response = await NetworkServices.FindOrCreateMatch(TeamMode.None, GameMode.Classic);
        await NetworkServices.JoinMatch();
    }
    private void CreateOfflineMatch()
    {

    }
    private void SubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected += OnActionSelected;

        GamePlayEvents.DiceSelected -= OnDiceSelected;
    }
    private void UnsubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected -= OnActionSelected;

        GamePlayEvents.DiceSelected -= OnDiceSelected;
    }
    private void OnActionSelected(int actionIndex)
    {
        if (NetworkServices.IsOnline)
        {
            _ = NetworkServices
                .SendActionSelected(actionIndex);
        }
        else
        {
            HandleLocalActionSelected(actionIndex);
        }
    }
    private async void OnDiceSelected()
    {
        if (NetworkServices.IsOnline)
        {
            await NetworkServices.SendRollDice();
            return;
        }

        HandleOfflineDiceRoll();
    }
    private void HandleLocalActionSelected(int actionIndex)
    {
        // ساخت همان Commandای که در حالت آنلاین
        // بعد از دریافت پیام سرور ساخته می‌شود
    }
    private void HandleOfflineDiceRoll()
    {
        // منطق Roll در حالت آفلاین
    }
    public void Dispose()
    {
        UnsubscribeGamePlayEvents();
    }
}