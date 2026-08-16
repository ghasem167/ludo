using Nakama;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
public class GameNetworkServices
{

    private IClient _client;
    private ISocket _socket;
    private ISession _session;
    private CommandHandler commandHandler;
    public GameNetworkServices()
    {
        commandHandler = new CommandHandler();

    }


    public async Task InitializeAsync()
    {
        // 1. Create Nakama Client
        _client = new Client(
            "defaultkey",
            "127.0.0.1",
            7350,
            "http"
        );

        // 2. Authenticate user
        _session = await _client.AuthenticateDeviceAsync(
            SystemInfo.deviceUniqueIdentifier
        );

        // 3. Create Socket
        _socket = _client.NewSocket();

        // 4. Register listeners
        RegisterEvents();

        // 5. Connect socket
        await _socket.ConnectAsync(_session);

        Debug.Log("Nakama Connected");
    }


    private void RegisterEvents()
    {
        _socket.ReceivedMatchState += OnMatchState;
        _socket.ReceivedMatchPresence += OnMatchPresence;
        //_socket.Closed += OnSocketClosed;
        //_socket.Connected += OnSocketConnected;
    }
    private void OnMatchState(IMatchState message)
    {
        var command = Interpret(message);
        commandHandler.Enqueue(command);
    }

    private void OnMatchPresence(IMatchPresenceEvent message)
    {
        Debug.Log("Match Presence Event Received");
        // Handle match presence events here
    }
    public GameCommand Interpret(IMatchState message)
    {

        switch ((opcode)message.OpCode)
        {
            case opcode.LobbyStarted:
                return BuildLobbyStarted(message);
            case opcode.PlayerAdded:
                return BuildPlayerAdded(message);
            case opcode.Players:
                return BuildPlayers(message);
            case opcode.MatchStarted:
                return new MatchStartedCommand();
            case opcode.MatchFinished:
                return BuildMatchFinished(message);
            case opcode.LightsChanged:
                return BuildLightChanged(message);
            case opcode.TurnStarted:
                return BuildTurnStartedCommand(message);
            case opcode.DiceRolled:
                return BuildDiceRolledCommand(message);
            case opcode.ActionSelected:
                return BuildActionSelectedCommand(message);
            case opcode.PlayerFinished:
                return BuildPlayerFinishedCommand(message);
            case opcode.MatchState:
                return BuildMatchStateCommand(message);


            default:
                throw new NotImplementedException();
        }
    }
    private GameCommand BuildLobbyStarted(IMatchState message)
    {
        var dto = Deserialize<LobbyStartedDto>(message);

        return new LobbyStartedCommand(dto.Players);
    }
    private GameCommand BuildPlayerAdded(IMatchState message)
    {
        var dto = Deserialize<PlayerDto>(message);

        return new PlayerAddedCommand(dto);
    }
    private GameCommand BuildPlayers(IMatchState message)
    {
        var dto = Deserialize<PlayersDto>(message);

        return new PlayersCommand(dto.Players);
    }
    private GameCommand BuildMatchStarted(IMatchState message)
    {
        var dto = Deserialize<MatchStartedDto>(message);

        return new MatchStartedCommand();
    }
    private GameCommand BuildMatchFinished(IMatchState message)
    {
        var dto = Deserialize<MatchFinishedDto>(message);

        return new MatchFinishedCommand(dto.WinnerList);
    }
    private GameCommand BuildLightsChangedCommand(IMatchState message)
{
    var dto = Deserialize<LightsChangedDto>(message);
       return new LightsChangedCommand(
        dto.Player,
        dto.numOfLights
    );
        
    
}

   private GameCommand BuildTurnStartedCommand(IMatchState message)
{
    var dto = Deserialize<TurnStartedDto>(message);

    return new TurnStartedCommand(dto.PlayerColor);
}

    private GameCommand BuildDiceRolledCommand(IMatchState message)
    {
        var dto = Deserialize<DiceRolledDto>(message);

        return new DiceRolledCommand(dto);
    }

    private GameCommand BuildActionSelectedCommand(IMatchState message)
    {
        var dto = Deserialize<GameActionDto>(message);

        return new ActionSelectedCommand(dto);
    }

    private GameCommand BuildPlayerFinishedCommand(IMatchState message)
    {
        var dto = Deserialize<PlayerFinishedDto>(message);

        return new PlayerFinishedCommand(dto);
    }

    private GameCommand BuildMatchStateCommand(IMatchState message)
    {
        var dto = Deserialize<MatchStateDto>(message);

        return new MatchStateCommand(dto);
    }







    private T Deserialize<T>(IMatchState message)
    {
        string json = Encoding.UTF8.GetString(message.State);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
