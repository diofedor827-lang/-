using System;
using ExitGames.Client.Photon;
using UnityEngine;

// Token: 0x020000FC RID: 252
public class PhotonLagSimulationGui : MonoBehaviour
{
	// Token: 0x1700005E RID: 94
	// (get) Token: 0x060005E7 RID: 1511 RVA: 0x00038891 File Offset: 0x00036C91
	// (set) Token: 0x060005E8 RID: 1512 RVA: 0x00038899 File Offset: 0x00036C99
	public PhotonPeer Peer { get; set; }

	// Token: 0x060005E9 RID: 1513 RVA: 0x000388A2 File Offset: 0x00036CA2
	public void Start()
	{
		this.Peer = PhotonNetwork.networkingPeer;
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x000388B0 File Offset: 0x00036CB0
	public void OnGUI()
	{
		if (!this.Visible)
		{
			return;
		}
		if (this.Peer == null)
		{
			this.WindowRect = GUILayout.Window(this.WindowId, this.WindowRect, new GUI.WindowFunction(this.NetSimHasNoPeerWindow), "Netw. Sim.", new GUILayoutOption[0]);
		}
		else
		{
			this.WindowRect = GUILayout.Window(this.WindowId, this.WindowRect, new GUI.WindowFunction(this.NetSimWindow), "Netw. Sim.", new GUILayoutOption[0]);
		}
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x00038935 File Offset: 0x00036D35
	private void NetSimHasNoPeerWindow(int windowId)
	{
		GUILayout.Label("No peer to communicate with. ", new GUILayoutOption[0]);
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x00038948 File Offset: 0x00036D48
	private void NetSimWindow(int windowId)
	{
		GUILayout.Label(string.Format("Rtt:{0,4} +/-{1,3}", this.Peer.RoundTripTime, this.Peer.RoundTripTimeVariance), new GUILayoutOption[0]);
		bool isSimulationEnabled = this.Peer.IsSimulationEnabled;
		bool flag = GUILayout.Toggle(isSimulationEnabled, "Simulate", new GUILayoutOption[0]);
		if (flag != isSimulationEnabled)
		{
			this.Peer.IsSimulationEnabled = flag;
		}
		float num = (float)this.Peer.NetworkSimulationSettings.IncomingLag;
		GUILayout.Label("Lag " + num, new GUILayoutOption[0]);
		num = GUILayout.HorizontalSlider(num, 0f, 500f, new GUILayoutOption[0]);
		this.Peer.NetworkSimulationSettings.IncomingLag = (int)num;
		this.Peer.NetworkSimulationSettings.OutgoingLag = (int)num;
		float num2 = (float)this.Peer.NetworkSimulationSettings.IncomingJitter;
		GUILayout.Label("Jit " + num2, new GUILayoutOption[0]);
		num2 = GUILayout.HorizontalSlider(num2, 0f, 100f, new GUILayoutOption[0]);
		this.Peer.NetworkSimulationSettings.IncomingJitter = (int)num2;
		this.Peer.NetworkSimulationSettings.OutgoingJitter = (int)num2;
		float num3 = (float)this.Peer.NetworkSimulationSettings.IncomingLossPercentage;
		GUILayout.Label("Loss " + num3, new GUILayoutOption[0]);
		num3 = GUILayout.HorizontalSlider(num3, 0f, 10f, new GUILayoutOption[0]);
		this.Peer.NetworkSimulationSettings.IncomingLossPercentage = (int)num3;
		this.Peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)num3;
		if (GUI.changed)
		{
			this.WindowRect.height = 100f;
		}
		GUI.DragWindow();
	}

	// Token: 0x04000861 RID: 2145
	public Rect WindowRect = new Rect(0f, 100f, 120f, 100f);

	// Token: 0x04000862 RID: 2146
	public int WindowId = 101;

	// Token: 0x04000863 RID: 2147
	public bool Visible = true;
}
