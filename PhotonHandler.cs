using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000FB RID: 251
internal class PhotonHandler : MonoBehaviour
{
	// Token: 0x060005D5 RID: 1493 RVA: 0x000381B0 File Offset: 0x000365B0
	protected void Awake()
	{
		if (PhotonHandler.SP != null && PhotonHandler.SP != this && PhotonHandler.SP.gameObject != null)
		{
			UnityEngine.Object.DestroyImmediate(PhotonHandler.SP.gameObject);
		}
		PhotonHandler.SP = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		this.updateInterval = 1000 / PhotonNetwork.sendRate;
		this.updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
		PhotonHandler.StartFallbackSendAckThread();
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x00038239 File Offset: 0x00036639
	protected void Start()
	{
		SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode loadingMode)
		{
			PhotonNetwork.networkingPeer.NewSceneLoaded();
			PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName, false);
		};
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x0003825D File Offset: 0x0003665D
	protected void OnApplicationQuit()
	{
		PhotonHandler.AppQuits = true;
		PhotonHandler.StopFallbackSendAckThread();
		PhotonNetwork.Disconnect();
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x00038270 File Offset: 0x00036670
	protected void OnApplicationPause(bool pause)
	{
		if (PhotonNetwork.BackgroundTimeout > 0.1f)
		{
			if (PhotonHandler.timerToStopConnectionInBackground == null)
			{
				PhotonHandler.timerToStopConnectionInBackground = new Stopwatch();
			}
			PhotonHandler.timerToStopConnectionInBackground.Reset();
			if (pause)
			{
				PhotonHandler.timerToStopConnectionInBackground.Start();
			}
			else
			{
				PhotonHandler.timerToStopConnectionInBackground.Stop();
			}
		}
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x000382C9 File Offset: 0x000366C9
	protected void OnDestroy()
	{
		PhotonHandler.StopFallbackSendAckThread();
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x000382D0 File Offset: 0x000366D0
	protected void Update()
	{
		if (PhotonNetwork.networkingPeer == null)
		{
			UnityEngine.Debug.LogError("NetworkPeer broke!");
			return;
		}
		if (PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated || PhotonNetwork.connectionStateDetailed == ClientState.Disconnected || PhotonNetwork.offlineMode)
		{
			return;
		}
		if (!PhotonNetwork.isMessageQueueRunning)
		{
			return;
		}
		bool flag = true;
		while (PhotonNetwork.isMessageQueueRunning && flag)
		{
			flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
		}
		int num = (int)(Time.realtimeSinceStartup * 1000f);
		if (PhotonNetwork.isMessageQueueRunning && num > this.nextSendTickCountOnSerialize)
		{
			PhotonNetwork.networkingPeer.RunViewUpdate();
			this.nextSendTickCountOnSerialize = num + this.updateIntervalOnSerialize;
			this.nextSendTickCount = 0;
		}
		num = (int)(Time.realtimeSinceStartup * 1000f);
		if (num > this.nextSendTickCount)
		{
			bool flag2 = true;
			while (PhotonNetwork.isMessageQueueRunning && flag2)
			{
				flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands();
			}
			this.nextSendTickCount = num + this.updateInterval;
		}
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x000383CC File Offset: 0x000367CC
	protected void OnJoinedRoom()
	{
		PhotonNetwork.networkingPeer.LoadLevelIfSynced();
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x000383D8 File Offset: 0x000367D8
	protected void OnCreatedRoom()
	{
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName, false);
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x000383EA File Offset: 0x000367EA
	public static void StartFallbackSendAckThread()
	{
		if (PhotonHandler.sendThreadShouldRun)
		{
			return;
		}
		PhotonHandler.sendThreadShouldRun = true;
		if (PhotonHandler.<>f__mg$cache0 == null)
		{
			PhotonHandler.<>f__mg$cache0 = new Func<bool>(PhotonHandler.FallbackSendAckThread);
		}
		SupportClass.StartBackgroundCalls(PhotonHandler.<>f__mg$cache0, 100, string.Empty);
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x00038427 File Offset: 0x00036827
	public static void StopFallbackSendAckThread()
	{
		PhotonHandler.sendThreadShouldRun = false;
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x00038430 File Offset: 0x00036830
	public static bool FallbackSendAckThread()
	{
		if (PhotonHandler.sendThreadShouldRun && !PhotonNetwork.offlineMode && PhotonNetwork.networkingPeer != null)
		{
			if (PhotonHandler.timerToStopConnectionInBackground != null && PhotonNetwork.BackgroundTimeout > 0.1f && (float)PhotonHandler.timerToStopConnectionInBackground.ElapsedMilliseconds > PhotonNetwork.BackgroundTimeout * 1000f)
			{
				if (PhotonNetwork.connected)
				{
					PhotonNetwork.Disconnect();
				}
				PhotonHandler.timerToStopConnectionInBackground.Stop();
				PhotonHandler.timerToStopConnectionInBackground.Reset();
				return PhotonHandler.sendThreadShouldRun;
			}
			if (!PhotonNetwork.isMessageQueueRunning || PhotonNetwork.networkingPeer.ConnectionTime - PhotonNetwork.networkingPeer.LastSendOutgoingTime > 200)
			{
				PhotonNetwork.networkingPeer.SendAcksOnly();
			}
		}
		return PhotonHandler.sendThreadShouldRun;
	}

	// Token: 0x1700005D RID: 93
	// (get) Token: 0x060005E0 RID: 1504 RVA: 0x000384F4 File Offset: 0x000368F4
	// (set) Token: 0x060005E1 RID: 1505 RVA: 0x00038526 File Offset: 0x00036926
	internal static CloudRegionCode BestRegionCodeInPreferences
	{
		get
		{
			string @string = PlayerPrefs.GetString("PUNCloudBestRegion", string.Empty);
			if (!string.IsNullOrEmpty(@string))
			{
				return Region.Parse(@string);
			}
			return CloudRegionCode.none;
		}
		set
		{
			if (value == CloudRegionCode.none)
			{
				PlayerPrefs.DeleteKey("PUNCloudBestRegion");
			}
			else
			{
				PlayerPrefs.SetString("PUNCloudBestRegion", value.ToString());
			}
		}
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x00038555 File Offset: 0x00036955
	protected internal static void PingAvailableRegionsAndConnectToBest()
	{
		PhotonHandler.SP.StartCoroutine(PhotonHandler.SP.PingAvailableRegionsCoroutine(true));
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x00038570 File Offset: 0x00036970
	internal IEnumerator PingAvailableRegionsCoroutine(bool connectToBest)
	{
		while (PhotonNetwork.networkingPeer.AvailableRegions == null)
		{
			if (PhotonNetwork.connectionStateDetailed != ClientState.ConnectingToNameServer && PhotonNetwork.connectionStateDetailed != ClientState.ConnectedToNameServer)
			{
				UnityEngine.Debug.LogError("Call ConnectToNameServer to ping available regions.");
				yield break;
			}
			UnityEngine.Debug.Log(string.Concat(new object[]
			{
				"Waiting for AvailableRegions. State: ",
				PhotonNetwork.connectionStateDetailed,
				" Server: ",
				PhotonNetwork.Server,
				" PhotonNetwork.networkingPeer.AvailableRegions ",
				PhotonNetwork.networkingPeer.AvailableRegions != null
			}));
			yield return new WaitForSeconds(0.25f);
		}
		if (PhotonNetwork.networkingPeer.AvailableRegions == null || PhotonNetwork.networkingPeer.AvailableRegions.Count == 0)
		{
			UnityEngine.Debug.LogError("No regions available. Are you sure your appid is valid and setup?");
			yield break;
		}
		PhotonPingManager pingManager = new PhotonPingManager();
		foreach (Region region in PhotonNetwork.networkingPeer.AvailableRegions)
		{
			PhotonHandler.SP.StartCoroutine(pingManager.PingSocket(region));
		}
		while (!pingManager.Done)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Region best = pingManager.BestRegion;
		PhotonHandler.BestRegionCodeInPreferences = best.Code;
		UnityEngine.Debug.Log(string.Concat(new object[]
		{
			"Found best region: '",
			best.Code,
			"' ping: ",
			best.Ping,
			". Calling ConnectToRegionMaster() is: ",
			connectToBest
		}));
		if (connectToBest)
		{
			PhotonNetwork.networkingPeer.ConnectToRegionMaster(best.Code);
		}
		yield break;
	}

	// Token: 0x04000855 RID: 2133
	public static PhotonHandler SP;

	// Token: 0x04000856 RID: 2134
	public int updateInterval;

	// Token: 0x04000857 RID: 2135
	public int updateIntervalOnSerialize;

	// Token: 0x04000858 RID: 2136
	private int nextSendTickCount;

	// Token: 0x04000859 RID: 2137
	private int nextSendTickCountOnSerialize;

	// Token: 0x0400085A RID: 2138
	private static bool sendThreadShouldRun;

	// Token: 0x0400085B RID: 2139
	private static Stopwatch timerToStopConnectionInBackground;

	// Token: 0x0400085C RID: 2140
	protected internal static bool AppQuits;

	// Token: 0x0400085D RID: 2141
	protected internal static Type PingImplementation;

	// Token: 0x0400085E RID: 2142
	private const string PlayerPrefsKey = "PUNCloudBestRegion";

	// Token: 0x04000860 RID: 2144
	[CompilerGenerated]
	private static Func<bool> <>f__mg$cache0;
}
