using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level page: switches between the Weekly / Monthly / General panels with the three select buttons.
/// </summary>
public class LevelMenuPage : MenuPage
{
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private GameObject[] tabPanels;
    [SerializeField, Range(0, 5)] private int defaultTab = 0;

    private int _currentTab = -1;

    protected override void OnPageAwake()
    {
        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] == null) continue;
                int index = i;
                tabButtons[i].onClick.RemoveAllListeners();
                tabButtons[i].onClick.AddListener(() => OpenTab(index));
            }
        }

        OpenTab(defaultTab);
    }

    public override void OnOpened()
    {
        OpenTab(_currentTab >= 0 ? _currentTab : defaultTab);
    }

    public void OpenTab(int index)
    {
        if (tabPanels == null || tabPanels.Length == 0) return;

        index = Mathf.Clamp(index, 0, tabPanels.Length - 1);

        for (int i = 0; i < tabPanels.Length; i++)
            if (tabPanels[i] != null) tabPanels[i].SetActive(i == index);

        _currentTab = index;
    }
}
