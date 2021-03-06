using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using GetAllLinks.Core.Infrastructure.Interfaces;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Shared.Presenter;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Droid.Shared.Fragments;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Shared.Caching;
using GetAllLinks.Core.ViewModels;
using GetAllLinks.Droid.Helpers;
using MvvmCross.Core.Views;
using MvvmCross.Platform;

namespace GetAllLinks.Droid
{
	[Activity(
		Theme = "@style/MyTheme.Base",
		Label = "GetAllLinks",
		Name = "com.sp.GetAllLinks.MainActivity",
		MainLauncher = false)]
	public class MainActivity : MvxAppCompatActivity<MainActivityViewModel>, IMvxFragmentHost, IFragmentCacheableActivity
	{
		private static MvxViewModel currentViewModel;

		public IFragmentCacheConfiguration FragmentCacheConfiguration => new DefaultFragmentCacheConfiguration();

		public bool Close(IMvxViewModel viewModel)
		{
			return true;
		}

		public bool Show(MvxViewModelRequest request, Bundle bundle, Type fragmentType, MvxFragmentAttribute fragmentAttribute)
		{
			var fragmentView = (IMvxFragmentView)Activator.CreateInstance(fragmentType);
			fragmentView.LoadViewModelFrom(request);
			currentViewModel = fragmentView.DataContext as MvxViewModel;

			var ft = SupportFragmentManager.BeginTransaction();
			ft.Replace(Resource.Id.main, fragmentView.ToFragment());
			ft.Commit();

			return true;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.mainView);

			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				Task.Run(async () => await GeneralException.HandleGeneralException(s, e));
			};

			TaskScheduler.UnobservedTaskException += (s, e) =>
			{
				Task.Run(async () => await GeneralException.HandleGeneralException(s, e));
			};
		}

		public override void OnBackPressed()
		{
			(currentViewModel as ICloseable)?.OnClose();

			if (currentViewModel?.GetType() == typeof(SettingsViewModel))
			{
				var viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
				var request = MvxViewModelRequest.GetDefaultRequest(typeof(GetAllLinksViewModel));
				viewDispatcher.ShowViewModel(request);
				return;
			}

			base.OnBackPressed();
		}
	}
}