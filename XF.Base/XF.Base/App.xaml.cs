using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Base.Services;

namespace XF.Base
{
    public partial class App : Application
    {
        public static Size ScreenSize => new Size(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density,
            DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density);
        public static double Density => DeviceDisplay.MainDisplayInfo.Density;

        public App()
        {
            InitializeComponent();
            DialogService.Init(this);
            NavigationService.Init(Enums.Pages.Home);

        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
