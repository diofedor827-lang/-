using System;

// Token: 0x0200011B RID: 283
[Serializable]
public class PhotonTransformViewRotationModel
{
	// Token: 0x04000943 RID: 2371
	public bool SynchronizeEnabled;

	// Token: 0x04000944 RID: 2372
	public PhotonTransformViewRotationModel.InterpolateOptions InterpolateOption = PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards;

	// Token: 0x04000945 RID: 2373
	public float InterpolateRotateTowardsSpeed = 180f;

	// Token: 0x04000946 RID: 2374
	public float InterpolateLerpSpeed = 5f;

	// Token: 0x0200011C RID: 284
	public enum InterpolateOptions
	{
		// Token: 0x04000948 RID: 2376
		Disabled,
		// Token: 0x04000949 RID: 2377
		RotateTowards,
		// Token: 0x0400094A RID: 2378
		Lerp
	}
}
