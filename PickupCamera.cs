using System;
using Photon;
using UnityEngine;

// Token: 0x0200009C RID: 156
public class PickupCamera : Photon.MonoBehaviour
{
	// Token: 0x06000368 RID: 872 RVA: 0x000292A4 File Offset: 0x000276A4
	private void OnEnable()
	{
		if (base.photonView != null && !base.photonView.isMine)
		{
			base.enabled = false;
			return;
		}
		if (!this.cameraTransform && Camera.main)
		{
			this.cameraTransform = Camera.main.transform;
		}
		if (!this.cameraTransform)
		{
			Debug.Log("Please assign a camera to the ThirdPersonCamera script.");
			base.enabled = false;
		}
		this.m_CameraTransformCamera = this.cameraTransform.GetComponent<Camera>();
		this._target = base.transform;
		if (this._target)
		{
			this.controller = this._target.GetComponent<PickupController>();
		}
		if (this.controller)
		{
			CharacterController characterController = (CharacterController)this._target.GetComponent<Collider>();
			this.centerOffset = characterController.bounds.center - this._target.position;
			this.headOffset = this.centerOffset;
			this.headOffset.y = characterController.bounds.max.y - this._target.position.y;
		}
		else
		{
			Debug.Log("Please assign a target to the camera that has a ThirdPersonController script attached.");
		}
		this.Cut(this._target, this.centerOffset);
	}

	// Token: 0x06000369 RID: 873 RVA: 0x00029410 File Offset: 0x00027810
	private void DebugDrawStuff()
	{
		Debug.DrawLine(this._target.position, this._target.position + this.headOffset);
	}

	// Token: 0x0600036A RID: 874 RVA: 0x00029438 File Offset: 0x00027838
	private float AngleDistance(float a, float b)
	{
		a = Mathf.Repeat(a, 360f);
		b = Mathf.Repeat(b, 360f);
		return Mathf.Abs(b - a);
	}

	// Token: 0x0600036B RID: 875 RVA: 0x0002945C File Offset: 0x0002785C
	private void Apply(Transform dummyTarget, Vector3 dummyCenter)
	{
		if (!this.controller)
		{
			return;
		}
		Vector3 vector = this._target.position + this.centerOffset;
		Vector3 headPos = this._target.position + this.headOffset;
		float y = this._target.eulerAngles.y;
		float num = this.cameraTransform.eulerAngles.y;
		float num2 = y;
		if (Input.GetButton("Fire2"))
		{
			this.snap = true;
		}
		if (this.snap)
		{
			if (this.AngleDistance(num, y) < 3f)
			{
				this.snap = false;
			}
			num = Mathf.SmoothDampAngle(num, num2, ref this.angleVelocity, this.snapSmoothLag, this.snapMaxSpeed);
		}
		else
		{
			if (this.controller.GetLockCameraTimer() < this.lockCameraTimeout)
			{
				num2 = num;
			}
			if (this.AngleDistance(num, num2) > 160f && this.controller.IsMovingBackwards())
			{
				num2 += 180f;
			}
			num = Mathf.SmoothDampAngle(num, num2, ref this.angleVelocity, this.angularSmoothLag, this.angularMaxSpeed);
		}
		if (this.controller.IsJumping())
		{
			float num3 = vector.y + this.height;
			if (num3 < this.targetHeight || num3 - this.targetHeight > 5f)
			{
				this.targetHeight = vector.y + this.height;
			}
		}
		else
		{
			this.targetHeight = vector.y + this.height;
		}
		float num4 = this.cameraTransform.position.y;
		num4 = Mathf.SmoothDamp(num4, this.targetHeight, ref this.heightVelocity, this.heightSmoothLag);
		Quaternion rotation = Quaternion.Euler(0f, num, 0f);
		this.cameraTransform.position = vector;
		this.cameraTransform.position += rotation * Vector3.back * this.distance;
		this.cameraTransform.position = new Vector3(this.cameraTransform.position.x, num4, this.cameraTransform.position.z);
		this.SetUpRotation(vector, headPos);
	}

	// Token: 0x0600036C RID: 876 RVA: 0x000296C3 File Offset: 0x00027AC3
	private void LateUpdate()
	{
		this.Apply(base.transform, Vector3.zero);
	}

	// Token: 0x0600036D RID: 877 RVA: 0x000296D8 File Offset: 0x00027AD8
	private void Cut(Transform dummyTarget, Vector3 dummyCenter)
	{
		float num = this.heightSmoothLag;
		float num2 = this.snapMaxSpeed;
		float num3 = this.snapSmoothLag;
		this.snapMaxSpeed = 10000f;
		this.snapSmoothLag = 0.001f;
		this.heightSmoothLag = 0.001f;
		this.snap = true;
		this.Apply(base.transform, Vector3.zero);
		this.heightSmoothLag = num;
		this.snapMaxSpeed = num2;
		this.snapSmoothLag = num3;
	}

	// Token: 0x0600036E RID: 878 RVA: 0x00029748 File Offset: 0x00027B48
	private void SetUpRotation(Vector3 centerPos, Vector3 headPos)
	{
		Vector3 position = this.cameraTransform.position;
		Vector3 vector = centerPos - position;
		Quaternion lhs = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z));
		Vector3 forward = Vector3.forward * this.distance + Vector3.down * this.height;
		this.cameraTransform.rotation = lhs * Quaternion.LookRotation(forward);
		Ray ray = this.m_CameraTransformCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
		Ray ray2 = this.m_CameraTransformCamera.ViewportPointToRay(new Vector3(0.5f, this.clampHeadPositionScreenSpace, 1f));
		Vector3 point = ray.GetPoint(this.distance);
		Vector3 point2 = ray2.GetPoint(this.distance);
		float num = Vector3.Angle(ray.direction, ray2.direction);
		float num2 = num / (point.y - point2.y);
		float num3 = num2 * (point.y - centerPos.y);
		if (num3 >= num)
		{
			num3 -= num;
			this.cameraTransform.rotation *= Quaternion.Euler(-num3, 0f, 0f);
		}
	}

	// Token: 0x0600036F RID: 879 RVA: 0x000298A6 File Offset: 0x00027CA6
	private Vector3 GetCenterOffset()
	{
		return this.centerOffset;
	}

	// Token: 0x0400053D RID: 1341
	public Transform cameraTransform;

	// Token: 0x0400053E RID: 1342
	private Transform _target;

	// Token: 0x0400053F RID: 1343
	public float distance = 7f;

	// Token: 0x04000540 RID: 1344
	public float height = 3f;

	// Token: 0x04000541 RID: 1345
	public float angularSmoothLag = 0.3f;

	// Token: 0x04000542 RID: 1346
	public float angularMaxSpeed = 15f;

	// Token: 0x04000543 RID: 1347
	public float heightSmoothLag = 0.3f;

	// Token: 0x04000544 RID: 1348
	public float snapSmoothLag = 0.2f;

	// Token: 0x04000545 RID: 1349
	public float snapMaxSpeed = 720f;

	// Token: 0x04000546 RID: 1350
	public float clampHeadPositionScreenSpace = 0.75f;

	// Token: 0x04000547 RID: 1351
	public float lockCameraTimeout = 0.2f;

	// Token: 0x04000548 RID: 1352
	private Vector3 headOffset = Vector3.zero;

	// Token: 0x04000549 RID: 1353
	private Vector3 centerOffset = Vector3.zero;

	// Token: 0x0400054A RID: 1354
	private float heightVelocity;

	// Token: 0x0400054B RID: 1355
	private float angleVelocity;

	// Token: 0x0400054C RID: 1356
	private bool snap;

	// Token: 0x0400054D RID: 1357
	private PickupController controller;

	// Token: 0x0400054E RID: 1358
	private float targetHeight = 100000f;

	// Token: 0x0400054F RID: 1359
	private Camera m_CameraTransformCamera;
}
