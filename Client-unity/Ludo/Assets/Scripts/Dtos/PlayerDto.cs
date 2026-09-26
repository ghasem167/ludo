
using System;
using Newtonsoft.Json;

[Serializable]
public class PlayerDto
{
    public string Id;

    /// <summary>
    /// Used by offline matches.
    /// </summary>
    public string Username;

    [JsonProperty("userNikeName")]
    public string UserNickname;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(Username))
                return Username;

            if (!string.IsNullOrEmpty(UserNickname))
                return UserNickname;

            return "Player";
        }
    }
}

