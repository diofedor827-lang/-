using System;
using UnityEngine;

// Token: 0x0200011A RID: 282
public class PhotonTransformViewRotationControl
{
	// Token: 0x06000762 RID: 1890 RVA: 0x0003EDE1 File Offset: 0x0003D1E1
	public PhotonTransformViewRotationControl(PhotonTransformViewRotationModel model)
	{
		this.m_Model = model;
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x0003EDF0 File Offset: 0x0003D1F0
	public Quaternion GetNetworkRotation()
	{
		return this.m_NetworkRotation;
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x0003EDF8 File Offset: 0x0003D1F8
	public Quaternion GetRotation(Quaternion currentRotation)
	{
		switch (this.m_Model.InterpolateOption)
		{
		default:
			return this.m_NetworkRotation;
		case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
			return Quaternion.RotateTowards(currentRotation, this.m_NetworkRotation, this.m_Model.InterpolateRotateTowardsSpeed * Time.deltaTime);
		case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
			return Quaternion.Lerp(currentRotation, this.m_NetworkRotation, this.m_Model.InterpolateLerpSpeed * Time.deltaTime);
		}
	}

	// Token: 0x06000765 RID: 1893 RVA: 0x0003EE6C File Offset: 0x0003D26C
	public void OnPhotonSerializeView(Quaternion currentRotation, PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.m_Model.SynchronizeEnabled)
		{
			return;
		}
		if (stream.isWriting)
		{
			stream.SendNext(currentRotation);
			this.m_NetworkRotation = currentRotation;
		}
		else
		{
			this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
		}
	}

	// Token: 0x04000941 RID: 2369
	private PhotonTransformViewRotationModel m_Model;

	// Token: 0x04000942 RID: 2370
	private Quaternion m_NetworkRotation;
}
