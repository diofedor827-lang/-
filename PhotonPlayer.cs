using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

// Token: 0x020000FF RID: 255
public class PhotonPlayer : IComparable<PhotonPlayer>, IComparable<int>, IEquatable<PhotonPlayer>, IEquatable<int>
{
	// Token: 0x06000682 RID: 1666 RVA: 0x0003AD32 File Offset: 0x00039132
	public PhotonPlayer(bool isLocal, int actorID, string name)
	{
		this.CustomProperties = new Hashtable();
		this.IsLocal = isLocal;
		this.actorID = actorID;
		this.nameField = name;
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x0003AD6C File Offset: 0x0003916C
	protected internal PhotonPlayer(bool isLocal, int actorID, Hashtable properties)
	{
		this.CustomProperties = new Hashtable();
		this.IsLocal = isLocal;
		this.actorID = actorID;
		this.InternalCacheProperties(properties);
	}

	// Token: 0x1700008E RID: 142
	// (get) Token: 0x06000684 RID: 1668 RVA: 0x0003ADA6 File Offset: 0x000391A6
	public int ID
	{
		get
		{
			return this.actorID;
		}
	}

	// Token: 0x1700008F RID: 143
	// (get) Token: 0x06000685 RID: 1669 RVA: 0x0003ADAE File Offset: 0x000391AE
	// (set) Token: 0x06000686 RID: 1670 RVA: 0x0003ADB8 File Offset: 0x000391B8
	public string NickName
	{
		get
		{
			return this.nameField;
		}
		set
		{
			if (!this.IsLocal)
			{
				Debug.LogError("Error: Cannot change the name of a remote player!");
				return;
			}
			if (string.IsNullOrEmpty(value) || value.Equals(this.nameField))
			{
				return;
			}
			this.nameField = value;
			PhotonNetwork.playerName = value;
		}
	}

	// Token: 0x17000090 RID: 144
	// (get) Token: 0x06000687 RID: 1671 RVA: 0x0003AE05 File Offset: 0x00039205
	// (set) Token: 0x06000688 RID: 1672 RVA: 0x0003AE0D File Offset: 0x0003920D
	public string UserId { get; internal set; }

	// Token: 0x17000091 RID: 145
	// (get) Token: 0x06000689 RID: 1673 RVA: 0x0003AE16 File Offset: 0x00039216
	public bool IsMasterClient
	{
		get
		{
			return PhotonNetwork.networkingPeer.mMasterClientId == this.ID;
		}
	}

	// Token: 0x17000092 RID: 146
	// (get) Token: 0x0600068A RID: 1674 RVA: 0x0003AE2A File Offset: 0x0003922A
	// (set) Token: 0x0600068B RID: 1675 RVA: 0x0003AE32 File Offset: 0x00039232
	public bool IsInactive { get; set; }

	// Token: 0x17000093 RID: 147
	// (get) Token: 0x0600068C RID: 1676 RVA: 0x0003AE3B File Offset: 0x0003923B
	// (set) Token: 0x0600068D RID: 1677 RVA: 0x0003AE43 File Offset: 0x00039243
	public Hashtable CustomProperties { get; internal set; }

	// Token: 0x17000094 RID: 148
	// (get) Token: 0x0600068E RID: 1678 RVA: 0x0003AE4C File Offset: 0x0003924C
	public Hashtable AllProperties
	{
		get
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Merge(this.CustomProperties);
			hashtable[byte.MaxValue] = this.NickName;
			return hashtable;
		}
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x0003AE84 File Offset: 0x00039284
	public override bool Equals(object p)
	{
		PhotonPlayer photonPlayer = p as PhotonPlayer;
		return photonPlayer != null && this.GetHashCode() == photonPlayer.GetHashCode();
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x0003AEAF File Offset: 0x000392AF
	public override int GetHashCode()
	{
		return this.ID;
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x0003AEB7 File Offset: 0x000392B7
	internal void InternalChangeLocalID(int newID)
	{
		if (!this.IsLocal)
		{
			Debug.LogError("ERROR You should never change PhotonPlayer IDs!");
			return;
		}
		this.actorID = newID;
	}

	// Token: 0x06000692 RID: 1682 RVA: 0x0003AED8 File Offset: 0x000392D8
	internal void InternalCacheProperties(Hashtable properties)
	{
		if (properties == null || properties.Count == 0 || this.CustomProperties.Equals(properties))
		{
			return;
		}
		if (properties.ContainsKey(255))
		{
			this.nameField = (string)properties[byte.MaxValue];
		}
		if (properties.ContainsKey(253))
		{
			this.UserId = (string)properties[253];
		}
		if (properties.ContainsKey(254))
		{
			this.IsInactive = (bool)properties[254];
		}
		this.CustomProperties.MergeStringKeys(properties);
		this.CustomProperties.StripKeysWithNullValues();
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x0003AFB0 File Offset: 0x000393B0
	public void SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues = null, bool webForward = false)
	{
		if (propertiesToSet == null)
		{
			return;
		}
		Hashtable hashtable = propertiesToSet.StripToStringKeys();
		Hashtable hashtable2 = expectedValues.StripToStringKeys();
		bool flag = hashtable2 == null || hashtable2.Count == 0;
		bool flag2 = this.actorID > 0 && !PhotonNetwork.offlineMode;
		if (flag)
		{
			this.CustomProperties.Merge(hashtable);
			this.CustomProperties.StripKeysWithNullValues();
		}
		if (flag2)
		{
			PhotonNetwork.networkingPeer.OpSetPropertiesOfActor(this.actorID, hashtable, hashtable2, webForward);
		}
		if (!flag2 || flag)
		{
			this.InternalCacheProperties(hashtable);
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, new object[]
			{
				this,
				hashtable
			});
		}
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x0003B05C File Offset: 0x0003945C
	public static PhotonPlayer Find(int ID)
	{
		if (PhotonNetwork.networkingPeer != null)
		{
			return PhotonNetwork.networkingPeer.GetPlayerWithId(ID);
		}
		return null;
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0003B075 File Offset: 0x00039475
	public PhotonPlayer Get(int id)
	{
		return PhotonPlayer.Find(id);
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x0003B07D File Offset: 0x0003947D
	public PhotonPlayer GetNext()
	{
		return this.GetNextFor(this.ID);
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x0003B08B File Offset: 0x0003948B
	public PhotonPlayer GetNextFor(PhotonPlayer currentPlayer)
	{
		if (currentPlayer == null)
		{
			return null;
		}
		return this.GetNextFor(currentPlayer.ID);
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x0003B0A4 File Offset: 0x000394A4
	public PhotonPlayer GetNextFor(int currentPlayerId)
	{
		if (PhotonNetwork.networkingPeer == null || PhotonNetwork.networkingPeer.mActors == null || PhotonNetwork.networkingPeer.mActors.Count < 2)
		{
			return null;
		}
		Dictionary<int, PhotonPlayer> mActors = PhotonNetwork.networkingPeer.mActors;
		int num = int.MaxValue;
		int num2 = currentPlayerId;
		foreach (int num3 in mActors.Keys)
		{
			if (num3 < num2)
			{
				num2 = num3;
			}
			else if (num3 > currentPlayerId && num3 < num)
			{
				num = num3;
			}
		}
		return (num == int.MaxValue) ? mActors[num2] : mActors[num];
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x0003B17C File Offset: 0x0003957C
	public int CompareTo(PhotonPlayer other)
	{
		if (other == null)
		{
			return 0;
		}
		return this.GetHashCode().CompareTo(other.GetHashCode());
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0003B1A8 File Offset: 0x000395A8
	public int CompareTo(int other)
	{
		return this.GetHashCode().CompareTo(other);
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0003B1C4 File Offset: 0x000395C4
	public bool Equals(PhotonPlayer other)
	{
		return other != null && this.GetHashCode().Equals(other.GetHashCode());
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0003B1F0 File Offset: 0x000395F0
	public bool Equals(int other)
	{
		return this.GetHashCode().Equals(other);
	}

	// Token: 0x0600069D RID: 1693 RVA: 0x0003B20C File Offset: 0x0003960C
	public override string ToString()
	{
		if (string.IsNullOrEmpty(this.NickName))
		{
			return string.Format("#{0:00}{1}{2}", this.ID, (!this.IsInactive) ? " " : " (inactive)", (!this.IsMasterClient) ? string.Empty : "(master)");
		}
		return string.Format("'{0}'{1}{2}", this.NickName, (!this.IsInactive) ? " " : " (inactive)", (!this.IsMasterClient) ? string.Empty : "(master)");
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x0003B2B8 File Offset: 0x000396B8
	public string ToStringFull()
	{
		return string.Format("#{0:00} '{1}'{2} {3}", new object[]
		{
			this.ID,
			this.NickName,
			(!this.IsInactive) ? string.Empty : " (inactive)",
			this.CustomProperties.ToStringFull()
		});
	}

	// Token: 0x17000095 RID: 149
	// (get) Token: 0x0600069F RID: 1695 RVA: 0x0003B317 File Offset: 0x00039717
	// (set) Token: 0x060006A0 RID: 1696 RVA: 0x0003B31F File Offset: 0x0003971F
	[Obsolete("Please use NickName (updated case for naming).")]
	public string name
	{
		get
		{
			return this.NickName;
		}
		set
		{
			this.NickName = value;
		}
	}

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x060006A1 RID: 1697 RVA: 0x0003B328 File Offset: 0x00039728
	// (set) Token: 0x060006A2 RID: 1698 RVA: 0x0003B330 File Offset: 0x00039730
	[Obsolete("Please use UserId (updated case for naming).")]
	public string userId
	{
		get
		{
			return this.UserId;
		}
		internal set
		{
			this.UserId = value;
		}
	}

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x060006A3 RID: 1699 RVA: 0x0003B339 File Offset: 0x00039739
	[Obsolete("Please use IsLocal (updated case for naming).")]
	public bool isLocal
	{
		get
		{
			return this.IsLocal;
		}
	}

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x060006A4 RID: 1700 RVA: 0x0003B341 File Offset: 0x00039741
	[Obsolete("Please use IsMasterClient (updated case for naming).")]
	public bool isMasterClient
	{
		get
		{
			return this.IsMasterClient;
		}
	}

	// Token: 0x17000099 RID: 153
	// (get) Token: 0x060006A5 RID: 1701 RVA: 0x0003B349 File Offset: 0x00039749
	// (set) Token: 0x060006A6 RID: 1702 RVA: 0x0003B351 File Offset: 0x00039751
	[Obsolete("Please use IsInactive (updated case for naming).")]
	public bool isInactive
	{
		get
		{
			return this.IsInactive;
		}
		set
		{
			this.IsInactive = value;
		}
	}

	// Token: 0x1700009A RID: 154
	// (get) Token: 0x060006A7 RID: 1703 RVA: 0x0003B35A File Offset: 0x0003975A
	// (set) Token: 0x060006A8 RID: 1704 RVA: 0x0003B362 File Offset: 0x00039762
	[Obsolete("Please use CustomProperties (updated case for naming).")]
	public Hashtable customProperties
	{
		get
		{
			return this.CustomProperties;
		}
		internal set
		{
			this.CustomProperties = value;
		}
	}

	// Token: 0x1700009B RID: 155
	// (get) Token: 0x060006A9 RID: 1705 RVA: 0x0003B36B File Offset: 0x0003976B
	[Obsolete("Please use AllProperties (updated case for naming).")]
	public Hashtable allProperties
	{
		get
		{
			return this.AllProperties;
		}
	}

	// Token: 0x04000888 RID: 2184
	private int actorID = -1;

	// Token: 0x04000889 RID: 2185
	private string nameField = string.Empty;

	// Token: 0x0400088B RID: 2187
	public readonly bool IsLocal;

	// Token: 0x0400088E RID: 2190
	public object TagObject;
}
