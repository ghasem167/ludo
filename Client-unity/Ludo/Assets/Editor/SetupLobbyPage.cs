using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the lobby page (idempotent - run it again after changing the lobby structure):
///   * gives every slot of the lobby grid a LobbyPlayerSlot with its "empty" (Null User) and
///     "filled" (Loded User) visuals, the nickname text and the avatar image,
///   * fills LobbyMenuPage (slots, waiting text, timer),
///   * registers LobbyPage in MainMenu.pages (and drops duplicate entries),
///   * removes the missing scripts left over from renamed page classes,
///   * shapes page titles that are still stored as raw Persian (TMP cannot shape at runtime).
/// It writes Temp/lobby_setup.txt and saves the scene.
/// </summary>
public static class SetupLobbyPage
{
    private const string ReportPath = "Temp/lobby_setup.txt";
    private const string EmptySlotPrefab = "Assets/UI/Prefabs/Null User.prefab";
    private const string FilledSlotPrefab = "Assets/UI/Prefabs/Loded User.prefab";
    private const string EmptySlotName = "Null User";
    private const string FilledSlotName = "Loded User";

    [MenuItem("Tools/Setup Lobby Page")]
    public static void Setup()
    {
        var log = new List<string>();

        var menuObject = GameObject.Find("MainMenu");
        if (menuObject == null) { Debug.LogError("[SetupLobbyPage] MainMenu not found"); return; }

        var menu = menuObject.GetComponent<MainMenu>();
        if (menu == null) { Debug.LogError("[SetupLobbyPage] MainMenu component not found"); return; }

        var lobbyPage = FindPage(menu, MenuPageId.Lobby);
        if (lobbyPage == null) { Debug.LogError("[SetupLobbyPage] LobbyPage not found"); return; }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup Lobby Page");

        WireSlots(lobbyPage.transform, log);
        WirePage(lobbyPage, log);
        EnsureLobbyTitle(lobbyPage, log);
        WirePages(menu, log);
        ShapeTitles(menu, log);
        RemoveMissingScripts(menuObject, log);

        Undo.CollapseUndoOperations(undoGroup);

        var scene = menuObject.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        log.Add("scene saved: " + saved);
        File.WriteAllText(ReportPath, string.Join("\n", log));
        Debug.Log("[SetupLobbyPage] done. saved=" + saved + "\n  " + string.Join("\n  ", log));
    }

    /// <summary>Builds the 4 slots: placeholder + filled row + the bindings of every slot.</summary>
    private static void WireSlots(Transform lobbyPage, List<string> log)
    {
        var grid = FindGrid(lobbyPage);
        if (grid == null) { log.Add("MISSING: the lobby grid (GridLayoutGroup) was not found"); return; }

        var emptyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EmptySlotPrefab);
        var filledPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FilledSlotPrefab);

        if (emptyPrefab == null) log.Add("MISSING prefab " + EmptySlotPrefab);
        if (filledPrefab == null) log.Add("MISSING prefab " + FilledSlotPrefab);

        for (int i = 0; i < grid.childCount; i++)
        {
            var slotTransform = grid.GetChild(i);

            var slot = slotTransform.GetComponent<LobbyPlayerSlot>();
            if (slot == null) slot = Undo.AddComponent<LobbyPlayerSlot>(slotTransform.gameObject);

            var so = new SerializedObject(slot);

            // rows that are already inside the slot (anything with a text: 'Null User' has none)
            var rows = new List<GameObject>();
            foreach (Transform child in slotTransform)
            {
                if (child.name == EmptySlotName) continue;
                if (child.GetComponentInChildren<TMP_Text>(true) == null) continue;

                rows.Add(child.gameObject);
            }

            GameObject filled = so.FindProperty("filledVisual").objectReferenceValue as GameObject;

            if (filled == null)
            {
                // a row the designer placed in the slot wins over the shared prefab
                foreach (var row in rows)
                    if (row.name.StartsWith(FilledSlotName)) { filled = row; break; }

                if (filled == null && rows.Count > 0) filled = rows[0];

                if (filled == null && filledPrefab != null)
                    filled = (GameObject)PrefabUtility.InstantiatePrefab(filledPrefab, slotTransform);
            }

            // hide the leftover rows (old design templates) so exactly one row shows per player
            foreach (var row in rows)
            {
                if (row == filled || !row.activeSelf) continue;

                row.SetActive(false);
                log.Add($"slot '{slotTransform.name}': extra row '{row.name}' hidden");
            }

            var emptyNode = FindChild(slotTransform, EmptySlotName);
            GameObject empty = emptyNode != null ? emptyNode.gameObject : null;
            if (empty == null && emptyPrefab != null)
                empty = (GameObject)PrefabUtility.InstantiatePrefab(emptyPrefab, slotTransform);

            var nameText = filled != null ? filled.GetComponentInChildren<TMP_Text>(true) : null;
            var avatar = filled != null ? FindImage(filled.transform, "User Image") : null;

            so.FindProperty("emptyVisual").objectReferenceValue = empty;
            so.FindProperty("filledVisual").objectReferenceValue = filled;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("avatarImage").objectReferenceValue = avatar;
            so.ApplyModifiedPropertiesWithoutUndo();

            // scene state before the first Refresh(): placeholder visible, row hidden
            if (empty != null) empty.SetActive(true);
            if (filled != null) filled.SetActive(false);

            log.Add($"slot {i} '{slotTransform.name}': empty={(empty != null ? empty.name : "-")} " +
                    $"filled={(filled != null ? filled.name : "-")} " +
                    $"name={(nameText != null ? nameText.name : "-")} " +
                    $"avatar={(avatar != null ? avatar.name : "-")} rows={rows.Count}");
        }

        log.Add($"slots on the grid: {grid.childCount}");
    }

    /// <summary>Fills the LobbyMenuPage fields (slots, waiting text, timer).</summary>
    private static void WirePage(MenuPage lobbyPage, List<string> log)
    {
        var slots = new List<LobbyPlayerSlot>();
        var grid = FindGrid(lobbyPage.transform);

        if (grid != null)
            for (int i = 0; i < grid.childCount; i++)
                slots.Add(grid.GetChild(i).GetComponent<LobbyPlayerSlot>());

        var waitingNode = FindChild(lobbyPage.transform, "waitingtext");
        var timerNode = FindChild(lobbyPage.transform, "timer");

        var so = new SerializedObject(lobbyPage);

        var slotsProperty = so.FindProperty("slots");
        slotsProperty.arraySize = slots.Count;
        for (int i = 0; i < slots.Count; i++)
            slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

        so.FindProperty("waitingText").objectReferenceValue =
            waitingNode != null ? waitingNode.GetComponent<TMP_Text>() : null;
        so.FindProperty("timerText").objectReferenceValue =
            timerNode != null ? timerNode.GetComponent<TMP_Text>() : null;

        so.ApplyModifiedPropertiesWithoutUndo();

        // the countdown is off by default (showTimer = false): keep it hidden in the scene as well
        if (timerNode != null && !so.FindProperty("showTimer").boolValue)
            timerNode.gameObject.SetActive(false);

        log.Add($"page: slots={slots.Count} waiting='{(waitingNode != null ? waitingNode.name : "-")}' " +
                $"timer='{(timerNode != null ? timerNode.name : "-")}'");
    }

    /// <summary>
    /// Rebuilds MainMenu.pages from the MenuPage components that exist in the scene, ordered by id:
    /// this registers LobbyPage and removes the duplicated array entries.
    /// </summary>
    private static void WirePages(MainMenu menu, List<string> log)
    {
        var pages = new List<MenuPage>();
        var seen = new HashSet<MenuPageId>();

        foreach (var page in menu.GetComponentsInChildren<MenuPage>(true))
        {
            if (page == null || page.Id == MenuPageId.None) continue;
            if (!seen.Add(page.Id))
            {
                log.Add($"duplicate id {page.Id} on '{Path(page.transform)}' - skipped");
                continue;
            }

            pages.Add(page);
        }

        pages.Sort((a, b) => ((int)a.Id).CompareTo((int)b.Id));

        var so = new SerializedObject(menu);
        var property = so.FindProperty("pages");
        property.arraySize = pages.Count;
        for (int i = 0; i < pages.Count; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = pages[i];

        so.ApplyModifiedPropertiesWithoutUndo();

        log.Add($"MainMenu.pages = {pages.Count}");
        foreach (var page in pages)
            log.Add($"  {(int)page.Id,-3} {page.Id,-20} '{Path(page.transform)}'");
    }

    /// <summary>Titles must be pre-shaped: TMP in this project cannot shape Arabic script itself.</summary>
    private static void ShapeTitles(MainMenu menu, List<string> log)
    {
        foreach (var page in menu.GetComponentsInChildren<MenuPage>(true))
        {
            if (page == null) continue;

            string title = page.Title;
            if (string.IsNullOrEmpty(title) || !HasRawArabic(title)) continue;

            string shaped = PersianText.Shape(title);

            Undo.RecordObject(page, "Shape page title");
            page.SetTitle(shaped);
            EditorUtility.SetDirty(page);

            log.Add($"title shaped: {page.Id} -> '{shaped}'");
        }
    }

    /// <summary>Drops the components whose scripts no longer exist (renamed/removed page classes).</summary>
    private static void RemoveMissingScripts(GameObject root, List<string> log)
    {
        int removed = 0;

        foreach (var node in root.GetComponentsInChildren<Transform>(true))
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
            if (count <= 0) continue;

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(node.gameObject);
            removed += count;
            log.Add($"missing script x{count} removed from '{Path(node)}'");
        }

        log.Add($"missing scripts removed: {removed}");
    }

    private static bool HasRawArabic(string value)
    {
        foreach (char c in value)
            if (c >= '\u0600' && c <= '\u06FF') return true;

        return false;
    }

    /// <summary>The shared header shows the page title, so the lobby page needs one.</summary>
    private static void EnsureLobbyTitle(MenuPage lobbyPage, List<string> log)
    {
        if (!string.IsNullOrEmpty(lobbyPage.Title)) return;

        Undo.RecordObject(lobbyPage, "Lobby title");
        lobbyPage.SetTitle(PersianText.Shape("\u0627\u0646\u062A\u0638\u0627\u0631"));
        EditorUtility.SetDirty(lobbyPage);

        log.Add($"lobby title set to '{lobbyPage.Title}'");
    }

    /// <summary>Finds the lobby grid: the only child of the page that uses a GridLayoutGroup.</summary>
    private static Transform FindGrid(Transform parent)
    {
        foreach (var grid in parent.GetComponentsInChildren<GridLayoutGroup>(true))
            if (grid != null) return grid.transform;

        return null;
    }

    private static MenuPage FindPage(MainMenu menu, MenuPageId id)
    {
        foreach (var page in menu.GetComponentsInChildren<MenuPage>(true))
            if (page != null && page.Id == id) return page;

        return null;
    }

    private static Transform FindChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindChild(parent.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }

    /// <summary>Image of the child called <paramref name="name"/>, else the first image of the row.</summary>
    private static Image FindImage(Transform root, string name)
    {
        var node = FindChild(root, name);
        var image = node != null ? node.GetComponent<Image>() : null;
        if (image != null) return image;

        foreach (var candidate in root.GetComponentsInChildren<Image>(true))
            if (candidate != null && candidate.transform != root) return candidate;

        return null;
    }

    private static string Path(Transform node)
    {
        var path = node.name;
        var parent = node.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
