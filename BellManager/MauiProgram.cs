using BellManager.Pages;
using BellManager.Services;
using BellManager.ViewModels;
using BellManager.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Maui.Devices;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace BellManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


		// Configure API base address
		string apiBase;
		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			apiBase = "http://192.168.0.198:8080/"; // Your laptop's current IP address on port 8080
		}
		else
		{
			apiBase = "http://localhost:8080/"; // iOS simulator/Windows/MacCatalyst on port 8080
		}


		builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
		builder.Services.AddSingleton<AlarmApiService>();
		builder.Services.AddSingleton<AuthApiService>();
		builder.Services.AddSingleton<UserService>();
		builder.Services.AddSingleton<ChurchApiService>();
		builder.Services.AddSingleton<AdminUserApiService>();
		builder.Services.AddTransient<AlarmsViewModel>();
		builder.Services.AddTransient<AlarmsPage>();
		builder.Services.AddTransient<EditAlarmViewModel>();
		builder.Services.AddTransient<EditAlarmPage>();
		builder.Services.AddTransient<ChurchesViewModel>();
		builder.Services.AddTransient<ChurchesPage>();
		builder.Services.AddTransient<AssignUserChurchViewModel>();
		builder.Services.AddTransient<AssignUserChurchPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		ServiceHelper.Services = app.Services;
		return app;
	}
}
