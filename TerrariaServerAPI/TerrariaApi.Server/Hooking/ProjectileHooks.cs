using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class ProjectileHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Projectile hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Projectile.PostSetDefaults += OnPostSetDefaults;
			Hooks.Projectile.PreAI += OnPreAI;
		}

		private static void OnPostSetDefaults(Hooks.Projectile.SetDefaultsEventArgs args)
		{
			var type = args.Type;
			_hookManager.InvokeProjectileSetDefaults(ref type, args.Projectile);
			args.Type = type;
		}

		private static HookResult OnPreAI(Hooks.Projectile.AIEventArgs args)
		{
			if (_hookManager.InvokeProjectileAIUpdate(args.Projectile))
				return HookResult.Cancel;

			return HookResult.Continue;
		}
	}
}
