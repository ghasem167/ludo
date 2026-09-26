
/// <summary>
/// Payload of the server's "PlayerAdded" opcode (matchJoin, when a bot is converted to a human):
///   { "player": { "id": "…", "nikeName": "…", "color": 0 } }
/// The message wraps the player, so it cannot be deserialized straight into <see cref="PlayerDto"/>.
/// </summary>
public class PlayerAddedDto
{
    public PlayerMatchDto Player;
}
