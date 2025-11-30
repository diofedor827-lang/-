using System;
using UnityEngine;

// Token: 0x02000117 RID: 279
[Serializable]
public class PhotonTransformViewPositionModel
{
	// Token: 0x04000928 RID: 2344
	public bool SynchronizeEnabled;

	// Token: 0x04000929 RID: 2345
	public bool TeleportEnabled = true;

	// Token: 0x0400092A RID: 2346
	public float TeleportIfDistanceGreaterThan = 3f;

	// Token: 0x0400092B RID: 2347
	public PhotonTransformViewPositionModel.InterpolateOptions InterpolateOption = PhotonTransformViewPositionModel.InterpolateOptions.EstimatedSpeed;

	// Token: 0x0400092C RID: 2348
	public float InterpolateMoveTowardsSpeed = 1f;

	// Token: 0x0400092D RID: 2349
	public float InterpolateLerpSpeed = 1f;

	// Token: 0x0400092E RID: 2350
	public float InterpolateMoveTowardsAcceleration = 2f;

	// Token: 0x0400092F RID: 2351
	public float InterpolateMoveTowardsDeceleration = 2f;

	// Token: 0x04000930 RID: 2352
	public AnimationCurve InterpolateSpeedCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(-1f, 0f, 0f, float.PositiveInfinity),
		new Keyframe(0f, 1f, 0f, 0f),
		new Keyframe(1f, 1f, 0f, 1f),
		new Keyframe(4f, 4f, 1f, 0f)
	});

	// Token: 0x04000931 RID: 2353
	public PhotonTransformViewPositionModel.ExtrapolateOptions ExtrapolateOption;

	// Token: 0x04000932 RID: 2354
	public float ExtrapolateSpeed = 1f;

	// Token: 0x04000933 RID: 2355
	public bool ExtrapolateIncludingRoundTripTime = true;

	// Token: 0x04000934 RID: 2356
	public int ExtrapolateNumberOfStoredPositions = 1;

	// Token: 0x04000935 RID: 2357
	public bool DrawErrorGizmo = true;

	// Token: 0x02000118 RID: 280
	public enum InterpolateOptions
	{
		// Token: 0x04000937 RID: 2359
		Disabled,
		// Token: 0x04000938 RID: 2360
		FixedSpeed,
		// Token: 0x04000939 RID: 2361
		EstimatedSpeed,
		// Token: 0x0400093A RID: 2362
		SynchronizeValues,
		// Token: 0x0400093B RID: 2363
		Lerp
	}

	// Token: 0x02000119 RID: 281
	public enum ExtrapolateOptions
	{
		// Token: 0x0400093D RID: 2365
		Disabled,
		// Token: 0x0400093E RID: 2366
		SynchronizeValues,
		// Token: 0x0400093F RID: 2367
		EstimateSpeedAndTurn,
		// Token: 0x04000940 RID: 2368
		FixedSpeed
	}
}
