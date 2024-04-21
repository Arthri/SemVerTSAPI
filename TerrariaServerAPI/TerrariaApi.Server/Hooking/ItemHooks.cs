using OTAPI;
using Terraria;
using Terraria.GameContent.Items;

namespace TerrariaApi.Server.Hooking
{
	internal static class ItemHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Item hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.Item.PreSetDefaults += OnPreSetDefaults;
			Hooks.Item.PreNetDefaults += OnPreNetDefaults;

			Hooks.Chest.QuickStack += OnQuickStack;
		}

		private static HookResult OnPreNetDefaults(Hooks.Item.NetDefaultsEventArgs args)
		{
			var type = args.Type;
			var hookResult = _hookManager.InvokeItemNetDefaults(ref type, args.Item);
			args.Type = type;
			if (hookResult)
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		private static HookResult OnPreSetDefaults(Hooks.Item.SetDefaultsEventArgs args)
		{
			var type = args.Type;
			var hookResult = _hookManager.InvokeItemSetDefaultsInt(ref type, args.Item, args.Variant);
			args.Type = type;
			if (hookResult)
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		private static void OnQuickStack(object sender, Hooks.Chest.QuickStackEventArgs e)
		{
			if (_hookManager.InvokeItemForceIntoChest(Main.chest[e.ChestIndex], e.Item, Main.player[e.PlayerId]))
			{
				e.Result = HookResult.Cancel;
			}
		}
	}
}
