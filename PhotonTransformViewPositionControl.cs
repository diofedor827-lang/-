using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000116 RID: 278
public class PhotonTransformViewPositionControl
{
	// Token: 0x06000758 RID: 1880 RVA: 0x0003E877 File Offset: 0x0003CC77
	public PhotonTransformViewPositionControl(PhotonTransformViewPositionModel model)
	{
		this.m_Model = model;
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x0003E8A4 File Offset: 0x0003CCA4
	private Vector3 GetOldestStoredNetworkPosition()
	{
		Vector3 result = this.m_NetworkPosition;
		if (this.m_OldNetworkPositions.Count > 0)
		{
			result = this.m_OldNetworkPositions.Peek();
		}
		return result;
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x0003E8D6 File Offset: 0x0003CCD6
	public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
	{
		this.m_SynchronizedSpeed = speed;
		this.m_SynchronizedTurnSpeed = turnSpeed;
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x0003E8E8 File Offset: 0x0003CCE8
	public Vector3 UpdatePosition(Vector3 currentPosition)
	{
		Vector3 vector = this.GetNetworkPosition() + this.GetExtrapolatedPositionOffset();
		switch (this.m_Model.InterpolateOption)
		{
		case PhotonTransformViewPositionModel.InterpolateOptions.Disabled:
			if (!this.m_UpdatedPositionAfterOnSerialize)
			{
				currentPosition = vector;
				this.m_UpdatedPositionAfterOnSerialize = true;
			}
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
			currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * this.m_Model.InterpolateMoveTowardsSpeed);
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.EstimatedSpeed:
			if (this.m_OldNetworkPositions.Count != 0)
			{
				float num = Vector3.Distance(this.m_NetworkPosition, this.GetOldestStoredNetworkPosition()) / (float)this.m_OldNetworkPositions.Count * (float)PhotonNetwork.sendRateOnSerialize;
				currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * num);
			}
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues:
			if (this.m_SynchronizedSpeed.magnitude == 0f)
			{
				currentPosition = vector;
			}
			else
			{
				currentPosition = Vector3.MoveTowards(currentPosition, vector, Time.deltaTime * this.m_SynchronizedSpeed.magnitude);
			}
			break;
		case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
			currentPosition = Vector3.Lerp(currentPosition, vector, Time.deltaTime * this.m_Model.InterpolateLerpSpeed);
			break;
		}
		if (this.m_Model.TeleportEnabled && Vector3.Distance(currentPosition, this.GetNetworkPosition()) > this.m_Model.TeleportIfDistanceGreaterThan)
		{
			currentPosition = this.GetNetworkPosition();
		}
		return currentPosition;
	}

	// Token: 0x0600075C RID: 1884 RVA: 0x0003EA4B File Offset: 0x0003CE4B
	public Vector3 GetNetworkPosition()
	{
		return this.m_NetworkPosition;
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x0003EA54 File Offset: 0x0003CE54
	public Vector3 GetExtrapolatedPositionOffset()
	{
		float num = (float)(PhotonNetwork.time - this.m_LastSerializeTime);
		if (this.m_Model.ExtrapolateIncludingRoundTripTime)
		{
			num += (float)PhotonNetwork.GetPing() / 1000f;
		}
		Vector3 result = Vector3.zero;
		PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption = this.m_Model.ExtrapolateOption;
		if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues)
		{
			if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed)
			{
				if (extrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.EstimateSpeedAndTurn)
				{
					Vector3 a = (this.m_NetworkPosition - this.GetOldestStoredNetworkPosition()) * (float)PhotonNetwork.sendRateOnSerialize;
					result = a * num;
				}
			}
			else
			{
				Vector3 normalized = (this.m_NetworkPosition - this.GetOldestStoredNetworkPosition()).normalized;
				result = normalized * this.m_Model.ExtrapolateSpeed * num;
			}
		}
		else
		{
			Quaternion rotation = Quaternion.Euler(0f, this.m_SynchronizedTurnSpeed * num, 0f);
			result = rotation * (this.m_SynchronizedSpeed * num);
		}
		return result;
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x0003EB54 File Offset: 0x0003CF54
	public void OnPhotonSerializeView(Vector3 currentPosition, PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.m_Model.SynchronizeEnabled)
		{
			return;
		}
		if (stream.isWriting)
		{
			this.SerializeData(currentPosition, stream, info);
		}
		else
		{
			this.DeserializeData(stream, info);
		}
		this.m_LastSerializeTime = PhotonNetwork.time;
		this.m_UpdatedPositionAfterOnSerialize = false;
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x0003EBA8 File Offset: 0x0003CFA8
	private void SerializeData(Vector3 currentPosition, PhotonStream stream, PhotonMessageInfo info)
	{
		stream.SendNext(currentPosition);
		this.m_NetworkPosition = currentPosition;
		if (this.m_Model.ExtrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues || this.m_Model.InterpolateOption == PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues)
		{
			stream.SendNext(this.m_SynchronizedSpeed);
			stream.SendNext(this.m_SynchronizedTurnSpeed);
		}
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x0003EC0C File Offset: 0x0003D00C
	private void DeserializeData(PhotonStream stream, PhotonMessageInfo info)
	{
		Vector3 networkPosition = (Vector3)stream.ReceiveNext();
		if (this.m_Model.ExtrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues || this.m_Model.InterpolateOption == PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues)
		{
			this.m_SynchronizedSpeed = (Vector3)stream.ReceiveNext();
			this.m_SynchronizedTurnSpeed = (float)stream.ReceiveNext();
		}
		if (this.m_OldNetworkPositions.Count == 0)
		{
			this.m_NetworkPosition = networkPosition;
		}
		this.m_OldNetworkPositions.Enqueue(this.m_NetworkPosition);
		this.m_NetworkPosition = networkPosition;
		while (this.m_OldNetworkPositions.Count > this.m_Model.ExtrapolateNumberOfStoredPositions)
		{
			this.m_OldNetworkPositions.Dequeue();
		}
	}

	// Token: 0x04000920 RID: 2336
	private PhotonTransformViewPositionModel m_Model;

	// Token: 0x04000921 RID: 2337
	private float m_CurrentSpeed;

	// Token: 0x04000922 RID: 2338
	private double m_LastSerializeTime;

	// Token: 0x04000923 RID: 2339
	private Vector3 m_SynchronizedSpeed = Vector3.zero;

	// Token: 0x04000924 RID: 2340
	private float m_SynchronizedTurnSpeed;

	// Token: 0x04000925 RID: 2341
	private Vector3 m_NetworkPosition;

	// Token: 0x04000926 RID: 2342
	private Queue<Vector3> m_OldNetworkPositions = new Queue<Vector3>();

	// Token: 0x04000927 RID: 2343
	private bool m_UpdatedPositionAfterOnSerialize = true;
}
