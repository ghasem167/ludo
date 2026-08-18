using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public bool IsOnline
    {
        get
        {
            return _client != null &&
                   _session != null &&
                   _socket != null &&
                   _socket.IsConnected;
        }
    }
    public bool IsAuthenticated =>
    _client != null && _session != null;

    public bool IsConnected =>
        _socket != null && _socket.IsConnected;
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
    public async Task<string> ReadInventoryAsync()
    {
        var result = await _client.ReadStorageObjectsAsync(
            _session,
            new IApiReadStorageObjectId[]
            {
            new StorageObjectId
            {
                Collection = "player",
                Key = "inventory",
                UserId = _session.UserId
            }
            });

        foreach (var obj in result.Objects)
        {
            return obj.Value;
        }

        return null;
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
            case opcode.PiecesPosition:
                return BuildPiecePositionOnBoardCommand(message);
            case opcode.LightsChanged:
                return BuildLightsChanged(message);
            case opcode.TurnStarted:
                return BuildTurnStartedCommand(message);
            case opcode.DiceValue:
                return BuildDiceValueCommand(message);
            case opcode.AvailableActions:
                return BuildAvailableActionCommand(message);
            case opcode.NewAction:
                return BuildNewActionCommand(message);
            case opcode.PlayerFinished:
                return new PlayerFinishedCommand();
            case opcode.MatchFinished:
                return BuildMatchFinished(message);



            default:
                throw new NotImplementedException();
        }
    }

    public async Task<FindOrCreateMatchResult> FindOrCreateMatch(
    string matchId,
    TeamMode teamMode,
    GameMode gameMode)
    {
        var payload = new
        {
            matchId = matchId,
            teamMode = (int)teamMode,
            gameMode = (int)gameMode
        };

        var response = await _client.RpcAsync(
            _session,
            "FindOrCreateGame",
            JsonConvert.SerializeObject(payload)
        );

        return JsonConvert.DeserializeObject<FindOrCreateMatchResult>(
            response.Payload
        );
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
    private GameCommand BuildPiecePositionOnBoardCommand(IMatchState message)
    {
        var dto = Deserialize<List<PiecePositionDto>>(message);
        return new PiecesPositionCommand(dto);

    }
    private GameCommand BuildMatchFinished(IMatchState message)
    {
        var dto = Deserialize<MatchFinishedDto>(message);

        return new MatchFinishedCommand(dto.WinnerList);
    }
    private GameCommand BuildLightsChanged(IMatchState message)
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

    private GameCommand BuildDiceValueCommand(IMatchState message)
    {
        var dto = Deserialize<DiceValueDto>(message);

        return new DiceValueCommand(
            dto.diceValue
        );
    }
    private GameCommand BuildAvailableActionCommand(IMatchState message)
    {
        var dto = Deserialize<AvailableActionDto>(message);

        return new AvailableActionCommand(
            dto
        );
    }

    private GameCommand BuildNewActionCommand(IMatchState message)
    {
        var dto = Deserialize<GameActionDto>(message);

        return new NewActionCommand(dto);
    }



    private T Deserialize<T>(IMatchState message)
    {
        string json = Encoding.UTF8.GetString(message.State);
        return JsonConvert.DeserializeObject<T>(json);
    }
    public async Task<string> BuyAssetAsync(string assetId)
    {
        var payload = new
        {
            assetId = assetId
        };

        string json = JsonUtility.ToJson(
            new BuyAssetRequest
            {
                AssetId = assetId
            });

        var response = await _client.RpcAsync(
            _session,
            "buy_asset",
            json);

        return response.Payload;
    }
}
