using Microsoft.Xna.Framework;
using OTAPI;

namespace TerrariaApi.Server.Hooking
{
	internal static class GameHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Game hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Main.PreUpdate += OnPreUpdate;
			Hooks.Main.PostUpdate += OnPostUpdate;
			Hooks.Main.PreInitialize += OnPreInitialize;
			Hooks.Netplay.PreStartServer += OnPreStartServer;

			Hooks.WorldGen.HardmodeTilePlace += OnHardmodeTilePlace;
			Hooks.WorldGen.HardmodeTileUpdate += OnHardmodeTileUpdate;
			Hooks.Item.MechSpawn += OnItemMechSpawn;
			Hooks.NPC.MechSpawn += OnNpcMechSpawn;
		}

		private static HookResult OnPreUpdate(Hooks.Main.UpdateEventArgs args)
		{
			_hookManager.InvokeGameUpdate();
			return HookResult.Continue;
		}

		private static void OnPostUpdate(Hooks.Main.UpdateEventArgs args)
		{
			_hookManager.InvokeGamePostUpdate();
		}

		private static void OnHardmodeTileUpdate(object sender, Hooks.WorldGen.HardmodeTileUpdateEventArgs e)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(e.X, e.Y, e.Type))
			{
				e.Result = HookResult.Cancel;
			}
		}

		private static void OnHardmodeTilePlace(object sender, Hooks.WorldGen.HardmodeTilePlaceEventArgs e)
		{
			if (_hookManager.InvokeGameHardmodeTileUpdate(e.X, e.Y, e.Type))
			{
				e.Result = HardmodeTileUpdateResult.Cancel;
			}
		}

		private static HookResult OnPreInitialize(Hooks.Main.InitializeEventArgs args)
		{
			HookManager.InitialiseAPI();
			_hookManager.InvokeGameInitialize();
			return HookResult.Continue;
		}

		private static HookResult OnPreStartServer(Hooks.Netplay.StartServerEventArgs args)
		{
			_hookManager.InvokeGamePostInitialize();
			return HookResult.Continue;
		}

		private static void OnItemMechSpawn(object sender, Hooks.Item.MechSpawnEventArgs e)
		{
			if (!_hookManager.InvokeGameStatueSpawn(e.Num2, e.Num3, e.Num, (int)(e.X / 16f), (int)(e.Y / 16f), e.Type, false))
			{
				e.Result = HookResult.Cancel;
			}
		}

		private static void OnNpcMechSpawn(object sender, Hooks.NPC.MechSpawnEventArgs e)
		{
			if (!_hookManager.InvokeGameStatueSpawn(e.Num2, e.Num3, e.Num, (int)(e.X / 16f), (int)(e.Y / 16f), e.Type, true))
			{
				e.Result = HookResult.Cancel;
			}
		}
	}
}
