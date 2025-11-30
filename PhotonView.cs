using System;
using System.Collections.Generic;
using System.Reflection;
using Photon;
using UnityEngine;

// Token: 0x02000106 RID: 262
[AddComponentMenu("Photon Networking/Photon View &v")]
public class PhotonView : Photon.MonoBehaviour
{
	// Token: 0x1700009C RID: 156
	// (get) Token: 0x060006B8 RID: 1720 RVA: 0x0003BB5D File Offset: 0x00039F5D
	// (set) Token: 0x060006B9 RID: 1721 RVA: 0x0003BB8B File Offset: 0x00039F8B
	public int prefix
	{
		get
		{
			if (this.prefixBackup == -1 && PhotonNetwork.networkingPeer != null)
			{
				this.prefixBackup = (int)PhotonNetwork.networkingPeer.currentLevelPrefix;
			}
			return this.prefixBackup;
		}
		set
		{
			this.prefixBackup = value;
		}
	}

	// Token: 0x1700009D RID: 157
	// (get) Token: 0x060006BA RID: 1722 RVA: 0x0003BB94 File Offset: 0x00039F94
	// (set) Token: 0x060006BB RID: 1723 RVA: 0x0003BBBD File Offset: 0x00039FBD
	public object[] instantiationData
	{
		get
		{
			if (!this.didAwake)
			{
				this.instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(this.instantiationId);
			}
			return this.instantiationDataField;
		}
		set
		{
			this.instantiationDataField = value;
		}
	}

	// Token: 0x1700009E RID: 158
	// (get) Token: 0x060006BC RID: 1724 RVA: 0x0003BBC6 File Offset: 0x00039FC6
	// (set) Token: 0x060006BD RID: 1725 RVA: 0x0003BBD0 File Offset: 0x00039FD0
	public int viewID
	{
		get
		{
			return this.viewIdField;
		}
		set
		{
			bool flag = this.didAwake && this.viewIdField == 0;
			this.ownerId = value / PhotonNetwork.MAX_VIEW_IDS;
			this.viewIdField = value;
			if (flag)
			{
				PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			}
		}
	}

	// Token: 0x1700009F RID: 159
	// (get) Token: 0x060006BE RID: 1726 RVA: 0x0003BC1A File Offset: 0x0003A01A
	public bool isSceneView
	{
		get
		{
			return this.CreatorActorNr == 0;
		}
	}

	// Token: 0x170000A0 RID: 160
	// (get) Token: 0x060006BF RID: 1727 RVA: 0x0003BC25 File Offset: 0x0003A025
	public PhotonPlayer owner
	{
		get
		{
			return PhotonPlayer.Find(this.ownerId);
		}
	}

	// Token: 0x170000A1 RID: 161
	// (get) Token: 0x060006C0 RID: 1728 RVA: 0x0003BC32 File Offset: 0x0003A032
	public int OwnerActorNr
	{
		get
		{
			return this.ownerId;
		}
	}

	// Token: 0x170000A2 RID: 162
	// (get) Token: 0x060006C1 RID: 1729 RVA: 0x0003BC3A File Offset: 0x0003A03A
	public bool isOwnerActive
	{
		get
		{
			return this.ownerId != 0 && PhotonNetwork.networkingPeer.mActors.ContainsKey(this.ownerId);
		}
	}

	// Token: 0x170000A3 RID: 163
	// (get) Token: 0x060006C2 RID: 1730 RVA: 0x0003BC5F File Offset: 0x0003A05F
	public int CreatorActorNr
	{
		get
		{
			return this.viewIdField / PhotonNetwork.MAX_VIEW_IDS;
		}
	}

	// Token: 0x170000A4 RID: 164
	// (get) Token: 0x060006C3 RID: 1731 RVA: 0x0003BC6D File Offset: 0x0003A06D
	public bool isMine
	{
		get
		{
			return this.ownerId == PhotonNetwork.player.ID || (!this.isOwnerActive && PhotonNetwork.isMasterClient);
		}
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x0003BC9A File Offset: 0x0003A09A
	protected internal void Awake()
	{
		if (this.viewID != 0)
		{
			PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			this.instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(this.instantiationId);
		}
		this.didAwake = true;
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x0003BCCF File Offset: 0x0003A0CF
	public void RequestOwnership()
	{
		PhotonNetwork.networkingPeer.RequestOwnership(this.viewID, this.ownerId);
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x0003BCE7 File Offset: 0x0003A0E7
	public void TransferOwnership(PhotonPlayer newOwner)
	{
		this.TransferOwnership(newOwner.ID);
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x0003BCF5 File Offset: 0x0003A0F5
	public void TransferOwnership(int newOwnerId)
	{
		PhotonNetwork.networkingPeer.TransferOwnership(this.viewID, newOwnerId);
		this.ownerId = newOwnerId;
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x0003BD10 File Offset: 0x0003A110
	public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (this.CreatorActorNr == 0 && !this.OwnerShipWasTransfered && (this.currentMasterID == -1 || this.ownerId == this.currentMasterID))
		{
			this.ownerId = newMasterClient.ID;
		}
		this.currentMasterID = newMasterClient.ID;
	}

	// Token: 0x060006C9 RID: 1737 RVA: 0x0003BD68 File Offset: 0x0003A168
	protected internal void OnDestroy()
	{
		if (!this.removedFromLocalViewList)
		{
			bool flag = PhotonNetwork.networkingPeer.LocalCleanPhotonView(this);
			bool flag2 = false;
			if (flag && !flag2 && this.instantiationId > 0 && !PhotonHandler.AppQuits && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log("PUN-instantiated '" + base.gameObject.name + "' got destroyed by engine. This is OK when loading levels. Otherwise use: PhotonNetwork.Destroy().");
			}
		}
	}

	// Token: 0x060006CA RID: 1738 RVA: 0x0003BDDC File Offset: 0x0003A1DC
	public void SerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
		{
			for (int i = 0; i < this.ObservedComponents.Count; i++)
			{
				this.SerializeComponent(this.ObservedComponents[i], stream, info);
			}
		}
	}

	// Token: 0x060006CB RID: 1739 RVA: 0x0003BE38 File Offset: 0x0003A238
	public void DeserializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
		{
			for (int i = 0; i < this.ObservedComponents.Count; i++)
			{
				this.DeserializeComponent(this.ObservedComponents[i], stream, info);
			}
		}
	}

	// Token: 0x060006CC RID: 1740 RVA: 0x0003BE94 File Offset: 0x0003A294
	protected internal void DeserializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is UnityEngine.MonoBehaviour)
		{
			this.ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (this.onSerializeTransformOption)
			{
			case OnSerializeTransform.OnlyPosition:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyRotation:
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyScale:
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.PositionAndRotation:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			case OnSerializeTransform.All:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			OnSerializeRigidBody onSerializeRigidBody = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody == OnSerializeRigidBody.OnlyVelocity)
					{
						rigidbody.velocity = (Vector3)stream.ReceiveNext();
					}
				}
				else
				{
					rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
				}
			}
			else
			{
				rigidbody.velocity = (Vector3)stream.ReceiveNext();
				rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			OnSerializeRigidBody onSerializeRigidBody2 = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody2 != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody2 != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody2 == OnSerializeRigidBody.OnlyVelocity)
					{
						rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
					}
				}
				else
				{
					rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
				}
			}
			else
			{
				rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
				rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
			}
		}
		else
		{
			Debug.LogError("Type of observed is unknown when receiving.");
		}
	}

	// Token: 0x060006CD RID: 1741 RVA: 0x0003C0CC File Offset: 0x0003A4CC
	protected internal void SerializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is UnityEngine.MonoBehaviour)
		{
			this.ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (this.onSerializeTransformOption)
			{
			case OnSerializeTransform.OnlyPosition:
				stream.SendNext(transform.localPosition);
				break;
			case OnSerializeTransform.OnlyRotation:
				stream.SendNext(transform.localRotation);
				break;
			case OnSerializeTransform.OnlyScale:
				stream.SendNext(transform.localScale);
				break;
			case OnSerializeTransform.PositionAndRotation:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				break;
			case OnSerializeTransform.All:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				stream.SendNext(transform.localScale);
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			OnSerializeRigidBody onSerializeRigidBody = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody == OnSerializeRigidBody.OnlyVelocity)
					{
						stream.SendNext(rigidbody.velocity);
					}
				}
				else
				{
					stream.SendNext(rigidbody.angularVelocity);
				}
			}
			else
			{
				stream.SendNext(rigidbody.velocity);
				stream.SendNext(rigidbody.angularVelocity);
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			OnSerializeRigidBody onSerializeRigidBody2 = this.onSerializeRigidBodyOption;
			if (onSerializeRigidBody2 != OnSerializeRigidBody.All)
			{
				if (onSerializeRigidBody2 != OnSerializeRigidBody.OnlyAngularVelocity)
				{
					if (onSerializeRigidBody2 == OnSerializeRigidBody.OnlyVelocity)
					{
						stream.SendNext(rigidbody2D.velocity);
					}
				}
				else
				{
					stream.SendNext(rigidbody2D.angularVelocity);
				}
			}
			else
			{
				stream.SendNext(rigidbody2D.velocity);
				stream.SendNext(rigidbody2D.angularVelocity);
			}
		}
		else
		{
			Debug.LogError("Observed type is not serializable: " + component.GetType());
		}
	}

	// Token: 0x060006CE RID: 1742 RVA: 0x0003C310 File Offset: 0x0003A710
	protected internal void ExecuteComponentOnSerialize(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		IPunObservable punObservable = component as IPunObservable;
		if (punObservable != null)
		{
			punObservable.OnPhotonSerializeView(stream, info);
		}
		else if (component != null)
		{
			MethodInfo methodInfo = null;
			if (!this.m_OnSerializeMethodInfos.TryGetValue(component, out methodInfo))
			{
				if (!NetworkingPeer.GetMethod(component as UnityEngine.MonoBehaviour, PhotonNetworkingMessage.OnPhotonSerializeView.ToString(), out methodInfo))
				{
					Debug.LogError("The observed monobehaviour (" + component.name + ") of this PhotonView does not implement OnPhotonSerializeView()!");
					methodInfo = null;
				}
				this.m_OnSerializeMethodInfos.Add(component, methodInfo);
			}
			if (methodInfo != null)
			{
				methodInfo.Invoke(component, new object[]
				{
					stream,
					info
				});
			}
		}
	}

	// Token: 0x060006CF RID: 1743 RVA: 0x0003C3C9 File Offset: 0x0003A7C9
	public void RefreshRpcMonoBehaviourCache()
	{
		this.RpcMonoBehaviours = base.GetComponents<UnityEngine.MonoBehaviour>();
	}

	// Token: 0x060006D0 RID: 1744 RVA: 0x0003C3D7 File Offset: 0x0003A7D7
	public void RPC(string methodName, PhotonTargets target, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, false, parameters);
	}

	// Token: 0x060006D1 RID: 1745 RVA: 0x0003C3E3 File Offset: 0x0003A7E3
	public void RpcSecure(string methodName, PhotonTargets target, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, encrypt, parameters);
	}

	// Token: 0x060006D2 RID: 1746 RVA: 0x0003C3F0 File Offset: 0x0003A7F0
	public void RPC(string methodName, PhotonPlayer targetPlayer, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, false, parameters);
	}

	// Token: 0x060006D3 RID: 1747 RVA: 0x0003C3FC File Offset: 0x0003A7FC
	public void RpcSecure(string methodName, PhotonPlayer targetPlayer, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, encrypt, parameters);
	}

	// Token: 0x060006D4 RID: 1748 RVA: 0x0003C409 File Offset: 0x0003A809
	public static PhotonView Get(Component component)
	{
		return component.GetComponent<PhotonView>();
	}

	// Token: 0x060006D5 RID: 1749 RVA: 0x0003C411 File Offset: 0x0003A811
	public static PhotonView Get(GameObject gameObj)
	{
		return gameObj.GetComponent<PhotonView>();
	}

	// Token: 0x060006D6 RID: 1750 RVA: 0x0003C419 File Offset: 0x0003A819
	public static PhotonView Find(int viewID)
	{
		return PhotonNetwork.networkingPeer.GetPhotonView(viewID);
	}

	// Token: 0x060006D7 RID: 1751 RVA: 0x0003C428 File Offset: 0x0003A828
	public override string ToString()
	{
		return string.Format("View ({3}){0} on {1} {2}", new object[]
		{
			this.viewID,
			(!(base.gameObject != null)) ? "GO==null" : base.gameObject.name,
			(!this.isSceneView) ? string.Empty : "(scene)",
			this.prefix
		});
	}

	// Token: 0x040008B1 RID: 2225
	public int ownerId;

	// Token: 0x040008B2 RID: 2226
	public byte group;

	// Token: 0x040008B3 RID: 2227
	protected internal bool mixedModeIsReliable;

	// Token: 0x040008B4 RID: 2228
	public bool OwnerShipWasTransfered;

	// Token: 0x040008B5 RID: 2229
	public int prefixBackup = -1;

	// Token: 0x040008B6 RID: 2230
	internal object[] instantiationDataField;

	// Token: 0x040008B7 RID: 2231
	protected internal object[] lastOnSerializeDataSent;

	// Token: 0x040008B8 RID: 2232
	protected internal object[] lastOnSerializeDataReceived;

	// Token: 0x040008B9 RID: 2233
	public ViewSynchronization synchronization;

	// Token: 0x040008BA RID: 2234
	public OnSerializeTransform onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;

	// Token: 0x040008BB RID: 2235
	public OnSerializeRigidBody onSerializeRigidBodyOption = OnSerializeRigidBody.All;

	// Token: 0x040008BC RID: 2236
	public OwnershipOption ownershipTransfer;

	// Token: 0x040008BD RID: 2237
	public List<Component> ObservedComponents;

	// Token: 0x040008BE RID: 2238
	private Dictionary<Component, MethodInfo> m_OnSerializeMethodInfos = new Dictionary<Component, MethodInfo>(3);

	// Token: 0x040008BF RID: 2239
	[SerializeField]
	private int viewIdField;

	// Token: 0x040008C0 RID: 2240
	public int instantiationId;

	// Token: 0x040008C1 RID: 2241
	public int currentMasterID = -1;

	// Token: 0x040008C2 RID: 2242
	protected internal bool didAwake;

	// Token: 0x040008C3 RID: 2243
	[SerializeField]
	protected internal bool isRuntimeInstantiated;

	// Token: 0x040008C4 RID: 2244
	protected internal bool removedFromLocalViewList;

	// Token: 0x040008C5 RID: 2245
	internal UnityEngine.MonoBehaviour[] RpcMonoBehaviours;

	// Token: 0x040008C6 RID: 2246
	private MethodInfo OnSerializeMethodInfo;

	// Token: 0x040008C7 RID: 2247
	private bool failedToFindOnSerialize;
}
