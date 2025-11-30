using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200010E RID: 270
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Photon Networking/Photon Animator View")]
public class PhotonAnimatorView : MonoBehaviour, IPunObservable
{
	// Token: 0x06000734 RID: 1844 RVA: 0x0003D7E9 File Offset: 0x0003BBE9
	private void Awake()
	{
		this.m_PhotonView = base.GetComponent<PhotonView>();
		this.m_StreamQueue = new PhotonStreamQueue(120);
		this.m_Animator = base.GetComponent<Animator>();
	}

	// Token: 0x06000735 RID: 1845 RVA: 0x0003D810 File Offset: 0x0003BC10
	private void Update()
	{
		if (this.m_Animator.applyRootMotion && !this.m_PhotonView.isMine && PhotonNetwork.connected)
		{
			this.m_Animator.applyRootMotion = false;
		}
		if (!PhotonNetwork.inRoom || PhotonNetwork.room.PlayerCount <= 1)
		{
			this.m_StreamQueue.Reset();
			return;
		}
		if (this.m_PhotonView.isMine)
		{
			this.SerializeDataContinuously();
			this.CacheDiscreteTriggers();
		}
		else
		{
			this.DeserializeDataContinuously();
		}
	}

	// Token: 0x06000736 RID: 1846 RVA: 0x0003D8A0 File Offset: 0x0003BCA0
	public void CacheDiscreteTriggers()
	{
		for (int i = 0; i < this.m_SynchronizeParameters.Count; i++)
		{
			PhotonAnimatorView.SynchronizedParameter synchronizedParameter = this.m_SynchronizeParameters[i];
			if (synchronizedParameter.SynchronizeType == PhotonAnimatorView.SynchronizeType.Discrete && synchronizedParameter.Type == PhotonAnimatorView.ParameterType.Trigger && this.m_Animator.GetBool(synchronizedParameter.Name) && synchronizedParameter.Type == PhotonAnimatorView.ParameterType.Trigger)
			{
				this.m_raisedDiscreteTriggersCache.Add(synchronizedParameter.Name);
				break;
			}
		}
	}

	// Token: 0x06000737 RID: 1847 RVA: 0x0003D928 File Offset: 0x0003BD28
	public bool DoesLayerSynchronizeTypeExist(int layerIndex)
	{
		return this.m_SynchronizeLayers.FindIndex((PhotonAnimatorView.SynchronizedLayer item) => item.LayerIndex == layerIndex) != -1;
	}

	// Token: 0x06000738 RID: 1848 RVA: 0x0003D960 File Offset: 0x0003BD60
	public bool DoesParameterSynchronizeTypeExist(string name)
	{
		return this.m_SynchronizeParameters.FindIndex((PhotonAnimatorView.SynchronizedParameter item) => item.Name == name) != -1;
	}

	// Token: 0x06000739 RID: 1849 RVA: 0x0003D997 File Offset: 0x0003BD97
	public List<PhotonAnimatorView.SynchronizedLayer> GetSynchronizedLayers()
	{
		return this.m_SynchronizeLayers;
	}

	// Token: 0x0600073A RID: 1850 RVA: 0x0003D99F File Offset: 0x0003BD9F
	public List<PhotonAnimatorView.SynchronizedParameter> GetSynchronizedParameters()
	{
		return this.m_SynchronizeParameters;
	}

	// Token: 0x0600073B RID: 1851 RVA: 0x0003D9A8 File Offset: 0x0003BDA8
	public PhotonAnimatorView.SynchronizeType GetLayerSynchronizeType(int layerIndex)
	{
		int num = this.m_SynchronizeLayers.FindIndex((PhotonAnimatorView.SynchronizedLayer item) => item.LayerIndex == layerIndex);
		if (num == -1)
		{
			return PhotonAnimatorView.SynchronizeType.Disabled;
		}
		return this.m_SynchronizeLayers[num].SynchronizeType;
	}

	// Token: 0x0600073C RID: 1852 RVA: 0x0003D9F4 File Offset: 0x0003BDF4
	public PhotonAnimatorView.SynchronizeType GetParameterSynchronizeType(string name)
	{
		int num = this.m_SynchronizeParameters.FindIndex((PhotonAnimatorView.SynchronizedParameter item) => item.Name == name);
		if (num == -1)
		{
			return PhotonAnimatorView.SynchronizeType.Disabled;
		}
		return this.m_SynchronizeParameters[num].SynchronizeType;
	}

	// Token: 0x0600073D RID: 1853 RVA: 0x0003DA40 File Offset: 0x0003BE40
	public void SetLayerSynchronized(int layerIndex, PhotonAnimatorView.SynchronizeType synchronizeType)
	{
		if (Application.isPlaying)
		{
			this.m_WasSynchronizeTypeChanged = true;
		}
		int num = this.m_SynchronizeLayers.FindIndex((PhotonAnimatorView.SynchronizedLayer item) => item.LayerIndex == layerIndex);
		if (num == -1)
		{
			this.m_SynchronizeLayers.Add(new PhotonAnimatorView.SynchronizedLayer
			{
				LayerIndex = layerIndex,
				SynchronizeType = synchronizeType
			});
		}
		else
		{
			this.m_SynchronizeLayers[num].SynchronizeType = synchronizeType;
		}
	}

	// Token: 0x0600073E RID: 1854 RVA: 0x0003DAC8 File Offset: 0x0003BEC8
	public void SetParameterSynchronized(string name, PhotonAnimatorView.ParameterType type, PhotonAnimatorView.SynchronizeType synchronizeType)
	{
		if (Application.isPlaying)
		{
			this.m_WasSynchronizeTypeChanged = true;
		}
		int num = this.m_SynchronizeParameters.FindIndex((PhotonAnimatorView.SynchronizedParameter item) => item.Name == name);
		if (num == -1)
		{
			this.m_SynchronizeParameters.Add(new PhotonAnimatorView.SynchronizedParameter
			{
				Name = name,
				Type = type,
				SynchronizeType = synchronizeType
			});
		}
		else
		{
			this.m_SynchronizeParameters[num].SynchronizeType = synchronizeType;
		}
	}

	// Token: 0x0600073F RID: 1855 RVA: 0x0003DB58 File Offset: 0x0003BF58
	private void SerializeDataContinuously()
	{
		if (this.m_Animator == null)
		{
			return;
		}
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			if (this.m_SynchronizeLayers[i].SynchronizeType == PhotonAnimatorView.SynchronizeType.Continuous)
			{
				this.m_StreamQueue.SendNext(this.m_Animator.GetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex));
			}
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			PhotonAnimatorView.SynchronizedParameter synchronizedParameter = this.m_SynchronizeParameters[j];
			if (synchronizedParameter.SynchronizeType == PhotonAnimatorView.SynchronizeType.Continuous)
			{
				PhotonAnimatorView.ParameterType type = synchronizedParameter.Type;
				switch (type)
				{
				case PhotonAnimatorView.ParameterType.Float:
					this.m_StreamQueue.SendNext(this.m_Animator.GetFloat(synchronizedParameter.Name));
					break;
				default:
					if (type == PhotonAnimatorView.ParameterType.Trigger)
					{
						this.m_StreamQueue.SendNext(this.m_Animator.GetBool(synchronizedParameter.Name));
					}
					break;
				case PhotonAnimatorView.ParameterType.Int:
					this.m_StreamQueue.SendNext(this.m_Animator.GetInteger(synchronizedParameter.Name));
					break;
				case PhotonAnimatorView.ParameterType.Bool:
					this.m_StreamQueue.SendNext(this.m_Animator.GetBool(synchronizedParameter.Name));
					break;
				}
			}
		}
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x0003DCD0 File Offset: 0x0003C0D0
	private void DeserializeDataContinuously()
	{
		if (!this.m_StreamQueue.HasQueuedObjects())
		{
			return;
		}
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			if (this.m_SynchronizeLayers[i].SynchronizeType == PhotonAnimatorView.SynchronizeType.Continuous)
			{
				this.m_Animator.SetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex, (float)this.m_StreamQueue.ReceiveNext());
			}
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			PhotonAnimatorView.SynchronizedParameter synchronizedParameter = this.m_SynchronizeParameters[j];
			if (synchronizedParameter.SynchronizeType == PhotonAnimatorView.SynchronizeType.Continuous)
			{
				PhotonAnimatorView.ParameterType type = synchronizedParameter.Type;
				switch (type)
				{
				case PhotonAnimatorView.ParameterType.Float:
					this.m_Animator.SetFloat(synchronizedParameter.Name, (float)this.m_StreamQueue.ReceiveNext());
					break;
				default:
					if (type == PhotonAnimatorView.ParameterType.Trigger)
					{
						this.m_Animator.SetBool(synchronizedParameter.Name, (bool)this.m_StreamQueue.ReceiveNext());
					}
					break;
				case PhotonAnimatorView.ParameterType.Int:
					this.m_Animator.SetInteger(synchronizedParameter.Name, (int)this.m_StreamQueue.ReceiveNext());
					break;
				case PhotonAnimatorView.ParameterType.Bool:
					this.m_Animator.SetBool(synchronizedParameter.Name, (bool)this.m_StreamQueue.ReceiveNext());
					break;
				}
			}
		}
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x0003DE48 File Offset: 0x0003C248
	private void SerializeDataDiscretly(PhotonStream stream)
	{
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			if (this.m_SynchronizeLayers[i].SynchronizeType == PhotonAnimatorView.SynchronizeType.Discrete)
			{
				stream.SendNext(this.m_Animator.GetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex));
			}
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			PhotonAnimatorView.SynchronizedParameter synchronizedParameter = this.m_SynchronizeParameters[j];
			if (synchronizedParameter.SynchronizeType == PhotonAnimatorView.SynchronizeType.Discrete)
			{
				PhotonAnimatorView.ParameterType type = synchronizedParameter.Type;
				switch (type)
				{
				case PhotonAnimatorView.ParameterType.Float:
					stream.SendNext(this.m_Animator.GetFloat(synchronizedParameter.Name));
					break;
				default:
					if (type == PhotonAnimatorView.ParameterType.Trigger)
					{
						stream.SendNext(this.m_raisedDiscreteTriggersCache.Contains(synchronizedParameter.Name));
					}
					break;
				case PhotonAnimatorView.ParameterType.Int:
					stream.SendNext(this.m_Animator.GetInteger(synchronizedParameter.Name));
					break;
				case PhotonAnimatorView.ParameterType.Bool:
					stream.SendNext(this.m_Animator.GetBool(synchronizedParameter.Name));
					break;
				}
			}
		}
		this.m_raisedDiscreteTriggersCache.Clear();
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x0003DFA0 File Offset: 0x0003C3A0
	private void DeserializeDataDiscretly(PhotonStream stream)
	{
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			if (this.m_SynchronizeLayers[i].SynchronizeType == PhotonAnimatorView.SynchronizeType.Discrete)
			{
				this.m_Animator.SetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex, (float)stream.ReceiveNext());
			}
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			PhotonAnimatorView.SynchronizedParameter synchronizedParameter = this.m_SynchronizeParameters[j];
			if (synchronizedParameter.SynchronizeType == PhotonAnimatorView.SynchronizeType.Discrete)
			{
				PhotonAnimatorView.ParameterType type = synchronizedParameter.Type;
				switch (type)
				{
				case PhotonAnimatorView.ParameterType.Float:
					if (!(stream.PeekNext() is float))
					{
						return;
					}
					this.m_Animator.SetFloat(synchronizedParameter.Name, (float)stream.ReceiveNext());
					break;
				default:
					if (type == PhotonAnimatorView.ParameterType.Trigger)
					{
						if (!(stream.PeekNext() is bool))
						{
							return;
						}
						if ((bool)stream.ReceiveNext())
						{
							this.m_Animator.SetTrigger(synchronizedParameter.Name);
						}
					}
					break;
				case PhotonAnimatorView.ParameterType.Int:
					if (!(stream.PeekNext() is int))
					{
						return;
					}
					this.m_Animator.SetInteger(synchronizedParameter.Name, (int)stream.ReceiveNext());
					break;
				case PhotonAnimatorView.ParameterType.Bool:
					if (!(stream.PeekNext() is bool))
					{
						return;
					}
					this.m_Animator.SetBool(synchronizedParameter.Name, (bool)stream.ReceiveNext());
					break;
				}
			}
		}
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0003E138 File Offset: 0x0003C538
	private void SerializeSynchronizationTypeState(PhotonStream stream)
	{
		byte[] array = new byte[this.m_SynchronizeLayers.Count + this.m_SynchronizeParameters.Count];
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			array[i] = (byte)this.m_SynchronizeLayers[i].SynchronizeType;
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			array[this.m_SynchronizeLayers.Count + j] = (byte)this.m_SynchronizeParameters[j].SynchronizeType;
		}
		stream.SendNext(array);
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x0003E1D8 File Offset: 0x0003C5D8
	private void DeserializeSynchronizationTypeState(PhotonStream stream)
	{
		byte[] array = (byte[])stream.ReceiveNext();
		for (int i = 0; i < this.m_SynchronizeLayers.Count; i++)
		{
			this.m_SynchronizeLayers[i].SynchronizeType = (PhotonAnimatorView.SynchronizeType)array[i];
		}
		for (int j = 0; j < this.m_SynchronizeParameters.Count; j++)
		{
			this.m_SynchronizeParameters[j].SynchronizeType = (PhotonAnimatorView.SynchronizeType)array[this.m_SynchronizeLayers.Count + j];
		}
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x0003E260 File Offset: 0x0003C660
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.m_Animator == null)
		{
			return;
		}
		if (stream.isWriting)
		{
			if (this.m_WasSynchronizeTypeChanged)
			{
				this.m_StreamQueue.Reset();
				this.SerializeSynchronizationTypeState(stream);
				this.m_WasSynchronizeTypeChanged = false;
			}
			this.m_StreamQueue.Serialize(stream);
			this.SerializeDataDiscretly(stream);
		}
		else
		{
			if (stream.PeekNext() is byte[])
			{
				this.DeserializeSynchronizationTypeState(stream);
			}
			this.m_StreamQueue.Deserialize(stream);
			this.DeserializeDataDiscretly(stream);
		}
	}

	// Token: 0x040008F8 RID: 2296
	private Animator m_Animator;

	// Token: 0x040008F9 RID: 2297
	private PhotonStreamQueue m_StreamQueue;

	// Token: 0x040008FA RID: 2298
	[HideInInspector]
	[SerializeField]
	private bool ShowLayerWeightsInspector = true;

	// Token: 0x040008FB RID: 2299
	[HideInInspector]
	[SerializeField]
	private bool ShowParameterInspector = true;

	// Token: 0x040008FC RID: 2300
	[HideInInspector]
	[SerializeField]
	private List<PhotonAnimatorView.SynchronizedParameter> m_SynchronizeParameters = new List<PhotonAnimatorView.SynchronizedParameter>();

	// Token: 0x040008FD RID: 2301
	[HideInInspector]
	[SerializeField]
	private List<PhotonAnimatorView.SynchronizedLayer> m_SynchronizeLayers = new List<PhotonAnimatorView.SynchronizedLayer>();

	// Token: 0x040008FE RID: 2302
	private Vector3 m_ReceiverPosition;

	// Token: 0x040008FF RID: 2303
	private float m_LastDeserializeTime;

	// Token: 0x04000900 RID: 2304
	private bool m_WasSynchronizeTypeChanged = true;

	// Token: 0x04000901 RID: 2305
	private PhotonView m_PhotonView;

	// Token: 0x04000902 RID: 2306
	private List<string> m_raisedDiscreteTriggersCache = new List<string>();

	// Token: 0x0200010F RID: 271
	public enum ParameterType
	{
		// Token: 0x04000904 RID: 2308
		Float = 1,
		// Token: 0x04000905 RID: 2309
		Int = 3,
		// Token: 0x04000906 RID: 2310
		Bool,
		// Token: 0x04000907 RID: 2311
		Trigger = 9
	}

	// Token: 0x02000110 RID: 272
	public enum SynchronizeType
	{
		// Token: 0x04000909 RID: 2313
		Disabled,
		// Token: 0x0400090A RID: 2314
		Discrete,
		// Token: 0x0400090B RID: 2315
		Continuous
	}

	// Token: 0x02000111 RID: 273
	[Serializable]
	public class SynchronizedParameter
	{
		// Token: 0x0400090C RID: 2316
		public PhotonAnimatorView.ParameterType Type;

		// Token: 0x0400090D RID: 2317
		public PhotonAnimatorView.SynchronizeType SynchronizeType;

		// Token: 0x0400090E RID: 2318
		public string Name;
	}

	// Token: 0x02000112 RID: 274
	[Serializable]
	public class SynchronizedLayer
	{
		// Token: 0x0400090F RID: 2319
		public PhotonAnimatorView.SynchronizeType SynchronizeType;

		// Token: 0x04000910 RID: 2320
		public int LayerIndex;
	}
}
