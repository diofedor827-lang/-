using System;
using UnityEngine;

// Token: 0x020000A0 RID: 160
public class PickupTriggerForward : MonoBehaviour
{
	// Token: 0x06000387 RID: 903 RVA: 0x0002A774 File Offset: 0x00028B74
	public void OnTriggerEnter(Collider other)
	{
		PickupItem component = base.transform.parent.GetComponent<PickupItem>();
		if (component != null)
		{
			component.OnTriggerEnter(other);
		}
	}
}
