using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000FD RID: 253
public static class PhotonNetwork
{
	// Token: 0x060005ED RID: 1517 RVA: 0x00038B18 File Offset: 0x00036F18
	static PhotonNetwork()
	{
		if (PhotonNetwork.PhotonServerSettings != null)
		{
			Application.runInBackground = PhotonNetwork.PhotonServerSettings.RunInBackground;
		}
		GameObject gameObject = new GameObject();
		PhotonNetwork.photonMono = gameObject.AddComponent<PhotonHandler>();
		gameObject.name = "PhotonMono";
		gameObject.hideFlags = HideFlags.HideInHierarchy;
		ConnectionProtocol protocol = PhotonNetwork.PhotonServerSettings.Protocol;
		PhotonNetwork.networkingPeer = new NetworkingPeer(string.Empty, protocol);
		PhotonNetwork.networkingPeer.QuickResendAttempts = 2;
		PhotonNetwork.networkingPeer.SentCountAllowance = 7;
		if (PhotonNetwork.UsePreciseTimer)
		{
			UnityEngine.Debug.Log("Using Stopwatch as precision timer for PUN.");
			PhotonNetwork.startupStopwatch = new Stopwatch();
			PhotonNetwork.startupStopwatch.Start();
			PhotonNetwork.networkingPeer.LocalMsTimestampDelegate = (() => (int)PhotonNetwork.startupStopwatch.ElapsedMilliseconds);
		}
		CustomTypes.Register();
	}

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x060005EE RID: 1518 RVA: 0x00038CA4 File Offset: 0x000370A4
	// (set) Token: 0x060005EF RID: 1519 RVA: 0x00038CAB File Offset: 0x000370AB
	public static string gameVersion { get; set; }

	// Token: 0x17000060 RID: 96
	// (get) Token: 0x060005F0 RID: 1520 RVA: 0x00038CB3 File Offset: 0x000370B3
	public static string ServerAddress
	{
		get
		{
			return (PhotonNetwork.networkingPeer == null) ? "<not connected>" : PhotonNetwork.networkingPeer.ServerAddress;
		}
	}

	// Token: 0x17000061 RID: 97
	// (get) Token: 0x060005F1 RID: 1521 RVA: 0x00038CD3 File Offset: 0x000370D3
	public static CloudRegionCode CloudRegion
	{
		get
		{
			return (PhotonNetwork.networkingPeer == null || !PhotonNetwork.connected || PhotonNetwork.Server == ServerConnection.NameServer) ? CloudRegionCode.none : PhotonNetwork.networkingPeer.CloudRegion;
		}
	}

	// Token: 0x17000062 RID: 98
	// (get) Token: 0x060005F2 RID: 1522 RVA: 0x00038D04 File Offset: 0x00037104
	public static bool connected
	{
		get
		{
			return PhotonNetwork.offlineMode || (PhotonNetwork.networkingPeer != null && (!PhotonNetwork.networkingPeer.IsInitialConnect && PhotonNetwork.networkingPeer.State != ClientState.PeerCreated && PhotonNetwork.networkingPeer.State != ClientState.Disconnected && PhotonNetwork.networkingPeer.State != ClientState.Disconnecting) && PhotonNetwork.networkingPeer.State != ClientState.ConnectingToNameServer);
		}
	}

	// Token: 0x17000063 RID: 99
	// (get) Token: 0x060005F3 RID: 1523 RVA: 0x00038D7E File Offset: 0x0003717E
	public static bool connecting
	{
		get
		{
			return PhotonNetwork.networkingPeer.IsInitialConnect && !PhotonNetwork.offlineMode;
		}
	}

	// Token: 0x17000064 RID: 100
	// (get) Token: 0x060005F4 RID: 1524 RVA: 0x00038D9C File Offset: 0x0003719C
	public static bool connectedAndReady
	{
		get
		{
			if (!PhotonNetwork.connected)
			{
				return false;
			}
			if (PhotonNetwork.offlineMode)
			{
				return true;
			}
			ClientState connectionStateDetailed = PhotonNetwork.connectionStateDetailed;
			switch (connectionStateDetailed)
			{
			case ClientState.ConnectingToMasterserver:
			case ClientState.Disconnecting:
			case ClientState.Disconnected:
			case ClientState.ConnectingToNameServer:
			case ClientState.Authenticating:
				break;
			default:
				switch (connectionStateDetailed)
				{
				case ClientState.ConnectingToGameserver:
				case ClientState.Joining:
					break;
				default:
					if (connectionStateDetailed != ClientState.PeerCreated)
					{
						return true;
					}
					break;
				}
				break;
			}
			return false;
		}
	}

	// Token: 0x17000065 RID: 101
	// (get) Token: 0x060005F5 RID: 1525 RVA: 0x00038E18 File Offset: 0x00037218
	public static ConnectionState connectionState
	{
		get
		{
			if (PhotonNetwork.offlineMode)
			{
				return ConnectionState.Connected;
			}
			if (PhotonNetwork.networkingPeer == null)
			{
				return ConnectionState.Disconnected;
			}
			PeerStateValue peerState = PhotonNetwork.networkingPeer.PeerState;
			switch (peerState)
			{
			case PeerStateValue.Disconnected:
				return ConnectionState.Disconnected;
			case PeerStateValue.Connecting:
				return ConnectionState.Connecting;
			default:
				if (peerState != PeerStateValue.InitializingApplication)
				{
					return ConnectionState.Disconnected;
				}
				return ConnectionState.InitializingApplication;
			case PeerStateValue.Connected:
				return ConnectionState.Connected;
			case PeerStateValue.Disconnecting:
				return ConnectionState.Disconnecting;
			}
		}
	}

	// Token: 0x17000066 RID: 102
	// (get) Token: 0x060005F6 RID: 1526 RVA: 0x00038E7A File Offset: 0x0003727A
	public static ClientState connectionStateDetailed
	{
		get
		{
			if (PhotonNetwork.offlineMode)
			{
				return (PhotonNetwork.offlineModeRoom == null) ? ClientState.ConnectedToMaster : ClientState.Joined;
			}
			if (PhotonNetwork.networkingPeer == null)
			{
				return ClientState.Disconnected;
			}
			return PhotonNetwork.networkingPeer.State;
		}
	}

	// Token: 0x17000067 RID: 103
	// (get) Token: 0x060005F7 RID: 1527 RVA: 0x00038EB1 File Offset: 0x000372B1
	public static ServerConnection Server
	{
		get
		{
			return (PhotonNetwork.networkingPeer == null) ? ServerConnection.NameServer : PhotonNetwork.networkingPeer.Server;
		}
	}

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x060005F8 RID: 1528 RVA: 0x00038ECD File Offset: 0x000372CD
	// (set) Token: 0x060005F9 RID: 1529 RVA: 0x00038EE9 File Offset: 0x000372E9
	public static AuthenticationValues AuthValues
	{
		get
		{
			return (PhotonNetwork.networkingPeer == null) ? null : PhotonNetwork.networkingPeer.AuthValues;
		}
		set
		{
			if (PhotonNetwork.networkingPeer != null)
			{
				PhotonNetwork.networkingPeer.AuthValues = value;
			}
		}
	}

	// Token: 0x17000069 RID: 105
	// (get) Token: 0x060005FA RID: 1530 RVA: 0x00038F00 File Offset: 0x00037300
	public static Room room
	{
		get
		{
			if (PhotonNetwork.isOfflineMode)
			{
				return PhotonNetwork.offlineModeRoom;
			}
			return PhotonNetwork.networkingPeer.CurrentRoom;
		}
	}

	// Token: 0x1700006A RID: 106
	// (get) Token: 0x060005FB RID: 1531 RVA: 0x00038F1C File Offset: 0x0003731C
	public static PhotonPlayer player
	{
		get
		{
			if (PhotonNetwork.networkingPeer == null)
			{
				return null;
			}
			return PhotonNetwork.networkingPeer.LocalPlayer;
		}
	}

	// Token: 0x1700006B RID: 107
	// (get) Token: 0x060005FC RID: 1532 RVA: 0x00038F34 File Offset: 0x00037334
	public static PhotonPlayer masterClient
	{
		get
		{
			if (PhotonNetwork.offlineMode)
			{
				return PhotonNetwork.player;
			}
			if (PhotonNetwork.networkingPeer == null)
			{
				return null;
			}
			return PhotonNetwork.networkingPeer.GetPlayerWithId(PhotonNetwork.networkingPeer.mMasterClientId);
		}
	}

	// Token: 0x1700006C RID: 108
	// (get) Token: 0x060005FD RID: 1533 RVA: 0x00038F66 File Offset: 0x00037366
	// (set) Token: 0x060005FE RID: 1534 RVA: 0x00038F72 File Offset: 0x00037372
	public static string playerName
	{
		get
		{
			return PhotonNetwork.networkingPeer.PlayerName;
		}
		set
		{
			PhotonNetwork.networkingPeer.PlayerName = value;
		}
	}

	// Token: 0x1700006D RID: 109
	// (get) Token: 0x060005FF RID: 1535 RVA: 0x00038F7F File Offset: 0x0003737F
	public static PhotonPlayer[] playerList
	{
		get
		{
			if (PhotonNetwork.networkingPeer == null)
			{
				return new PhotonPlayer[0];
			}
			return PhotonNetwork.networkingPeer.mPlayerListCopy;
		}
	}

	// Token: 0x1700006E RID: 110
	// (get) Token: 0x06000600 RID: 1536 RVA: 0x00038F9C File Offset: 0x0003739C
	public static PhotonPlayer[] otherPlayers
	{
		get
		{
			if (PhotonNetwork.networkingPeer == null)
			{
				return new PhotonPlayer[0];
			}
			return PhotonNetwork.networkingPeer.mOtherPlayerListCopy;
		}
	}

	// Token: 0x1700006F RID: 111
	// (get) Token: 0x06000601 RID: 1537 RVA: 0x00038FB9 File Offset: 0x000373B9
	// (set) Token: 0x06000602 RID: 1538 RVA: 0x00038FC0 File Offset: 0x000373C0
	public static List<FriendInfo> Friends { get; internal set; }

	// Token: 0x17000070 RID: 112
	// (get) Token: 0x06000603 RID: 1539 RVA: 0x00038FC8 File Offset: 0x000373C8
	public static int FriendsListAge
	{
		get
		{
			return (PhotonNetwork.networkingPeer == null) ? 0 : PhotonNetwork.networkingPeer.FriendListAge;
		}
	}

	// Token: 0x17000071 RID: 113
	// (get) Token: 0x06000604 RID: 1540 RVA: 0x00038FE4 File Offset: 0x000373E4
	// (set) Token: 0x06000605 RID: 1541 RVA: 0x00038FF0 File Offset: 0x000373F0
	public static IPunPrefabPool PrefabPool
	{
		get
		{
			return PhotonNetwork.networkingPeer.ObjectPool;
		}
		set
		{
			PhotonNetwork.networkingPeer.ObjectPool = value;
		}
	}

	// Token: 0x17000072 RID: 114
	// (get) Token: 0x06000606 RID: 1542 RVA: 0x00038FFD File Offset: 0x000373FD
	// (set) Token: 0x06000607 RID: 1543 RVA: 0x00039004 File Offset: 0x00037404
	public static bool offlineMode
	{
		get
		{
			return PhotonNetwork.isOfflineMode;
		}
		set
		{
			if (value == PhotonNetwork.isOfflineMode)
			{
				return;
			}
			if (value && PhotonNetwork.connected)
			{
				UnityEngine.Debug.LogError("Can't start OFFLINE mode while connected!");
				return;
			}
			if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
			{
				PhotonNetwork.networkingPeer.Disconnect();
			}
			PhotonNetwork.isOfflineMode = value;
			if (PhotonNetwork.isOfflineMode)
			{
				PhotonNetwork.networkingPeer.ChangeLocalID(-1);
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster, new object[0]);
			}
			else
			{
				PhotonNetwork.offlineModeRoom = null;
				PhotonNetwork.networkingPeer.ChangeLocalID(-1);
			}
		}
	}

	// Token: 0x17000073 RID: 115
	// (get) Token: 0x06000608 RID: 1544 RVA: 0x0003908F File Offset: 0x0003748F
	// (set) Token: 0x06000609 RID: 1545 RVA: 0x00039096 File Offset: 0x00037496
	public static bool automaticallySyncScene
	{
		get
		{
			return PhotonNetwork._mAutomaticallySyncScene;
		}
		set
		{
			PhotonNetwork._mAutomaticallySyncScene = value;
			if (PhotonNetwork._mAutomaticallySyncScene && PhotonNetwork.room != null)
			{
				PhotonNetwork.networkingPeer.LoadLevelIfSynced();
			}
		}
	}

	// Token: 0x17000074 RID: 116
	// (get) Token: 0x0600060A RID: 1546 RVA: 0x000390BC File Offset: 0x000374BC
	// (set) Token: 0x0600060B RID: 1547 RVA: 0x000390C3 File Offset: 0x000374C3
	public static bool autoCleanUpPlayerObjects
	{
		get
		{
			return PhotonNetwork.m_autoCleanUpPlayerObjects;
		}
		set
		{
			if (PhotonNetwork.room != null)
			{
				UnityEngine.Debug.LogError("Setting autoCleanUpPlayerObjects while in a room is not supported.");
			}
			else
			{
				PhotonNetwork.m_autoCleanUpPlayerObjects = value;
			}
		}
	}

	// Token: 0x17000075 RID: 117
	// (get) Token: 0x0600060C RID: 1548 RVA: 0x000390E4 File Offset: 0x000374E4
	// (set) Token: 0x0600060D RID: 1549 RVA: 0x000390F0 File Offset: 0x000374F0
	public static bool autoJoinLobby
	{
		get
		{
			return PhotonNetwork.PhotonServerSettings.JoinLobby;
		}
		set
		{
			PhotonNetwork.PhotonServerSettings.JoinLobby = value;
		}
	}

	// Token: 0x17000076 RID: 118
	// (get) Token: 0x0600060E RID: 1550 RVA: 0x000390FD File Offset: 0x000374FD
	// (set) Token: 0x0600060F RID: 1551 RVA: 0x00039109 File Offset: 0x00037509
	public static bool EnableLobbyStatistics
	{
		get
		{
			return PhotonNetwork.PhotonServerSettings.EnableLobbyStatistics;
		}
		set
		{
			PhotonNetwork.PhotonServerSettings.EnableLobbyStatistics = value;
		}
	}

	// Token: 0x17000077 RID: 119
	// (get) Token: 0x06000610 RID: 1552 RVA: 0x00039116 File Offset: 0x00037516
	// (set) Token: 0x06000611 RID: 1553 RVA: 0x00039122 File Offset: 0x00037522
	public static List<TypedLobbyInfo> LobbyStatistics
	{
		get
		{
			return PhotonNetwork.networkingPeer.LobbyStatistics;
		}
		private set
		{
			PhotonNetwork.networkingPeer.LobbyStatistics = value;
		}
	}

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x06000612 RID: 1554 RVA: 0x0003912F File Offset: 0x0003752F
	public static bool insideLobby
	{
		get
		{
			return PhotonNetwork.networkingPeer.insideLobby;
		}
	}

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x06000613 RID: 1555 RVA: 0x0003913B File Offset: 0x0003753B
	// (set) Token: 0x06000614 RID: 1556 RVA: 0x00039147 File Offset: 0x00037547
	public static TypedLobby lobby
	{
		get
		{
			return PhotonNetwork.networkingPeer.lobby;
		}
		set
		{
			PhotonNetwork.networkingPeer.lobby = value;
		}
	}

	// Token: 0x1700007A RID: 122
	// (get) Token: 0x06000615 RID: 1557 RVA: 0x00039154 File Offset: 0x00037554
	// (set) Token: 0x06000616 RID: 1558 RVA: 0x00039161 File Offset: 0x00037561
	public static int sendRate
	{
		get
		{
			return 1000 / PhotonNetwork.sendInterval;
		}
		set
		{
			PhotonNetwork.sendInterval = 1000 / value;
			if (PhotonNetwork.photonMono != null)
			{
				PhotonNetwork.photonMono.updateInterval = PhotonNetwork.sendInterval;
			}
			if (value < PhotonNetwork.sendRateOnSerialize)
			{
				PhotonNetwork.sendRateOnSerialize = value;
			}
		}
	}

	// Token: 0x1700007B RID: 123
	// (get) Token: 0x06000617 RID: 1559 RVA: 0x0003919F File Offset: 0x0003759F
	// (set) Token: 0x06000618 RID: 1560 RVA: 0x000391AC File Offset: 0x000375AC
	public static int sendRateOnSerialize
	{
		get
		{
			return 1000 / PhotonNetwork.sendIntervalOnSerialize;
		}
		set
		{
			if (value > PhotonNetwork.sendRate)
			{
				UnityEngine.Debug.LogError("Error: Can not set the OnSerialize rate higher than the overall SendRate.");
				value = PhotonNetwork.sendRate;
			}
			PhotonNetwork.sendIntervalOnSerialize = 1000 / value;
			if (PhotonNetwork.photonMono != null)
			{
				PhotonNetwork.photonMono.updateIntervalOnSerialize = PhotonNetwork.sendIntervalOnSerialize;
			}
		}
	}

	// Token: 0x1700007C RID: 124
	// (get) Token: 0x06000619 RID: 1561 RVA: 0x00039200 File Offset: 0x00037600
	// (set) Token: 0x0600061A RID: 1562 RVA: 0x00039207 File Offset: 0x00037607
	public static bool isMessageQueueRunning
	{
		get
		{
			return PhotonNetwork.m_isMessageQueueRunning;
		}
		set
		{
			if (value)
			{
				PhotonHandler.StartFallbackSendAckThread();
			}
			PhotonNetwork.networkingPeer.IsSendingOnlyAcks = !value;
			PhotonNetwork.m_isMessageQueueRunning = value;
		}
	}

	// Token: 0x1700007D RID: 125
	// (get) Token: 0x0600061B RID: 1563 RVA: 0x00039228 File Offset: 0x00037628
	// (set) Token: 0x0600061C RID: 1564 RVA: 0x00039234 File Offset: 0x00037634
	public static int unreliableCommandsLimit
	{
		get
		{
			return PhotonNetwork.networkingPeer.LimitOfUnreliableCommands;
		}
		set
		{
			PhotonNetwork.networkingPeer.LimitOfUnreliableCommands = value;
		}
	}

	// Token: 0x1700007E RID: 126
	// (get) Token: 0x0600061D RID: 1565 RVA: 0x00039244 File Offset: 0x00037644
	public static double time
	{
		get
		{
			uint serverTimestamp = (uint)PhotonNetwork.ServerTimestamp;
			double num = serverTimestamp;
			return num / 1000.0;
		}
	}

	// Token: 0x1700007F RID: 127
	// (get) Token: 0x0600061E RID: 1566 RVA: 0x00039268 File Offset: 0x00037668
	public static int ServerTimestamp
	{
		get
		{
			if (!PhotonNetwork.offlineMode)
			{
				return PhotonNetwork.networkingPeer.ServerTimeInMilliSeconds;
			}
			if (PhotonNetwork.UsePreciseTimer && PhotonNetwork.startupStopwatch != null && PhotonNetwork.startupStopwatch.IsRunning)
			{
				return (int)PhotonNetwork.startupStopwatch.ElapsedMilliseconds;
			}
			return Environment.TickCount;
		}
	}

	// Token: 0x17000080 RID: 128
	// (get) Token: 0x0600061F RID: 1567 RVA: 0x000392BE File Offset: 0x000376BE
	public static bool isMasterClient
	{
		get
		{
			return PhotonNetwork.offlineMode || PhotonNetwork.networkingPeer.mMasterClientId == PhotonNetwork.player.ID;
		}
	}

	// Token: 0x17000081 RID: 129
	// (get) Token: 0x06000620 RID: 1568 RVA: 0x000392E2 File Offset: 0x000376E2
	public static bool inRoom
	{
		get
		{
			return PhotonNetwork.connectionStateDetailed == ClientState.Joined;
		}
	}

	// Token: 0x17000082 RID: 130
	// (get) Token: 0x06000621 RID: 1569 RVA: 0x000392ED File Offset: 0x000376ED
	public static bool isNonMasterClientInRoom
	{
		get
		{
			return !PhotonNetwork.isMasterClient && PhotonNetwork.room != null;
		}
	}

	// Token: 0x17000083 RID: 131
	// (get) Token: 0x06000622 RID: 1570 RVA: 0x00039307 File Offset: 0x00037707
	public static int countOfPlayersOnMaster
	{
		get
		{
			return PhotonNetwork.networkingPeer.PlayersOnMasterCount;
		}
	}

	// Token: 0x17000084 RID: 132
	// (get) Token: 0x06000623 RID: 1571 RVA: 0x00039313 File Offset: 0x00037713
	public static int countOfPlayersInRooms
	{
		get
		{
			return PhotonNetwork.networkingPeer.PlayersInRoomsCount;
		}
	}

	// Token: 0x17000085 RID: 133
	// (get) Token: 0x06000624 RID: 1572 RVA: 0x0003931F File Offset: 0x0003771F
	public static int countOfPlayers
	{
		get
		{
			return PhotonNetwork.networkingPeer.PlayersInRoomsCount + PhotonNetwork.networkingPeer.PlayersOnMasterCount;
		}
	}

	// Token: 0x17000086 RID: 134
	// (get) Token: 0x06000625 RID: 1573 RVA: 0x00039336 File Offset: 0x00037736
	public static int countOfRooms
	{
		get
		{
			return PhotonNetwork.networkingPeer.RoomsCount;
		}
	}

	// Token: 0x17000087 RID: 135
	// (get) Token: 0x06000626 RID: 1574 RVA: 0x00039342 File Offset: 0x00037742
	// (set) Token: 0x06000627 RID: 1575 RVA: 0x0003934E File Offset: 0x0003774E
	public static bool NetworkStatisticsEnabled
	{
		get
		{
			return PhotonNetwork.networkingPeer.TrafficStatsEnabled;
		}
		set
		{
			PhotonNetwork.networkingPeer.TrafficStatsEnabled = value;
		}
	}

	// Token: 0x17000088 RID: 136
	// (get) Token: 0x06000628 RID: 1576 RVA: 0x0003935B File Offset: 0x0003775B
	public static int ResentReliableCommands
	{
		get
		{
			return PhotonNetwork.networkingPeer.ResentReliableCommands;
		}
	}

	// Token: 0x17000089 RID: 137
	// (get) Token: 0x06000629 RID: 1577 RVA: 0x00039367 File Offset: 0x00037767
	// (set) Token: 0x0600062A RID: 1578 RVA: 0x00039374 File Offset: 0x00037774
	public static bool CrcCheckEnabled
	{
		get
		{
			return PhotonNetwork.networkingPeer.CrcEnabled;
		}
		set
		{
			if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
			{
				PhotonNetwork.networkingPeer.CrcEnabled = value;
			}
			else
			{
				UnityEngine.Debug.Log("Can't change CrcCheckEnabled while being connected. CrcCheckEnabled stays " + PhotonNetwork.networkingPeer.CrcEnabled);
			}
		}
	}

	// Token: 0x1700008A RID: 138
	// (get) Token: 0x0600062B RID: 1579 RVA: 0x000393C3 File Offset: 0x000377C3
	public static int PacketLossByCrcCheck
	{
		get
		{
			return PhotonNetwork.networkingPeer.PacketLossByCrc;
		}
	}

	// Token: 0x1700008B RID: 139
	// (get) Token: 0x0600062C RID: 1580 RVA: 0x000393CF File Offset: 0x000377CF
	// (set) Token: 0x0600062D RID: 1581 RVA: 0x000393DB File Offset: 0x000377DB
	public static int MaxResendsBeforeDisconnect
	{
		get
		{
			return PhotonNetwork.networkingPeer.SentCountAllowance;
		}
		set
		{
			if (value < 3)
			{
				value = 3;
			}
			if (value > 10)
			{
				value = 10;
			}
			PhotonNetwork.networkingPeer.SentCountAllowance = value;
		}
	}

	// Token: 0x1700008C RID: 140
	// (get) Token: 0x0600062E RID: 1582 RVA: 0x000393FE File Offset: 0x000377FE
	// (set) Token: 0x0600062F RID: 1583 RVA: 0x0003940A File Offset: 0x0003780A
	public static int QuickResends
	{
		get
		{
			return (int)PhotonNetwork.networkingPeer.QuickResendAttempts;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (value > 3)
			{
				value = 3;
			}
			PhotonNetwork.networkingPeer.QuickResendAttempts = (byte)value;
		}
	}

	// Token: 0x1700008D RID: 141
	// (get) Token: 0x06000630 RID: 1584 RVA: 0x0003942C File Offset: 0x0003782C
	// (set) Token: 0x06000631 RID: 1585 RVA: 0x00039433 File Offset: 0x00037833
	public static bool UseAlternativeUdpPorts { get; set; }

	// Token: 0x06000632 RID: 1586 RVA: 0x0003943B File Offset: 0x0003783B
	public static void SwitchToProtocol(ConnectionProtocol cp)
	{
		PhotonNetwork.networkingPeer.TransportProtocol = cp;
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x00039448 File Offset: 0x00037848
	public static bool ConnectUsingSettings(string gameVersion)
	{
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings == null)
		{
			UnityEngine.Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: PhotonServerSettings");
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.NotSet)
		{
			UnityEngine.Debug.LogError("You did not select a Hosting Type in your PhotonServerSettings. Please set it up or don't use ConnectUsingSettings().");
			return false;
		}
		if (PhotonNetwork.logLevel == PhotonLogLevel.ErrorsOnly)
		{
			PhotonNetwork.logLevel = PhotonNetwork.PhotonServerSettings.PunLogging;
		}
		if (PhotonNetwork.networkingPeer.DebugOut == DebugLevel.ERROR)
		{
			PhotonNetwork.networkingPeer.DebugOut = PhotonNetwork.PhotonServerSettings.NetworkLogging;
		}
		PhotonNetwork.SwitchToProtocol(PhotonNetwork.PhotonServerSettings.Protocol);
		PhotonNetwork.networkingPeer.SetApp(PhotonNetwork.PhotonServerSettings.AppID, gameVersion);
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
		{
			PhotonNetwork.offlineMode = true;
			return true;
		}
		if (PhotonNetwork.offlineMode)
		{
			UnityEngine.Debug.LogWarning("ConnectUsingSettings() disabled the offline mode. No longer offline.");
		}
		PhotonNetwork.offlineMode = false;
		PhotonNetwork.isMessageQueueRunning = true;
		PhotonNetwork.networkingPeer.IsInitialConnect = true;
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.SelfHosted)
		{
			PhotonNetwork.networkingPeer.IsUsingNameServer = false;
			PhotonNetwork.networkingPeer.MasterServerAddress = ((PhotonNetwork.PhotonServerSettings.ServerPort != 0) ? (PhotonNetwork.PhotonServerSettings.ServerAddress + ":" + PhotonNetwork.PhotonServerSettings.ServerPort) : PhotonNetwork.PhotonServerSettings.ServerAddress);
			return PhotonNetwork.networkingPeer.Connect(PhotonNetwork.networkingPeer.MasterServerAddress, ServerConnection.MasterServer);
		}
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.BestRegion)
		{
			return PhotonNetwork.ConnectToBestCloudServer(gameVersion);
		}
		return PhotonNetwork.networkingPeer.ConnectToRegionMaster(PhotonNetwork.PhotonServerSettings.PreferredRegion);
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x00039608 File Offset: 0x00037A08
	public static bool ConnectToMaster(string masterServerAddress, int port, string appID, string gameVersion)
	{
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("ConnectToMaster() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = false;
			UnityEngine.Debug.LogWarning("ConnectToMaster() disabled the offline mode. No longer offline.");
		}
		if (!PhotonNetwork.isMessageQueueRunning)
		{
			PhotonNetwork.isMessageQueueRunning = true;
			UnityEngine.Debug.LogWarning("ConnectToMaster() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
		}
		PhotonNetwork.networkingPeer.SetApp(appID, gameVersion);
		PhotonNetwork.networkingPeer.IsUsingNameServer = false;
		PhotonNetwork.networkingPeer.IsInitialConnect = true;
		PhotonNetwork.networkingPeer.MasterServerAddress = ((port != 0) ? (masterServerAddress + ":" + port) : masterServerAddress);
		return PhotonNetwork.networkingPeer.Connect(PhotonNetwork.networkingPeer.MasterServerAddress, ServerConnection.MasterServer);
	}

	// Token: 0x06000635 RID: 1589 RVA: 0x000396D8 File Offset: 0x00037AD8
	public static bool Reconnect()
	{
		if (string.IsNullOrEmpty(PhotonNetwork.networkingPeer.MasterServerAddress))
		{
			UnityEngine.Debug.LogWarning("Reconnect() failed. It seems the client wasn't connected before?! Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("Reconnect() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = false;
			UnityEngine.Debug.LogWarning("Reconnect() disabled the offline mode. No longer offline.");
		}
		if (!PhotonNetwork.isMessageQueueRunning)
		{
			PhotonNetwork.isMessageQueueRunning = true;
			UnityEngine.Debug.LogWarning("Reconnect() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
		}
		PhotonNetwork.networkingPeer.IsUsingNameServer = false;
		PhotonNetwork.networkingPeer.IsInitialConnect = false;
		return PhotonNetwork.networkingPeer.ReconnectToMaster();
	}

	// Token: 0x06000636 RID: 1590 RVA: 0x0003979C File Offset: 0x00037B9C
	public static bool ReconnectAndRejoin()
	{
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("ReconnectAndRejoin() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = false;
			UnityEngine.Debug.LogWarning("ReconnectAndRejoin() disabled the offline mode. No longer offline.");
		}
		if (string.IsNullOrEmpty(PhotonNetwork.networkingPeer.GameServerAddress))
		{
			UnityEngine.Debug.LogWarning("ReconnectAndRejoin() failed. It seems the client wasn't connected to a game server before (no address).");
			return false;
		}
		if (PhotonNetwork.networkingPeer.enterRoomParamsCache == null)
		{
			UnityEngine.Debug.LogWarning("ReconnectAndRejoin() failed. It seems the client doesn't have any previous room to re-join.");
			return false;
		}
		if (!PhotonNetwork.isMessageQueueRunning)
		{
			PhotonNetwork.isMessageQueueRunning = true;
			UnityEngine.Debug.LogWarning("ReconnectAndRejoin() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
		}
		PhotonNetwork.networkingPeer.IsUsingNameServer = false;
		PhotonNetwork.networkingPeer.IsInitialConnect = false;
		return PhotonNetwork.networkingPeer.ReconnectAndRejoin();
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x00039868 File Offset: 0x00037C68
	public static bool ConnectToBestCloudServer(string gameVersion)
	{
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("ConnectToBestCloudServer() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings == null)
		{
			UnityEngine.Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: PhotonServerSettings");
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
		{
			return PhotonNetwork.ConnectUsingSettings(gameVersion);
		}
		PhotonNetwork.networkingPeer.IsInitialConnect = true;
		PhotonNetwork.networkingPeer.SetApp(PhotonNetwork.PhotonServerSettings.AppID, gameVersion);
		return PhotonNetwork.networkingPeer.ConnectToNameServer();
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x00039904 File Offset: 0x00037D04
	public static bool ConnectToRegion(CloudRegionCode region, string gameVersion)
	{
		if (PhotonNetwork.networkingPeer.PeerState != PeerStateValue.Disconnected)
		{
			UnityEngine.Debug.LogWarning("ConnectToRegion() failed. Can only connect while in state 'Disconnected'. Current state: " + PhotonNetwork.networkingPeer.PeerState);
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings == null)
		{
			UnityEngine.Debug.LogError("Can't connect: ServerSettings asset must be in any 'Resources' folder as: PhotonServerSettings");
			return false;
		}
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
		{
			return PhotonNetwork.ConnectUsingSettings(gameVersion);
		}
		PhotonNetwork.networkingPeer.IsInitialConnect = true;
		PhotonNetwork.networkingPeer.SetApp(PhotonNetwork.PhotonServerSettings.AppID, gameVersion);
		if (region != CloudRegionCode.none)
		{
			UnityEngine.Debug.Log("ConnectToRegion: " + region);
			return PhotonNetwork.networkingPeer.ConnectToRegionMaster(region);
		}
		return false;
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x000399BC File Offset: 0x00037DBC
	public static void OverrideBestCloudServer(CloudRegionCode region)
	{
		PhotonHandler.BestRegionCodeInPreferences = region;
	}

	// Token: 0x0600063A RID: 1594 RVA: 0x000399C4 File Offset: 0x00037DC4
	public static void RefreshCloudServerRating()
	{
		throw new NotImplementedException("not available at the moment");
	}

	// Token: 0x0600063B RID: 1595 RVA: 0x000399D0 File Offset: 0x00037DD0
	public static void NetworkStatisticsReset()
	{
		PhotonNetwork.networkingPeer.TrafficStatsReset();
	}

	// Token: 0x0600063C RID: 1596 RVA: 0x000399DC File Offset: 0x00037DDC
	public static string NetworkStatisticsToString()
	{
		if (PhotonNetwork.networkingPeer == null || PhotonNetwork.offlineMode)
		{
			return "Offline or in OfflineMode. No VitalStats available.";
		}
		return PhotonNetwork.networkingPeer.VitalStatsToString(false);
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x00039A03 File Offset: 0x00037E03
	[Obsolete("Used for compatibility with Unity networking only. Encryption is automatically initialized while connecting.")]
	public static void InitializeSecurity()
	{
	}

	// Token: 0x0600063E RID: 1598 RVA: 0x00039A05 File Offset: 0x00037E05
	private static bool VerifyCanUseNetwork()
	{
		if (PhotonNetwork.connected)
		{
			return true;
		}
		UnityEngine.Debug.LogError("Cannot send messages when not connected. Either connect to Photon OR use offline mode!");
		return false;
	}

	// Token: 0x0600063F RID: 1599 RVA: 0x00039A20 File Offset: 0x00037E20
	public static void Disconnect()
	{
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineMode = false;
			PhotonNetwork.offlineModeRoom = null;
			PhotonNetwork.networkingPeer.State = ClientState.Disconnecting;
			PhotonNetwork.networkingPeer.OnStatusChanged(StatusCode.Disconnect);
			return;
		}
		if (PhotonNetwork.networkingPeer == null)
		{
			return;
		}
		PhotonNetwork.networkingPeer.Disconnect();
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x00039A74 File Offset: 0x00037E74
	public static bool FindFriends(string[] friendsToFind)
	{
		return PhotonNetwork.networkingPeer != null && !PhotonNetwork.isOfflineMode && PhotonNetwork.networkingPeer.OpFindFriends(friendsToFind);
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x00039A97 File Offset: 0x00037E97
	public static bool CreateRoom(string roomName)
	{
		return PhotonNetwork.CreateRoom(roomName, null, null, null);
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x00039AA2 File Offset: 0x00037EA2
	public static bool CreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby)
	{
		return PhotonNetwork.CreateRoom(roomName, roomOptions, typedLobby, null);
	}

	// Token: 0x06000643 RID: 1603 RVA: 0x00039AB0 File Offset: 0x00037EB0
	public static bool CreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby, string[] expectedUsers)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.offlineModeRoom != null)
			{
				UnityEngine.Debug.LogError("CreateRoom failed. In offline mode you still have to leave a room to enter another.");
				return false;
			}
			PhotonNetwork.EnterOfflineRoom(roomName, roomOptions, true);
			return true;
		}
		else
		{
			if (PhotonNetwork.networkingPeer.Server != ServerConnection.MasterServer || !PhotonNetwork.connectedAndReady)
			{
				UnityEngine.Debug.LogError("CreateRoom failed. Client is not on Master Server or not yet ready to call operations. Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
				return false;
			}
			typedLobby = (typedLobby ?? ((!PhotonNetwork.networkingPeer.insideLobby) ? null : PhotonNetwork.networkingPeer.lobby));
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.RoomOptions = roomOptions;
			enterRoomParams.Lobby = typedLobby;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return PhotonNetwork.networkingPeer.OpCreateGame(enterRoomParams);
		}
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x00039B62 File Offset: 0x00037F62
	public static bool JoinRoom(string roomName)
	{
		return PhotonNetwork.JoinRoom(roomName, null);
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x00039B6C File Offset: 0x00037F6C
	public static bool JoinRoom(string roomName, string[] expectedUsers)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.offlineModeRoom != null)
			{
				UnityEngine.Debug.LogError("JoinRoom failed. In offline mode you still have to leave a room to enter another.");
				return false;
			}
			PhotonNetwork.EnterOfflineRoom(roomName, null, true);
			return true;
		}
		else
		{
			if (PhotonNetwork.networkingPeer.Server != ServerConnection.MasterServer || !PhotonNetwork.connectedAndReady)
			{
				UnityEngine.Debug.LogError("JoinRoom failed. Client is not on Master Server or not yet ready to call operations. Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
				return false;
			}
			if (string.IsNullOrEmpty(roomName))
			{
				UnityEngine.Debug.LogError("JoinRoom failed. A roomname is required. If you don't know one, how will you join?");
				return false;
			}
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return PhotonNetwork.networkingPeer.OpJoinRoom(enterRoomParams);
		}
	}

	// Token: 0x06000646 RID: 1606 RVA: 0x00039BFE File Offset: 0x00037FFE
	public static bool JoinOrCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby)
	{
		return PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby, null);
	}

	// Token: 0x06000647 RID: 1607 RVA: 0x00039C0C File Offset: 0x0003800C
	public static bool JoinOrCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby, string[] expectedUsers)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.offlineModeRoom != null)
			{
				UnityEngine.Debug.LogError("JoinOrCreateRoom failed. In offline mode you still have to leave a room to enter another.");
				return false;
			}
			PhotonNetwork.EnterOfflineRoom(roomName, roomOptions, true);
			return true;
		}
		else
		{
			if (PhotonNetwork.networkingPeer.Server != ServerConnection.MasterServer || !PhotonNetwork.connectedAndReady)
			{
				UnityEngine.Debug.LogError("JoinOrCreateRoom failed. Client is not on Master Server or not yet ready to call operations. Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
				return false;
			}
			if (string.IsNullOrEmpty(roomName))
			{
				UnityEngine.Debug.LogError("JoinOrCreateRoom failed. A roomname is required. If you don't know one, how will you join?");
				return false;
			}
			typedLobby = (typedLobby ?? ((!PhotonNetwork.networkingPeer.insideLobby) ? null : PhotonNetwork.networkingPeer.lobby));
			EnterRoomParams enterRoomParams = new EnterRoomParams();
			enterRoomParams.RoomName = roomName;
			enterRoomParams.RoomOptions = roomOptions;
			enterRoomParams.Lobby = typedLobby;
			enterRoomParams.CreateIfNotExists = true;
			enterRoomParams.PlayerProperties = PhotonNetwork.player.CustomProperties;
			enterRoomParams.ExpectedUsers = expectedUsers;
			return PhotonNetwork.networkingPeer.OpJoinRoom(enterRoomParams);
		}
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x00039CEC File Offset: 0x000380EC
	public static bool JoinRandomRoom()
	{
		return PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, null, null, null);
	}

	// Token: 0x06000649 RID: 1609 RVA: 0x00039CF9 File Offset: 0x000380F9
	public static bool JoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers)
	{
		return PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, expectedMaxPlayers, MatchmakingMode.FillRoom, null, null, null);
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x00039D08 File Offset: 0x00038108
	public static bool JoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter, string[] expectedUsers = null)
	{
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.offlineModeRoom != null)
			{
				UnityEngine.Debug.LogError("JoinRandomRoom failed. In offline mode you still have to leave a room to enter another.");
				return false;
			}
			PhotonNetwork.EnterOfflineRoom("offline room", null, true);
			return true;
		}
		else
		{
			if (PhotonNetwork.networkingPeer.Server != ServerConnection.MasterServer || !PhotonNetwork.connectedAndReady)
			{
				UnityEngine.Debug.LogError("JoinRandomRoom failed. Client is not on Master Server or not yet ready to call operations. Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
				return false;
			}
			typedLobby = (typedLobby ?? ((!PhotonNetwork.networkingPeer.insideLobby) ? null : PhotonNetwork.networkingPeer.lobby));
			OpJoinRandomRoomParams opJoinRandomRoomParams = new OpJoinRandomRoomParams();
			opJoinRandomRoomParams.ExpectedCustomRoomProperties = expectedCustomRoomProperties;
			opJoinRandomRoomParams.ExpectedMaxPlayers = expectedMaxPlayers;
			opJoinRandomRoomParams.MatchingType = matchingType;
			opJoinRandomRoomParams.TypedLobby = typedLobby;
			opJoinRandomRoomParams.SqlLobbyFilter = sqlLobbyFilter;
			opJoinRandomRoomParams.ExpectedUsers = expectedUsers;
			return PhotonNetwork.networkingPeer.OpJoinRandomRoom(opJoinRandomRoomParams);
		}
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x00039DD0 File Offset: 0x000381D0
	public static bool ReJoinRoom(string roomName)
	{
		if (PhotonNetwork.offlineMode)
		{
			UnityEngine.Debug.LogError("ReJoinRoom failed due to offline mode.");
			return false;
		}
		if (PhotonNetwork.networkingPeer.Server != ServerConnection.MasterServer || !PhotonNetwork.connectedAndReady)
		{
			UnityEngine.Debug.LogError("ReJoinRoom failed. Client is not on Master Server or not yet ready to call operations. Wait for callback: OnJoinedLobby or OnConnectedToMaster.");
			return false;
		}
		if (string.IsNullOrEmpty(roomName))
		{
			UnityEngine.Debug.LogError("ReJoinRoom failed. A roomname is required. If you don't know one, how will you join?");
			return false;
		}
		EnterRoomParams enterRoomParams = new EnterRoomParams();
		enterRoomParams.RoomName = roomName;
		enterRoomParams.RejoinOnly = true;
		enterRoomParams.PlayerProperties = PhotonNetwork.player.CustomProperties;
		return PhotonNetwork.networkingPeer.OpJoinRoom(enterRoomParams);
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x00039E60 File Offset: 0x00038260
	private static void EnterOfflineRoom(string roomName, RoomOptions roomOptions, bool createdRoom)
	{
		PhotonNetwork.offlineModeRoom = new Room(roomName, roomOptions);
		PhotonNetwork.networkingPeer.ChangeLocalID(1);
		PhotonNetwork.networkingPeer.State = ClientState.ConnectingToGameserver;
		PhotonNetwork.networkingPeer.OnStatusChanged(StatusCode.Connect);
		PhotonNetwork.offlineModeRoom.MasterClientId = 1;
		if (createdRoom)
		{
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom, new object[0]);
		}
		NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom, new object[0]);
	}

	// Token: 0x0600064D RID: 1613 RVA: 0x00039EC8 File Offset: 0x000382C8
	public static bool JoinLobby()
	{
		return PhotonNetwork.JoinLobby(null);
	}

	// Token: 0x0600064E RID: 1614 RVA: 0x00039ED0 File Offset: 0x000382D0
	public static bool JoinLobby(TypedLobby typedLobby)
	{
		if (PhotonNetwork.connected && PhotonNetwork.Server == ServerConnection.MasterServer)
		{
			if (typedLobby == null)
			{
				typedLobby = TypedLobby.Default;
			}
			bool flag = PhotonNetwork.networkingPeer.OpJoinLobby(typedLobby);
			if (flag)
			{
				PhotonNetwork.networkingPeer.lobby = typedLobby;
			}
			return flag;
		}
		return false;
	}

	// Token: 0x0600064F RID: 1615 RVA: 0x00039F1E File Offset: 0x0003831E
	public static bool LeaveLobby()
	{
		return PhotonNetwork.connected && PhotonNetwork.Server == ServerConnection.MasterServer && PhotonNetwork.networkingPeer.OpLeaveLobby();
	}

	// Token: 0x06000650 RID: 1616 RVA: 0x00039F40 File Offset: 0x00038340
	public static bool LeaveRoom(bool becomeInactive = true)
	{
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.offlineModeRoom = null;
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom, new object[0]);
			return true;
		}
		if (PhotonNetwork.room == null)
		{
			UnityEngine.Debug.LogWarning("PhotonNetwork.room is null. You don't have to call LeaveRoom() when you're not in one. State: " + PhotonNetwork.connectionStateDetailed);
		}
		else
		{
			becomeInactive = (becomeInactive && PhotonNetwork.room.PlayerTtl != 0);
		}
		return PhotonNetwork.networkingPeer.OpLeaveRoom(becomeInactive);
	}

	// Token: 0x06000651 RID: 1617 RVA: 0x00039FBE File Offset: 0x000383BE
	public static bool GetCustomRoomList(TypedLobby typedLobby, string sqlLobbyFilter)
	{
		return PhotonNetwork.networkingPeer.OpGetGameList(typedLobby, sqlLobbyFilter);
	}

	// Token: 0x06000652 RID: 1618 RVA: 0x00039FCC File Offset: 0x000383CC
	public static RoomInfo[] GetRoomList()
	{
		if (PhotonNetwork.offlineMode || PhotonNetwork.networkingPeer == null)
		{
			return new RoomInfo[0];
		}
		return PhotonNetwork.networkingPeer.mGameListCopy;
	}

	// Token: 0x06000653 RID: 1619 RVA: 0x00039FF4 File Offset: 0x000383F4
	public static void SetPlayerCustomProperties(Hashtable customProperties)
	{
		if (customProperties == null)
		{
			customProperties = new Hashtable();
			foreach (object obj in PhotonNetwork.player.CustomProperties.Keys)
			{
				customProperties[(string)obj] = null;
			}
		}
		if (PhotonNetwork.room != null && PhotonNetwork.room.IsLocalClientInside)
		{
			PhotonNetwork.player.SetCustomProperties(customProperties, null, false);
		}
		else
		{
			PhotonNetwork.player.InternalCacheProperties(customProperties);
		}
	}

	// Token: 0x06000654 RID: 1620 RVA: 0x0003A0A4 File Offset: 0x000384A4
	public static void RemovePlayerCustomProperties(string[] customPropertiesToDelete)
	{
		if (customPropertiesToDelete == null || customPropertiesToDelete.Length == 0 || PhotonNetwork.player.CustomProperties == null)
		{
			PhotonNetwork.player.CustomProperties = new Hashtable();
			return;
		}
		foreach (string key in customPropertiesToDelete)
		{
			if (PhotonNetwork.player.CustomProperties.ContainsKey(key))
			{
				PhotonNetwork.player.CustomProperties.Remove(key);
			}
		}
	}

	// Token: 0x06000655 RID: 1621 RVA: 0x0003A11C File Offset: 0x0003851C
	public static bool RaiseEvent(byte eventCode, object eventContent, bool sendReliable, RaiseEventOptions options)
	{
		if (!PhotonNetwork.inRoom || eventCode >= 200)
		{
			UnityEngine.Debug.LogWarning("RaiseEvent() failed. Your event is not being sent! Check if your are in a Room and the eventCode must be less than 200 (0..199).");
			return false;
		}
		return PhotonNetwork.networkingPeer.OpRaiseEvent(eventCode, eventContent, sendReliable, options);
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x0003A150 File Offset: 0x00038550
	public static int AllocateViewID()
	{
		int num = PhotonNetwork.AllocateViewID(PhotonNetwork.player.ID);
		PhotonNetwork.manuallyAllocatedViewIds.Add(num);
		return num;
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x0003A17C File Offset: 0x0003857C
	public static int AllocateSceneViewID()
	{
		if (!PhotonNetwork.isMasterClient)
		{
			UnityEngine.Debug.LogError("Only the Master Client can AllocateSceneViewID(). Check PhotonNetwork.isMasterClient!");
			return -1;
		}
		int num = PhotonNetwork.AllocateViewID(0);
		PhotonNetwork.manuallyAllocatedViewIds.Add(num);
		return num;
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x0003A1B4 File Offset: 0x000385B4
	private static int AllocateViewID(int ownerId)
	{
		if (ownerId == 0)
		{
			int num = PhotonNetwork.lastUsedViewSubIdStatic;
			int num2 = ownerId * PhotonNetwork.MAX_VIEW_IDS;
			for (int i = 1; i < PhotonNetwork.MAX_VIEW_IDS; i++)
			{
				num = (num + 1) % PhotonNetwork.MAX_VIEW_IDS;
				if (num != 0)
				{
					int num3 = num + num2;
					if (!PhotonNetwork.networkingPeer.photonViewList.ContainsKey(num3))
					{
						PhotonNetwork.lastUsedViewSubIdStatic = num;
						return num3;
					}
				}
			}
			throw new Exception(string.Format("AllocateViewID() failed. Room (user {0}) is out of 'scene' viewIDs. It seems all available are in use.", ownerId));
		}
		int num4 = PhotonNetwork.lastUsedViewSubId;
		int num5 = ownerId * PhotonNetwork.MAX_VIEW_IDS;
		for (int j = 1; j < PhotonNetwork.MAX_VIEW_IDS; j++)
		{
			num4 = (num4 + 1) % PhotonNetwork.MAX_VIEW_IDS;
			if (num4 != 0)
			{
				int num6 = num4 + num5;
				if (!PhotonNetwork.networkingPeer.photonViewList.ContainsKey(num6) && !PhotonNetwork.manuallyAllocatedViewIds.Contains(num6))
				{
					PhotonNetwork.lastUsedViewSubId = num4;
					return num6;
				}
			}
		}
		throw new Exception(string.Format("AllocateViewID() failed. User {0} is out of subIds, as all viewIDs are used.", ownerId));
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x0003A2C8 File Offset: 0x000386C8
	private static int[] AllocateSceneViewIDs(int countOfNewViews)
	{
		int[] array = new int[countOfNewViews];
		for (int i = 0; i < countOfNewViews; i++)
		{
			array[i] = PhotonNetwork.AllocateViewID(0);
		}
		return array;
	}

	// Token: 0x0600065A RID: 1626 RVA: 0x0003A2F8 File Offset: 0x000386F8
	public static void UnAllocateViewID(int viewID)
	{
		PhotonNetwork.manuallyAllocatedViewIds.Remove(viewID);
		if (PhotonNetwork.networkingPeer.photonViewList.ContainsKey(viewID))
		{
			UnityEngine.Debug.LogWarning(string.Format("UnAllocateViewID() should be called after the PhotonView was destroyed (GameObject.Destroy()). ViewID: {0} still found in: {1}", viewID, PhotonNetwork.networkingPeer.photonViewList[viewID]));
		}
	}

	// Token: 0x0600065B RID: 1627 RVA: 0x0003A34B File Offset: 0x0003874B
	public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group)
	{
		return PhotonNetwork.Instantiate(prefabName, position, rotation, group, null);
	}

	// Token: 0x0600065C RID: 1628 RVA: 0x0003A358 File Offset: 0x00038758
	public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data)
	{
		if (!PhotonNetwork.connected || (PhotonNetwork.InstantiateInRoomOnly && !PhotonNetwork.inRoom))
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"Failed to Instantiate prefab: ",
				prefabName,
				". Client should be in a room. Current connectionStateDetailed: ",
				PhotonNetwork.connectionStateDetailed
			}));
			return null;
		}
		GameObject gameObject;
		if (!PhotonNetwork.UsePrefabCache || !PhotonNetwork.PrefabCache.TryGetValue(prefabName, out gameObject))
		{
			gameObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
			if (PhotonNetwork.UsePrefabCache)
			{
				PhotonNetwork.PrefabCache.Add(prefabName, gameObject);
			}
		}
		if (gameObject == null)
		{
			UnityEngine.Debug.LogError("Failed to Instantiate prefab: " + prefabName + ". Verify the Prefab is in a Resources folder (and not in a subfolder)");
			return null;
		}
		if (gameObject.GetComponent<PhotonView>() == null)
		{
			UnityEngine.Debug.LogError("Failed to Instantiate prefab:" + prefabName + ". Prefab must have a PhotonView component.");
			return null;
		}
		Component[] photonViewsInChildren = gameObject.GetPhotonViewsInChildren();
		int[] array = new int[photonViewsInChildren.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PhotonNetwork.AllocateViewID(PhotonNetwork.player.ID);
		}
		Hashtable evData = PhotonNetwork.networkingPeer.SendInstantiate(prefabName, position, rotation, group, array, data, false);
		return PhotonNetwork.networkingPeer.DoInstantiate(evData, PhotonNetwork.networkingPeer.LocalPlayer, gameObject);
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x0003A4AC File Offset: 0x000388AC
	public static GameObject InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data)
	{
		if (!PhotonNetwork.connected || (PhotonNetwork.InstantiateInRoomOnly && !PhotonNetwork.inRoom))
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"Failed to InstantiateSceneObject prefab: ",
				prefabName,
				". Client should be in a room. Current connectionStateDetailed: ",
				PhotonNetwork.connectionStateDetailed
			}));
			return null;
		}
		if (!PhotonNetwork.isMasterClient)
		{
			UnityEngine.Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". Client is not the MasterClient in this room.");
			return null;
		}
		GameObject gameObject;
		if (!PhotonNetwork.UsePrefabCache || !PhotonNetwork.PrefabCache.TryGetValue(prefabName, out gameObject))
		{
			gameObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
			if (PhotonNetwork.UsePrefabCache)
			{
				PhotonNetwork.PrefabCache.Add(prefabName, gameObject);
			}
		}
		if (gameObject == null)
		{
			UnityEngine.Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". Verify the Prefab is in a Resources folder (and not in a subfolder)");
			return null;
		}
		if (gameObject.GetComponent<PhotonView>() == null)
		{
			UnityEngine.Debug.LogError("Failed to InstantiateSceneObject prefab:" + prefabName + ". Prefab must have a PhotonView component.");
			return null;
		}
		Component[] photonViewsInChildren = gameObject.GetPhotonViewsInChildren();
		int[] array = PhotonNetwork.AllocateSceneViewIDs(photonViewsInChildren.Length);
		if (array == null)
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"Failed to InstantiateSceneObject prefab: ",
				prefabName,
				". No ViewIDs are free to use. Max is: ",
				PhotonNetwork.MAX_VIEW_IDS
			}));
			return null;
		}
		Hashtable evData = PhotonNetwork.networkingPeer.SendInstantiate(prefabName, position, rotation, group, array, data, true);
		return PhotonNetwork.networkingPeer.DoInstantiate(evData, PhotonNetwork.networkingPeer.LocalPlayer, gameObject);
	}

	// Token: 0x0600065E RID: 1630 RVA: 0x0003A630 File Offset: 0x00038A30
	public static int GetPing()
	{
		return PhotonNetwork.networkingPeer.RoundTripTime;
	}

	// Token: 0x0600065F RID: 1631 RVA: 0x0003A63C File Offset: 0x00038A3C
	public static void FetchServerTimestamp()
	{
		if (PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.FetchServerTimestamp();
		}
	}

	// Token: 0x06000660 RID: 1632 RVA: 0x0003A652 File Offset: 0x00038A52
	public static void SendOutgoingCommands()
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		while (PhotonNetwork.networkingPeer.SendOutgoingCommands())
		{
		}
	}

	// Token: 0x06000661 RID: 1633 RVA: 0x0003A674 File Offset: 0x00038A74
	public static bool CloseConnection(PhotonPlayer kickPlayer)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return false;
		}
		if (!PhotonNetwork.player.IsMasterClient)
		{
			UnityEngine.Debug.LogError("CloseConnection: Only the masterclient can kick another player.");
			return false;
		}
		if (kickPlayer == null)
		{
			UnityEngine.Debug.LogError("CloseConnection: No such player connected!");
			return false;
		}
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			TargetActors = new int[]
			{
				kickPlayer.ID
			}
		};
		return PhotonNetwork.networkingPeer.OpRaiseEvent(203, null, true, raiseEventOptions);
	}

	// Token: 0x06000662 RID: 1634 RVA: 0x0003A6EC File Offset: 0x00038AEC
	public static bool SetMasterClient(PhotonPlayer masterClientPlayer)
	{
		if (!PhotonNetwork.inRoom || !PhotonNetwork.VerifyCanUseNetwork() || PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.logLevel == PhotonLogLevel.Informational)
			{
				UnityEngine.Debug.Log("Can not SetMasterClient(). Not in room or in offlineMode.");
			}
			return false;
		}
		if (PhotonNetwork.room.serverSideMasterClient)
		{
			Hashtable gameProperties = new Hashtable
			{
				{
					248,
					masterClientPlayer.ID
				}
			};
			Hashtable expectedProperties = new Hashtable
			{
				{
					248,
					PhotonNetwork.networkingPeer.mMasterClientId
				}
			};
			return PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(gameProperties, expectedProperties, false);
		}
		return PhotonNetwork.isMasterClient && PhotonNetwork.networkingPeer.SetMasterClient(masterClientPlayer.ID, true);
	}

	// Token: 0x06000663 RID: 1635 RVA: 0x0003A7B2 File Offset: 0x00038BB2
	public static void Destroy(PhotonView targetView)
	{
		if (targetView != null)
		{
			PhotonNetwork.networkingPeer.RemoveInstantiatedGO(targetView.gameObject, !PhotonNetwork.inRoom);
		}
		else
		{
			UnityEngine.Debug.LogError("Destroy(targetPhotonView) failed, cause targetPhotonView is null.");
		}
	}

	// Token: 0x06000664 RID: 1636 RVA: 0x0003A7E7 File Offset: 0x00038BE7
	public static void Destroy(GameObject targetGo)
	{
		PhotonNetwork.networkingPeer.RemoveInstantiatedGO(targetGo, !PhotonNetwork.inRoom);
	}

	// Token: 0x06000665 RID: 1637 RVA: 0x0003A7FC File Offset: 0x00038BFC
	public static void DestroyPlayerObjects(PhotonPlayer targetPlayer)
	{
		if (PhotonNetwork.player == null)
		{
			UnityEngine.Debug.LogError("DestroyPlayerObjects() failed, cause parameter 'targetPlayer' was null.");
		}
		PhotonNetwork.DestroyPlayerObjects(targetPlayer.ID);
	}

	// Token: 0x06000666 RID: 1638 RVA: 0x0003A820 File Offset: 0x00038C20
	public static void DestroyPlayerObjects(int targetPlayerId)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		if (PhotonNetwork.player.IsMasterClient || targetPlayerId == PhotonNetwork.player.ID)
		{
			PhotonNetwork.networkingPeer.DestroyPlayerObjects(targetPlayerId, false);
		}
		else
		{
			UnityEngine.Debug.LogError("DestroyPlayerObjects() failed, cause players can only destroy their own GameObjects. A Master Client can destroy anyone's. This is master: " + PhotonNetwork.isMasterClient);
		}
	}

	// Token: 0x06000667 RID: 1639 RVA: 0x0003A881 File Offset: 0x00038C81
	public static void DestroyAll()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.networkingPeer.DestroyAll(false);
		}
		else
		{
			UnityEngine.Debug.LogError("Couldn't call DestroyAll() as only the master client is allowed to call this.");
		}
	}

	// Token: 0x06000668 RID: 1640 RVA: 0x0003A8A7 File Offset: 0x00038CA7
	public static void RemoveRPCs(PhotonPlayer targetPlayer)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		if (!targetPlayer.IsLocal && !PhotonNetwork.isMasterClient)
		{
			UnityEngine.Debug.LogError("Error; Only the MasterClient can call RemoveRPCs for other players.");
			return;
		}
		PhotonNetwork.networkingPeer.OpCleanRpcBuffer(targetPlayer.ID);
	}

	// Token: 0x06000669 RID: 1641 RVA: 0x0003A8E4 File Offset: 0x00038CE4
	public static void RemoveRPCs(PhotonView targetPhotonView)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.CleanRpcBufferIfMine(targetPhotonView);
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x0003A8FC File Offset: 0x00038CFC
	public static void RemoveRPCsInGroup(int targetGroup)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.RemoveRPCsInGroup(targetGroup);
	}

	// Token: 0x0600066B RID: 1643 RVA: 0x0003A914 File Offset: 0x00038D14
	internal static void RPC(PhotonView view, string methodName, PhotonTargets target, bool encrypt, params object[] parameters)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		if (PhotonNetwork.room == null)
		{
			UnityEngine.Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
			return;
		}
		if (PhotonNetwork.networkingPeer != null)
		{
			if (PhotonNetwork.room.serverSideMasterClient)
			{
				PhotonNetwork.networkingPeer.RPC(view, methodName, target, null, encrypt, parameters);
			}
			else if (PhotonNetwork.networkingPeer.hasSwitchedMC && target == PhotonTargets.MasterClient)
			{
				PhotonNetwork.networkingPeer.RPC(view, methodName, PhotonTargets.Others, PhotonNetwork.masterClient, encrypt, parameters);
			}
			else
			{
				PhotonNetwork.networkingPeer.RPC(view, methodName, target, null, encrypt, parameters);
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
		}
	}

	// Token: 0x0600066C RID: 1644 RVA: 0x0003A9D8 File Offset: 0x00038DD8
	internal static void RPC(PhotonView view, string methodName, PhotonPlayer targetPlayer, bool encrpyt, params object[] parameters)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		if (PhotonNetwork.room == null)
		{
			UnityEngine.Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
			return;
		}
		if (PhotonNetwork.player == null)
		{
			UnityEngine.Debug.LogError("RPC can't be sent to target PhotonPlayer being null! Did not send \"" + methodName + "\" call.");
		}
		if (PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.RPC(view, methodName, PhotonTargets.Others, targetPlayer, encrpyt, parameters);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Could not execute RPC " + methodName + ". Possible scene loading in progress?");
		}
	}

	// Token: 0x0600066D RID: 1645 RVA: 0x0003AA64 File Offset: 0x00038E64
	public static void CacheSendMonoMessageTargets(Type type)
	{
		if (type == null)
		{
			type = PhotonNetwork.SendMonoMessageTargetType;
		}
		PhotonNetwork.SendMonoMessageTargets = PhotonNetwork.FindGameObjectsWithComponent(type);
	}

	// Token: 0x0600066E RID: 1646 RVA: 0x0003AA80 File Offset: 0x00038E80
	public static HashSet<GameObject> FindGameObjectsWithComponent(Type type)
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Component[] array = (Component[])UnityEngine.Object.FindObjectsOfType(type);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				hashSet.Add(array[i].gameObject);
			}
		}
		return hashSet;
	}

	// Token: 0x0600066F RID: 1647 RVA: 0x0003AAD1 File Offset: 0x00038ED1
	[Obsolete("Use SetInterestGroups(byte group, bool enabled) instead.")]
	public static void SetReceivingEnabled(int group, bool enabled)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.SetInterestGroups((byte)group, enabled);
	}

	// Token: 0x06000670 RID: 1648 RVA: 0x0003AAE8 File Offset: 0x00038EE8
	public static void SetInterestGroups(byte group, bool enabled)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		if (enabled)
		{
			byte[] enableGroups = new byte[]
			{
				group
			};
			PhotonNetwork.networkingPeer.SetInterestGroups(null, enableGroups);
		}
		else
		{
			byte[] disableGroups = new byte[]
			{
				group
			};
			PhotonNetwork.networkingPeer.SetInterestGroups(disableGroups, null);
		}
	}

	// Token: 0x06000671 RID: 1649 RVA: 0x0003AB3C File Offset: 0x00038F3C
	[Obsolete("Use SetInterestGroups(byte[] disableGroups, byte[] enableGroups) instead. Mind the parameter order!")]
	public static void SetReceivingEnabled(int[] enableGroups, int[] disableGroups)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		byte[] array = null;
		byte[] array2 = null;
		if (enableGroups != null)
		{
			array2 = new byte[enableGroups.Length];
			Array.Copy(enableGroups, array2, enableGroups.Length);
		}
		if (disableGroups != null)
		{
			array = new byte[disableGroups.Length];
			Array.Copy(disableGroups, array, disableGroups.Length);
		}
		PhotonNetwork.networkingPeer.SetInterestGroups(array, array2);
	}

	// Token: 0x06000672 RID: 1650 RVA: 0x0003AB96 File Offset: 0x00038F96
	public static void SetInterestGroups(byte[] disableGroups, byte[] enableGroups)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.SetInterestGroups(disableGroups, enableGroups);
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x0003ABAF File Offset: 0x00038FAF
	[Obsolete("Use SetSendingEnabled(byte group, bool enabled). Interest Groups have a byte-typed ID. Mind the parameter order.")]
	public static void SetSendingEnabled(int group, bool enabled)
	{
		PhotonNetwork.SetSendingEnabled((byte)group, enabled);
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x0003ABB9 File Offset: 0x00038FB9
	public static void SetSendingEnabled(byte group, bool enabled)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.SetSendingEnabled(group, enabled);
	}

	// Token: 0x06000675 RID: 1653 RVA: 0x0003ABD4 File Offset: 0x00038FD4
	[Obsolete("Use SetSendingEnabled(byte group, bool enabled). Interest Groups have a byte-typed ID. Mind the parameter order.")]
	public static void SetSendingEnabled(int[] enableGroups, int[] disableGroups)
	{
		byte[] array = null;
		byte[] array2 = null;
		if (enableGroups != null)
		{
			array2 = new byte[enableGroups.Length];
			Array.Copy(enableGroups, array2, enableGroups.Length);
		}
		if (disableGroups != null)
		{
			array = new byte[disableGroups.Length];
			Array.Copy(disableGroups, array, disableGroups.Length);
		}
		PhotonNetwork.SetSendingEnabled(array, array2);
	}

	// Token: 0x06000676 RID: 1654 RVA: 0x0003AC1E File Offset: 0x0003901E
	public static void SetSendingEnabled(byte[] disableGroups, byte[] enableGroups)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.SetSendingEnabled(disableGroups, enableGroups);
	}

	// Token: 0x06000677 RID: 1655 RVA: 0x0003AC37 File Offset: 0x00039037
	public static void SetLevelPrefix(short prefix)
	{
		if (!PhotonNetwork.VerifyCanUseNetwork())
		{
			return;
		}
		PhotonNetwork.networkingPeer.SetLevelPrefix(prefix);
	}

	// Token: 0x06000678 RID: 1656 RVA: 0x0003AC4F File Offset: 0x0003904F
	public static void LoadLevel(int levelNumber)
	{
		if (PhotonNetwork.automaticallySyncScene)
		{
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(levelNumber, false);
		}
		PhotonNetwork.isMessageQueueRunning = false;
		PhotonNetwork.networkingPeer.loadingLevelAndPausedNetwork = true;
		SceneManager.LoadScene(levelNumber);
	}

	// Token: 0x06000679 RID: 1657 RVA: 0x0003AC83 File Offset: 0x00039083
	public static AsyncOperation LoadLevelAsync(int levelNumber)
	{
		if (PhotonNetwork.automaticallySyncScene)
		{
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(levelNumber, true);
		}
		PhotonNetwork.isMessageQueueRunning = false;
		PhotonNetwork.networkingPeer.loadingLevelAndPausedNetwork = true;
		return SceneManager.LoadSceneAsync(levelNumber, LoadSceneMode.Single);
	}

	// Token: 0x0600067A RID: 1658 RVA: 0x0003ACB8 File Offset: 0x000390B8
	public static void LoadLevel(string levelName)
	{
		if (PhotonNetwork.automaticallySyncScene)
		{
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(levelName, false);
		}
		PhotonNetwork.isMessageQueueRunning = false;
		PhotonNetwork.networkingPeer.loadingLevelAndPausedNetwork = true;
		SceneManager.LoadScene(levelName);
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x0003ACE7 File Offset: 0x000390E7
	public static AsyncOperation LoadLevelAsync(string levelName)
	{
		if (PhotonNetwork.automaticallySyncScene)
		{
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(levelName, true);
		}
		PhotonNetwork.isMessageQueueRunning = false;
		PhotonNetwork.networkingPeer.loadingLevelAndPausedNetwork = true;
		return SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x0003AD17 File Offset: 0x00039117
	public static bool WebRpc(string name, object parameters)
	{
		return PhotonNetwork.networkingPeer.WebRpc(name, parameters);
	}

	// Token: 0x04000865 RID: 2149
	public const string versionPUN = "1.90";

	// Token: 0x04000867 RID: 2151
	internal static readonly PhotonHandler photonMono;

	// Token: 0x04000868 RID: 2152
	internal static NetworkingPeer networkingPeer;

	// Token: 0x04000869 RID: 2153
	public static readonly int MAX_VIEW_IDS = 1000;

	// Token: 0x0400086A RID: 2154
	internal const string serverSettingsAssetFile = "PhotonServerSettings";

	// Token: 0x0400086B RID: 2155
	public static ServerSettings PhotonServerSettings = (ServerSettings)Resources.Load("PhotonServerSettings", typeof(ServerSettings));

	// Token: 0x0400086C RID: 2156
	public static bool InstantiateInRoomOnly = true;

	// Token: 0x0400086D RID: 2157
	public static PhotonLogLevel logLevel = PhotonLogLevel.ErrorsOnly;

	// Token: 0x0400086F RID: 2159
	public static float precisionForVectorSynchronization = 9.9E-05f;

	// Token: 0x04000870 RID: 2160
	public static float precisionForQuaternionSynchronization = 1f;

	// Token: 0x04000871 RID: 2161
	public static float precisionForFloatSynchronization = 0.01f;

	// Token: 0x04000872 RID: 2162
	public static bool UseRpcMonoBehaviourCache;

	// Token: 0x04000873 RID: 2163
	public static bool UsePrefabCache = true;

	// Token: 0x04000874 RID: 2164
	public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

	// Token: 0x04000875 RID: 2165
	public static HashSet<GameObject> SendMonoMessageTargets;

	// Token: 0x04000876 RID: 2166
	public static Type SendMonoMessageTargetType = typeof(MonoBehaviour);

	// Token: 0x04000877 RID: 2167
	public static bool StartRpcsAsCoroutine = true;

	// Token: 0x04000878 RID: 2168
	private static bool isOfflineMode = false;

	// Token: 0x04000879 RID: 2169
	private static Room offlineModeRoom = null;

	// Token: 0x0400087A RID: 2170
	[Obsolete("Used for compatibility with Unity networking only.")]
	public static int maxConnections;

	// Token: 0x0400087B RID: 2171
	private static bool _mAutomaticallySyncScene = false;

	// Token: 0x0400087C RID: 2172
	private static bool m_autoCleanUpPlayerObjects = true;

	// Token: 0x0400087D RID: 2173
	private static int sendInterval = 50;

	// Token: 0x0400087E RID: 2174
	private static int sendIntervalOnSerialize = 100;

	// Token: 0x0400087F RID: 2175
	private static bool m_isMessageQueueRunning = true;

	// Token: 0x04000880 RID: 2176
	private static bool UsePreciseTimer = false;

	// Token: 0x04000881 RID: 2177
	private static Stopwatch startupStopwatch;

	// Token: 0x04000882 RID: 2178
	public static float BackgroundTimeout = 60f;

	// Token: 0x04000884 RID: 2180
	public static PhotonNetwork.EventCallback OnEventCall;

	// Token: 0x04000885 RID: 2181
	internal static int lastUsedViewSubId = 0;

	// Token: 0x04000886 RID: 2182
	internal static int lastUsedViewSubIdStatic = 0;

	// Token: 0x04000887 RID: 2183
	internal static List<int> manuallyAllocatedViewIds = new List<int>();

	// Token: 0x020000FE RID: 254
	// (Invoke) Token: 0x0600067F RID: 1663
	public delegate void EventCallback(byte eventCode, object content, int senderId);
}
