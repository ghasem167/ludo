using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabHandler : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public Button button;
        public TabPage page;
    }

    [SerializeField] private List<Tab> tabs = new();
    [SerializeField] private int defaultTab = 0;

    private int _currentTab = -1;

    private void Awake()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;

            tabs[i].button.onClick.AddListener(
                () => OpenTab(index)
            );
        }
    }

    private void Start()
    {
        OpenTab(defaultTab);
    }

    public void OpenTab(int index)
    {
        if (index < 0 || index >= tabs.Count)
            return;

        if (_currentTab == index)
            return;

        for (int i = 0; i < tabs.Count; i++)
        {
            if (i == index)
                tabs[i].page.Show();
            else
                tabs[i].page.Hide();
        }

        _currentTab = index;
    }

    private void OnDestroy()
    {
        foreach (var tab in tabs)
        {
            tab.button.onClick.RemoveAllListeners();
        }
    }
}