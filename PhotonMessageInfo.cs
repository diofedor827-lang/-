using System;

// Token: 0x020000F6 RID: 246
public struct PhotonMessageInfo
{
	// Token: 0x060005AF RID: 1455 RVA: 0x00037A85 File Offset: 0x00035E85
	public PhotonMessageInfo(PhotonPlayer player, int timestamp, PhotonView view)
	{
		this.sender = player;
		this.timeInt = timestamp;
		this.photonView = view;
	}

	// Token: 0x17000053 RID: 83
	// (get) Token: 0x060005B0 RID: 1456 RVA: 0x00037A9C File Offset: 0x00035E9C
	public double timestamp
	{
		get
		{
			uint num = (uint)this.timeInt;
			double num2 = num;
			return num2 / 1000.0;
		}
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x00037ABF File Offset: 0x00035EBF
	public override string ToString()
	{
		return string.Format("[PhotonMessageInfo: Sender='{1}' Senttime={0}]", this.timestamp, this.sender);
	}

	// Token: 0x0400083E RID: 2110
	private readonly int timeInt;

	// Token: 0x0400083F RID: 2111
	public readonly PhotonPlayer sender;

	// Token: 0x04000840 RID: 2112
	public readonly PhotonView photonView;
}
