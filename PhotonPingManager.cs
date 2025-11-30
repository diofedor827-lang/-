using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using ExitGames.Client.Photon;
using UnityEngine;

// Token: 0x02000107 RID: 263
public class PhotonPingManager
{
	// Token: 0x170000A5 RID: 165
	// (get) Token: 0x060006D9 RID: 1753 RVA: 0x0003C4B0 File Offset: 0x0003A8B0
	public Region BestRegion
	{
		get
		{
			Region result = null;
			int num = int.MaxValue;
			foreach (Region region in PhotonNetwork.networkingPeer.AvailableRegions)
			{
				UnityEngine.Debug.Log("BestRegion checks region: " + region);
				if (region.Ping != 0 && region.Ping < num)
				{
					num = region.Ping;
					result = region;
				}
			}
			return result;
		}
	}

	// Token: 0x170000A6 RID: 166
	// (get) Token: 0x060006DA RID: 1754 RVA: 0x0003C544 File Offset: 0x0003A944
	public bool Done
	{
		get
		{
			return this.PingsRunning == 0;
		}
	}

	// Token: 0x060006DB RID: 1755 RVA: 0x0003C550 File Offset: 0x0003A950
	public IEnumerator PingSocket(Region region)
	{
		region.Ping = PhotonPingManager.Attempts * PhotonPingManager.MaxMilliseconsPerPing;
		this.PingsRunning++;
		PhotonPing ping;
		if (PhotonHandler.PingImplementation == typeof(PingNativeDynamic))
		{
			UnityEngine.Debug.Log("Using constructor for new PingNativeDynamic()");
			ping = new PingNativeDynamic();
		}
		else if (PhotonHandler.PingImplementation == typeof(PingNativeStatic))
		{
			UnityEngine.Debug.Log("Using constructor for new PingNativeStatic()");
			ping = new PingNativeStatic();
		}
		else if (PhotonHandler.PingImplementation == typeof(PingMono))
		{
			ping = new PingMono();
		}
		else
		{
			ping = (PhotonPing)Activator.CreateInstance(PhotonHandler.PingImplementation);
		}
		float rttSum = 0f;
		int replyCount = 0;
		string regionAddress = region.HostAndPort;
		int indexOfColon = regionAddress.LastIndexOf(':');
		if (indexOfColon > 1)
		{
			regionAddress = regionAddress.Substring(0, indexOfColon);
		}
		int indexOfProtocol = regionAddress.IndexOf("wss://");
		if (indexOfProtocol > -1)
		{
			regionAddress = regionAddress.Substring(indexOfProtocol + "wss://".Length);
		}
		regionAddress = PhotonPingManager.ResolveHost(regionAddress);
		UnityEngine.Debug.Log(string.Concat(new object[]
		{
			"Ping Debug - PhotonHandler.PingImplementation: ",
			PhotonHandler.PingImplementation,
			" ping.GetType():",
			ping.GetType(),
			" regionAddress:",
			regionAddress
		}));
		for (int i = 0; i < PhotonPingManager.Attempts; i++)
		{
			bool overtime = false;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			try
			{
				ping.StartPing(regionAddress);
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.Log("catched: " + arg);
				this.PingsRunning--;
				break;
			}
			while (!ping.Done())
			{
				if (sw.ElapsedMilliseconds >= (long)PhotonPingManager.MaxMilliseconsPerPing)
				{
					overtime = true;
					break;
				}
				yield return 0;
			}
			int rtt = (int)sw.ElapsedMilliseconds;
			if (!PhotonPingManager.IgnoreInitialAttempt || i != 0)
			{
				if (ping.Successful && !overtime)
				{
					rttSum += (float)rtt;
					replyCount++;
					region.Ping = (int)(rttSum / (float)replyCount);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		ping.Dispose();
		this.PingsRunning--;
		yield return null;
		yield break;
	}

	// Token: 0x060006DC RID: 1756 RVA: 0x0003C574 File Offset: 0x0003A974
	public static string ResolveHost(string hostName)
	{
		string text = string.Empty;
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			if (hostAddresses.Length == 1)
			{
				return hostAddresses[0].ToString();
			}
			foreach (IPAddress ipaddress in hostAddresses)
			{
				if (ipaddress != null)
				{
					if (ipaddress.ToString().Contains(":"))
					{
						return ipaddress.ToString();
					}
					if (string.IsNullOrEmpty(text))
					{
						text = hostAddresses.ToString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("Exception caught! " + ex.Source + " Message: " + ex.Message);
		}
		return text;
	}

	// Token: 0x040008C8 RID: 2248
	public bool UseNative;

	// Token: 0x040008C9 RID: 2249
	public static int Attempts = 5;

	// Token: 0x040008CA RID: 2250
	public static bool IgnoreInitialAttempt = true;

	// Token: 0x040008CB RID: 2251
	public static int MaxMilliseconsPerPing = 800;

	// Token: 0x040008CC RID: 2252
	private const string wssProtocolString = "wss://";

	// Token: 0x040008CD RID: 2253
	private int PingsRunning;
}
