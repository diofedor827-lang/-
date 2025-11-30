using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

// Token: 0x02000137 RID: 311
[RequireComponent(typeof(PhotonView))]
public class PickupItemSyncer : Photon.MonoBehaviour
{
	// Token: 0x060007EF RID: 2031 RVA: 0x00041965 File Offset: 0x0003FD65
	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.SendPickedUpItems(newPlayer);
		}
	}

	// Token: 0x060007F0 RID: 2032 RVA: 0x00041978 File Offset: 0x0003FD78
	public void OnJoinedRoom()
	{
		Debug.Log(string.Concat(new object[]
		{
			"Joined Room. isMasterClient: ",
			PhotonNetwork.isMasterClient,
			" id: ",
			PhotonNetwork.player.ID
		}));
		this.IsWaitingForPickupInit = !PhotonNetwork.isMasterClient;
		if (PhotonNetwork.playerList.Length >= 2)
		{
			base.Invoke("AskForPickupItemSpawnTimes", 2f);
		}
	}

	// Token: 0x060007F1 RID: 2033 RVA: 0x000419F0 File Offset: 0x0003FDF0
	public void AskForPickupItemSpawnTimes()
	{
		if (this.IsWaitingForPickupInit)
		{
			if (PhotonNetwork.playerList.Length < 2)
			{
				Debug.Log("Cant ask anyone else for PickupItem spawn times.");
				this.IsWaitingForPickupInit = false;
				return;
			}
			PhotonPlayer next = PhotonNetwork.masterClient.GetNext();
			if (next == null || next.Equals(PhotonNetwork.player))
			{
				next = PhotonNetwork.player.GetNext();
			}
			if (next != null && !next.Equals(PhotonNetwork.player))
			{
				base.photonView.RPC("RequestForPickupItems", next, new object[0]);
			}
			else
			{
				Debug.Log("No player left to ask");
				this.IsWaitingForPickupInit = false;
			}
		}
	}

	// Token: 0x060007F2 RID: 2034 RVA: 0x00041A96 File Offset: 0x0003FE96
	[PunRPC]
	[Obsolete("Use RequestForPickupItems(PhotonMessageInfo msgInfo) with corrected typing instead.")]
	public void RequestForPickupTimes(PhotonMessageInfo msgInfo)
	{
		this.RequestForPickupItems(msgInfo);
	}

	// Token: 0x060007F3 RID: 2035 RVA: 0x00041A9F File Offset: 0x0003FE9F
	[PunRPC]
	public void RequestForPickupItems(PhotonMessageInfo msgInfo)
	{
		if (msgInfo.sender == null)
		{
			Debug.LogError("Unknown player asked for PickupItems");
			return;
		}
		this.SendPickedUpItems(msgInfo.sender);
	}

	// Token: 0x060007F4 RID: 2036 RVA: 0x00041AC8 File Offset: 0x0003FEC8
	private void SendPickedUpItems(PhotonPlayer targetPlayer)
	{
		if (targetPlayer == null)
		{
			Debug.LogWarning("Cant send PickupItem spawn times to unknown targetPlayer.");
			return;
		}
		double time = PhotonNetwork.time;
		double num = time + 0.20000000298023224;
		PickupItem[] array = new PickupItem[PickupItem.DisabledPickupItems.Count];
		PickupItem.DisabledPickupItems.CopyTo(array);
		List<float> list = new List<float>(array.Length * 2);
		foreach (PickupItem pickupItem in array)
		{
			if (pickupItem.SecondsBeforeRespawn <= 0f)
			{
				list.Add((float)pickupItem.ViewID);
				list.Add(0f);
			}
			else
			{
				double num2 = pickupItem.TimeOfRespawn - PhotonNetwork.time;
				if (pickupItem.TimeOfRespawn > num)
				{
					Debug.Log(string.Concat(new object[]
					{
						pickupItem.ViewID,
						" respawn: ",
						pickupItem.TimeOfRespawn,
						" timeUntilRespawn: ",
						num2,
						" (now: ",
						PhotonNetwork.time,
						")"
					}));
					list.Add((float)pickupItem.ViewID);
					list.Add((float)num2);
				}
			}
		}
		Debug.Log(string.Concat(new object[]
		{
			"Sent count: ",
			list.Count,
			" now: ",
			time
		}));
		base.photonView.RPC("PickupItemInit", targetPlayer, new object[]
		{
			PhotonNetwork.time,
			list.ToArray()
		});
	}

	// Token: 0x060007F5 RID: 2037 RVA: 0x00041C68 File Offset: 0x00040068
	[PunRPC]
	public void PickupItemInit(double timeBase, float[] inactivePickupsAndTimes)
	{
		this.IsWaitingForPickupInit = false;
		for (int i = 0; i < inactivePickupsAndTimes.Length / 2; i++)
		{
			int num = i * 2;
			int viewID = (int)inactivePickupsAndTimes[num];
			float num2 = inactivePickupsAndTimes[num + 1];
			PhotonView photonView = PhotonView.Find(viewID);
			PickupItem component = photonView.GetComponent<PickupItem>();
			if (num2 <= 0f)
			{
				component.PickedUp(0f);
			}
			else
			{
				double num3 = (double)num2 + timeBase;
				Debug.Log(string.Concat(new object[]
				{
					photonView.viewID,
					" respawn: ",
					num3,
					" timeUntilRespawnBasedOnTimeBase:",
					num2,
					" SecondsBeforeRespawn: ",
					component.SecondsBeforeRespawn
				}));
				double num4 = num3 - PhotonNetwork.time;
				if (num2 <= 0f)
				{
					num4 = 0.0;
				}
				component.PickedUp((float)num4);
			}
		}
	}

	// Token: 0x040009B8 RID: 2488
	public bool IsWaitingForPickupInit;

	// Token: 0x040009B9 RID: 2489
	private const float TimeDeltaToIgnore = 0.2f;
}
