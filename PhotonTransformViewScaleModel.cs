using System;

// Token: 0x0200011E RID: 286
[Serializable]
public class PhotonTransformViewScaleModel
{
	// Token: 0x0400094D RID: 2381
	public bool SynchronizeEnabled;

	// Token: 0x0400094E RID: 2382
	public PhotonTransformViewScaleModel.InterpolateOptions InterpolateOption;

	// Token: 0x0400094F RID: 2383
	public float InterpolateMoveTowardsSpeed = 1f;

	// Token: 0x04000950 RID: 2384
	public float InterpolateLerpSpeed;

	// Token: 0x0200011F RID: 287
	public enum InterpolateOptions
	{
		// Token: 0x04000952 RID: 2386
		Disabled,
		// Token: 0x04000953 RID: 2387
		MoveTowards,
		// Token: 0x04000954 RID: 2388
		Lerp
	}
}
