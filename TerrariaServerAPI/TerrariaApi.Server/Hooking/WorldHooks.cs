using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class WorldHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI World hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.IO.WorldFile.PreSaveWorld += WorldFile_OnPreSaveWorld;
			Hooks.WorldGen.PreStartHardmode += WorldGen_PreStartHardmode;
			Hooks.WorldGen.PreSpreadGrass += WorldGen_PreSpreadGrass;
			Hooks.Main.PreCheckChristmas += Main_OnPreCheckXMas;
			Hooks.Main.PreCheckHalloween += Main_OnPreCheckHalloween;

			Hooks.Collision.PressurePlate += OnPressurePlate;
			Hooks.WorldGen.Meteor += OnDropMeteor;
		}

		static void OnPressurePlate(object sender, Hooks.Collision.PressurePlateEventArgs e)
		{
			if (e.Entity is NPC npc)
			{
				if (_hookManager.InvokeNpcTriggerPressurePlate(npc, e.X, e.Y))
					e.Result = HookResult.Cancel;
			}
			else if (e.Entity is Player player)
			{
				if (_hookManager.InvokePlayerTriggerPressurePlate(player, e.X, e.Y))
					e.Result = HookResult.Cancel;
			}
			else if (e.Entity is Projectile projectile)
			{
				if (_hookManager.InvokeProjectileTriggerPressurePlate(projectile, e.X, e.Y))
					e.Result = HookResult.Cancel;
			}
		}

		static HookResult WorldFile_OnPreSaveWorld(Hooks.IO.WorldFile.SaveWorldEventArgs args)
		{
			if (_hookManager.InvokeWorldSave(args.ResetTime))
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		private static HookResult WorldGen_PreStartHardmode(Hooks.WorldGen.StartHardmodeEventArgs args)
		{
			if (_hookManager.InvokeWorldStartHardMode())
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static void OnDropMeteor(object sender, Hooks.WorldGen.MeteorEventArgs e)
		{
			if (_hookManager.InvokeWorldMeteorDrop(e.X, e.Y))
			{
				e.Result = HookResult.Cancel;
			}
		}

		private static HookResult Main_OnPreCheckXMas(Hooks.Main.CheckChristmasEventArgs args)
		{
			if (_hookManager.InvokeWorldChristmasCheck(ref Terraria.Main.xMas))
				return HookResult.Cancel;

			return HookResult.Cancel;
		}

		private static HookResult Main_OnPreCheckHalloween(Hooks.Main.CheckHalloweenEventArgs args)
		{
			if (_hookManager.InvokeWorldHalloweenCheck(ref Main.halloween))
				return HookResult.Cancel;

			return HookResult.Cancel;
		}

		private static HookResult WorldGen_PreSpreadGrass(Hooks.WorldGen.SpreadGrassEventArgs args)
		{
			if (_hookManager.InvokeWorldGrassSpread(args.X, args.Y, args.Dirt, args.Grass, args.Repeat, args.Color))
				return HookResult.Cancel;

			return HookResult.Continue;
		}
	}
}
