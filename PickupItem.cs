using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

// Token: 0x02000135 RID: 309
[RequireComponent(typeof(PhotonView))]
public class PickupItem : Photon.MonoBehaviour, IPunObservable
{
	// Token: 0x170000D6 RID: 214
	// (get) Token: 0x060007DE RID: 2014 RVA: 0x000414F6 File Offset: 0x0003F8F6
	public int ViewID
	{
		get
		{
			return base.photonView.viewID;
		}
	}

	// Token: 0x060007DF RID: 2015 RVA: 0x00041504 File Offset: 0x0003F904
	public void OnTriggerEnter(Collider other)
	{
		PhotonView component = other.GetComponent<PhotonView>();
		if (this.PickupOnTrigger && component != null && component.isMine)
		{
			this.Pickup();
		}
	}

	// Token: 0x060007E0 RID: 2016 RVA: 0x00041540 File Offset: 0x0003F940
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting && this.SecondsBeforeRespawn <= 0f)
		{
			stream.SendNext(base.gameObject.transform.position);
		}
		else
		{
			Vector3 position = (Vector3)stream.ReceiveNext();
			base.gameObject.transform.position = position;
		}
	}

	// Token: 0x060007E1 RID: 2017 RVA: 0x000415A5 File Offset: 0x0003F9A5
	public void Pickup()
	{
		if (this.SentPickup)
		{
			return;
		}
		this.SentPickup = true;
		base.photonView.RPC("PunPickup", PhotonTargets.AllViaServer, new object[0]);
	}

	// Token: 0x060007E2 RID: 2018 RVA: 0x000415D1 File Offset: 0x0003F9D1
	public void Drop()
	{
		if (this.PickupIsMine)
		{
			base.photonView.RPC("PunRespawn", PhotonTargets.AllViaServer, new object[0]);
		}
	}

	// Token: 0x060007E3 RID: 2019 RVA: 0x000415F5 File Offset: 0x0003F9F5
	public void Drop(Vector3 newPosition)
	{
		if (this.PickupIsMine)
		{
			base.photonView.RPC("PunRespawn", PhotonTargets.AllViaServer, new object[]
			{
				newPosition
			});
		}
	}

	// Token: 0x060007E4 RID: 2020 RVA: 0x00041624 File Offset: 0x0003FA24
	[PunRPC]
	public void PunPickup(PhotonMessageInfo msgInfo)
	{
		if (msgInfo.sender.IsLocal)
		{
			this.SentPickup = false;
		}
		if (!base.gameObject.GetActive())
		{
			Debug.Log(string.Concat(new object[]
			{
				"Ignored PU RPC, cause item is inactive. ",
				base.gameObject,
				" SecondsBeforeRespawn: ",
				this.SecondsBeforeRespawn,
				" TimeOfRespawn: ",
				this.TimeOfRespawn,
				" respawn in future: ",
				this.TimeOfRespawn > PhotonNetwork.time
			}));
			return;
		}
		this.PickupIsMine = msgInfo.sender.IsLocal;
		if (this.OnPickedUpCall != null)
		{
			this.OnPickedUpCall.SendMessage("OnPickedUp", this);
		}
		if (this.SecondsBeforeRespawn <= 0f)
		{
			this.PickedUp(0f);
		}
		else
		{
			double num = PhotonNetwork.time - msgInfo.timestamp;
			double num2 = (double)this.SecondsBeforeRespawn - num;
			if (num2 > 0.0)
			{
				this.PickedUp((float)num2);
			}
		}
	}

	// Token: 0x060007E5 RID: 2021 RVA: 0x00041748 File Offset: 0x0003FB48
	internal void PickedUp(float timeUntilRespawn)
	{
		base.gameObject.SetActive(false);
		PickupItem.DisabledPickupItems.Add(this);
		this.TimeOfRespawn = 0.0;
		if (timeUntilRespawn > 0f)
		{
			this.TimeOfRespawn = PhotonNetwork.time + (double)timeUntilRespawn;
			base.Invoke("PunRespawn", timeUntilRespawn);
		}
	}

	// Token: 0x060007E6 RID: 2022 RVA: 0x000417A1 File Offset: 0x0003FBA1
	[PunRPC]
	internal void PunRespawn(Vector3 pos)
	{
		Debug.Log("PunRespawn with Position.");
		this.PunRespawn();
		base.gameObject.transform.position = pos;
	}

	// Token: 0x060007E7 RID: 2023 RVA: 0x000417C4 File Offset: 0x0003FBC4
	[PunRPC]
	internal void PunRespawn()
	{
		PickupItem.DisabledPickupItems.Remove(this);
		this.TimeOfRespawn = 0.0;
		this.PickupIsMine = false;
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(true);
		}
	}

	// Token: 0x040009AE RID: 2478
	public float SecondsBeforeRespawn = 2f;

	// Token: 0x040009AF RID: 2479
	public bool PickupOnTrigger;

	// Token: 0x040009B0 RID: 2480
	public bool PickupIsMine;

	// Token: 0x040009B1 RID: 2481
	public UnityEngine.MonoBehaviour OnPickedUpCall;

	// Token: 0x040009B2 RID: 2482
	public bool SentPickup;

	// Token: 0x040009B3 RID: 2483
	public double TimeOfRespawn;

	// Token: 0x040009B4 RID: 2484
	public static HashSet<PickupItem> DisabledPickupItems = new HashSet<PickupItem>();
}
