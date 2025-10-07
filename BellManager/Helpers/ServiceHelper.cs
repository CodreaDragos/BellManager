using System;
using Microsoft.Extensions.DependencyInjection;

namespace BellManager.Helpers
{
	public static class ServiceHelper
	{
		public static IServiceProvider Services { get; set; } = default!;

		public static T GetRequiredService<T>() where T : notnull
		{
			return Services.GetRequiredService<T>();
		}
	}
}


