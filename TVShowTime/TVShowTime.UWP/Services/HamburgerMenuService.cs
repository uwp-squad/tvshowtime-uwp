using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.Views;
using Windows.UI.Xaml.Controls;

namespace TVShowTime.UWP.Services
{
    public interface IHamburgerNavigationService
    {
        void SetFrameElement(Frame frame);
        void Configure(string key, Type pageType);
        void NavigateTo(string pageKey);
    }

    public interface IHamburgerMenuService : IHamburgerNavigationService
    {
        List<MenuItem> MenuItems { get; }

        void SetHamburgerMenuElement(HamburgerMenu hamburgerMenu);
    }

    public class HamburgerMenuService : IHamburgerMenuService
    {
        #region Fields

        private Frame _frame;
        private Dictionary<string, Type> _pageTypes = new Dictionary<string, Type>();
        private HamburgerMenu _hamburgerMenu;
        private string _currentPageKey;

        #endregion

        #region Properties

        public List<MenuItem> MenuItems { get; private set; }

        #endregion

        #region Constructor

        public HamburgerMenuService()
        {
            MenuItems = new List<MenuItem>
            {
                new NavigationMenuItem
                {
                    Symbol = Symbol.List,
                    Name = "To Watch",
                    Type = MenuItemType.Main,
                    PageType = typeof(ToWatchPage)
                },
#if false
                new NavigationMenuItem
                {
                    Symbol = Symbol.Clock,
                    Name = "Upcoming",
                    Type = MenuItemType.Main,
                    PageType = typeof(UpcomingPage)
                }
#endif
                new NavigationMenuItem
                {
                    Symbol = Symbol.World,
                    Name = "Explore",
                    Type = MenuItemType.Main,
                    PageType = typeof(ExplorePage)
                }
            };

            if (StoreServicesFeedbackLauncher.IsSupported())
            {
                var feedbackMenuItem = new ActionMenuItem

                {
                    Glyph = "\uE939",
                    Name = "Feedback",
                    Type = MenuItemType.Options,
                    Action = async () =>
                    {
                        await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();

                        var pageType = GetPageTypeByKey(_currentPageKey);
                        ResetSelectedItem(pageType);
                    }
                };
                MenuItems.Add(feedbackMenuItem);
            }
        }

        #endregion

        #region Public methods

        public void SetFrameElement(Frame frame)
        {
            _frame = frame;
            _frame.Navigated += (sender, e) => TryResetCurrentPageKey();
        }

        public void SetHamburgerMenuElement(HamburgerMenu hamburgerMenu)
        {
            _hamburgerMenu = hamburgerMenu;
        }

        public void Configure(string key, Type pageType)
        {
            _pageTypes.Add(key, pageType);
        }

        public void NavigateTo(string pageKey)
        {
            var pageType = GetPageTypeByKey(pageKey);

            // No page type => no navigation
            if (pageType == null)
            {
                return;
            }

            // Can't navigate twice to the same page
            if (_currentPageKey == pageKey)
            {
                return;
            }

            _currentPageKey = pageKey;

            // Navigate and reset selected item of the hamburger menu
            _frame.Navigate(pageType);
            ResetSelectedItem(pageType);
        }

        #endregion

        #region Private methods

        private Type GetPageTypeByKey(string pageKey)
        {
            _pageTypes.TryGetValue(pageKey, out var pageType);
            return pageType;
        }

        private void TryResetCurrentPageKey()
        {
            if (_frame.CurrentSourcePageType != null)
            {
                foreach (var kv in _pageTypes)
                {
                    if (kv.Value == _frame.CurrentSourcePageType)
                    {
                        _currentPageKey = kv.Key;
                        ResetSelectedItem(kv.Value);
                        return;
                    }
                }
            }
        }

        private void ResetSelectedItem(Type pageType)
        {
            // Retrieve item in the hamburger menu that will be selected
            var menuItem = MenuItems.FirstOrDefault(m =>
            {
                return m is NavigationMenuItem navigationMenuItem &&
                       navigationMenuItem.PageType == pageType;
            });

            _hamburgerMenu.SelectedItem = menuItem;
            _hamburgerMenu.SelectedOptionsItem = null;
        }

        #endregion
    }
}
