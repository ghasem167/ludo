using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixProfilePage
{
    private static StringBuilder _log;

    [MenuItem("Tools/Fix Profile Page Layout")]
    public static void Fix()
    {
        _log = new StringBuilder();
        Undo.SetCurrentGroupName("Fix Profile Page Layout");
        int group = Undo.GetCurrentGroup();

        var mm = GameObject.Find("MainMenu");
        if (mm == null) { Debug.LogError("[FixProfile] MainMenu not found"); return; }

        Transform page = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Profile Page") { page = t; break; }
        if (page == null) { Debug.LogError("[FixProfile] Profile Page not found"); return; }

        // ---------- 0) Root page layout ----------
        var pageRect = (RectTransform)page;
        var rootVlg = GetOrAdd<VerticalLayoutGroup>(page.gameObject);
        Undo.RecordObject(pageRect, "fix");
        Undo.RecordObject(rootVlg, "fix");
        SetStretch(pageRect);
        rootVlg.padding = new RectOffset(24, 16, 24, 24);
        rootVlg.spacing = 12;
        rootVlg.childControlWidth = true;
        rootVlg.childControlHeight = true;
        rootVlg.childForceExpandWidth = true;
        rootVlg.childForceExpandHeight = false;
        rootVlg.childAlignment = TextAnchor.UpperLeft;

        // ---------- Identify the 5 root sections ----------
        if (page.childCount < 5) { Debug.LogError($"[FixProfile] expected 5 sections, got {page.childCount}"); return; }
        Transform card = page.GetChild(0);
        Transform stats = page.GetChild(1);
        Transform points = page.GetChild(2);
        Transform carts = page.GetChild(3);
        Transform footer = page.GetChild(4);

        // flexible heights (min / preferred / flexible)
        SetSection(card, "Profile Card", 280, 470, 3f);
        SetSection(stats, "Stats Row", 60, 100, 1f);
        SetSection(points, "Points Row", 100, 160, 1.5f);
        SetSection(carts, "Customization Row", 170, 300, 2.5f);
        SetSection(footer, "Footer Row", 60, 90, 0.5f);

        // ---------- 1) Profile card ----------
        var cardLayout = card.GetChild(0); // Group 1
        Rename(cardLayout, "Card Layout");
        var hlgCard = GetOrAdd<HorizontalLayoutGroup>(cardLayout.gameObject);
        Undo.RecordObject(hlgCard, "fix");
        hlgCard.padding = new RectOffset(20, 16, 20, 16);
        hlgCard.spacing = 16;
        hlgCard.childControlWidth = true;
        hlgCard.childControlHeight = true;
        hlgCard.childForceExpandWidth = false;
        hlgCard.childForceExpandHeight = true;
        hlgCard.childAlignment = TextAnchor.MiddleCenter;

        Transform avatar = cardLayout.GetChild(0);
        Rename(avatar, "Avatar");
        var leAvatar = GetOrAdd<LayoutElement>(avatar.gameObject);
        Undo.RecordObject(leAvatar, "fix");
        leAvatar.preferredWidth = 280; leAvatar.flexibleWidth = 0;
        leAvatar.minWidth = -1; leAvatar.minHeight = -1; leAvatar.preferredHeight = -1; leAvatar.flexibleHeight = -1;
        SetPreserve(avatar, true);

        Transform infoCol = cardLayout.GetChild(1);
        Rename(infoCol, "Info Column");
        SetStretch((RectTransform)infoCol);
        var leInfo = GetOrAdd<LayoutElement>(infoCol.gameObject);
        Undo.RecordObject(leInfo, "fix");
        leInfo.flexibleWidth = 1; leInfo.flexibleHeight = -1; leInfo.preferredWidth = -1; leInfo.preferredHeight = -1; leInfo.minWidth = -1; leInfo.minHeight = -1;
        var vlgInfo = GetOrAdd<VerticalLayoutGroup>(infoCol.gameObject);
        Undo.RecordObject(vlgInfo, "fix");
        vlgInfo.padding = new RectOffset(0, 0, 0, 0);
        vlgInfo.spacing = 4;
        vlgInfo.childControlWidth = true;
        vlgInfo.childControlHeight = true;
        vlgInfo.childForceExpandWidth = true;
        vlgInfo.childForceExpandHeight = false;
        vlgInfo.childAlignment = TextAnchor.MiddleCenter;

        // Name row
        Transform nameRow = infoCol.GetChild(0);
        Rename(nameRow, "Name Row");
        SetStretch((RectTransform)nameRow);
        var leName = GetOrAdd<LayoutElement>(nameRow.gameObject);
        Undo.RecordObject(leName, "fix");
        leName.flexibleHeight = 1.2f; leName.minHeight = 70; leName.flexibleWidth = -1; leName.preferredHeight = -1;
        var nameInner = nameRow.GetChild(0);
        var hlgName = GetOrAdd<HorizontalLayoutGroup>(nameInner.gameObject);
        Undo.RecordObject(hlgName, "fix");
        hlgName.padding = new RectOffset(0, 0, 0, 0);
        hlgName.spacing = 8;
        hlgName.childControlWidth = true; hlgName.childControlHeight = true;
        hlgName.childForceExpandWidth = false; hlgName.childForceExpandHeight = true;
        hlgName.childAlignment = TextAnchor.MiddleCenter;
        SetStretch((RectTransform)nameInner);
        var nameText = nameInner.GetChild(0);
        var leNameText = GetOrAdd<LayoutElement>(nameText.gameObject);
        Undo.RecordObject(leNameText, "fix");
        leNameText.flexibleWidth = 1; leNameText.flexibleHeight = -1;
        var nameTmp = nameText.GetComponent<TextMeshProUGUI>();
        if (nameTmp != null) { Undo.RecordObject(nameTmp, "fix"); nameTmp.alignment = TextAlignmentOptions.Left; nameTmp.enableAutoSizing = true; nameTmp.fontSizeMin = 24; nameTmp.fontSizeMax = 42; }
        Transform editCol = nameInner.GetChild(1);
        var leEdit = GetOrAdd<LayoutElement>(editCol.gameObject);
        Undo.RecordObject(leEdit, "fix");
        leEdit.preferredWidth = 90; leEdit.flexibleWidth = 0; leEdit.flexibleHeight = -1;
        SetPreserve(editCol, true);

        // Rank row
        Transform rankRow = infoCol.GetChild(1);
        Rename(rankRow, "Rank Row");
        SetStretch((RectTransform)rankRow);
        var leRank = GetOrAdd<LayoutElement>(rankRow.gameObject);
        Undo.RecordObject(leRank, "fix");
        leRank.flexibleHeight = 1.2f; leRank.minHeight = 70; leRank.flexibleWidth = -1; leRank.preferredHeight = -1;
        var rankInner = rankRow.GetChild(0);
        var hlgRank = GetOrAdd<HorizontalLayoutGroup>(rankInner.gameObject);
        Undo.RecordObject(hlgRank, "fix");
        hlgRank.padding = new RectOffset(0, 0, 0, 0);
        hlgRank.spacing = 8;
        hlgRank.childControlWidth = true; hlgRank.childControlHeight = true;
        hlgRank.childForceExpandWidth = false; hlgRank.childForceExpandHeight = true;
        hlgRank.childAlignment = TextAnchor.MiddleCenter;
        var rankBadge = rankInner.GetChild(0);
        var leBadge = GetOrAdd<LayoutElement>(rankBadge.gameObject);
        Undo.RecordObject(leBadge, "fix");
        leBadge.preferredWidth = 90; leBadge.flexibleWidth = 0; leBadge.flexibleHeight = -1;
        SetPreserve(rankBadge, true);
        var rankText = rankInner.GetChild(1);
        var leRankText = GetOrAdd<LayoutElement>(rankText.gameObject);
        Undo.RecordObject(leRankText, "fix");
        leRankText.flexibleWidth = 1; leRankText.flexibleHeight = -1;
        var rankTmp = rankText.GetComponent<TextMeshProUGUI>();
        if (rankTmp != null) { Undo.RecordObject(rankTmp, "fix"); rankTmp.text = "Master"; rankTmp.alignment = TextAlignmentOptions.Left; rankTmp.enableAutoSizing = true; rankTmp.fontSizeMin = 22; rankTmp.fontSizeMax = 36; }

        // XP row
        Transform xpRow = infoCol.GetChild(2);
        Rename(xpRow, "XP Row");
        SetStretch((RectTransform)xpRow);
        var leXp = GetOrAdd<LayoutElement>(xpRow.gameObject);
        Undo.RecordObject(leXp, "fix");
        leXp.flexibleHeight = 1f; leXp.minHeight = 46; leXp.flexibleWidth = -1; leXp.preferredHeight = -1;
        var xpInner = xpRow.GetChild(0); // Loaing Box
        var hlgXp = GetOrAdd<HorizontalLayoutGroup>(xpInner.gameObject);
        Undo.RecordObject(hlgXp, "fix");
        hlgXp.padding = new RectOffset(0, 0, 0, 0);
        hlgXp.spacing = 8;
        hlgXp.childControlWidth = true; hlgXp.childControlHeight = true;
        hlgXp.childForceExpandWidth = false; hlgXp.childForceExpandHeight = false;
        hlgXp.childAlignment = TextAnchor.MiddleCenter;
        var xpLabel = xpInner.GetChild(0);
        var leXpLabel = GetOrAdd<LayoutElement>(xpLabel.gameObject);
        Undo.RecordObject(leXpLabel, "fix");
        leXpLabel.preferredWidth = 60; leXpLabel.flexibleWidth = 0; leXpLabel.flexibleHeight = -1;
        var xpLabelTmp = xpLabel.GetComponent<TextMeshProUGUI>();
        if (xpLabelTmp != null) { Undo.RecordObject(xpLabelTmp, "fix"); xpLabelTmp.text = "XP"; xpLabelTmp.alignment = TextAlignmentOptions.MidlineRight; xpLabelTmp.enableAutoSizing = true; xpLabelTmp.fontSizeMin = 16; xpLabelTmp.fontSizeMax = 24; }
        Transform xpBar = xpInner.GetChild(1);
        var leXpBar = GetOrAdd<LayoutElement>(xpBar.gameObject);
        Undo.RecordObject(leXpBar, "fix");
        leXpBar.flexibleWidth = 1; leXpBar.preferredHeight = 40; leXpBar.minHeight = 40; leXpBar.flexibleHeight = -1;
        Transform barBg = xpBar.GetChild(0);
        Transform barFill = xpBar.GetChild(1);
        SetStretch((RectTransform)barBg);
        SetStretch((RectTransform)barFill);
        var fillImg = barFill.GetComponent<Image>();
        if (fillImg != null)
        {
            Undo.RecordObject(fillImg, "fix");
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0.56f;
            fillImg.preserveAspect = false;
        }
        var xpValue = xpInner.GetChild(2);
        var leXpValue = GetOrAdd<LayoutElement>(xpValue.gameObject);
        Undo.RecordObject(leXpValue, "fix");
        leXpValue.preferredWidth = 150; leXpValue.flexibleWidth = 0; leXpValue.flexibleHeight = -1;
        var xpValueTmp = xpValue.GetComponent<TextMeshProUGUI>();
        if (xpValueTmp != null) { Undo.RecordObject(xpValueTmp, "fix"); xpValueTmp.text = "2,800 / 5,000"; xpValueTmp.alignment = TextAlignmentOptions.MidlineLeft; xpValueTmp.enableAutoSizing = true; xpValueTmp.fontSizeMin = 16; xpValueTmp.fontSizeMax = 24; }

        // ---------- 2) Stats row ----------
        var statsInner = stats.GetChild(0); // Game Data
        var hlgStats = GetOrAdd<HorizontalLayoutGroup>(statsInner.gameObject);
        Undo.RecordObject(hlgStats, "fix");
        hlgStats.padding = new RectOffset(8, 8, 8, 8);
        hlgStats.spacing = 0;
        hlgStats.childControlWidth = true; hlgStats.childControlHeight = true;
        hlgStats.childForceExpandWidth = true; hlgStats.childForceExpandHeight = true;
        hlgStats.childAlignment = TextAnchor.MiddleCenter;
        string[] statLabels = { "Win Rate", "Games Won", "Total Games" };
        string[] statValues = { "64%", "160", "250" };
        for (int i = 0; i < 3; i++)
        {
            Transform data = statsInner.GetChild(i);
            var leData = GetOrAdd<LayoutElement>(data.gameObject);
            Undo.RecordObject(leData, "fix");
            leData.flexibleWidth = 1; leData.flexibleHeight = -1; leData.minWidth = -1; leData.minHeight = -1; leData.preferredWidth = -1; leData.preferredHeight = -1;
            var vlgData = GetOrAdd<VerticalLayoutGroup>(data.gameObject);
            Undo.RecordObject(vlgData, "fix");
            vlgData.padding = new RectOffset(0, 0, 0, 0);
            vlgData.spacing = 2;
            vlgData.childControlWidth = true; vlgData.childControlHeight = true;
            vlgData.childForceExpandWidth = true; vlgData.childForceExpandHeight = false;
            vlgData.childAlignment = TextAnchor.MiddleCenter;
            int labelIdx = 0, valueIdx = 1;
            if (data.childCount == 4) { labelIdx = 1; valueIdx = 2; } // Data 2 has side lines
            for (int c = 0; c < data.childCount; c++)
            {
                Transform ch = data.GetChild(c);
                bool isLine = ch.name.Contains("Line");
                if (isLine)
                {
                    var leLine = GetOrAdd<LayoutElement>(ch.gameObject);
                    Undo.RecordObject(leLine, "fix");
                    leLine.ignoreLayout = true;
                    continue;
                }
                bool isValue = c == valueIdx;
                var le = GetOrAdd<LayoutElement>(ch.gameObject);
                Undo.RecordObject(le, "fix");
                le.flexibleHeight = isValue ? 1.4f : 1f;
                le.minHeight = -1; le.flexibleWidth = -1; le.preferredHeight = -1;
                SetStretch((RectTransform)ch);
                var tmp = ch.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    Undo.RecordObject(tmp, "fix");
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableAutoSizing = true;
                    if (isValue) { tmp.fontSizeMin = 30; tmp.fontSizeMax = 60; tmp.text = statValues[i]; }
                    else { tmp.fontSizeMin = 18; tmp.fontSizeMax = 30; tmp.text = statLabels[i]; }
                }
            }
        }

        // ---------- 3) Points row (trophy + rank cards) ----------
        var pointsLayout = points.GetChild(0); // Layout
        Rename(pointsLayout, "Cards Layout");
        var hlgPoints = GetOrAdd<HorizontalLayoutGroup>(pointsLayout.gameObject);
        Undo.RecordObject(hlgPoints, "fix");
        hlgPoints.padding = new RectOffset(8, 8, 8, 8);
        hlgPoints.spacing = 16;
        hlgPoints.childControlWidth = true; hlgPoints.childControlHeight = true;
        hlgPoints.childForceExpandWidth = true; hlgPoints.childForceExpandHeight = true;
        hlgPoints.childAlignment = TextAnchor.MiddleCenter;

        FixPointCard(pointsLayout.GetChild(0), "Points Card", "Current Points", "1500", "Weekly: 524", null);
        FixPointCard(pointsLayout.GetChild(1), "Rank Card", "Level", "Master", null, new[] { "Level ", "5" });

        // ---------- 4) Customization carts ----------
        var cartsLayout = carts.GetChild(0); // Carts Group
        var hlgCarts = GetOrAdd<HorizontalLayoutGroup>(cartsLayout.gameObject);
        Undo.RecordObject(hlgCarts, "fix");
        hlgCarts.padding = new RectOffset(8, 4, 8, 4);
        hlgCarts.spacing = 20;
        hlgCarts.childControlWidth = true; hlgCarts.childControlHeight = true;
        hlgCarts.childForceExpandWidth = true; hlgCarts.childForceExpandHeight = true;
        hlgCarts.childAlignment = TextAnchor.MiddleCenter;
        string[] cartLabels = { "Avatar", "Dice", "Badge" };
        for (int i = 0; i < 3 && i < cartsLayout.childCount; i++)
        {
            Transform cart = cartsLayout.GetChild(i);
            var leCart = GetOrAdd<LayoutElement>(cart.gameObject);
            Undo.RecordObject(leCart, "fix");
            leCart.flexibleWidth = 1; leCart.flexibleHeight = -1; leCart.minWidth = -1; leCart.minHeight = -1; leCart.preferredWidth = -1; leCart.preferredHeight = -1;
            var vlgCart = GetOrAdd<VerticalLayoutGroup>(cart.gameObject);
            Undo.RecordObject(vlgCart, "fix");
            vlgCart.padding = new RectOffset(10, 10, 10, 10);
            vlgCart.spacing = 4;
            vlgCart.childControlWidth = true; vlgCart.childControlHeight = true;
            vlgCart.childForceExpandWidth = true; vlgCart.childForceExpandHeight = false;
            vlgCart.childAlignment = TextAnchor.MiddleCenter;

            for (int c = 0; c < cart.childCount; c++)
            {
                Transform ch = cart.GetChild(c);
                if (ch.name.StartsWith("Text"))
                {
                    var le = GetOrAdd<LayoutElement>(ch.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.preferredHeight = 34; le.flexibleHeight = 0; le.flexibleWidth = -1; le.minHeight = -1;
                    SetStretch((RectTransform)ch);
                    var tmp = ch.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) { Undo.RecordObject(tmp, "fix"); tmp.text = cartLabels[i]; tmp.alignment = TextAlignmentOptions.Center; tmp.enableAutoSizing = true; tmp.fontSizeMin = 18; tmp.fontSizeMax = 28; }
                }
                else if (ch.name == "Image")
                {
                    var le = GetOrAdd<LayoutElement>(ch.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.flexibleHeight = 1; le.flexibleWidth = -1; le.minHeight = -1; le.preferredHeight = -1;
                    SetStretch((RectTransform)ch);
                    SetPreserve(ch, true);
                    // nested image (avatar overlay etc.)
                    if (ch.childCount > 0) SetPreserve(ch.GetChild(0), true);
                }
                else if (ch.name == "Btn Change")
                {
                    var le = GetOrAdd<LayoutElement>(ch.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.preferredHeight = 56; le.flexibleHeight = 0; le.flexibleWidth = -1; le.minHeight = -1;
                    SetStretch((RectTransform)ch);
                    if (ch.childCount > 0)
                    {
                        Transform btnText = ch.GetChild(0);
                        SetStretch((RectTransform)btnText);
                        var tmp = btnText.GetComponent<TextMeshProUGUI>();
                        if (tmp != null) { Undo.RecordObject(tmp, "fix"); tmp.text = "Edit"; tmp.alignment = TextAlignmentOptions.Center; tmp.enableAutoSizing = true; tmp.fontSizeMin = 16; tmp.fontSizeMax = 26; }
                    }
                }
            }
        }

        // ---------- 5) Footer ----------
        Transform btnCustom = footer.GetChild(0);
        var btnRect = (RectTransform)btnCustom;
        Undo.RecordObject(btnRect, "fix");
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 1f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(480, -16);
        btnRect.anchoredPosition = Vector2.zero;
        if (btnCustom.childCount > 0)
        {
            Transform btnText = btnCustom.GetChild(0);
            SetStretch((RectTransform)btnText);
            var tmp = btnText.GetComponent<TextMeshProUGUI>();
            if (tmp != null) { Undo.RecordObject(tmp, "fix"); tmp.text = "Build Profile"; tmp.alignment = TextAlignmentOptions.Center; tmp.enableAutoSizing = true; tmp.fontSizeMin = 26; tmp.fontSizeMax = 48; }
        }

        // ---------- finalize ----------
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
        EditorUtility.SetDirty(page.gameObject);
        EditorSceneManager.MarkSceneDirty(page.gameObject.scene);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixProfile] DONE\n" + _log);
        File.WriteAllText("Temp/fix_profile_log.txt", _log.ToString());
    }

    private static void FixPointCard(Transform card, string newName, string label, string value, string sub, string[] miniRow)
    {
        Rename(card, newName);
        var leCard = GetOrAdd<LayoutElement>(card.gameObject);
        Undo.RecordObject(leCard, "fix");
        leCard.flexibleWidth = 1; leCard.flexibleHeight = -1; leCard.minWidth = -1; leCard.minHeight = -1; leCard.preferredWidth = -1; leCard.preferredHeight = -1;
        SetPreserve(card, false);

        Transform inner = card.GetChild(0); // GameObject (2)
        var hlg = GetOrAdd<HorizontalLayoutGroup>(inner.gameObject);
        Undo.RecordObject(hlg, "fix");
        hlg.padding = new RectOffset(10, 8, 10, 8);
        hlg.spacing = 12;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        // image cell
        Transform imgCell = inner.GetChild(0);
        var leCell = GetOrAdd<LayoutElement>(imgCell.gameObject);
        Undo.RecordObject(leCell, "fix");
        leCell.preferredWidth = 110; leCell.flexibleWidth = 0; leCell.flexibleHeight = -1;
        if (imgCell.childCount > 0)
        {
            Transform img = imgCell.GetChild(0);
            var imgRect = (RectTransform)img;
            Undo.RecordObject(imgRect, "fix");
            imgRect.anchorMin = new Vector2(0.5f, 0.5f);
            imgRect.anchorMax = new Vector2(0.5f, 0.5f);
            imgRect.sizeDelta = new Vector2(100, 110);
            imgRect.anchoredPosition = Vector2.zero;
            SetPreserve(img, true);
        }

        // text column
        Transform col = inner.GetChild(1);
        SetStretch((RectTransform)col);
        var leCol = GetOrAdd<LayoutElement>(col.gameObject);
        Undo.RecordObject(leCol, "fix");
        leCol.flexibleWidth = 1; leCol.flexibleHeight = -1;
        var vlgCol = GetOrAdd<VerticalLayoutGroup>(col.gameObject);
        Undo.RecordObject(vlgCol, "fix");
        vlgCol.padding = new RectOffset(0, 0, 0, 0);
        vlgCol.spacing = 2;
        vlgCol.childControlWidth = true; vlgCol.childControlHeight = true;
        vlgCol.childForceExpandWidth = true; vlgCol.childForceExpandHeight = false;
        vlgCol.childAlignment = TextAnchor.MiddleCenter;

        int slot = 0;
        ProcessColStack(col, label, value, sub, miniRow, ref slot);
    }

    // Recursively processes a text column. Handles three child kinds:
    //  - HorizontalLayoutGroup  -> mini row (e.g. "Level 5")
    //  - VerticalLayoutGroup    -> nested stack wrapper found in some cards; configure & recurse
    //  - TextMeshProUGUI        -> slot 0 = label, slot 1 = value, slot 2+ = sub lines
    private static void ProcessColStack(Transform node, string label, string value, string sub, string[] miniRow, ref int slot)
    {
        for (int c = 0; c < node.childCount; c++)
        {
            Transform ch = node.GetChild(c);
            var hlg = ch.GetComponent<HorizontalLayoutGroup>();
            var vlg = ch.GetComponent<VerticalLayoutGroup>();
            var tmp = ch.GetComponent<TextMeshProUGUI>();
            if (hlg != null)
            {
                var le = GetOrAdd<LayoutElement>(ch.gameObject);
                Undo.RecordObject(le, "fix");
                le.flexibleHeight = 1f; le.minHeight = 22;
                le.flexibleWidth = -1; le.minWidth = -1; le.preferredHeight = -1; le.preferredWidth = -1;
                SetStretch((RectTransform)ch);
                Undo.RecordObject(hlg, "fix");
                hlg.padding = new RectOffset(0, 0, 0, 0);
                hlg.spacing = 6;
                hlg.childControlWidth = true; hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
                hlg.childAlignment = TextAnchor.MiddleCenter;
                if (miniRow != null && ch.childCount >= 2)
                {
                    var t0 = ch.GetChild(0).GetComponent<TextMeshProUGUI>();
                    var t1 = ch.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (t0 != null) { Undo.RecordObject(t0, "fix"); t0.text = miniRow[0]; t0.alignment = TextAlignmentOptions.MidlineRight; t0.enableAutoSizing = true; t0.fontSizeMin = 18; t0.fontSizeMax = 28; }
                    if (t1 != null) { Undo.RecordObject(t1, "fix"); t1.text = miniRow[1]; t1.alignment = TextAlignmentOptions.MidlineLeft; t1.enableAutoSizing = true; t1.fontSizeMin = 18; t1.fontSizeMax = 28; }
                }
            }
            else if (vlg != null)
            {
                var le = GetOrAdd<LayoutElement>(ch.gameObject);
                Undo.RecordObject(le, "fix");
                le.flexibleHeight = 1f; le.minHeight = -1;
                le.flexibleWidth = -1; le.minWidth = -1; le.preferredHeight = -1; le.preferredWidth = -1;
                SetStretch((RectTransform)ch);
                Undo.RecordObject(vlg, "fix");
                vlg.padding = new RectOffset(0, 0, 0, 0);
                vlg.spacing = 2;
                vlg.childControlWidth = true; vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
                vlg.childAlignment = TextAnchor.MiddleCenter;
                ProcessColStack(ch, label, value, sub, miniRow, ref slot);
            }
            else if (tmp != null)
            {
                var le = GetOrAdd<LayoutElement>(ch.gameObject);
                Undo.RecordObject(le, "fix");
                SetStretch((RectTransform)ch);
                Undo.RecordObject(tmp, "fix");
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableAutoSizing = true;
                switch (slot)
                {
                    case 0:
                        le.flexibleHeight = 1f; le.minHeight = 20; tmp.text = label;
                        tmp.fontSizeMin = 16; tmp.fontSizeMax = 24; break;
                    case 1:
                        le.flexibleHeight = 2f; le.minHeight = 36; tmp.text = value;
                        tmp.fontSizeMin = 34; tmp.fontSizeMax = 58; break;
                    default:
                        le.flexibleHeight = 1f; le.minHeight = 18;
                        if (!string.IsNullOrEmpty(sub)) tmp.text = sub;
                        tmp.fontSizeMin = 15; tmp.fontSizeMax = 22; break;
                }
                slot++;
            }
        }
    }

    // ---------- helpers ----------
    private static void Rename(Transform t, string newName)
    {
        if (t.name == newName) return;
        Undo.RecordObject(t.gameObject, "fix");
        t.name = newName;
        _log.AppendLine($"renamed -> {newName}");
    }

    private static void SetStretch(RectTransform rt)
    {
        Undo.RecordObject(rt, "fix");
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private static void SetSection(Transform section, string newName, float minH, float prefH, float flexH)
    {
        Rename(section, newName);
        var rect = (RectTransform)section;
        Undo.RecordObject(rect, "fix");
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        var le = GetOrAdd<LayoutElement>(section.gameObject);
        Undo.RecordObject(le, "fix");
        le.minHeight = minH;
        le.preferredHeight = prefH;
        le.flexibleHeight = flexH;
        le.minWidth = -1; le.preferredWidth = -1; le.flexibleWidth = -1;
        le.ignoreLayout = false;
    }

    private static void SetPreserve(Transform t, bool value)
    {
        var img = t.GetComponent<Image>();
        if (img != null && img.preserveAspect != value) { Undo.RecordObject(img, "fix"); img.preserveAspect = value; }
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(go);
        return c;
    }
}
