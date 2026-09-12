using System;
using System.Threading.Tasks;
using UnityEngine;

public class GamePlayHandler : IDisposable
{

    public GamePlayEvents GamePlayEvents { get; set; }

    public GamePlayContext LastContext { get; private set; }
    public CommandHandler CommandHandler;
    private GameNetworkServices NetworkServices;
    private ServerMatchHandler serverMatchHandler;
    private Ludo.Offline.MatchHandler matchHandler;

    public SelectionManager SelectionManager { get; private set; } = new SelectionManager();
    public GamePlayHandler()
    {
        LastContext = new GamePlayContext();
        LastContext.CurrentPlayMode = PlayMode.Offline;
        GamePlayEvents = new GamePlayEvents();

        GameObject commandHandlerObject =
            new GameObject("CommandHandler");

        CommandHandler =
            commandHandlerObject.AddComponent<CommandHandler>();
        CreateOfflineMatch();
        SubscribeGamePlayEvents();
    }
    public GamePlayHandler(GameNetworkServices networkServices)
    {
        LastContext = new GamePlayContext();
        LastContext.CurrentPlayMode = PlayMode.Online;
        GamePlayEvents = new GamePlayEvents();

        GameObject commandHandlerObject =
            new GameObject("CommandHandler");

        CommandHandler =
            commandHandlerObject.AddComponent<CommandHandler>();
        NetworkServices = networkServices;
        CreateOnlineMatch(TeamMode.None, GameMode.Modern);
        SubscribeGamePlayEvents();
    }
    private async void CreateOfflineMatch()
    {

        await Task.Delay(1000);
        CreateOfflineMatch(TeamMode.None, GameMode.Modern, PlayerColor.Blue);
    }
    private async void CreateOnlineMatch(TeamMode teamMode, GameMode gameMode)
    {

        await Task.Delay(1000);
        var response = await NetworkServices.FindOrCreateMatch(teamMode, gameMode);
        Debug.Log("Response of Match Create Requeste: " + response);
        await NetworkServices.JoinMatch();
        serverMatchHandler = new ServerMatchHandler(NetworkServices, CommandHandler);

    }
    private void CreateOfflineMatch(TeamMode teamMode = TeamMode.None, GameMode gameMode = GameMode.Modern, PlayerColor playerColor = PlayerColor.Blue)
    {
        Debug.Log("Creating Offline Match");

        matchHandler = new Ludo.Offline.MatchHandler(
            CommandHandler,
            gameMode,
            teamMode
        );
        CommandHandler.Enqueue(new PlayerAddedCommand(new PlayerDto
        {
            Id = "offline_player_1",
            Username = "Player 1",
            Color = playerColor,
        }));
        _ = matchHandler.RunMatchLoop();
    }


    private void SubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected += OnActionSelected;

        GamePlayEvents.DiceSelected += OnDiceSelected;
    }
    private void UnsubscribeGamePlayEvents()
    {
        GamePlayEvents.ActionSelected -= OnActionSelected;

        GamePlayEvents.DiceSelected -= OnDiceSelected;
    }
    private void OnActionSelected(int actionIndex)
    {
        if (LastContext.CurrentPlayMode == PlayMode.Online)
        {
            _ = serverMatchHandler
                .SendActionSelected(actionIndex);
        }
        else
        {
            HandleLocalActionSelected(actionIndex);
        }
    }
    private async void OnDiceSelected()
    {
        if (LastContext.CurrentPlayMode == PlayMode.Online)
        {
            await serverMatchHandler.SendRollDice();
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
        matchHandler?.Dispose();
    }

}