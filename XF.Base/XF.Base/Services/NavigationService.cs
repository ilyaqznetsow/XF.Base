/* MIT License
Copyright (c) 2018 Binwell https://binwell.com
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using XF.Base.Enums;
using XF.Base.Helpers;
using XF.Base.Styles;
using XF.Base.View;
using XF.Base.ViewModel;

namespace XF.Base.Services
{
    public sealed class NavigationService
    {
        static readonly Lazy<NavigationService> LazyInstance =
            new Lazy<NavigationService>(() => new NavigationService(), true);

        readonly Dictionary<string, Type> _pageTypes;
        readonly Dictionary<string, Type> _viewModelTypes;

        volatile bool _isBusy;

        NavigationService()
        {
            _pageTypes = GetAssemblyPageTypes();
            _viewModelTypes = GetAssemblyViewModelTypes();
            MessagingCenter.Subscribe<MessageBus, NavigationPushInfo>(this, Constants.NavigationPushMessage,
                NavigationPushCallback);
            MessagingCenter.Subscribe<MessageBus, NavigationPopInfo>(this, Constants.NavigationPopMessage,
                NavigationPopCallback);
        }

        public static void Init(Pages detail)
        {
            Instance.Initialize(detail);
        }

        void Initialize(Pages page)
        {
            var initPage = GetInitializedPage(page.ToString());
            RootPush(initPage);
        }

        public static NavigationService Instance => LazyInstance.Value;

        void NavigationPushCallback(MessageBus bus, NavigationPushInfo navigationPushInfo)
        {
            if (navigationPushInfo == null) throw new ArgumentNullException(nameof(navigationPushInfo));

            if (string.IsNullOrEmpty(navigationPushInfo.To))
                throw new FieldAccessException(@"'To' page value should be set");
            if (_isBusy) return;
            _isBusy = true;
            Push(navigationPushInfo);
        }

        void NavigationPopCallback(MessageBus bus, NavigationPopInfo navigationPopInfo)
        {
            if (navigationPopInfo == null) throw new ArgumentNullException(nameof(navigationPopInfo));
            if (_isBusy) return;
            _isBusy = true;
            Pop(navigationPopInfo);
        }

        #region NavigationService internals

        INavigation GetTopNavigation()
        {
            var mainPage = Application.Current.MainPage;
            if (mainPage is TabbedPage tp)
                if (tp.CurrentPage is NavigationPage np)
                    return np.Navigation;
            return (mainPage as NavigationPage)?.Navigation;
        }

        #region Push

        void Push(NavigationPushInfo pushInfo)
        {

            var newPage = GetInitializedPage(pushInfo);

            switch (pushInfo.Mode)
            {
                case NavigationMode.Normal:
                    NormalPush(newPage, pushInfo);
                    break;
                case NavigationMode.Modal:
                    ModalPush(newPage, pushInfo);
                    break;
                case NavigationMode.RootPage:
                    RootPush(newPage, pushInfo);
                    break;
                case NavigationMode.Popup:
                    PopupPush(newPage, pushInfo);
                    break;
               
                default:
                    throw new NotImplementedException();
            }
        }


        void NormalPush(Page newPage, NavigationPushInfo pushInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await GetTopNavigation().PushAsync(newPage, pushInfo.WithAnimtation);
                    pushInfo.OnCompletedTask.SetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    pushInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }
            });
        }

        void ModalPush(Page newPage, NavigationPushInfo pushInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    if (pushInfo.NewNavigationStack)
                    {
                        await GetTopNavigation().PopToRootAsync(pushInfo.WithAnimtation);
                        newPage = new NavigationPage(newPage)
                        {
                            BarTextColor = Color.White
                        };
                        await Task.Delay(10);
                        Application.Current.MainPage = newPage;
                    }
                    else
                    {
                        await GetTopNavigation().PushModalAsync(newPage, pushInfo.WithAnimtation);
                    }

                    pushInfo.OnCompletedTask.SetResult(true);
                }
                catch (Exception ex)
                {
                    pushInfo.OnCompletedTask.SetResult(false);
                    Debug.WriteLine(ex);
                }
                finally
                {
                    _isBusy = false;
                }
            });
        }

        void RootPush(Page newPage, NavigationPushInfo pushInfo = null)
        {

            try
            {

                var navPage = new NavigationPage(newPage)
                {
                    BarTextColor = Colors.White,
                    BackgroundColor = Colors.White
                };
                Xamarin.Forms.PlatformConfiguration.iOSSpecific.NavigationPage.SetIsNavigationBarTranslucent(navPage, false);
                Xamarin.Forms.PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(navPage, true);

                if (Application.Current.MainPage == null || newPage is ContentPage)
                {

                    Device.BeginInvokeOnMainThread(() => Application.Current.MainPage = navPage);
                }
                else if (newPage is TabbedPage tp)
                {

                    Device.BeginInvokeOnMainThread(() => Application.Current.MainPage = navPage);

                    pushInfo.OnCompletedTask?.SetResult(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                pushInfo.OnCompletedTask?.SetResult(false);
            }
            finally
            {
                _isBusy = false;
            }

        }

        void PopupPush(Page newPage, NavigationPushInfo pushInfo)
        {

            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    var nav = PopupNavigation.Instance;
                    var page = (PopupPage)newPage;
                    await nav.PushAsync(page, pushInfo.WithAnimtation);
                    pushInfo.OnCompletedTask.SetResult(true);
                }
                catch
                {
                    pushInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }
            });
        }

        #endregion

        #region Pop

        void Pop(NavigationPopInfo popInfo)
        {

            switch (popInfo.Mode)
            {
                case NavigationMode.Normal:
                    NormalPop(popInfo);
                    break;
                case NavigationMode.Modal:
                    ModalPop(popInfo);
                    break;
                case NavigationMode.Popup:
                    PopupPop(popInfo);
                    break;
                case NavigationMode.AllPopup:
                    AllPopupPop(popInfo);
                    break;
              
                default:
                    throw new NotImplementedException();
            }
        }

        void ModalPop(NavigationPopInfo popInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await GetTopNavigation().PopModalAsync(popInfo.WithAnimtation);
                    popInfo.OnCompletedTask.SetResult(true);
                }
                catch
                {
                    popInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }

            });
        }

        void PopupPop(NavigationPopInfo popInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await GetTopNavigation().PopPopupAsync(popInfo.WithAnimtation);
                    popInfo.OnCompletedTask.SetResult(true);
                }
                catch
                {
                    popInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }

            });
        }

        void AllPopupPop(NavigationPopInfo popInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await GetTopNavigation().PopAllPopupAsync(popInfo.WithAnimtation);
                    popInfo.OnCompletedTask.SetResult(true);
                }
                catch
                {
                    popInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }
            });
        }

        void NormalPop(NavigationPopInfo popInfo)
        {
            Device.BeginInvokeOnMainThread(async () => {
                try
                {
                    await GetTopNavigation().PopAsync(popInfo.WithAnimtation);
                    popInfo.OnCompletedTask.SetResult(true);
                }
                catch
                {
                    popInfo.OnCompletedTask.SetResult(false);
                }
                finally
                {
                    _isBusy = false;
                }
            });
        }

        

        #endregion

        static string GetTypeBaseName(MemberInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return info.Name.Replace(@"Page", "").Replace(@"ViewModel", "");
        }

        static Dictionary<string, Type> GetAssemblyPageTypes()
        {
            var definedTypes = typeof(BasePage).GetTypeInfo().Assembly.DefinedTypes;
            var types = definedTypes
                .Where(ti =>
                   ti.BaseType != null && ti.IsClass && !ti.IsAbstract && ti.Name.Contains(@"Page") &&
                   ti.BaseType.Name.Contains(@"Page"))
                .ToDictionary(GetTypeBaseName, ti => ti.AsType());
            return types;
        }

        static Dictionary<string, Type> GetAssemblyViewModelTypes()
        {
            return typeof(BaseViewModel).GetTypeInfo().Assembly.DefinedTypes
                .Where(ti =>
                   ti.BaseType != null && ti.IsClass && !ti.IsAbstract && ti.Name.Contains(@"ViewModel") &&
                   ti.BaseType.Name.Contains(@"ViewModel"))
                .ToDictionary(GetTypeBaseName, ti => ti.AsType());
        }

        BasePage GetInitializedPage(string toName, Dictionary<string, object> navParams = null)
        {
            var page = GetPage(toName);
            var viewModel = GetViewModel(toName);
            viewModel.SetNavigationParams(navParams);
            page.BindingContext = viewModel;
            return (XF.Base.View.BasePage)page;
        }

        Page GetInitializedPage(NavigationPushInfo navigationPushInfo)
        {
            return GetInitializedPage(navigationPushInfo.To, navigationPushInfo.NavigationParams);
        }

        Page GetPage(string pageName)
        {
            if (!_pageTypes.ContainsKey(pageName)) throw new KeyNotFoundException($@"Page for {pageName} not found");

            Page page;
            try
            {
                var pageType = _pageTypes[pageName];
                var pageObject = Activator.CreateInstance(pageType);
                page = pageObject as Page;
            }
            catch (Exception e)
            {
                throw new TypeLoadException($@"Unable create instance for {pageName}Page", e);
            }

            return page;
        }

        BaseViewModel GetViewModel(string pageName)
        {
            if (!_viewModelTypes.ContainsKey(pageName))
                throw new KeyNotFoundException($@"ViewModel for {pageName} not found");

            BaseViewModel viewModel;
            try
            {
                viewModel = Activator.CreateInstance(_viewModelTypes[pageName]) as BaseViewModel;
            }
            catch (Exception e)
            {
                throw new TypeLoadException($@"Unable create instance for {pageName} ViewModel", e);
            }

            return viewModel;
        }

        #endregion
    }

    public class NavigationPushInfo
    {
        public string From { get; set; }
        public string To { get; set; }
        public Dictionary<string, object> NavigationParams { get; set; }
        public NavigationMode Mode { get; set; } = NavigationMode.Normal;
        public bool NewNavigationStack { get; set; }
        public TaskCompletionSource<bool> OnCompletedTask { get; set; }
        public bool WithAnimtation { get; set; } = true;
    }

    public class NavigationPopInfo
    {
        public Dictionary<string, object> NavigationParams { get; set; }
        public NavigationMode Mode { get; set; } = NavigationMode.Normal;
        public TaskCompletionSource<bool> OnCompletedTask { get; set; }
        public string To { get; set; }
        public bool WithAnimtation { get; set; } = true;
    }
}
