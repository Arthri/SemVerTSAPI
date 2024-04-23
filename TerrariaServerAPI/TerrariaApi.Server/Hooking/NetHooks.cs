using Microsoft.Xna.Framework;
using OTAPI;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.Net;

namespace TerrariaApi.Server.Hooking
{
	internal class NetHooks
	{
		private static HookManager _hookManager;

		public static readonly object syncRoot = new object();

		/// <summary>
		/// Attaches any of the OTAPI Net hooks to the existing <see cref="HookManager"/> implementation
		/// </summary>
		/// <param name="hookManager">HookManager instance which will receive the events</param>
		public static void AttachTo(HookManager hookManager)
		{
			_hookManager = hookManager;

			Hooks.NetMessage.PreGreetPlayer += OnPreGreetPlayer;
			Hooks.Netplay.PreConnectionAccepted += OnPreConnectionAccepted;
			Hooks.Chat.ChatHelper.PreBroadcastChatMessage += OnPreBroadcastChatMessage;
			Hooks.Net.NetManager.PreSendData += OnPreSendNetData;

			Hooks.NetMessage.SendData += OnSendData;
			Hooks.NetMessage.SendBytes += OnSendBytes;
			Hooks.MessageBuffer.GetData += OnReceiveData;
			Hooks.MessageBuffer.NameCollision += OnNameCollision;
		}

		static HookResult OnPreBroadcastChatMessage(Hooks.Chat.ChatHelper.BroadcastChatMessageEventArgs args)
		{
			var text = args.Text;
			var color = args.Color;
			float r = color.R, g = color.G, b = color.B;

			var cancel = _hookManager.InvokeServerBroadcast(ref text, ref r, ref g, ref b);

			args.Text = text;
			args.Color = new Color(r, g, b, color.A);

			if (cancel)
			{
				return HookResult.Cancel;
			}

			return HookResult.Continue;
		}

		static void OnSendData(object sender, Hooks.NetMessage.SendDataEventArgs e)
		{
			if (e.Event == HookEvent.Before)
			{
				var msgType = e.MsgType;
				var remoteClient = e.RemoteClient;
				var ignoreClient = e.IgnoreClient;
				var text = e.Text;
				var number = e.Number;
				var number2 = e.Number2;
				var number3 = e.Number3;
				var number4 = e.Number4;
				var number5 = e.Number5;
				var number6 = e.Number6;
				var number7 = e.Number7;
				if (_hookManager.InvokeNetSendData
				(
					ref msgType,
					ref remoteClient,
					ref ignoreClient,
					ref text,
					ref number,
					ref number2,
					ref number3,
					ref number4,
					ref number5,
					ref number6,
					ref number7
				))
				{
					e.Result = HookResult.Cancel;
				}

				e.MsgType = msgType;
				e.RemoteClient = remoteClient;
				e.IgnoreClient = ignoreClient;
				e.Text = text;
				e.Number = number;
				e.Number2 = number2;
				e.Number3 = number3;
				e.Number4 = number4;
				e.Number5 = number5;
				e.Number6 = number6;
				e.Number7 = number7;
			}
		}

		static HookResult OnPreSendNetData(Hooks.Net.NetManager.SendDataEventArgs args)
		{
			var netmanager = args.Instance;
			var socket = args.Socket;
			var packet = args.Packet;

			var cancel = _hookManager.InvokeNetSendNetData(ref netmanager, ref socket, ref packet);

			args.Socket = socket;
			args.Packet = packet;

			if (cancel)
			{
				return HookResult.Cancel;
			}

			return HookResult.Continue;
		}

		static void OnReceiveData(object sender, Hooks.MessageBuffer.GetDataEventArgs e)
		{
			if (!Enum.IsDefined(typeof(PacketTypes), (int)e.PacketId))
			{
				e.Result = HookResult.Cancel;
			}
			else
			{
				var msgId = e.PacketId;
				var readOffset = e.ReadOffset;
				var length = e.Length;

				if (_hookManager.InvokeNetGetData(ref msgId, e.Instance, ref readOffset, ref length))
				{
					e.Result = HookResult.Cancel;
				}

				e.PacketId = msgId;
				e.ReadOffset = readOffset;
				e.Length = length;
			}
		}

		static HookResult OnPreGreetPlayer(Hooks.NetMessage.GreetPlayerEventArgs args)
		{
			if (_hookManager.InvokeNetGreetPlayer(args.Player))
				return HookResult.Cancel;

			return HookResult.Continue;
		}

		static void OnSendBytes(object sender, Hooks.NetMessage.SendBytesEventArgs e)
		{
			if (_hookManager.InvokeNetSendBytes(Netplay.Clients[e.RemoteClient], e.Data, e.Offset, e.Size))
			{
				e.Result = HookResult.Cancel;
			}
		}

		static void OnNameCollision(object sender, Hooks.MessageBuffer.NameCollisionEventArgs e)
		{
			if (_hookManager.InvokeNetNameCollision(e.Player.whoAmI, e.Player.name))
			{
				e.Result = HookResult.Cancel;
			}
		}

		static HookResult OnPreConnectionAccepted(Hooks.Netplay.ConnectionAcceptedEventArgs args)
		{
			int slot = FindNextOpenClientSlot();
			if (slot != -1)
			{
				Netplay.Clients[slot].Reset();
				Netplay.Clients[slot].Socket = args.Socket;
			}
			if (FindNextOpenClientSlot() == -1)
			{
				Netplay.StopListening();
			}

			return HookResult.Cancel;
		}

		static int FindNextOpenClientSlot()
		{
			lock (syncRoot)
			{
				for (int i = 0; i < Main.maxNetPlayers; i++)
				{
					if (!Netplay.Clients[i].IsConnected())
					{
						return i;
					}
				}
			}
			return -1;
		}
	}
}
