using System;
using UnityEngine;

// Token: 0x02000113 RID: 275
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("Photon Networking/Photon Rigidbody 2D View")]
public class PhotonRigidbody2DView : MonoBehaviour, IPunObservable
{
	// Token: 0x06000749 RID: 1865 RVA: 0x0003E3AF File Offset: 0x0003C7AF
	private void Awake()
	{
		this.m_Body = base.GetComponent<Rigidbody2D>();
	}

	// Token: 0x0600074A RID: 1866 RVA: 0x0003E3C0 File Offset: 0x0003C7C0
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
				this.m_Body.velocity = (Vector2)stream.ReceiveNext();
			}
			if (this.m_SynchronizeAngularVelocity)
			{
				this.m_Body.angularVelocity = (float)stream.ReceiveNext();
			}
		}
	}

	// Token: 0x04000911 RID: 2321
	[SerializeField]
	private bool m_SynchronizeVelocity = true;

	// Token: 0x04000912 RID: 2322
	[SerializeField]
	private bool m_SynchronizeAngularVelocity = true;

	// Token: 0x04000913 RID: 2323
	private Rigidbody2D m_Body;
}
