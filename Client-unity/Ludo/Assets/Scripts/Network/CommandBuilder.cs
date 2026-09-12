using System.Collections.Generic;
using System.Text;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

public class CommandBuilder
{
    public GameCommand BuildLobbyStarted(IMatchState message)
    {
        var dto = Deserialize<LobbyStartedDto>(message);

        return new LobbyStartedCommand();
    }
    public GameCommand BuildPlayerAdded(IMatchState message)
    {
        var dto = Deserialize<PlayerDto>(message);

        return new PlayerAddedCommand(dto);
    }
    public GameCommand BuildPlayers(IMatchState message)
    {
        var dto = Deserialize<PlayersDto>(message);

        return new PlayersCommand(dto.Players);
    }
    public GameCommand BuildMatchStarted(IMatchState message)
    {
        Debug.Log("match started");
        var dto = Deserialize<MatchStartedDto>(message);

        return new MatchStartedCommand();
    }
    public GameCommand BuildPiecePositionOnBoardCommand(IMatchState message)
    {
        var dto = Deserialize<List<PiecePositionDto>>(message);
        return new PiecesPositionCommand(dto);

    }
    public GameCommand BuildMatchFinished(IMatchState message)
    {
        var dto = Deserialize<MatchFinishedDto>(message);

        return new MatchFinishedCommand(dto.WinnerList);
    }
    public GameCommand BuildLightsChanged(IMatchState message)
    {
        var dto = Deserialize<LightsChangedDto>(message);
        return new LightsChangedCommand(
         dto.Player,
         dto.numOfLights
     );


    }

    public GameCommand BuildTurnStartedCommand(IMatchState message)
    {
        var dto = Deserialize<TurnStartedDto>(message);

        return new TurnStartedCommand(dto.PlayerColor);
    }

    public GameCommand BuildDiceValueCommand(IMatchState message)
    {
        var diceValue = Deserialize<int>(message);

        return new DiceValueCommand(
            diceValue
        );
    }
    public GameCommand BuildAvailableActionCommand(IMatchState message)
    {
        var actions = Deserialize<List<GameActionDto>>(message);

        return new AvailableActionCommand(actions);
    }

    public GameCommand BuildNewActionCommand(IMatchState message)
    {
        var dto = Deserialize<GameActionDto>(message);

        return new NewActionCommand(dto);
    }
    public GameCommand BuildCapturePieceCommand(IMatchState message)
    {
        var dto = Deserialize<PiecePositionDto>(message);

        return new CapturePieceCommand(dto);
    }


    private T Deserialize<T>(IMatchState message)
    {
        string json = Encoding.UTF8.GetString(message.State);
        return JsonConvert.DeserializeObject<T>(json);
    }
}