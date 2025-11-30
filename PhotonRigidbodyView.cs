using System;
using UnityEngine;

// Token: 0x02000114 RID: 276
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Photon Networking/Photon Rigidbody View")]
public class PhotonRigidbodyView : MonoBehaviour, IPunObservable
{
	// Token: 0x0600074C RID: 1868 RVA: 0x0003E477 File Offset: 0x0003C877
	private void Awake()
	{
		this.m_Body = base.GetComponent<Rigidbody>();
	}

	// Token: 0x0600074D RID: 1869 RVA: 0x0003E488 File Offset: 0x0003C888
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (this.m_SynchronizeVelocity)
			{
				stream.SendNext(this.m_Body.velocity);
			}
			if (this.m_SynchronizeAngularVelocity)
			{
				stream.SendNext(this.m_Body.angularVelocity);
			}
		}
		else
		{
			if (this.m_SynchronizeVelocity)
			{
				this.m_Body.velocity = (Vector3)stream.ReceiveNext();
			}
			if (this.m_SynchronizeAngularVelocity)
			{
				this.m_Body.angularVelocity = (Vector3)stream.ReceiveNext();
			}
		}
	}

	// Token: 0x04000914 RID: 2324
	[SerializeField]
	private bool m_SynchronizeVelocity = true;

	// Token: 0x04000915 RID: 2325
	[SerializeField]
	private bool m_SynchronizeAngularVelocity = true;

	// Token: 0x04000916 RID: 2326
	private Rigidbody m_Body;
}
