using Microsoft.Xna.Framework;
using OTAPI;
using Terraria;

namespace TerrariaApi.Server.Hooking
{
	internal static class NpcHooks
	{
		private static HookManager _hookManager;

		/// <summary>
		/// Attaches any of the OTAPI Npc hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.NPC.PreSetDefaults += OnPreSetDefaultsById;
			Hooks.NPC.PreSetDefaultsFromNetId += OnPreSetDefaultsFromNetId;
			Hooks.NPC.PreStrikeNPC += OnPreStrike;
			Hooks.NPC.PreTransform += OnPreTransform;
			Hooks.NPC.PreAI += OnPreAI;

			Hooks.NPC.Spawn += OnSpawn;
			Hooks.NPC.DropLoot += OnDropLoot;
			Hooks.NPC.BossBag += OnBossBagItem;
			Hooks.NPC.Killed += OnKilled;
		}

		static void OnKilled(object sender, Hooks.NPC.KilledEventArgs e)
		{
			_hookManager.InvokeNpcKilled(e.Npc);
		}

		static HookResult OnPreSetDefaultsById(Hooks.NPC.SetDefaultsEventArgs args)
		{
			var type = args.Type;
			var cancel = _hookManager.InvokeNpcSetDefaultsInt(ref type, args.NPC);
			args.Type = type;
			if (cancel)
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static HookResult OnPreSetDefaultsFromNetId(Hooks.NPC.SetDefaultsFromNetIdEventArgs args)
		{
			var id = args.Type;
			var cancel = _hookManager.InvokeNpcNetDefaults(ref id, args.NPC);
			args.Type = id;
			if (cancel)
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static HookResult OnPreStrike(Hooks.NPC.StrikeNPCEventArgs args)
		{
			if (args.Attacker is Player player)
			{
				var damage = args.Damage;
				var knockback = args.Knockback;
				var hitDirection = args.HitDirection;
				var isCritical = args.IsCritical;
				var hasNoEffect = args.HasNoEffect;
				var fromNetwork = args.FromNetwork;
				var cancel = _hookManager.InvokeNpcStrike(args.NPC, ref damage, ref knockback, ref hitDirection, ref isCritical, ref hasNoEffect, ref fromNetwork, player);
				args.Damage = damage;
				args.Knockback = knockback;
				args.HitDirection = hitDirection;
				args.IsCritical = isCritical;
				args.HasNoEffect = hasNoEffect;
				args.FromNetwork = fromNetwork;
				if (cancel)
				{
					return HookResult.Cancel;
				}
			}

			return HookResult.Continue;
		}

		static HookResult OnPreTransform(Hooks.NPC.TransformEventArgs args)
		{
			if (_hookManager.InvokeNpcTransformation(args.NPC.whoAmI))
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static void OnSpawn(object sender, Hooks.NPC.SpawnEventArgs e)
		{
			var index = e.Index;
			if (_hookManager.InvokeNpcSpawn(ref index))
			{
				e.Result = HookResult.Cancel;
				e.Index = index;
			}
		}

		static void OnDropLoot(object sender, Hooks.NPC.DropLootEventArgs e)
		{
			if (e.Event == HookEvent.Before)
			{
				var Width = e.Width;
				var Height = e.Height;
				var Type = e.Type;
				var Stack = e.Stack;
				var noBroadcast = e.NoBroadcast;
				var pfix = e.Pfix;
				var noGrabDelay = e.NoGrabDelay;
				var reverseLookup = e.ReverseLookup;

				var position = new Vector2(e.X, e.Y);
				if (_hookManager.InvokeNpcLootDrop
				(
					ref position,
					ref Width,
					ref Height,
					ref Type,
					ref Stack,
					ref noBroadcast,
					ref pfix,
					e.Npc.type,
					e.Npc.whoAmI,
					ref noGrabDelay,
					ref reverseLookup
				))
				{
					e.X = (int)position.X;
					e.Y = (int)position.Y;
					e.Result = HookResult.Cancel;
				}
				e.X = (int)position.X;
				e.Y = (int)position.Y;

				e.Width = Width;
				e.Height = Height;
				e.Type = Type;
				e.Stack = Stack;
				e.NoBroadcast = noBroadcast;
				e.Pfix = pfix;
				e.NoGrabDelay = noGrabDelay;
				e.ReverseLookup = reverseLookup;
			}
		}

		static void OnBossBagItem(object sender, Hooks.NPC.BossBagEventArgs e)
		{
			var Width = e.Width;
			var Height = e.Height;
			var Type = e.Type;
			var Stack = e.Stack;
			var noBroadcast = e.NoBroadcast;
			var pfix = e.Pfix;
			var noGrabDelay = e.NoGrabDelay;
			var reverseLookup = e.ReverseLookup;

			var positon = new Vector2(e.X, e.Y);
			if (_hookManager.InvokeDropBossBag
			(
				ref positon,
				ref Width,
				ref Height,
				ref Type,
				ref Stack,
				ref noBroadcast,
				ref pfix,
				e.Npc.type,
				e.Npc.whoAmI,
				ref noGrabDelay,
				ref reverseLookup
			))
			{
				e.Result = HookResult.Cancel;
			}

			e.Width = Width;
			e.Height = Height;
			e.Type = Type;
			e.Stack = Stack;
			e.NoBroadcast = noBroadcast;
			e.Pfix = pfix;
			e.NoGrabDelay = noGrabDelay;
			e.ReverseLookup = reverseLookup;
		}

		static HookResult OnPreAI(Hooks.NPC.AIEventArgs args)
		{
			if (_hookManager.InvokeNpcAIUpdate(args.NPC))
				return HookResult.Cancel;

			return HookResult.Continue;
		}
	}
}
