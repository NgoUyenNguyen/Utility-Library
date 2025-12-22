using System.Collections.Generic;
using UnityEngine;

namespace NgoUyenNguyen
{
    [AddComponentMenu("UI/Tab Group")]
    public class TabGroup : MonoBehaviour
    {
        private List<TabButton> tabButtons = new();
        private TabButton selectedTab;

        public List<TabButton> TabButtons
        {
            get => tabButtons;
            set => tabButtons = value;
        }

        public TabButton SelectedTab => selectedTab;

        public void Subcribe(TabButton tabButton) => tabButtons?.Add(tabButton);
        public void OnTabEnter(TabButton tab) => tab.Hover(tab != selectedTab);
        public void OnTabExit(TabButton tab) => tab.Unhover(tab != selectedTab);
        public void OnTabSelected(TabButton tab)
        {
            selectedTab?.Deselect();
            selectedTab = tab;
            tab.Select();
        }
    }
}