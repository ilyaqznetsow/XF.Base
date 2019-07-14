using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFView = Xamarin.Forms.View;
namespace XF.Base.UI.Components
{
    [ContentProperty("Conditions")]
    public class StateContainer : Grid
    {
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public StateContainer()
        {
            _conditions.CollectionChanged += async (sender, args) => {
                await _semaphoreSlim.WaitAsync();
                var scState = GetState(this);
                if (args.NewItems == null) return;

                foreach (var item in args.NewItems)
                {
                    if (!(item is XFView view)) continue;
                    var viewState = GetState(view);
                    view.IsVisible = viewState != null && viewState.ToString() == scState?.ToString();
                    Children.Add(view);
                }

                _semaphoreSlim.Release();
            };
        }

        readonly ObservableCollection<XFView> _conditions = new ObservableCollection<XFView>();
        public IList<XFView> Conditions => _conditions;

        public static readonly BindableProperty StateProperty =
            BindableProperty.CreateAttached(
                "State",
                typeof(object),
                typeof(StateContainer),
                default(object),
                propertyChanged: StateChanged);

        public static object GetState(BindableObject bo)
        {
            return bo.GetValue(StateProperty);
        }

        public static void SetState(BindableObject bo, object value)
        {
            bo.SetValue(StateProperty, value);
        }

        static async void StateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is StateContainer sc)
            {
                await sc.ChooseStateProperty(GetState(sc));
            }
        }

        async Task ChooseStateProperty(object newValue)
        {
            await _semaphoreSlim.WaitAsync();

            if (Conditions.Count == 0 || newValue == null)
            {
                var view = await HideContent();
                view.IsVisible = false;
                return;
            }

            try
            {
                var condition = Conditions.FirstOrDefault(cond => {
                    var state = GetState(cond);
                    return state != null && state.ToString() == newValue.ToString();
                });

                var view = await HideContent();
                if (condition != null)
                {
                    await ShowContent(view, condition);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"StateContainer ChooseStateProperty {newValue} error: {e}");
            }

            _semaphoreSlim.Release();
        }

        async Task<XFView> HideContent()
        {
            var shown = Children.FirstOrDefault(e => e.IsVisible);
            if (shown != null)
            {
                await shown.FadeTo(0, 100U);
                shown.Opacity = 0;
            }

            return shown;
        }

        async Task ShowContent(XFView oldView, XFView newView)
        {
            newView.Opacity = 0;
            newView.IsVisible = true;
            if (oldView != null)
                oldView.IsVisible = false;
            await newView.FadeTo(1);
            newView.Opacity = 1;
        }
    }

    public static class StateContainerExt
    {
        public static T SetState<T>(this T bo, object state) where T : BindableObject
        {
            StateContainer.SetState(bo, state);
            return bo;
        }
    }
}
