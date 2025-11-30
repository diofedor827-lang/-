using System;
using UnityEngine;

// Token: 0x020000C6 RID: 198
public class PlayerDiamond : MonoBehaviour
{
	// Token: 0x1700001F RID: 31
	// (get) Token: 0x06000468 RID: 1128 RVA: 0x0002F4EB File Offset: 0x0002D8EB
	private PhotonView PhotonView
	{
		get
		{
			if (this.m_PhotonView == null)
			{
				this.m_PhotonView = base.transform.parent.GetComponent<PhotonView>();
			}
			return this.m_PhotonView;
		}
	}

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x06000469 RID: 1129 RVA: 0x0002F51A File Offset: 0x0002D91A
	private Renderer DiamondRenderer
	{
		get
		{
			if (this.m_DiamondRenderer == null)
			{
				this.m_DiamondRenderer = base.GetComponentInChildren<Renderer>();
			}
			return this.m_DiamondRenderer;
		}
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x0002F540 File Offset: 0x0002D940
	private void Start()
	{
		this.m_Height = this.HeightOffset;
		if (this.HeadTransform != null)
		{
			this.m_Height += this.HeadTransform.position.y;
		}
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x0002F58A File Offset: 0x0002D98A
	private void Update()
	{
		this.UpdateDiamondPosition();
		this.UpdateDiamondRotation();
		this.UpdateDiamondVisibility();
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x0002F5A0 File Offset: 0x0002D9A0
	private void UpdateDiamondPosition()
	{
		Vector3 b = Vector3.zero;
		if (this.HeadTransform != null)
		{
			b = this.HeadTransform.position;
		}
		b.y = this.m_Height;
		if (!float.IsNaN(b.x) && !float.IsNaN(b.z))
		{
			base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * 10f);
		}
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x0002F628 File Offset: 0x0002DA28
	private void UpdateDiamondRotation()
	{
		this.m_Rotation += Time.deltaTime * 180f;
		this.m_Rotation %= 360f;
		base.transform.rotation = Quaternion.Euler(0f, this.m_Rotation, 0f);
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x0002F67F File Offset: 0x0002DA7F
	private void UpdateDiamondVisibility()
	{
		this.DiamondRenderer.enabled = true;
		if (this.PhotonView == null || !this.PhotonView.isMine)
		{
			this.DiamondRenderer.enabled = false;
		}
	}

	// Token: 0x0400066E RID: 1646
	public Transform HeadTransform;

	// Token: 0x0400066F RID: 1647
	public float HeightOffset = 0.5f;

	// Token: 0x04000670 RID: 1648
	private PhotonView m_PhotonView;

	// Token: 0x04000671 RID: 1649
	private Renderer m_DiamondRenderer;

	// Token: 0x04000672 RID: 1650
	private float m_Rotation;

	// Token: 0x04000673 RID: 1651
	private float m_Height;
}
