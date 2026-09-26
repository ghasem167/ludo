using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the menu flow into the scene (idempotent - run again after changing the structure):
///   * adds the right MenuPage component to every page and fills its id/title,
///   * fills MainMenu (pages, header, title, fonts, back button),
///   * rewires the buttons: every legacy "SetActive(page)" call becomes MainMenu.OpenPage(page)
///     and the buttons that had no listener yet (bottom nav, offline, territories) get theirs.
/// </summary>
public static class SetupMainMenuFlow
{
    private class PageSpec
    {
        public string objectName;
        public MenuPageId id;
        public string title;
        public bool root;
        public System.Type type;
    }

    private static readonly PageSpec[] Specs =
    {
        new PageSpec { objectName = "LobbyPage", id = MenuPageId.Lobby, title = "\u0627\u0646\u062A\u0638\u0627\u0631", type = typeof(LobbyMenuPage) },
        new PageSpec { objectName = "Home Page",                 id = MenuPageId.Home,               title = "لودو",            root = true, type = typeof(HomeMenuPage) },
        new PageSpec { objectName = "Store Page",                id = MenuPageId.Store,              title = "فروشگاه",         type = typeof(StoreMenuPage) },
        new PageSpec { objectName = "Friends Page",              id = MenuPageId.Friends,            title = "دوستان",          type = typeof(SimpleMenuPage) },
        new PageSpec { objectName = "Level Page",                id = MenuPageId.Levels,             title = "رتبه‌بندی",        type = typeof(LevelMenuPage) },
        new PageSpec { objectName = "Game Page",                 id = MenuPageId.GamePage,      title = "انتخاب قلمرو",     type = typeof(GameMenuPage) },
        new PageSpec { objectName = "Messages Page",             id = MenuPageId.Messages,           title = "پیام‌ها",          type = typeof(SimpleMenuPage) },
        new PageSpec { objectName = "Lucky Wheel Page",          id = MenuPageId.LuckyWheel,         title = "گردونه شانس",      type = typeof(SimpleMenuPage) },
        new PageSpec { objectName = "VIP Wheel Page",            id = MenuPageId.VipWheel,           title = "گردونه VIP",       type = typeof(SimpleMenuPage) },
        new PageSpec { objectName = "Profile Page",              id = MenuPageId.Profile,            title = "پروفایل",          type = typeof(SimpleMenuPage) },
        new PageSpec { objectName = "Playing with Friends Page", id = MenuPageId.PlayingWithFriends, title = "بازی با دوستان",   type = typeof(PlayingWithFriendsMenuPage) }
    };

    private static readonly Dictionary<string, MenuPageId> NameMap = new Dictionary<string, MenuPageId>
    {
        // bottom navigation bar
        { "Store_Btn", MenuPageId.Store },
        { "Friends_Btn", MenuPageId.Friends },
        { "Home_Btn", MenuPageId.Home },
        { "Level_Btn", MenuPageId.Levels },
        { "Setting_Btn", MenuPageId.Profile },
        { "BtnOffline", MenuPageId.GamePage },
        { "Playing With Friends Btn", MenuPageId.PlayingWithFriends },

        // home page shortcuts (the diamond box + the user card + the two "box 2" buttons)
        { "Btn Group ", MenuPageId.Profile },
        { "Btn Plus", MenuPageId.Store },
        { "Btn Random", MenuPageId.LuckyWheel },
        { "Btn Email", MenuPageId.Messages }
    };

    [MenuItem("Tools/Setup Main Menu Flow")]
    public static void Setup()
    {
        var log = new List<string>();

        var menuObject = GameObject.Find("MainMenu");
        if (menuObject == null) { Debug.LogError("[SetupMainMenuFlow] MainMenu not found"); return; }

        var menu = menuObject.GetComponent<MainMenu>();
        if (menu == null) { Debug.LogError("[SetupMainMenuFlow] MainMenu component not found"); return; }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup Main Menu Flow");

        // ---------- 1) pages ----------
        var pageComponents = new List<MenuPage>();

        foreach (var spec in Specs)
        {
            var pageTransform = FindTransform(menuObject.transform, spec.objectName);
            if (pageTransform == null) { log.Add($"MISSING page object '{spec.objectName}'"); continue; }

            var page = pageTransform.GetComponent<MenuPage>();
            if (page == null || page.GetType() != spec.type)
            {
                if (page != null) Undo.DestroyObjectImmediate(page);
                page = (MenuPage)Undo.AddComponent(pageTransform.gameObject, spec.type);
            }

            if (pageTransform.GetComponent<CanvasGroup>() == null)
                Undo.AddComponent<CanvasGroup>(pageTransform.gameObject);

            var pageSo = new SerializedObject(page);
            pageSo.FindProperty("id").enumValueIndex = (int)spec.id;
            pageSo.FindProperty("title").stringValue = PersianText.Shape(spec.title);
            pageSo.FindProperty("root").boolValue = spec.root;
            pageSo.ApplyModifiedPropertiesWithoutUndo();

            pageComponents.Add(page);
        }

        // ---------- 2) buttons ----------
        var header = FindTransform(menuObject.transform, "Header");
        var headerBack = header != null ? FindButton(header, "Btn Back") : null;

       // var bindings = CollectPageBindings(menuObject.transform, pageComponents, log);
      //  AddNameMapBindings(menuObject.transform, bindings, log);

        var backButtons = new List<Button>();
        if (headerBack != null) backButtons.Add(headerBack);
        CollectPageBackButtons(pageComponents, headerBack, backButtons, log);

        // ---------- 3) MainMenu fields ----------
        var menuSo = new SerializedObject(menu);

        var pagesProperty = menuSo.FindProperty("pages");
        pagesProperty.arraySize = pageComponents.Count;
        for (int i = 0; i < pageComponents.Count; i++)
            pagesProperty.GetArrayElementAtIndex(i).objectReferenceValue = pageComponents[i];

        menuSo.FindProperty("startPage").enumValueIndex = (int)MenuPageId.Home;
        menuSo.FindProperty("headerRoot").objectReferenceValue = header != null ? header.gameObject : null;

        var titleNode = FindTransform(header, "Title");
        var titleText = titleNode != null ? titleNode.GetComponentInChildren<TMP_Text>(true) : null;
        menuSo.FindProperty("titleText").objectReferenceValue = titleText;
        menuSo.FindProperty("backButton").objectReferenceValue = headerBack;

        var persianFont = LoadFontAsset("BRoyaBd SDF Dynamic");
        var latinFont = LoadFontAsset("LiberationSans SDF");
        menuSo.FindProperty("persianFont").objectReferenceValue = persianFont;
        menuSo.FindProperty("latinFont").objectReferenceValue = latinFont;

        var bindingsProperty = menuSo.FindProperty("pageButtons");
      //  bindingsProperty.arraySize = bindings.Count;
      /*  for (int i = 0; i < bindings.Count; i++)
        {
            var element = bindingsProperty.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("button").objectReferenceValue = bindings[i].button;
            element.FindPropertyRelative("page").enumValueIndex = (int)bindings[i].page;
        }*/

        var backProperty = menuSo.FindProperty("backButtons");
        backProperty.arraySize = backButtons.Count;
        for (int i = 0; i < backButtons.Count; i++)
            backProperty.GetArrayElementAtIndex(i).objectReferenceValue = backButtons[i];

        menuSo.ApplyModifiedPropertiesWithoutUndo();

        log.Add($"pages wired: {pageComponents.Count}, title: {(titleText != null ? titleText.name : "-")}, " +
                $"header back: {(headerBack != null ? headerBack.name : "-")}, " +
                $"fonts: {(persianFont != null ? persianFont.name : "-")}/{(latinFont != null ? latinFont.name : "-")}");
       // log.Add($"page buttons: {bindings.Count}, back buttons: {backButtons.Count}");
       // foreach (var binding in bindings)
       //     log.Add($"  bind {Path(binding.button.transform)} -> {binding.page}");
        foreach (var button in backButtons)
            log.Add($"  back {Path(button.transform)}");

        // ---------- 4) page specific wiring ----------
        WireLevelPage(menuObject.transform, log);
        WireClassicalGamePage(menuObject.transform, log);
        WirePlayingWithFriendsPage(menuObject.transform, log);

        Undo.CollapseUndoOperations(undoGroup);

        var scene = menuObject.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        Debug.Log("[SetupMainMenuFlow] done. saved=" + saved + "\n  " + string.Join("\n  ", log));
    }

    /// <summary>
    /// Reads the legacy navigation from the scene: buttons that used to call SetActive(page, true).
    /// Those listeners are removed (the navigator owns page visibility now) and turned into a
    /// page binding. Buttons whose intent cannot be resolved are left completely untouched.
    /// </summary>
   /* private static List<MainMenu.PageButton> CollectPageBindings(Transform root, List<MenuPage> pages, List<string> log)
    {
        var result = new List<MainMenu.PageButton>();

        foreach (var button in root.GetComponentsInChildren<Button>(true))
        {
            var so = new SerializedObject(button);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            if (calls == null || calls.arraySize == 0) continue;

            var legacyIndices = new List<int>();
            MenuPageId opened = MenuPageId.None;
            int openedCount = 0;

            for (int i = 0; i < calls.arraySize; i++)
            {
                var call = calls.GetArrayElementAtIndex(i);
                if (call.FindPropertyRelative("m_MethodName").stringValue != "SetActive") continue;

                var target = call.FindPropertyRelative("m_Target").objectReferenceValue as GameObject;
                if (target == null) continue;

                var page = target.GetComponent<MenuPage>();
                if (page == null || page.Id == MenuPageId.None) continue; // an overlay, not a menu page

                legacyIndices.Add(i);

                var boolArgument = call.FindPropertyRelative("m_BoolArgument");
                if (boolArgument != null && boolArgument.boolValue)
                {
                    opened = page.Id;
                    openedCount++;
                }
            }

            if (legacyIndices.Count == 0) continue;

            if (openedCount != 1)
            {
                log.Add($"WARNING '{Path(button.transform)}': {legacyIndices.Count} legacy page listener(s) " +
                        $"with {openedCount} clear target(s) - left untouched");
                continue;
            }

            for (int i = legacyIndices.Count - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(button.onClick, legacyIndices[i]);

            result.Add(new MainMenu.PageButton { button = button, page = opened });
            log.Add($"legacy '{Path(button.transform)}' -> {opened}");
        }

        return result;
    }*/

    /// <summary>
    /// Fallback for buttons that carry no legacy listener (bottom nav bar, offline button, ...):
    /// they are matched by name.
    /// </summary>
   /* private static void AddNameMapBindings(Transform root, List<MainMenu.PageButton> bindings, List<string> log)
    {
        foreach (var pair in NameMap)
        {
            var button = FindButton(root, pair.Key);
            if (button == null) { log.Add($"MISSING nav button '{pair.Key}'"); continue; }

            if (bindings.Exists(b => b.button == button)) continue;

          //  bindings.Add(new MainMenu.PageButton { button = button, page = pair.Value });
            log.Add($"nav '{pair.Key}' -> {pair.Value}");
        }
    }*/

    /// <summary>Every page local "Btn Back" (Messages / VIP wheel / ...) returns to the previous page.</summary>
    private static void CollectPageBackButtons(List<MenuPage> pages, Button headerBack, List<Button> result, List<string> log)
    {
        foreach (var page in pages)
        {
            if (page == null) continue;

            foreach (var button in page.GetComponentsInChildren<Button>(true))
            {
                if (button == null || button == headerBack) continue;
                if (button.name != "Btn Back") continue;
                if (result.Contains(button)) continue;

                result.Add(button);
                log.Add($"page back button '{Path(button.transform)}'");
            }
        }
    }

    private static void WireLevelPage(Transform root, List<string> log)
    {
        var page = root.GetComponentInChildren<LevelMenuPage>(true);
        if (page == null) { log.Add("LevelMenuPage missing"); return; }

        var tabs = new List<Button>();
        for (int i = 1; i <= 3; i++)
        {
            var button = FindButton(page.transform, $"Btn Select {i}");
            if (button != null) tabs.Add(button);
        }

        var panels = new List<GameObject>();
        foreach (var name in new[] { "Weekly", "Monthly", "General" })
        {
            var transform = FindTransform(page.transform, name);
            if (transform != null) panels.Add(transform.gameObject);
        }

        var so = new SerializedObject(page);

        var buttonsProperty = so.FindProperty("tabButtons");
        buttonsProperty.arraySize = tabs.Count;
        for (int i = 0; i < tabs.Count; i++)
            buttonsProperty.GetArrayElementAtIndex(i).objectReferenceValue = tabs[i];

        var panelsProperty = so.FindProperty("tabPanels");
        panelsProperty.arraySize = panels.Count;
        for (int i = 0; i < panels.Count; i++)
            panelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = panels[i];

        so.ApplyModifiedPropertiesWithoutUndo();
        log.Add($"level page: {tabs.Count} tab buttons / {panels.Count} panels");
    }

    private static void WireClassicalGamePage(Transform root, List<string> log)
    {
        var page = root.GetComponentInChildren<GameMenuPage>(true);
        if (page == null) { log.Add("ClassicalGameMenuPage missing"); return; }

        var box = FindTransform(page.transform, "Level Box");
        if (box == null) { log.Add("Level Box not found - territory cards not wired"); return; }

        var cards = new List<Transform>();
        foreach (var name in new[] { "Beginner Level", "Team Level", "Pro Level", "Aristocratic Level" })
        {
            var transform = FindTransform(box, name);
            if (transform != null) cards.Add(transform);
        }

        var so = new SerializedObject(page);
        var cardsProperty = so.FindProperty("cards");
        cardsProperty.arraySize = cards.Count;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var element = cardsProperty.GetArrayElementAtIndex(i);

            element.FindPropertyRelative("button").objectReferenceValue = FindButton(card, "Btn");
            element.FindPropertyRelative("territory").enumValueIndex = i;
            element.FindPropertyRelative("teamMode").enumValueIndex =
                (i == (int)Territory.Team) ? (int)TeamMode.TwoVsTwo : (int)TeamMode.None;
            element.FindPropertyRelative("entryCost").intValue = ReadEntryCost(card);
            element.FindPropertyRelative("highlight").objectReferenceValue = card.GetComponent<Graphic>();
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        log.Add($"classical page: {cards.Count} territory cards wired");
    }

    private static void WirePlayingWithFriendsPage(Transform root, List<string> log)
    {
        var page = root.GetComponentInChildren<PlayingWithFriendsMenuPage>(true);
        if (page == null) { log.Add("PlayingWithFriendsMenuPage missing"); return; }

        var createButton = FindButton(page.transform, "Btn Creat") ?? FindButton(page.transform, "Create Game Btn");

        var so = new SerializedObject(page);
        so.FindProperty("createGameButton").objectReferenceValue = createButton;
        so.ApplyModifiedPropertiesWithoutUndo();

        log.Add($"friends page: create button '{(createButton != null ? createButton.name : "NOT FOUND")}'");
    }

    private static int ReadEntryCost(Transform card)
    {
        var payment = FindTransform(card, "PayMent");
        if (payment == null) return 0;

        foreach (var text in payment.GetComponentsInChildren<TMP_Text>(true))
        {
            int cost;
            if (text.text != null && int.TryParse(text.text.Trim(), out cost)) return cost;
        }

        return 0;
    }

    #region helpers

    private static Transform FindTransform(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindTransform(root.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }

    /// <summary>First object with that name that actually carries a Button component.</summary>
    private static Button FindButton(Transform root, string name)
    {
        if (root == null) return null;

        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
            if (button.name == name) return button;

        return null;
    }

    private static string Path(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static TMP_FontAsset LoadFontAsset(string assetName)
    {
        foreach (string guid in AssetDatabase.FindAssets($"{assetName} t:TMP_FontAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null && font.name == assetName) return font;
        }

        return null;
    }

    #endregion
}

