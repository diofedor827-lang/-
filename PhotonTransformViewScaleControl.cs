using System;
using UnityEngine;

// Token: 0x0200011D RID: 285
public class PhotonTransformViewScaleControl
{
	// Token: 0x06000767 RID: 1895 RVA: 0x0003EEE3 File Offset: 0x0003D2E3
	public PhotonTransformViewScaleControl(PhotonTransformViewScaleModel model)
	{
		this.m_Model = model;
	}

	// Token: 0x06000768 RID: 1896 RVA: 0x0003EEFD File Offset: 0x0003D2FD
	public Vector3 GetNetworkScale()
	{
		return this.m_NetworkScale;
	}

	// Token: 0x06000769 RID: 1897 RVA: 0x0003EF08 File Offset: 0x0003D308
	public Vector3 GetScale(Vector3 currentScale)
	{
		switch (this.m_Model.InterpolateOption)
		{
		default:
			return this.m_NetworkScale;
		case PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards:
			return Vector3.MoveTowards(currentScale, this.m_NetworkScale, this.m_Model.InterpolateMoveTowardsSpeed * Time.deltaTime);
		case PhotonTransformViewScaleModel.InterpolateOptions.Lerp:
			return Vector3.Lerp(currentScale, this.m_NetworkScale, this.m_Model.InterpolateLerpSpeed * Time.deltaTime);
		}
	}

	// Token: 0x0600076A RID: 1898 RVA: 0x0003EF7C File Offset: 0x0003D37C
	public void OnPhotonSerializeView(Vector3 currentScale, PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.m_Model.SynchronizeEnabled)
		{
			return;
		}
		if (stream.isWriting)
		{
			stream.SendNext(currentScale);
			this.m_NetworkScale = currentScale;
		}
		else
		{
			this.m_NetworkScale = (Vector3)stream.ReceiveNext();
		}
	}

	// Token: 0x0400094B RID: 2379
	private PhotonTransformViewScaleModel m_Model;

	// Token: 0x0400094C RID: 2380
	private Vector3 m_NetworkScale = Vector3.one;
}
