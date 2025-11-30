using System;
using Photon;
using UnityEngine;

// Token: 0x02000136 RID: 310
[RequireComponent(typeof(PhotonView))]
public class PickupItemSimple : Photon.MonoBehaviour
{
	// Token: 0x060007EA RID: 2026 RVA: 0x00041830 File Offset: 0x0003FC30
	public void OnTriggerEnter(Collider other)
	{
		PhotonView component = other.GetComponent<PhotonView>();
		if (this.PickupOnCollide && component != null && component.isMine)
		{
			this.Pickup();
		}
	}

	// Token: 0x060007EB RID: 2027 RVA: 0x0004186C File Offset: 0x0003FC6C
	public void Pickup()
	{
		if (this.SentPickup)
		{
			return;
		}
		this.SentPickup = true;
		base.photonView.RPC("PunPickupSimple", PhotonTargets.AllViaServer, new object[0]);
	}

	// Token: 0x060007EC RID: 2028 RVA: 0x00041898 File Offset: 0x0003FC98
	[PunRPC]
	public void PunPickupSimple(PhotonMessageInfo msgInfo)
	{
		if (!this.SentPickup || !msgInfo.sender.IsLocal || base.gameObject.GetActive())
		{
		}
		this.SentPickup = false;
		if (!base.gameObject.GetActive())
		{
			Debug.Log("Ignored PU RPC, cause item is inactive. " + base.gameObject);
			return;
		}
		double num = PhotonNetwork.time - msgInfo.timestamp;
		float num2 = this.SecondsBeforeRespawn - (float)num;
		if (num2 > 0f)
		{
			base.gameObject.SetActive(false);
			base.Invoke("RespawnAfter", num2);
		}
	}

	// Token: 0x060007ED RID: 2029 RVA: 0x0004193E File Offset: 0x0003FD3E
	public void RespawnAfter()
	{
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(true);
		}
	}

	// Token: 0x040009B5 RID: 2485
	public float SecondsBeforeRespawn = 2f;

	// Token: 0x040009B6 RID: 2486
	public bool PickupOnCollide;

	// Token: 0x040009B7 RID: 2487
	public bool SentPickup;
}
