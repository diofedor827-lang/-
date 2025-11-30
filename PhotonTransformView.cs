using System;
using UnityEngine;

// Token: 0x02000115 RID: 277
[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Photon Networking/Photon Transform View")]
public class PhotonTransformView : MonoBehaviour, IPunObservable
{
	// Token: 0x0600074F RID: 1871 RVA: 0x0003E554 File Offset: 0x0003C954
	private void Awake()
	{
		this.m_PhotonView = base.GetComponent<PhotonView>();
		this.m_PositionControl = new PhotonTransformViewPositionControl(this.m_PositionModel);
		this.m_RotationControl = new PhotonTransformViewRotationControl(this.m_RotationModel);
		this.m_ScaleControl = new PhotonTransformViewScaleControl(this.m_ScaleModel);
	}

	// Token: 0x06000750 RID: 1872 RVA: 0x0003E5A0 File Offset: 0x0003C9A0
	private void OnEnable()
	{
		this.m_firstTake = true;
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x0003E5A9 File Offset: 0x0003C9A9
	private void Update()
	{
		if (this.m_PhotonView == null || this.m_PhotonView.isMine || !PhotonNetwork.connected)
		{
			return;
		}
		this.UpdatePosition();
		this.UpdateRotation();
		this.UpdateScale();
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x0003E5E9 File Offset: 0x0003C9E9
	private void UpdatePosition()
	{
		if (!this.m_PositionModel.SynchronizeEnabled || !this.m_ReceivedNetworkUpdate)
		{
			return;
		}
		base.transform.localPosition = this.m_PositionControl.UpdatePosition(base.transform.localPosition);
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x0003E628 File Offset: 0x0003CA28
	private void UpdateRotation()
	{
		if (!this.m_RotationModel.SynchronizeEnabled || !this.m_ReceivedNetworkUpdate)
		{
			return;
		}
		base.transform.localRotation = this.m_RotationControl.GetRotation(base.transform.localRotation);
	}

	// Token: 0x06000754 RID: 1876 RVA: 0x0003E667 File Offset: 0x0003CA67
	private void UpdateScale()
	{
		if (!this.m_ScaleModel.SynchronizeEnabled || !this.m_ReceivedNetworkUpdate)
		{
			return;
		}
		base.transform.localScale = this.m_ScaleControl.GetScale(base.transform.localScale);
	}

	// Token: 0x06000755 RID: 1877 RVA: 0x0003E6A6 File Offset: 0x0003CAA6
	public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
	{
		this.m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
	}

	// Token: 0x06000756 RID: 1878 RVA: 0x0003E6B8 File Offset: 0x0003CAB8
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		this.m_PositionControl.OnPhotonSerializeView(base.transform.localPosition, stream, info);
		this.m_RotationControl.OnPhotonSerializeView(base.transform.localRotation, stream, info);
		this.m_ScaleControl.OnPhotonSerializeView(base.transform.localScale, stream, info);
		if (!this.m_PhotonView.isMine && this.m_PositionModel.DrawErrorGizmo)
		{
			this.DoDrawEstimatedPositionError();
		}
		if (stream.isReading)
		{
			this.m_ReceivedNetworkUpdate = true;
			if (this.m_firstTake)
			{
				this.m_firstTake = false;
				if (this.m_PositionModel.SynchronizeEnabled)
				{
					base.transform.localPosition = this.m_PositionControl.GetNetworkPosition();
				}
				if (this.m_RotationModel.SynchronizeEnabled)
				{
					base.transform.localRotation = this.m_RotationControl.GetNetworkRotation();
				}
				if (this.m_ScaleModel.SynchronizeEnabled)
				{
					base.transform.localScale = this.m_ScaleControl.GetNetworkScale();
				}
			}
		}
	}

	// Token: 0x06000757 RID: 1879 RVA: 0x0003E7CC File Offset: 0x0003CBCC
	private void DoDrawEstimatedPositionError()
	{
		Vector3 vector = this.m_PositionControl.GetNetworkPosition();
		if (base.transform.parent != null)
		{
			vector = base.transform.parent.position + vector;
		}
		Debug.DrawLine(vector, base.transform.position, Color.red, 2f);
		Debug.DrawLine(base.transform.position, base.transform.position + Vector3.up, Color.green, 2f);
		Debug.DrawLine(vector, vector + Vector3.up, Color.red, 2f);
	}

	// Token: 0x04000917 RID: 2327
	[SerializeField]
	public PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();

	// Token: 0x04000918 RID: 2328
	[SerializeField]
	public PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();

	// Token: 0x04000919 RID: 2329
	[SerializeField]
	public PhotonTransformViewScaleModel m_ScaleModel = new PhotonTransformViewScaleModel();

	// Token: 0x0400091A RID: 2330
	private PhotonTransformViewPositionControl m_PositionControl;

	// Token: 0x0400091B RID: 2331
	private PhotonTransformViewRotationControl m_RotationControl;

	// Token: 0x0400091C RID: 2332
	private PhotonTransformViewScaleControl m_ScaleControl;

	// Token: 0x0400091D RID: 2333
	private PhotonView m_PhotonView;

	// Token: 0x0400091E RID: 2334
	private bool m_ReceivedNetworkUpdate;

	// Token: 0x0400091F RID: 2335
	private bool m_firstTake;
}
