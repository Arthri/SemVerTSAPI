using OTAPI;
using System;
using System.Linq;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class ServerHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Server hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Main.PreStartDedInput += OnPre_startDedInput;
			Hooks.RemoteClient.PreReset += RemoteClient_OnPreReset;
			Hooks.Main.CommandProcess += OnProcess;
		}

		static HookResult OnPre_startDedInput(Hooks.Main.StartDedicatedInputEventArgs args)
		{
			if (Environment.GetCommandLineArgs().Any(x => x.Equals("-disable-commands")))
			{
				Console.WriteLine("Command thread has been disabled.");
				return HookResult.Cancel;
			}

			return HookResult.Continue;
		}

		static void OnProcess(object sender, Hooks.Main.CommandProcessEventArgs e)
		{
			if (_hookManager.InvokeServerCommand(e.Command))
			{
				e.Result = HookResult.Cancel;
			}
		}

		static HookResult RemoteClient_OnPreReset(Hooks.RemoteClient.ResetEventArgs args)
		{
			if (!Netplay.Disconnect)
			{
				if (args.RemoteClient.IsActive)
				{
					_hookManager.InvokeServerLeave(args.RemoteClient.Id);
				}
				_hookManager.InvokeServerSocketReset(args.RemoteClient);
			}

			return HookResult.Continue;
		}
	}
}
