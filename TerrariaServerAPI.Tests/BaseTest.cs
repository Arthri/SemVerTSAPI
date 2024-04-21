using NUnit.Framework;
using OTAPI;
using System;
using System.Threading;

namespace TerrariaServerAPI.Tests;

public class BaseTest
{
	private static bool _initialized;

	[OneTimeSetUp]
	public void EnsureInitialised()
	{
		if (!_initialized)
		{
			var are = new AutoResetEvent(false);
			Exception? error = null;
			PreHookHandler<Hooks.Main.DedicatedServerEventArgs> cb = (Hooks.Main.DedicatedServerEventArgs args) =>
			{
				args.Instance.Initialize();
				are.Set();
				_initialized = true;
				return HookResult.Cancel;
			};
			Hooks.Main.PreDedicatedServer += cb;

			global::TerrariaApi.Server.Program.Main(new string[] { });

			_initialized = are.WaitOne(TimeSpan.FromSeconds(30));

			Hooks.Main.PreDedicatedServer -= cb;

			Assert.That(_initialized, Is.True);
			Assert.That(error, Is.Null);
		}
	}
}
