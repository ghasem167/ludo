using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixFriendsPage
{
    private static StringBuilder _log;
    private static TMP_FontAsset _fa;  // BRoyaBd SDF Dynamic (Persian, dynamic atlas)
    private static TMP_FontAsset _lib; // LiberationSans SDF (Latin names / fallback)

    [MenuItem("Tools/Fix Playing With Friends Page")]
    public static void Fix()
    {
        _log = new StringBuilder();
        Undo.SetCurrentGroupName("Fix Playing With Friends Page");
        int group = Undo.GetCurrentGroup();

        var mm = GameObject.Find("MainMenu");
        if (mm == null) { Debug.LogError("[FixFriends] MainMenu not found"); return; }

        // ---------- canvas back to overlay (in case capture helper left it in camera mode) ----------
        var canvas = mm.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Undo.RecordObject(canvas, "fix");
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            _log.AppendLine("canvas reverted to ScreenSpaceOverlay");
        }

        LoadFonts();

        Transform page = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Playing with Friends Page") { page = t; break; }
        if (page == null) { Debug.LogError("[FixFriends] Playing with Friends Page not found"); return; }

        // ---------- locate / flatten the 4 root sections ----------
        Transform card, header, rows, bottom;
        card = FindDirect(page, "Create Game Card");
        if (card != null)
        {
            _log.AppendLine("already flattened - reusing sections");
            header = FindDirect(page, "List Header");
            rows = FindDirect(page, "Friends Rows");
            bottom = FindDirect(page, "Bottom Text");
        }
        else
        {
            if (!Flatten(page, out card, out header, out rows, out bottom)) { Debug.LogError("[FixFriends] flatten failed - see log"); WriteLog(); return; }
        }
        if (card == null || header == null || rows == null || bottom == null) { Debug.LogError("[FixFriends] sections missing"); WriteLog(); return; }

        // destroy leftover intermediate wrappers (e.g. from an interrupted earlier run)
        for (int i = page.childCount - 1; i >= 0; i--)
        {
            var ch = page.GetChild(i);
            if (ch != card && ch != header && ch != rows && ch != bottom)
            {
                _log.AppendLine($"destroy leftover page child: {ch.name}");
                Undo.DestroyObjectImmediate(ch.gameObject);
            }
        }

        // ---------- 0) root page layout ----------
        var pageRect = (RectTransform)page;
        var rootVlg = GetOrAdd<VerticalLayoutGroup>(page.gameObject);
        Undo.RecordObject(pageRect, "fix"); Undo.RecordObject(rootVlg, "fix");
        SetStretch(pageRect);
        rootVlg.padding = new RectOffset(20, 16, 20, 24);
        rootVlg.spacing = 10;
        rootVlg.childControlWidth = true; rootVlg.childControlHeight = true;
        rootVlg.childForceExpandWidth = true; rootVlg.childForceExpandHeight = false;
        rootVlg.childAlignment = TextAnchor.UpperLeft;

        SetSection(card, "Create Game Card", 392, 450, 2f);
        SetSection(header, "List Header", 34, 40, 0f);
        SetSection(rows, "Friends Rows", 392, 420, 3f);
        SetSection(bottom, "Bottom Text", 52, 64, 1f);

        // ---------- 1) create-game card ----------
        var cardVlg = GetOrAdd<VerticalLayoutGroup>(card.gameObject);
        Undo.RecordObject(cardVlg, "fix");
        cardVlg.padding = new RectOffset(18, 16, 16, 10);
        cardVlg.spacing = 6;
        cardVlg.childControlWidth = true; cardVlg.childControlHeight = true;
        cardVlg.childForceExpandWidth = true; cardVlg.childForceExpandHeight = false;
        cardVlg.childAlignment = TextAnchor.UpperLeft;

        // 1a) mode tabs
        Transform selMode = FindDirect(card, "Select Mode");
        if (selMode == null) { _log.AppendLine("ERROR: Select Mode missing"); WriteLog(); return; }
        SetStretch((RectTransform)selMode);
        var leMode = GetOrAdd<LayoutElement>(selMode.gameObject);
        Undo.RecordObject(leMode, "fix");
        leMode.minHeight = 84; leMode.preferredHeight = -1; leMode.flexibleHeight = 1;
        leMode.minWidth = -1; leMode.preferredWidth = -1; leMode.flexibleWidth = -1;
        Transform layoutSel = FindDirect(selMode, "Layout Select") ?? selMode.GetChild(0);
        var hlgSel = GetOrAdd<HorizontalLayoutGroup>(layoutSel.gameObject);
        Undo.RecordObject(hlgSel, "fix");
        SetStretch((RectTransform)layoutSel);
        hlgSel.padding = new RectOffset(0, 0, 0, 0);
        hlgSel.spacing = 16;
        hlgSel.childControlWidth = true; hlgSel.childControlHeight = true;
        hlgSel.childForceExpandWidth = true; hlgSel.childForceExpandHeight = true;
        hlgSel.childAlignment = TextAnchor.MiddleCenter;
        string[] tabTexts = { "اشرافی", "حرفه\u200Cای", "مبتدی" };
        for (int i = 0; i < layoutSel.childCount && i < 3; i++)
        {
            Transform tab = layoutSel.GetChild(i);
            Rename(tab, $"Tab {i + 1} ({(i == 0 ? "Elite" : i == 1 ? "Pro" : "Beginner")})");
            var leTab = GetOrAdd<LayoutElement>(tab.gameObject);
            Undo.RecordObject(leTab, "fix");
            leTab.flexibleWidth = 1f; leTab.minWidth = 90; leTab.minHeight = 76;
            leTab.preferredWidth = -1; leTab.preferredHeight = -1; leTab.flexibleHeight = -1;
            var img = tab.GetComponent<Image>();
            if (img != null)
            {
                Undo.RecordObject(img, "fix");
                img.color = (i == 2) ? Color.white : new Color(0.55f, 0.62f, 0.70f, 0.55f); // rightmost = selected
            }
            var tmp = FindTmp(tab);
            if (tmp != null)
            {
                SetStretch((RectTransform)tmp.transform);
                Fa(tmp, tabTexts[i], "#FFFFFF", 18, 28, TextAlignmentOptions.Center);
            }
        }

        // 1b) fee banner (Entry right / Prize left like the mockup RTL layout)
        Transform feeRow = FindDirect(card, "Data Diamond");
        if (feeRow == null) { _log.AppendLine("ERROR: Data Diamond missing"); WriteLog(); return; }
        SetStretch((RectTransform)feeRow);
        var leFee = GetOrAdd<LayoutElement>(feeRow.gameObject);
        Undo.RecordObject(leFee, "fix");
        leFee.minHeight = 72; leFee.preferredHeight = -1; leFee.flexibleHeight = 1;
        leFee.minWidth = -1; leFee.preferredWidth = -1; leFee.flexibleWidth = -1;
        Transform feePanel = FindDirect(feeRow, "Data") ?? feeRow.GetChild(0);
        var hlgFee = GetOrAdd<HorizontalLayoutGroup>(feePanel.gameObject);
        Undo.RecordObject(hlgFee, "fix");
        SetStretch((RectTransform)feePanel);
        hlgFee.padding = new RectOffset(18, 4, 12, 4);
        hlgFee.spacing = 0;
        hlgFee.childControlWidth = true; hlgFee.childControlHeight = true;
        hlgFee.childForceExpandWidth = true; hlgFee.childForceExpandHeight = true;
        hlgFee.childAlignment = TextAnchor.MiddleCenter;
        string[] feeAmounts = { "250", "100" };      // left half, right half
        string[] feeLabels = { "جایزه", "ورودی" };  // left half, right half
        for (int h = 0; h < feePanel.childCount && h < 2; h++)
        {
            Transform half = feePanel.GetChild(h);
            Rename(half, h == 0 ? "Prize Half (Left)" : "Entry Half (Right)");
            SetStretch((RectTransform)half);
            var hlgHalf = GetOrAdd<HorizontalLayoutGroup>(half.gameObject);
            Undo.RecordObject(hlgHalf, "fix");
            hlgHalf.padding = new RectOffset(0, 0, 0, 0);
            hlgHalf.spacing = 10;
            hlgHalf.childControlWidth = true; hlgHalf.childControlHeight = true;
            hlgHalf.childForceExpandWidth = false; hlgHalf.childForceExpandHeight = true;
            hlgHalf.childAlignment = TextAnchor.MiddleCenter;
            for (int c = 0; c < half.childCount; c++)
            {
                Transform el = half.GetChild(c);
                var tmp = el.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    Rename(el, c == 1 ? "Fee Amount" : "Fee Label");
                    SetStretch((RectTransform)el);
                    var le = GetOrAdd<LayoutElement>(el.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.flexibleWidth = 0; le.flexibleHeight = -1; le.minWidth = -1; le.minHeight = -1;
                    le.preferredWidth = (c == 1) ? 70 : 100; le.preferredHeight = -1;
                    if (c == 1) Fa(tmp, feeAmounts[h], "#FFFFFF", 20, 30, TextAlignmentOptions.Center);
                    else Fa(tmp, feeLabels[h], "#FFFFFF", 15, 22, TextAlignmentOptions.Center);
                }
                else if (el.GetComponent<Image>() != null)
                {
                    Rename(el, "Fee Icon");
                    var le = GetOrAdd<LayoutElement>(el.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.preferredWidth = 30; le.preferredHeight = 30;
                    le.flexibleWidth = 0; le.flexibleHeight = -1; le.minWidth = -1; le.minHeight = -1;
                    SetPreserve(el, true);
                }
            }
        }

        // 1c) invite slots (filled slot on the RIGHT like the mockup)
        Transform slotsRow = FindDirect(card, "Invite List");
        if (slotsRow == null) { _log.AppendLine("ERROR: Invite List missing"); WriteLog(); return; }
        SetStretch((RectTransform)slotsRow);
        var leSlots = GetOrAdd<LayoutElement>(slotsRow.gameObject);
        Undo.RecordObject(leSlots, "fix");
        leSlots.minHeight = 76; leSlots.preferredHeight = -1; leSlots.flexibleHeight = 1;
        leSlots.minWidth = -1; leSlots.preferredWidth = -1; leSlots.flexibleWidth = -1;
        Transform slotsHlg = FindDirect(slotsRow, "Layout User") ?? slotsRow.GetChild(0);
        var hlgSlots = GetOrAdd<HorizontalLayoutGroup>(slotsHlg.gameObject);
        Undo.RecordObject(hlgSlots, "fix");
        SetStretch((RectTransform)slotsHlg);
        hlgSlots.padding = new RectOffset(0, 0, 0, 0);
        hlgSlots.spacing = 10;
        hlgSlots.childControlWidth = true; hlgSlots.childControlHeight = true;
        hlgSlots.childForceExpandWidth = true; hlgSlots.childForceExpandHeight = true;
        hlgSlots.childAlignment = TextAnchor.MiddleCenter;
        Transform filled = null;
        var boxList = new List<Transform>();
        for (int i = 0; i < slotsHlg.childCount; i++)
        {
            Transform box = slotsHlg.GetChild(i);
            boxList.Add(box);
            if (FindDescendant(box, "User Image") != null) filled = box;
        }
        foreach (var box in boxList)
        {
            bool isFilled = box == filled;
            var leBox = GetOrAdd<LayoutElement>(box.gameObject);
            Undo.RecordObject(leBox, "fix");
            leBox.flexibleWidth = isFilled ? 1.4f : 1f;
            leBox.minWidth = 110; leBox.minHeight = 68;
            leBox.preferredWidth = -1; leBox.preferredHeight = -1; leBox.flexibleHeight = -1;
            if (isFilled)
            {
                Rename(box, "Slot Filled (Right)");
                var img = box.GetComponent<Image>();
                if (img != null) { Undo.RecordObject(img, "fix"); img.color = new Color(0.40f, 1f, 0.55f, 1f); }
                var hlgBox = GetOrAdd<HorizontalLayoutGroup>(box.gameObject);
                Undo.RecordObject(hlgBox, "fix");
                hlgBox.padding = new RectOffset(10, 6, 8, 8);
                hlgBox.spacing = 10;
                hlgBox.childControlWidth = true; hlgBox.childControlHeight = true;
                hlgBox.childForceExpandWidth = false; hlgBox.childForceExpandHeight = true;
                hlgBox.childAlignment = TextAnchor.MiddleCenter;
                Transform userImg = FindDescendant(box, "User Image");
                if (userImg != null)
                {
                    Rename(userImg, "Slot Avatar");
                    SetStretch((RectTransform)userImg);
                    var leAv = GetOrAdd<LayoutElement>(userImg.gameObject);
                    Undo.RecordObject(leAv, "fix");
                    leAv.preferredWidth = 44; leAv.preferredHeight = 44;
                    leAv.flexibleWidth = 0; leAv.flexibleHeight = 0; leAv.minWidth = -1; leAv.minHeight = -1;
                    SetPreserve(userImg, true);
                    Transform avInner = userImg.childCount > 0 ? userImg.GetChild(0) : null;
                    if (avInner != null) { SetStretch((RectTransform)avInner); SetPreserve(avInner, true); }
                }
                var nameTmp = FindTmp(box);
                if (nameTmp != null)
                {
                    Rename(nameTmp.transform, "Slot Name");
                    SetStretch((RectTransform)nameTmp.transform);
                    var leT = GetOrAdd<LayoutElement>(nameTmp.gameObject);
                    Undo.RecordObject(leT, "fix");
                    leT.flexibleWidth = 1; leT.flexibleHeight = -1; leT.preferredWidth = -1; leT.minWidth = -1;
                    nameTmp.font = _lib; // latin nickname
                    nameTmp.enableAutoSizing = true; nameTmp.fontSizeMin = 14; nameTmp.fontSizeMax = 22;
                    nameTmp.alignment = TextAlignmentOptions.Center;
                }
            }
            else Rename(box, $"Slot Empty ({box.name})");
        }
        if (filled != null)
        {
            Undo.RecordObject(slotsHlg, "fix");
            filled.SetSiblingIndex(slotsHlg.childCount - 1); // rightmost in LTR = first in the mockup's RTL
        }

        // 1d) create-game button
        Transform btnRow = FindDirect(card, "Btn");
        if (btnRow == null) { _log.AppendLine("ERROR: create Btn row missing"); WriteLog(); return; }
        SetStretch((RectTransform)btnRow);
        var leBtn = GetOrAdd<LayoutElement>(btnRow.gameObject);
        Undo.RecordObject(leBtn, "fix");
        leBtn.minHeight = 72; leBtn.preferredHeight = -1; leBtn.flexibleHeight = 0.5f;
        leBtn.minWidth = -1; leBtn.preferredWidth = -1; leBtn.flexibleWidth = -1;
        var hlgBtnRow = GetOrAdd<HorizontalLayoutGroup>(btnRow.gameObject);
        Undo.RecordObject(hlgBtnRow, "fix");
        hlgBtnRow.padding = new RectOffset(0, 0, 0, 0);
        hlgBtnRow.spacing = 0;
        hlgBtnRow.childControlWidth = true; hlgBtnRow.childControlHeight = true;
        hlgBtnRow.childForceExpandWidth = false; hlgBtnRow.childForceExpandHeight = true;
        hlgBtnRow.childAlignment = TextAnchor.MiddleCenter;
        Transform btnCreat = FindDirect(btnRow, "Btn Creat") ?? btnRow.GetChild(0);
        var leCreat = GetOrAdd<LayoutElement>(btnCreat.gameObject);
        Undo.RecordObject(leCreat, "fix");
        leCreat.preferredWidth = 260; leCreat.preferredHeight = 62;
        leCreat.flexibleWidth = 0; leCreat.flexibleHeight = -1; leCreat.minWidth = -1; leCreat.minHeight = -1;
        var creatTmp = FindTmp(btnCreat);
        if (creatTmp != null)
        {
            SetStretch((RectTransform)creatTmp.transform);
            Fa(creatTmp, "ساخت بازی", "#EAFF00", 22, 32, TextAlignmentOptions.Center);
        }

        // 1e) card footer note (new object)
        Transform oldFooter = FindDirect(card, "Card Footer");
        Transform footer = oldFooter;
        if (footer == null)
        {
            var go = new GameObject("Card Footer", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, "fix");
            footer = go.transform;
            footer.SetParent(card, false);
        }
        var leFoot = GetOrAdd<LayoutElement>(footer.gameObject);
        Undo.RecordObject(leFoot, "fix");
        leFoot.minHeight = 34; leFoot.preferredHeight = -1; leFoot.flexibleHeight = 0;
        leFoot.minWidth = -1; leFoot.preferredWidth = -1; leFoot.flexibleWidth = -1;
        var footTmp = footer.GetComponent<TextMeshProUGUI>();
        Undo.RecordObject(footTmp, "fix");
        footTmp.raycastTarget = false;
        Fa(footTmp, "برای ساخت بازی حداقل 1 و حداکثر 3 بازیکن دعوت کنید", "#E6F2FF", 12, 18, TextAlignmentOptions.Center);

        // ---------- 2) list header ----------
        SetStretch((RectTransform)header);
        Transform lineText = FindDirect(header, "Line and Text") ?? (header.childCount > 0 ? header.GetChild(0) : null);
        if (lineText != null)
        {
            SetStretch((RectTransform)lineText);
            var hlgLt = GetOrAdd<HorizontalLayoutGroup>(lineText.gameObject);
            Undo.RecordObject(hlgLt, "fix");
            hlgLt.padding = new RectOffset(0, 0, 0, 0);
            hlgLt.spacing = 16;
            hlgLt.childControlWidth = true; hlgLt.childControlHeight = true;
            hlgLt.childForceExpandWidth = false; hlgLt.childForceExpandHeight = true;
            hlgLt.childAlignment = TextAnchor.MiddleCenter;
            for (int i = 0; i < lineText.childCount; i++)
            {
                Transform el = lineText.GetChild(i);
                var tmp = el.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    Rename(el, "List Title");
                    var le = GetOrAdd<LayoutElement>(el.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.preferredWidth = 240; le.preferredHeight = 34;
                    le.flexibleWidth = 0; le.flexibleHeight = -1; le.minWidth = -1; le.minHeight = -1;
                    Fa(tmp, "لیست دوستان", "#DFF4FF", 16, 24, TextAlignmentOptions.Center);
                }
                else
                {
                    Rename(el, i == 0 ? "Start Line" : "End Line");
                    SetStretch((RectTransform)el);
                    var le = GetOrAdd<LayoutElement>(el.gameObject);
                    Undo.RecordObject(le, "fix");
                    le.flexibleWidth = 1f; le.preferredHeight = 14; le.flexibleHeight = 0;
                    le.minWidth = -1; le.minHeight = -1; le.preferredWidth = -1;
                    var hlgLine = GetOrAdd<HorizontalLayoutGroup>(el.gameObject);
                    Undo.RecordObject(hlgLine, "fix");
                    hlgLine.padding = new RectOffset(0, 0, 0, 0);
                    hlgLine.spacing = 4;
                    hlgLine.childControlWidth = true; hlgLine.childControlHeight = true;
                    hlgLine.childForceExpandWidth = true; hlgLine.childForceExpandHeight = false;
                    hlgLine.childAlignment = TextAnchor.MiddleCenter;
                    for (int j = 0; j < el.childCount; j++)
                    {
                        Transform part = el.GetChild(j);
                        var leP = GetOrAdd<LayoutElement>(part.gameObject);
                        Undo.RecordObject(leP, "fix");
                        SetPreserve(part, true);
                        if (part.name.StartsWith("Line")) { leP.flexibleWidth = 1f; leP.preferredHeight = 10; leP.flexibleHeight = 0; }
                        else { leP.preferredWidth = 10; leP.preferredHeight = 10; leP.flexibleWidth = 0; leP.flexibleHeight = -1; }
                        leP.minWidth = -1; leP.minHeight = -1; leP.preferredWidth = leP.preferredWidth < 0 ? -1 : leP.preferredWidth;
                    }
                }
            }
        }

        // ---------- 3) friend rows ----------
        var rowsVlg = GetOrAdd<VerticalLayoutGroup>(rows.gameObject);
        Undo.RecordObject(rowsVlg, "fix");
        SetStretch((RectTransform)rows);
        rowsVlg.padding = new RectOffset(0, 0, 0, 0);
        rowsVlg.spacing = 8;
        rowsVlg.childControlWidth = true; rowsVlg.childControlHeight = true;
        rowsVlg.childForceExpandWidth = true; rowsVlg.childForceExpandHeight = false;
        rowsVlg.childAlignment = TextAnchor.UpperLeft;
        int rowIdx = 0;
        for (int i = 0; i < rows.childCount; i++)
        {
            Transform row = rows.GetChild(i);
            if (row.name.StartsWith("Friend") == false) { _log.AppendLine($"skip non-row child: {row.name}"); continue; }
            var leRow = GetOrAdd<LayoutElement>(row.gameObject);
            Undo.RecordObject(leRow, "fix");
            leRow.minHeight = 72; leRow.preferredHeight = 72; leRow.flexibleHeight = 1;
            leRow.minWidth = -1; leRow.preferredWidth = -1; leRow.flexibleWidth = -1;
            var rowImg = row.GetComponent<Image>();
            if (rowImg != null)
            {
                Undo.RecordObject(rowImg, "fix");
                rowImg.color = (rowIdx == 0) ? new Color(1f, 0.90f, 0.30f, 0.90f) : Color.white; // invited row = golden border
            }
            Transform inner = FindDirect(row, "Layout") ?? row.GetChild(0);
            var hlgInner = GetOrAdd<HorizontalLayoutGroup>(inner.gameObject);
            Undo.RecordObject(hlgInner, "fix");
            SetStretch((RectTransform)inner);
            hlgInner.padding = new RectOffset(10, 6, 6, 6);
            hlgInner.spacing = 0;
            hlgInner.childControlWidth = true; hlgInner.childControlHeight = true;
            hlgInner.childForceExpandWidth = true; hlgInner.childForceExpandHeight = true;
            hlgInner.childAlignment = TextAnchor.UpperLeft;

            bool invited = rowIdx == 0;

            // status column (left)
            Transform statusCol = FindDirect(inner, "Status");
            if (statusCol != null)
            {
                SetStretch((RectTransform)statusCol);
                var le = GetOrAdd<LayoutElement>(statusCol.gameObject);
                Undo.RecordObject(le, "fix");
                le.flexibleWidth = 1f; le.flexibleHeight = -1;
                le.minWidth = -1; le.minHeight = -1; le.preferredWidth = -1; le.preferredHeight = -1;
                Transform line = FindDirect(statusCol, "Line");
                if (line != null)
                {
                    var rt = (RectTransform)line;
                    Undo.RecordObject(rt, "fix");
                    rt.anchorMin = new Vector2(1f, 0.5f); rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(1.5f, 48);
                    rt.anchoredPosition = new Vector2(-4f, 0);
                }
                var tmp = FindTmp(statusCol);
                if (tmp != null)
                {
                    var rt = (RectTransform)tmp.transform;
                    Undo.RecordObject(rt, "fix");
                    rt.anchorMin = new Vector2(0f, 0.15f); rt.anchorMax = new Vector2(1f, 0.85f);
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    Fa(tmp, invited ? "آنلاین" : "آفلاین", invited ? "#1AFF59" : "#ADADAD", 13, 20, TextAlignmentOptions.Center);
                }
            }

            // name column (center)
            Transform nameCol = FindDirect(inner, "Name");
            if (nameCol != null)
            {
                SetStretch((RectTransform)nameCol);
                var le = GetOrAdd<LayoutElement>(nameCol.gameObject);
                Undo.RecordObject(le, "fix");
                le.flexibleWidth = 2.2f; le.flexibleHeight = -1;
                le.minWidth = -1; le.minHeight = -1; le.preferredWidth = -1; le.preferredHeight = -1;
                var hlgName = GetOrAdd<HorizontalLayoutGroup>(nameCol.gameObject);
                Undo.RecordObject(hlgName, "fix");
                hlgName.padding = new RectOffset(6, 6, 0, 0);
                hlgName.spacing = 10;
                hlgName.childControlWidth = true; hlgName.childControlHeight = true;
                hlgName.childForceExpandWidth = false; hlgName.childForceExpandHeight = true;
                hlgName.childAlignment = TextAnchor.MiddleCenter;
                for (int c = 0; c < nameCol.childCount; c++)
                {
                    Transform el = nameCol.GetChild(c);
                    if (el.name == "Icon")
                    {
                        var leIcon = GetOrAdd<LayoutElement>(el.gameObject);
                        Undo.RecordObject(leIcon, "fix");
                        leIcon.preferredWidth = 46; leIcon.preferredHeight = 46;
                        leIcon.flexibleWidth = 0; leIcon.flexibleHeight = -1; leIcon.minWidth = -1; leIcon.minHeight = -1;
                        SetPreserve(el, true);
                        if (el.childCount > 0) { SetStretch((RectTransform)el.GetChild(0)); SetPreserve(el.GetChild(0), true); }
                    }
                    else if (el.GetComponent<TextMeshProUGUI>() != null)
                    {
                        var tmp = el.GetComponent<TextMeshProUGUI>();
                        SetStretch((RectTransform)el);
                        var leT = GetOrAdd<LayoutElement>(el.gameObject);
                        Undo.RecordObject(leT, "fix");
                        leT.flexibleWidth = 1; leT.flexibleHeight = -1; leT.preferredWidth = -1; leT.minWidth = -1;
                        tmp.font = _lib;
                        tmp.enableAutoSizing = true; tmp.fontSizeMin = 14; tmp.fontSizeMax = 24;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = Color.white;
                    }
                }
            }

            // invite column (right)
            Transform btnCol = FindDirect(inner, "Btn");
            if (btnCol != null)
            {
                SetStretch((RectTransform)btnCol);
                var le = GetOrAdd<LayoutElement>(btnCol.gameObject);
                Undo.RecordObject(le, "fix");
                le.preferredWidth = 118; le.flexibleWidth = 0; le.flexibleHeight = -1;
                le.minWidth = -1; le.minHeight = -1; le.preferredHeight = -1;
                Transform line = FindDirect(btnCol, "Line");
                if (line != null)
                {
                    var rt = (RectTransform)line;
                    Undo.RecordObject(rt, "fix");
                    rt.anchorMin = new Vector2(0f, 0.5f); rt.anchorMax = new Vector2(0f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(1.5f, 48);
                    rt.anchoredPosition = new Vector2(4f, 0);
                }
                Transform invitedTmp = FindDirect(btnCol, "Text");
                Transform inviteTmp = FindDirect(btnCol, "Btn Text");
                if (invitedTmp != null)
                {
                    var go = invitedTmp.gameObject;
                    Undo.RecordObject(go, "fix");
                    go.SetActive(invited);
                    var rt = (RectTransform)invitedTmp;
                    Undo.RecordObject(rt, "fix");
                    rt.anchorMin = new Vector2(0f, 0.2f); rt.anchorMax = new Vector2(1f, 0.8f);
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    Fa(invitedTmp.GetComponent<TextMeshProUGUI>(), "دعوت شده", "#FFEA00", 13, 21, TextAlignmentOptions.Center);
                }
                if (inviteTmp != null)
                {
                    var go = inviteTmp.gameObject;
                    Undo.RecordObject(go, "fix");
                    go.SetActive(!invited);
                    var rt = (RectTransform)inviteTmp;
                    Undo.RecordObject(rt, "fix");
                    rt.anchorMin = new Vector2(0f, 0.2f); rt.anchorMax = new Vector2(1f, 0.8f);
                    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                    Fa(inviteTmp.GetComponent<TextMeshProUGUI>(), "دعوت", "#FFFFFF", 13, 21, TextAlignmentOptions.Center);
                }
            }
            rowIdx++;
        }
        _log.AppendLine($"processed {rowIdx} friend rows");

        // ---------- 4) bottom text ----------
        var bottomTmp = FindTmp(bottom);
        if (bottomTmp != null)
        {
            SetStretch((RectTransform)bottomTmp.transform);
            FaRaw(bottomTmp,
                ShapeLines("بعد از ساخت بازی، دعوتنامه در قسمت پیام‌ها",
                           "برای دوستان شما قابل مشاهده است و می‌توانند وارد بازی شوند"),
                "#E8F4FF", 13, 20, TextAlignmentOptions.Center);
        }

        // ---------- 5) shared header title ----------
        Transform titleNode = null;
        foreach (var t in mm.GetComponentsInChildren<Transform>(true))
            if (t.name == "Title" && t.parent != null && t.parent.name == "Title&BackButton") { titleNode = t; break; }
        if (titleNode != null)
        {
            var rt = (RectTransform)titleNode;
            Undo.RecordObject(rt, "fix");
            rt.sizeDelta = new Vector2(250, 53);
            var tmp = FindTmp(titleNode);
            if (tmp != null) Fa(tmp, "بازی با دوستان", "#FFFFFF", 18, 28, TextAlignmentOptions.Center);
            _log.AppendLine("header title set to 'بازی با دوستان'");
        }

        // ---------- 6) cleanup stray persian texts from the font test ----------
        var pageTmps = page.GetComponentsInChildren<TextMeshProUGUI>(true);
        var keep = new HashSet<Object>();
        foreach (var t in pageTmps) if (t.font == _fa) keep.Add(t);
        if (titleNode != null) { var t = FindTmp(titleNode); if (t != null) keep.Add(t); }
        foreach (var t in mm.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.font == _fa && !keep.Contains(t))
            {
                Undo.RecordObject(t, "fix");
                _log.AppendLine($"revert stray persian tmp: {t.transform.name} (parent {t.transform.parent.name})");
                t.font = _lib;
                t.text = "xxxxxxxxxx";
            }
        }

        // ---------- 7) responsive fitter (uniform compression on short viewports) ----------
        var oldFitter = page.GetComponent<ResponsivePageFitter>();
        if (oldFitter != null) Undo.DestroyObjectImmediate(oldFitter);
        Undo.AddComponent<ResponsivePageFitter>(page.gameObject);
        _log.AppendLine("responsive fitter attached");

        // ---------- finish ----------
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
        Undo.CollapseUndoOperations(group);
        var scene = page.gameObject.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        _log.AppendLine($"scene saved: {saved}");
        WriteLog();
        Debug.Log($"[FixFriends] done. rows={rowIdx}. log -> Temp/friends_fix_log.txt");
    }

    private static bool Flatten(Transform page, out Transform card, out Transform header, out Transform rows, out Transform bottom)
    {
        card = header = rows = bottom = null;
        Transform wrapper = null;
        for (int i = 0; i < page.childCount; i++)
        {
            var ch = page.GetChild(i);
            if (ch.name == "Group 3") bottom = ch; else wrapper = ch;
        }
        if (wrapper == null || bottom == null) { _log.AppendLine($"flatten: expected wrapper+bottom, page children = {ListNames(page)}"); return false; }

        card = FindDescendant(wrapper, "Creat Game Group");
        rows = FindDescendant(wrapper, "Friends List");
        if (card == null || rows == null) { _log.AppendLine($"flatten: card/rows not found under {wrapper.name}"); return false; }

        var cardArea = card.parent;            // Group 1 (card area)
        var outerLayout = cardArea.parent;     // Layout (pad 35,20,35,0)
        Transform friendsWrap = null;
        for (int i = 0; i < outerLayout.childCount; i++)
            if (outerLayout.GetChild(i) != cardArea) friendsWrap = outerLayout.GetChild(i);
        if (friendsWrap == null) { _log.AppendLine("flatten: friendsWrap not found"); return false; }
        var innerLayout = friendsWrap.childCount > 0 ? friendsWrap.GetChild(0) : null;
        if (innerLayout == null || innerLayout.childCount < 2) { _log.AppendLine("flatten: innerLayout not found"); return false; }
        header = innerLayout.GetChild(0);      // Group 1 (header)
        var rowsWrap = innerLayout.GetChild(1);// Group 2 (rows wrapper)
        if (rows.parent != rowsWrap) { _log.AppendLine("flatten: unexpected rows parent"); return false; }

        MoveUnder(card, page);
        Rename(card, "Create Game Card");
        MoveUnder(header, page);
        Rename(header, "List Header");
        MoveUnder(rows, page);
        Rename(rows, "Friends Rows");
        MoveUnder(bottom, page);
        Rename(bottom, "Bottom Text");

        // order: card, header, rows, bottom
        Undo.RecordObject(page, "fix");
        card.SetSiblingIndex(0);
        header.SetSiblingIndex(1);
        rows.SetSiblingIndex(2);
        bottom.SetSiblingIndex(3);

        DestroyIf(rowsWrap);
        DestroyIf(innerLayout);
        DestroyIf(cardArea);
        DestroyIf(friendsWrap);
        DestroyIf(outerLayout);
        DestroyIf(wrapper);
        _log.AppendLine("flattened old wrappers");
        return true;
    }

    // ---------- helpers ----------
    private static void LoadFonts()
    {
        _fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/BRoyaBd SDF Dynamic.asset");
        _lib = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (_fa == null) { Debug.LogError("[FixFriends] BRoyaBd SDF Dynamic asset missing - run Tools/Test Persian Font first"); WriteLog(); return; }
        if (_lib == null) _lib = TMP_Settings.defaultFontAsset;
        if (_fa.fallbackFontAssetTable == null) _fa.fallbackFontAssetTable = new List<TMP_FontAsset>();
        if (_lib != null && !_fa.fallbackFontAssetTable.Contains(_lib)) _fa.fallbackFontAssetTable.Add(_lib);
        EditorUtility.SetDirty(_fa);
        _log.AppendLine($"fonts: fa={_fa.name}, lib={(_lib != null ? _lib.name : "null")}");
    }

    private static void Fa(TextMeshProUGUI t, string logical, string hexColor, int minF, int maxF, TextAlignmentOptions align)
    {
        if (t == null) return;
        FaRaw(t, PersianText.Shape(logical), hexColor, minF, maxF, align);
    }

    /// <summary>
    /// Same as <see cref="Fa"/> but takes already-shaped text. Use this for multi-line strings:
    /// pass Shape(lineA) + "\n" + Shape(lineB) so the pre-shaped runs are never split by word wrap
    /// (which would show the last logical line first).
    /// </summary>
    private static void FaRaw(TextMeshProUGUI t, string shaped, string hexColor, int minF, int maxF, TextAlignmentOptions align)
    {
        if (t == null) return;
        Undo.RecordObject(t, "fix");
        t.font = _fa;
        t.text = shaped;
        Color c;
        if (!string.IsNullOrEmpty(hexColor) && ColorUtility.TryParseHtmlString(hexColor, out c)) t.color = c;
        t.enableAutoSizing = true;
        t.fontSizeMin = minF;
        t.fontSizeMax = maxF;
        t.alignment = align;
        t.fontStyle = FontStyles.Normal; // BRoyaBd is already a bold face
        t.raycastTarget = false;
    }

    private static string ShapeLines(params string[] logicalLines)
    {
        var sb = new StringBuilder();
        foreach (var l in logicalLines)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(PersianText.Shape(l));
        }
        return sb.ToString();
    }

    private static void MoveUnder(Transform t, Transform parent)
    {
        Undo.SetTransformParent(t, parent, "fix");
        SetStretch((RectTransform)t);
    }

    private static void DestroyIf(Transform t)
    {
        if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
    }

    private static void Rename(Transform t, string newName)
    {
        if (t.name == newName) return;
        Undo.RecordObject(t.gameObject, "fix");
        t.name = newName;
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

    private static Transform FindDirect(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        return null;
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static TextMeshProUGUI FindTmp(Transform parent)
    {
        foreach (var t in parent.GetComponentsInChildren<TextMeshProUGUI>(true)) return t;
        return null;
    }

    private static string ListNames(Transform t)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < t.childCount; i++) sb.Append(t.GetChild(i).name).Append("; ");
        return sb.ToString();
    }

    private static void WriteLog()
    {
        File.WriteAllText("Temp/friends_fix_log.txt", _log.ToString());
    }
}