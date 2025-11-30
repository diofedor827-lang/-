using System;
using UnityEngine;

// Token: 0x0200009E RID: 158
[RequireComponent(typeof(CharacterController))]
public class PickupController : MonoBehaviour, IPunObservable
{
	// Token: 0x06000371 RID: 881 RVA: 0x000299D0 File Offset: 0x00027DD0
	private void Awake()
	{
		PhotonView component = base.gameObject.GetComponent<PhotonView>();
		if (component != null)
		{
			this.isControllable = component.isMine;
			if (this.AssignAsTagObject)
			{
				component.owner.TagObject = base.gameObject;
			}
			if (!this.DoRotate && component.ObservedComponents != null)
			{
				for (int i = 0; i < component.ObservedComponents.Count; i++)
				{
					if (component.ObservedComponents[i] is Transform)
					{
						component.onSerializeTransformOption = OnSerializeTransform.OnlyPosition;
						break;
					}
				}
			}
		}
		this.moveDirection = base.transform.TransformDirection(Vector3.forward);
		this._animation = base.GetComponent<Animation>();
		if (!this._animation)
		{
			Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		}
		if (!this.idleAnimation)
		{
			this._animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		if (!this.walkAnimation)
		{
			this._animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		if (!this.runAnimation)
		{
			this._animation = null;
			Debug.Log("No run animation found. Turning off animations.");
		}
		if (!this.jumpPoseAnimation && this.canJump)
		{
			this._animation = null;
			Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
		}
	}

	// Token: 0x06000372 RID: 882 RVA: 0x00029B3C File Offset: 0x00027F3C
	private void Update()
	{
		if (this.isControllable)
		{
			if (Input.GetButtonDown("Jump"))
			{
				this.lastJumpButtonTime = Time.time;
			}
			this.UpdateSmoothedMovementDirection();
			this.ApplyGravity();
			this.ApplyJumping();
			Vector3 vector = this.moveDirection * this.moveSpeed + new Vector3(0f, this.verticalSpeed, 0f) + this.inAirVelocity;
			vector *= Time.deltaTime;
			CharacterController component = base.GetComponent<CharacterController>();
			this.collisionFlags = component.Move(vector);
		}
		if (this.remotePosition != Vector3.zero)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, this.remotePosition, Time.deltaTime * this.RemoteSmoothing);
		}
		this.velocity = (base.transform.position - this.lastPos) * 25f;
		if (this._animation)
		{
			if (this._characterState == PickupCharacterState.Jumping)
			{
				if (!this.jumpingReachedApex)
				{
					this._animation[this.jumpPoseAnimation.name].speed = this.jumpAnimationSpeed;
					this._animation[this.jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					this._animation.CrossFade(this.jumpPoseAnimation.name);
				}
				else
				{
					this._animation[this.jumpPoseAnimation.name].speed = -this.landAnimationSpeed;
					this._animation[this.jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
					this._animation.CrossFade(this.jumpPoseAnimation.name);
				}
			}
			else
			{
				if (this._characterState == PickupCharacterState.Idle)
				{
					this._animation.CrossFade(this.idleAnimation.name);
				}
				else if (this._characterState == PickupCharacterState.Running)
				{
					this._animation[this.runAnimation.name].speed = this.runMaxAnimationSpeed;
					if (this.isControllable)
					{
						this._animation[this.runAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0f, this.runMaxAnimationSpeed);
					}
					this._animation.CrossFade(this.runAnimation.name);
				}
				else if (this._characterState == PickupCharacterState.Trotting)
				{
					this._animation[this.walkAnimation.name].speed = this.trotMaxAnimationSpeed;
					if (this.isControllable)
					{
						this._animation[this.walkAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0f, this.trotMaxAnimationSpeed);
					}
					this._animation.CrossFade(this.walkAnimation.name);
				}
				else if (this._characterState == PickupCharacterState.Walking)
				{
					this._animation[this.walkAnimation.name].speed = this.walkMaxAnimationSpeed;
					if (this.isControllable)
					{
						this._animation[this.walkAnimation.name].speed = Mathf.Clamp(this.velocity.magnitude, 0f, this.walkMaxAnimationSpeed);
					}
					this._animation.CrossFade(this.walkAnimation.name);
				}
				if (this._characterState != PickupCharacterState.Running)
				{
					this._animation[this.runAnimation.name].time = 0f;
				}
			}
		}
		if (this.IsGrounded())
		{
			if (this.DoRotate)
			{
				base.transform.rotation = Quaternion.LookRotation(this.moveDirection);
			}
		}
		if (this.IsGrounded())
		{
			this.lastGroundedTime = Time.time;
			this.inAirVelocity = Vector3.zero;
			if (this.jumping)
			{
				this.jumping = false;
				base.SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
			}
		}
		this.lastPos = base.transform.position;
	}

	// Token: 0x06000373 RID: 883 RVA: 0x00029F80 File Offset: 0x00028380
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(base.transform.position);
			stream.SendNext((byte)this._characterState);
		}
		else
		{
			bool flag = this.remotePosition == Vector3.zero;
			this.remotePosition = (Vector3)stream.ReceiveNext();
			this._characterState = (PickupCharacterState)((byte)stream.ReceiveNext());
			if (flag)
			{
				base.transform.position = this.remotePosition;
			}
		}
	}

	// Token: 0x06000374 RID: 884 RVA: 0x0002A010 File Offset: 0x00028410
	private void UpdateSmoothedMovementDirection()
	{
		Transform transform = Camera.main.transform;
		bool flag = this.IsGrounded();
		Vector3 a = transform.TransformDirection(Vector3.forward);
		a.y = 0f;
		a = a.normalized;
		Vector3 a2 = new Vector3(a.z, 0f, -a.x);
		float axisRaw = Input.GetAxisRaw("Vertical");
		float axisRaw2 = Input.GetAxisRaw("Horizontal");
		if (axisRaw < -0.2f)
		{
			this.movingBack = true;
		}
		else
		{
			this.movingBack = false;
		}
		bool flag2 = this.isMoving;
		this.isMoving = (Mathf.Abs(axisRaw2) > 0.1f || Mathf.Abs(axisRaw) > 0.1f);
		Vector3 vector = axisRaw2 * a2 + axisRaw * a;
		if (flag)
		{
			this.lockCameraTimer += Time.deltaTime;
			if (this.isMoving != flag2)
			{
				this.lockCameraTimer = 0f;
			}
			if (vector != Vector3.zero)
			{
				if (this.moveSpeed < this.walkSpeed * 0.9f && flag)
				{
					this.moveDirection = vector.normalized;
				}
				else
				{
					this.moveDirection = Vector3.RotateTowards(this.moveDirection, vector, this.rotateSpeed * 0.017453292f * Time.deltaTime, 1000f);
					this.moveDirection = this.moveDirection.normalized;
				}
			}
			float t = this.speedSmoothing * Time.deltaTime;
			float num = Mathf.Min(vector.magnitude, 1f);
			this._characterState = PickupCharacterState.Idle;
			if ((Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift)) && this.isMoving)
			{
				num *= this.runSpeed;
				this._characterState = PickupCharacterState.Running;
			}
			else if (Time.time - this.trotAfterSeconds > this.walkTimeStart)
			{
				num *= this.trotSpeed;
				this._characterState = PickupCharacterState.Trotting;
			}
			else if (this.isMoving)
			{
				num *= this.walkSpeed;
				this._characterState = PickupCharacterState.Walking;
			}
			this.moveSpeed = Mathf.Lerp(this.moveSpeed, num, t);
			if (this.moveSpeed < this.walkSpeed * 0.3f)
			{
				this.walkTimeStart = Time.time;
			}
		}
		else
		{
			if (this.jumping)
			{
				this.lockCameraTimer = 0f;
			}
			if (this.isMoving)
			{
				this.inAirVelocity += vector.normalized * Time.deltaTime * this.inAirControlAcceleration;
			}
		}
	}

	// Token: 0x06000375 RID: 885 RVA: 0x0002A2CC File Offset: 0x000286CC
	private void ApplyJumping()
	{
		if (this.lastJumpTime + this.jumpRepeatTime > Time.time)
		{
			return;
		}
		if (this.IsGrounded() && this.canJump && Time.time < this.lastJumpButtonTime + this.jumpTimeout)
		{
			this.verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);
			base.SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
		}
	}

	// Token: 0x06000376 RID: 886 RVA: 0x0002A33C File Offset: 0x0002873C
	private void ApplyGravity()
	{
		if (this.isControllable)
		{
			if (this.jumping && !this.jumpingReachedApex && this.verticalSpeed <= 0f)
			{
				this.jumpingReachedApex = true;
				base.SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
			}
			if (this.IsGrounded())
			{
				this.verticalSpeed = 0f;
			}
			else
			{
				this.verticalSpeed -= this.gravity * Time.deltaTime;
			}
		}
	}

	// Token: 0x06000377 RID: 887 RVA: 0x0002A3C1 File Offset: 0x000287C1
	private float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		return Mathf.Sqrt(2f * targetJumpHeight * this.gravity);
	}

	// Token: 0x06000378 RID: 888 RVA: 0x0002A3D6 File Offset: 0x000287D6
	private void DidJump()
	{
		this.jumping = true;
		this.jumpingReachedApex = false;
		this.lastJumpTime = Time.time;
		this.lastJumpButtonTime = -10f;
		this._characterState = PickupCharacterState.Jumping;
	}

	// Token: 0x06000379 RID: 889 RVA: 0x0002A404 File Offset: 0x00028804
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.moveDirection.y > 0.01f)
		{
			return;
		}
	}

	// Token: 0x0600037A RID: 890 RVA: 0x0002A42A File Offset: 0x0002882A
	public float GetSpeed()
	{
		return this.moveSpeed;
	}

	// Token: 0x0600037B RID: 891 RVA: 0x0002A432 File Offset: 0x00028832
	public bool IsJumping()
	{
		return this.jumping;
	}

	// Token: 0x0600037C RID: 892 RVA: 0x0002A43A File Offset: 0x0002883A
	public bool IsGrounded()
	{
		return (this.collisionFlags & CollisionFlags.Below) != CollisionFlags.None;
	}

	// Token: 0x0600037D RID: 893 RVA: 0x0002A44A File Offset: 0x0002884A
	public Vector3 GetDirection()
	{
		return this.moveDirection;
	}

	// Token: 0x0600037E RID: 894 RVA: 0x0002A452 File Offset: 0x00028852
	public bool IsMovingBackwards()
	{
		return this.movingBack;
	}

	// Token: 0x0600037F RID: 895 RVA: 0x0002A45A File Offset: 0x0002885A
	public float GetLockCameraTimer()
	{
		return this.lockCameraTimer;
	}

	// Token: 0x06000380 RID: 896 RVA: 0x0002A462 File Offset: 0x00028862
	public bool IsMoving()
	{
		return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
	}

	// Token: 0x06000381 RID: 897 RVA: 0x0002A48A File Offset: 0x0002888A
	public bool HasJumpReachedApex()
	{
		return this.jumpingReachedApex;
	}

	// Token: 0x06000382 RID: 898 RVA: 0x0002A492 File Offset: 0x00028892
	public bool IsGroundedWithTimeout()
	{
		return this.lastGroundedTime + this.groundedTimeout > Time.time;
	}

	// Token: 0x06000383 RID: 899 RVA: 0x0002A4A8 File Offset: 0x000288A8
	public void Reset()
	{
		base.gameObject.tag = "Player";
	}

	// Token: 0x04000556 RID: 1366
	public AnimationClip idleAnimation;

	// Token: 0x04000557 RID: 1367
	public AnimationClip walkAnimation;

	// Token: 0x04000558 RID: 1368
	public AnimationClip runAnimation;

	// Token: 0x04000559 RID: 1369
	public AnimationClip jumpPoseAnimation;

	// Token: 0x0400055A RID: 1370
	public float walkMaxAnimationSpeed = 0.75f;

	// Token: 0x0400055B RID: 1371
	public float trotMaxAnimationSpeed = 1f;

	// Token: 0x0400055C RID: 1372
	public float runMaxAnimationSpeed = 1f;

	// Token: 0x0400055D RID: 1373
	public float jumpAnimationSpeed = 1.15f;

	// Token: 0x0400055E RID: 1374
	public float landAnimationSpeed = 1f;

	// Token: 0x0400055F RID: 1375
	private Animation _animation;

	// Token: 0x04000560 RID: 1376
	public PickupCharacterState _characterState;

	// Token: 0x04000561 RID: 1377
	public float walkSpeed = 2f;

	// Token: 0x04000562 RID: 1378
	public float trotSpeed = 4f;

	// Token: 0x04000563 RID: 1379
	public float runSpeed = 6f;

	// Token: 0x04000564 RID: 1380
	public float inAirControlAcceleration = 3f;

	// Token: 0x04000565 RID: 1381
	public float jumpHeight = 0.5f;

	// Token: 0x04000566 RID: 1382
	public float gravity = 20f;

	// Token: 0x04000567 RID: 1383
	public float speedSmoothing = 10f;

	// Token: 0x04000568 RID: 1384
	public float rotateSpeed = 500f;

	// Token: 0x04000569 RID: 1385
	public float trotAfterSeconds = 3f;

	// Token: 0x0400056A RID: 1386
	public bool canJump;

	// Token: 0x0400056B RID: 1387
	private float jumpRepeatTime = 0.05f;

	// Token: 0x0400056C RID: 1388
	private float jumpTimeout = 0.15f;

	// Token: 0x0400056D RID: 1389
	private float groundedTimeout = 0.25f;

	// Token: 0x0400056E RID: 1390
	private float lockCameraTimer;

	// Token: 0x0400056F RID: 1391
	private Vector3 moveDirection = Vector3.zero;

	// Token: 0x04000570 RID: 1392
	private float verticalSpeed;

	// Token: 0x04000571 RID: 1393
	private float moveSpeed;

	// Token: 0x04000572 RID: 1394
	private CollisionFlags collisionFlags;

	// Token: 0x04000573 RID: 1395
	private bool jumping;

	// Token: 0x04000574 RID: 1396
	private bool jumpingReachedApex;

	// Token: 0x04000575 RID: 1397
	private bool movingBack;

	// Token: 0x04000576 RID: 1398
	private bool isMoving;

	// Token: 0x04000577 RID: 1399
	private float walkTimeStart;

	// Token: 0x04000578 RID: 1400
	private float lastJumpButtonTime = -10f;

	// Token: 0x04000579 RID: 1401
	private float lastJumpTime = -1f;

	// Token: 0x0400057A RID: 1402
	private Vector3 inAirVelocity = Vector3.zero;

	// Token: 0x0400057B RID: 1403
	private float lastGroundedTime;

	// Token: 0x0400057C RID: 1404
	private Vector3 velocity = Vector3.zero;

	// Token: 0x0400057D RID: 1405
	private Vector3 lastPos;

	// Token: 0x0400057E RID: 1406
	private Vector3 remotePosition;

	// Token: 0x0400057F RID: 1407
	public bool isControllable;

	// Token: 0x04000580 RID: 1408
	public bool DoRotate = true;

	// Token: 0x04000581 RID: 1409
	public float RemoteSmoothing = 5f;

	// Token: 0x04000582 RID: 1410
	public bool AssignAsTagObject = true;
}
