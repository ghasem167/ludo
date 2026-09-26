using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dumps the lobby page tree (structure, active state, rect, prefab instance, TMP text) to
/// Temp/lobby_dump.txt - used while wiring the lobby slots.
/// </summary>
public static class DumpLobbyPage
{
    [MenuItem("Tools/Dump Lobby Page")]
    public static void Dump()
    {
        var sb = new StringBuilder();

        var menuObject = GameObject.Find("MainMenu");
        if (menuObject == null) { File.WriteAllText("Temp/lobby_dump.txt", "MainMenu not found"); return; }

        var menu = menuObject.GetComponent<MainMenu>();
        MenuPage lobby = null;

        foreach (var page in menuObject.GetComponentsInChildren<MenuPage>(true))
            if (page != null && page.Id == MenuPageId.Lobby) lobby = page;

        if (lobby == null) { File.WriteAllText("Temp/lobby_dump.txt", "LobbyPage not found"); return; }

        sb.AppendLine($"LobbyPage id={lobby.Id} titleLen={(lobby.Title != null ? lobby.Title.Length : 0)} " +
                      $"slots={(lobby.GetComponent<LobbyMenuPage>() != null ? "-" : "-")}");

        Dump(lobby.transform, 0, sb);

        File.WriteAllText("Temp/lobby_dump.txt", sb.ToString());
        Debug.Log("[DumpLobbyPage] written Temp/lobby_dump.txt");
    }

    private static void Dump(Transform node, int depth, StringBuilder sb)
    {
        var rect = node as RectTransform;
        string rectInfo = rect != null
            ? $"size={rect.rect.width:F0}x{rect.rect.height:F0} pos={rect.anchoredPosition.x:F0},{rect.anchoredPosition.y:F0}"
            : "-";

        string kind = "";
        if (PrefabUtility.IsPartOfPrefabInstance(node))
            kind = " [prefab " + PrefabUtility.GetCorrespondingObjectFromSource(node.gameObject).name + "]";

        string text = "";
        var tmp = node.GetComponent<TMP_Text>();
        if (tmp != null) text = $" text='{tmp.text}' font={(tmp.font != null ? tmp.font.name : "-")}";

        string slot = node.GetComponent<LobbyPlayerSlot>() != null ? " [LobbyPlayerSlot]" : "";

        sb.AppendLine($"{new string(' ', depth * 2)}{node.name} activeSelf={node.gameObject.activeSelf} " +
                      $"{rectInfo}{kind}{slot}{text}");

        foreach (Transform child in node)
            Dump(child, depth + 1, sb);
    }
}
