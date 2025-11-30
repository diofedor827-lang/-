using System;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class PlayerNetwork : Photon.MonoBehaviour
{
	// Token: 0x0600016D RID: 365 RVA: 0x0001E9D4 File Offset: 0x0001CDD4
	private void Start()
	{
		base.photonView.synchronization = ViewSynchronization.Unreliable;
		this.localMouseLook = base.GetComponent<FPSMouseLook>();
		this.fpsController = base.GetComponent<FPSController>();
		this.fpsController.pn = this;
		this.playerWeapons.playerNetwork = this;
		this.playerWeapons.soldierAnimation = this.soldierAnimation;
		this.playerWeapons.fpsController = this.fpsController;
		this.playerWeapons.QuickSetup(base.photonView.isMine);
		base.gameObject.name = base.photonView.owner.name;
		this.thisT = base.transform;
		this.playerKilled = false;
		base.gameObject.layer = 2;
		if (!base.photonView.isMine)
		{
			this.localMouseLook.enabled = false;
			this.fpsController.enabled = false;
			this.playerWeapons.enabled = false;
			this.firstPersonView.SetActive(false);
			if (!this.soldierAnimation.gameObject.activeSelf)
			{
				this.soldierAnimation.gameObject.SetActive(true);
			}
			this.soldierAnimation.playerWeapons = this.playerWeapons;
			this.soldierAnimation.playerNetwork = this;
			this.soldierAnimation.Setup();
			this.SetupBoxes(this.headHitBoxes, HitBox.BodyPart.Head);
			this.SetupBoxes(this.torsoHitBoxes, HitBox.BodyPart.Torso);
			this.SetupBoxes(this.limbsHitBoxes, HitBox.BodyPart.Limbs);
			GameSettings.rc.otherPlayers.Add(this);
		}
		else
		{
			this.firstPersonView.SetActive(true);
			this.soldierAnimation.gameObject.SetActive(false);
			this.cameraMouseLook = this.playerWeapons.playerCamera.GetComponent<FPSMouseLook>();
			this.nameLabelTransform.gameObject.SetActive(false);
			base.gameObject.tag = "Player";
			GameObject gameObject = GameObject.Find("FirstPersonWeapons");
			for (int i = 0; i < gameObject.transform.childCount - 1; i++)
			{
				gameObject.transform.GetChild(i).transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = this.soldierBody.sharedMaterial.GetTexture("_MainTex");
			}
		}
		if (PhotonNetwork.isMasterClient)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("PlayerHP", 100);
			base.photonView.owner.SetCustomProperties(hashtable, null, false);
		}
		this.playerTeam = (int)base.photonView.owner.customProperties["Team"];
		this.playerID = base.photonView.owner.ID;
		if (this.playerTeam == 1 || this.playerTeam == 2)
		{
			if (base.photonView.isMine)
			{
				this.fpsHandMaterial.SetTexture("_MainTex", this.soldierBody.sharedMaterial.GetTexture("_MainTex"));
				this.fpsHandMaterial.color = this.soldierBody.sharedMaterial.color;
			}
			else
			{
				this.nameLabel.text = base.photonView.name;
				this.nameLabelShadow.text = base.photonView.name;
				this.nameLabel.color = ((this.playerTeam != 1) ? GameSettings.teamBColor : GameSettings.teamAColor);
			}
		}
	}

	// Token: 0x0600016E RID: 366 RVA: 0x0001ED40 File Offset: 0x0001D140
	private void SetupBoxes(Collider[] tmpBoxes, HitBox.BodyPart bp)
	{
		for (int i = 0; i < tmpBoxes.Length; i++)
		{
			tmpBoxes[i].isTrigger = true;
			tmpBoxes[i].gameObject.AddComponent<HitBox>().AssignVariables(this, bp);
			tmpBoxes[i].tag = "Body";
		}
	}

	// Token: 0x0600016F RID: 367 RVA: 0x0001ED8C File Offset: 0x0001D18C
	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (!this.thisT)
			{
				return;
			}
			stream.SendNext(this.thisT.position);
			stream.SendNext(this.playerWeapons.playerCamera.position + this.playerWeapons.playerCamera.forward * 100f);
			stream.SendNext(this.playerWeapons.globalWeaponIndex);
			stream.SendNext(this.playerWeapons.isFiring);
			stream.SendNext(this.fpsController.movementState);
		}
		else
		{
			this.playerPos = (Vector3)stream.ReceiveNext();
			this.aimPos = (Vector3)stream.ReceiveNext();
			this.currentWeaponIndex = (int)stream.ReceiveNext();
			this.isFiringRemote = (bool)stream.ReceiveNext();
			this.soldierAnimation.movementState = (int)stream.ReceiveNext();
			for (int i = this.m_BufferedState.Length - 1; i >= 1; i--)
			{
				this.m_BufferedState[i] = this.m_BufferedState[i - 1];
			}
			PlayerNetwork.State state = default(PlayerNetwork.State);
			state.timestamp = info.timestamp;
			state.pos = this.playerPos;
			this.m_BufferedState[0] = state;
			this.m_TimestampCount = Mathf.Min(this.m_TimestampCount + 1, this.m_BufferedState.Length);
			for (int j = 0; j < this.m_TimestampCount - 1; j++)
			{
				if (this.m_BufferedState[j].timestamp < this.m_BufferedState[j + 1].timestamp)
				{
					Debug.Log("State inconsistent");
				}
			}
		}
	}

	// Token: 0x06000170 RID: 368 RVA: 0x0001EF84 File Offset: 0x0001D384
	private void Update()
	{
		if (!base.photonView.isMine)
		{
			this.InterpolatePosition();
			this.smoothAimPos = Vector3.Lerp(this.smoothAimPos, this.aimPos, Time.deltaTime * this.positionSmoother);
			if (this.aimPos != Vector3.zero)
			{
				this.thisT.LookAt(new Vector3(this.smoothAimPos.x, this.thisT.position.y, this.smoothAimPos.z));
				this.playerWeapons.playerCamera.LookAt(this.smoothAimPos);
			}
			if (this.isFiringRemote)
			{
				this.playerWeapons.FireRemote();
			}
			if (this.previousWeaponIndex != this.currentWeaponIndex)
			{
				this.previousWeaponIndex = this.currentWeaponIndex;
				this.playerWeapons.globalWeaponIndex = this.currentWeaponIndex;
				this.playerWeapons.SwitchWeaponRemote();
			}
			if (!this.playerKilled && ((this.playerTeam == GameSettings.ourTeam && GameSettings.currentGameMode != "FFA") || GameSettings.ourTeam == 0))
			{
				if (!this.nameLabelTransform.gameObject.activeSelf)
				{
					this.nameLabelTransform.gameObject.SetActive(true);
				}
				if (!this.mainCamera || !this.mainCamera.gameObject.activeInHierarchy)
				{
					if (Camera.main)
					{
						this.mainCamera = Camera.main;
						this.mainCameraT = this.mainCamera.transform;
					}
				}
				else
				{
					this.offset = Vector3.Distance(this.mainCameraT.position, this.thisT.position) / 50f;
					this.screenPos = this.mainCamera.WorldToViewportPoint(new Vector3(this.thisT.position.x, this.thisT.position.y + 2.6f + this.offset, this.thisT.position.z));
					if (this.screenPos.z > 0f)
					{
						this.nameLabelTransform.position = new Vector3(this.screenPos.x, this.screenPos.y);
					}
					else
					{
						this.nameLabelTransform.position = new Vector3(-350f, -350f);
					}
				}
				this.nameLabelTransform.eulerAngles = Vector3.zero;
			}
			else if (this.nameLabelTransform.gameObject.activeSelf)
			{
				this.nameLabelTransform.gameObject.SetActive(false);
			}
			this.receivedMovementState = this.soldierAnimation.movementState;
		}
		else
		{
			this.receivedMovementState = this.fpsController.movementState;
		}
		if (!this.playerKilled)
		{
			if (this.movementStateLocal != this.receivedMovementState)
			{
				this.movementStateLocal = this.receivedMovementState;
				this.PlayWalkingSound();
			}
		}
		else if (this.walkingAudio.isPlaying)
		{
			this.walkingAudio.Stop();
		}
	}

	// Token: 0x06000171 RID: 369 RVA: 0x0001F2C0 File Offset: 0x0001D6C0
	private void InterpolatePosition()
	{
		this.d = Vector3.Distance(this.thisT.position, this.m_BufferedState[0].pos);
		this.soldierAnimation.isMoving = (this.d > 0.1f);
		double time = PhotonNetwork.time;
		double num = time - this.interpolationBackTime;
		if (this.m_BufferedState[0].timestamp > num)
		{
			for (int i = 0; i < this.m_TimestampCount; i++)
			{
				if (this.m_BufferedState[i].timestamp <= num || i == this.m_TimestampCount - 1)
				{
					PlayerNetwork.State state = this.m_BufferedState[Mathf.Max(i - 1, 0)];
					PlayerNetwork.State state2 = this.m_BufferedState[i];
					double num2 = state.timestamp - state2.timestamp;
					float t = 0f;
					if (num2 > 0.0001)
					{
						t = (float)((num - state2.timestamp) / num2);
					}
					this.thisT.position = Vector3.Lerp(state2.pos, state.pos, t);
					return;
				}
			}
		}
		else
		{
			PlayerNetwork.State state3 = this.m_BufferedState[0];
			this.thisT.position = state3.pos;
		}
	}

	// Token: 0x06000172 RID: 370 RVA: 0x0001F41D File Offset: 0x0001D81D
	public void FireSingleRemote()
	{
		base.photonView.RPC("FireRemoteRPC", PhotonTargets.Others, new object[0]);
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0001F436 File Offset: 0x0001D836
	[PunRPC]
	private void FireRemoteRPC()
	{
		this.playerWeapons.FireRemote();
	}

	// Token: 0x06000174 RID: 372 RVA: 0x0001F443 File Offset: 0x0001D843
	public void DoReload()
	{
		base.photonView.RPC("DoReloadRemote", PhotonTargets.Others, new object[0]);
	}

	// Token: 0x06000175 RID: 373 RVA: 0x0001F45C File Offset: 0x0001D85C
	[PunRPC]
	private void DoReloadRemote()
	{
		if (this.playerWeapons.currentSelectedWeapon)
		{
			this.playerWeapons.currentSelectedWeapon.ReloadNetworkSync();
		}
	}

	// Token: 0x06000176 RID: 374 RVA: 0x0001F484 File Offset: 0x0001D884
	public void ApplyDamage(int[] values)
	{
		if (!this.playerKilled && (this.playerTeam != GameSettings.ourTeam || base.photonView.isMine || GameSettings.currentGameMode == "FFA"))
		{
			base.photonView.RPC("DamageRemote", PhotonTargets.All, new object[]
			{
				values,
				PhotonNetwork.player.ID
			});
		}
	}

	// Token: 0x06000177 RID: 375 RVA: 0x0001F4FD File Offset: 0x0001D8FD
	public void DieByWater()
	{
		base.photonView.RPC("KillPlayer", PhotonTargets.All, new object[]
		{
			base.photonView.ownerId
		});
	}

	// Token: 0x06000178 RID: 376 RVA: 0x0001F52C File Offset: 0x0001D92C
	[PunRPC]
	private void DamageRemote(int[] values, int killerID)
	{
		if (base.photonView.isMine)
		{
			GameSettings.rc.DoHitDetector(values[2]);
			this.fpsController.fallSlowDown = 0.5f;
			this.PlayHitSound();
		}
		else
		{
			this.soldierAnimation.DoHitMovement();
		}
		if (!this.playerKilled)
		{
			if (PhotonNetwork.player.ID == killerID)
			{
				this.lastWeaponName = ((!GameSettings.rc.ourPlayer) ? string.Empty : GameSettings.rc.ourPlayer.playerWeapons.currentSelectedWeapon.weaponName);
				this.lastBodyPart = values[1];
			}
			if (PhotonNetwork.isMasterClient)
			{
				int num = (base.photonView.owner.customProperties["PlayerHP"] == null) ? 100 : ((int)base.photonView.owner.customProperties["PlayerHP"]);
				num -= this.GetDMG(values[0], values[1]);
				Hashtable hashtable = new Hashtable();
				hashtable.Add("PlayerHP", num);
				base.photonView.owner.SetCustomProperties(hashtable, null, false);
				if (num < 1)
				{
					base.photonView.RPC("KillPlayer", PhotonTargets.All, new object[]
					{
						killerID
					});
					this.playerKilled = true;
				}
			}
		}
	}

	// Token: 0x06000179 RID: 377 RVA: 0x0001F694 File Offset: 0x0001DA94
	[PunRPC]
	private void KillPlayer(int killerID)
	{
		this.playerKilled = true;
		if (base.photonView.isMine)
		{
			this.soldierAnimation.gameObject.SetActive(true);
			this.firstPersonView.SetActive(false);
			this.localMouseLook.enabled = false;
			this.playerWeapons.enabled = false;
			this.playerWeapons.isFiring = false;
			GameSettings.rc.PrepareRespawn(-killerID, false);
		}
		if (PhotonNetwork.isMasterClient && base.photonView.owner != null)
		{
			int num = (base.photonView.owner.customProperties["Deaths"] != null) ? ((int)base.photonView.owner.customProperties["Deaths"] + 1) : 1;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("Deaths", num);
			base.photonView.owner.SetCustomProperties(hashtable, null, false);
			if (base.photonView.owner.ID != killerID)
			{
				PhotonPlayer photonPlayer = null;
				PhotonPlayer[] playerList = PhotonNetwork.playerList;
				for (int i = 0; i < playerList.Length; i++)
				{
					if (playerList[i].ID == killerID)
					{
						photonPlayer = playerList[i];
					}
				}
				if (photonPlayer != null)
				{
					int num2 = (photonPlayer.customProperties["Kills"] != null) ? ((int)photonPlayer.customProperties["Kills"] + 1) : 1;
					photonPlayer.SetCustomProperties(new Hashtable
					{
						{
							"Kills",
							num2
						}
					}, null, false);
					if (GameSettings.currentGameMode == "TDM")
					{
						Hashtable hashtable2 = new Hashtable();
						if ((int)photonPlayer.customProperties["Team"] == 1)
						{
							int num3 = (PhotonNetwork.room.customProperties["TeamAScore"] == null) ? 1 : ((int)PhotonNetwork.room.customProperties["TeamAScore"] + 1);
							hashtable2.Add("TeamAScore", num3);
						}
						if ((int)photonPlayer.customProperties["Team"] == 2)
						{
							int num3 = (PhotonNetwork.room.customProperties["TeamBScore"] == null) ? 1 : ((int)PhotonNetwork.room.customProperties["TeamBScore"] + 1);
							hashtable2.Add("TeamBScore", num3);
						}
						if (hashtable2.Count > 0)
						{
							PhotonNetwork.room.SetCustomProperties(hashtable2, null, false);
						}
					}
				}
			}
		}
		if (PhotonNetwork.player.ID == killerID)
		{
			string text = "[" + GameSettings.GetWeaponName(this.lastWeaponName) + "]";
			string killedName = base.photonView.owner.name;
			int killedTeam = (int)base.photonView.owner.customProperties["Team"];
			if (this.lastBodyPart == -35)
			{
				text = "fell";
				killedName = "down";
				killedTeam = 0;
			}
			else if (this.lastBodyPart == 0)
			{
				text += " -> Headshot";
			}
			GameSettings.rc.ReportKill(killedName, text, killedTeam);
			GameSettings.rc.AddKillCash(this.lastBodyPart);
		}
		this.soldierAnimation.PlayKillAnimation();
	}

	// Token: 0x0600017A RID: 378 RVA: 0x0001FA0C File Offset: 0x0001DE0C
	private int GetDMG(int weaponIndex, int bodyPart)
	{
		if (weaponIndex <= -1 || weaponIndex >= this.playerWeapons.totalWeapons.Count)
		{
			return Mathf.Abs(weaponIndex);
		}
		int num = (this.playerWeapons.totalWeapons[weaponIndex].fireType != PlayerWeapons.FireType.Shotgun) ? 1 : 5;
		if (bodyPart != 0 && bodyPart != 1)
		{
			return this.playerWeapons.totalWeapons[weaponIndex].limbsDamage / num;
		}
		if (bodyPart == 0)
		{
			return this.playerWeapons.totalWeapons[weaponIndex].headDamage / num;
		}
		return this.playerWeapons.totalWeapons[weaponIndex].torsoDamage / num;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x0001FAC0 File Offset: 0x0001DEC0
	public void ThrowGrenade(float weaponCookTime, float maxCookTime)
	{
		GameObject go = PhotonNetwork.Instantiate("Grenade", base.transform.position + base.transform.forward + base.transform.up * 2.5f, Quaternion.identity, 0);
		go.GetPhotonView().RPC("Throw", PhotonTargets.All, new object[]
		{
			base.photonView.viewID,
			Mathf.Lerp(550f, 1400f, weaponCookTime / maxCookTime),
			base.photonView.isMine
		});
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0001FB6C File Offset: 0x0001DF6C
	public void FireRPGRocket()
	{
		GameObject gameObject = PhotonNetwork.Instantiate("RPGRocket", base.transform.position + base.transform.forward * 1.5f + base.transform.up * 2f, Quaternion.Euler(base.transform.forward), 0);
		gameObject.GetComponent<RocketController>().Fire(base.photonView.viewID);
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0001FBEC File Offset: 0x0001DFEC
	private void PlayHitSound()
	{
		this.playerAudio.Stop();
		if (this.playerAudio.clip != this.hitSound)
		{
			this.playerAudio.clip = this.hitSound;
		}
		this.playerAudio.Play();
	}

	// Token: 0x0600017E RID: 382 RVA: 0x0001FC3C File Offset: 0x0001E03C
	private void PlayWalkingSound()
	{
		base.CancelInvoke("PlayWalkingSoundInvoke");
		this.stepLength = 0f;
		if (this.movementStateLocal == 0 || this.movementStateLocal == 1 || this.movementStateLocal == 2 || this.movementStateLocal == 4)
		{
			if (this.movementStateLocal == 4)
			{
				this.stepLength = 2.5f / this.fpsController.ladderSpeed;
			}
			else if (this.movementStateLocal == 2)
			{
				this.stepLength = 3.5f / this.fpsController.crouchSpeed;
			}
			else if (this.movementStateLocal == 0)
			{
				this.stepLength = 3.5f / this.fpsController.walkSpeed;
			}
			else
			{
				this.stepLength = 3.5f / this.fpsController.runSpeed;
			}
		}
		if (this.stepLength > 0f)
		{
			base.InvokeRepeating("PlayWalkingSoundInvoke", 0.09f, this.stepLength);
		}
	}

	// Token: 0x0600017F RID: 383 RVA: 0x0001FD44 File Offset: 0x0001E144
	private void PlayWalkingSoundInvoke()
	{
		this.walkingAudio.Stop();
		if (this.fpsController.isMoving || this.soldierAnimation.isMoving)
		{
			if (this.movementStateLocal == 4)
			{
				this.walkingAudio.clip = this.ladderSounds[UnityEngine.Random.Range(0, this.ladderSounds.Length - 1)];
			}
			else
			{
				this.walkingAudio.clip = this.walkingSounds[UnityEngine.Random.Range(0, this.walkingSounds.Length - 1)];
			}
			this.walkingAudio.Play();
		}
	}

	// Token: 0x06000180 RID: 384 RVA: 0x0001FDDC File Offset: 0x0001E1DC
	[PunRPC]
	public void PlayKnifeSound()
	{
		this.playerAudio.Stop();
		if (this.playerAudio.clip != this.knifeAudioClip)
		{
			this.playerAudio.clip = this.knifeAudioClip;
		}
		this.playerAudio.Play();
	}

	// Token: 0x06000181 RID: 385 RVA: 0x0001FE2C File Offset: 0x0001E22C
	[PunRPC]
	private void SetPlayerLook(int look)
	{
		this.teamALook1Mesh.SetActive(false);
		this.teamALook2Mesh.SetActive(false);
		this.teamALook3Mesh.SetActive(false);
		this.teamALook4Mesh.SetActive(false);
		this.teamBLook1Mesh.SetActive(false);
		this.teamBLook2Mesh.SetActive(false);
		this.teamBLook3Mesh.SetActive(false);
		this.teamBLook4Mesh.SetActive(false);
		this.playerTeam = (int)base.photonView.owner.customProperties["Team"];
		if (this.playerTeam == 1 || this.playerTeam == 2)
		{
			if (this.playerTeam == 1)
			{
				if (look == 0)
				{
					this.soldierBody.sharedMaterial = this.teamALook1;
					this.teamALook1Mesh.SetActive(true);
				}
				else if (look == 1)
				{
					this.soldierBody.sharedMaterial = this.teamALook2;
					this.teamALook2Mesh.SetActive(true);
				}
				else if (look == 2)
				{
					this.soldierBody.sharedMaterial = this.teamALook3;
					this.teamALook3Mesh.SetActive(true);
				}
				else if (look == 3)
				{
					this.soldierBody.sharedMaterial = this.teamALook4;
					this.teamALook4Mesh.SetActive(true);
				}
			}
			else if (look == 0)
			{
				this.soldierBody.sharedMaterial = this.teamBLook1;
				this.teamBLook1Mesh.SetActive(true);
			}
			else if (look == 1)
			{
				this.soldierBody.sharedMaterial = this.teamBLook2;
				this.teamBLook2Mesh.SetActive(true);
			}
			else if (look == 2)
			{
				this.soldierBody.sharedMaterial = this.teamBLook3;
				this.teamBLook3Mesh.SetActive(true);
			}
			else if (look == 3)
			{
				this.soldierBody.sharedMaterial = this.teamBLook4;
				this.teamBLook4Mesh.SetActive(true);
			}
		}
	}

	// Token: 0x0400037D RID: 893
	public SoldierAnimation soldierAnimation;

	// Token: 0x0400037E RID: 894
	public Renderer soldierBody;

	// Token: 0x0400037F RID: 895
	public Material teamALook1;

	// Token: 0x04000380 RID: 896
	public Material teamALook2;

	// Token: 0x04000381 RID: 897
	public Material teamALook3;

	// Token: 0x04000382 RID: 898
	public Material teamALook4;

	// Token: 0x04000383 RID: 899
	public GameObject teamALook1Mesh;

	// Token: 0x04000384 RID: 900
	public GameObject teamALook2Mesh;

	// Token: 0x04000385 RID: 901
	public GameObject teamALook3Mesh;

	// Token: 0x04000386 RID: 902
	public GameObject teamALook4Mesh;

	// Token: 0x04000387 RID: 903
	public Material teamBLook1;

	// Token: 0x04000388 RID: 904
	public Material teamBLook2;

	// Token: 0x04000389 RID: 905
	public Material teamBLook3;

	// Token: 0x0400038A RID: 906
	public Material teamBLook4;

	// Token: 0x0400038B RID: 907
	public GameObject teamBLook1Mesh;

	// Token: 0x0400038C RID: 908
	public GameObject teamBLook2Mesh;

	// Token: 0x0400038D RID: 909
	public GameObject teamBLook3Mesh;

	// Token: 0x0400038E RID: 910
	public GameObject teamBLook4Mesh;

	// Token: 0x0400038F RID: 911
	public Material fpsHandMaterial;

	// Token: 0x04000390 RID: 912
	public PlayerWeapons playerWeapons;

	// Token: 0x04000391 RID: 913
	public GameObject firstPersonView;

	// Token: 0x04000392 RID: 914
	public Transform nameLabelTransform;

	// Token: 0x04000393 RID: 915
	public GUIText nameLabel;

	// Token: 0x04000394 RID: 916
	public GUIText nameLabelShadow;

	// Token: 0x04000395 RID: 917
	public AudioSource playerAudio;

	// Token: 0x04000396 RID: 918
	public AudioSource walkingAudio;

	// Token: 0x04000397 RID: 919
	public AudioClip hitSound;

	// Token: 0x04000398 RID: 920
	public AudioClip[] walkingSounds;

	// Token: 0x04000399 RID: 921
	public AudioClip[] ladderSounds;

	// Token: 0x0400039A RID: 922
	public AudioClip knifeAudioClip;

	// Token: 0x0400039B RID: 923
	public Collider[] headHitBoxes;

	// Token: 0x0400039C RID: 924
	public Collider[] torsoHitBoxes;

	// Token: 0x0400039D RID: 925
	public Collider[] limbsHitBoxes;

	// Token: 0x0400039E RID: 926
	private Vector3 playerPos = Vector3.zero;

	// Token: 0x0400039F RID: 927
	[HideInInspector]
	public Vector3 aimPos = Vector3.zero;

	// Token: 0x040003A0 RID: 928
	private Vector3 smoothAimPos = Vector3.zero;

	// Token: 0x040003A1 RID: 929
	private float positionSmoother = 17.5f;

	// Token: 0x040003A2 RID: 930
	private float d;

	// Token: 0x040003A3 RID: 931
	[HideInInspector]
	public FPSController fpsController;

	// Token: 0x040003A4 RID: 932
	[HideInInspector]
	public FPSMouseLook localMouseLook;

	// Token: 0x040003A5 RID: 933
	[HideInInspector]
	public FPSMouseLook cameraMouseLook;

	// Token: 0x040003A6 RID: 934
	[HideInInspector]
	public Transform thisT;

	// Token: 0x040003A7 RID: 935
	[HideInInspector]
	public int playerID;

	// Token: 0x040003A8 RID: 936
	[HideInInspector]
	public bool playerKilled;

	// Token: 0x040003A9 RID: 937
	private int currentWeaponIndex = -1;

	// Token: 0x040003AA RID: 938
	private int previousWeaponIndex = -1;

	// Token: 0x040003AB RID: 939
	private bool isFiringRemote;

	// Token: 0x040003AC RID: 940
	private int playerTeam;

	// Token: 0x040003AD RID: 941
	private Camera mainCamera;

	// Token: 0x040003AE RID: 942
	private Transform mainCameraT;

	// Token: 0x040003AF RID: 943
	private Vector3 screenPos;

	// Token: 0x040003B0 RID: 944
	private float offset;

	// Token: 0x040003B1 RID: 945
	private string lastWeaponName = string.Empty;

	// Token: 0x040003B2 RID: 946
	private int lastBodyPart = -1;

	// Token: 0x040003B3 RID: 947
	private double interpolationBackTime = 0.37;

	// Token: 0x040003B4 RID: 948
	private PlayerNetwork.State[] m_BufferedState = new PlayerNetwork.State[20];

	// Token: 0x040003B5 RID: 949
	private int m_TimestampCount;

	// Token: 0x040003B6 RID: 950
	private int movementStateLocal = -1;

	// Token: 0x040003B7 RID: 951
	private int receivedMovementState = -1;

	// Token: 0x040003B8 RID: 952
	private float stepLength;

	// Token: 0x02000042 RID: 66
	internal struct State
	{
		// Token: 0x040003B9 RID: 953
		internal double timestamp;

		// Token: 0x040003BA RID: 954
		internal Vector3 pos;

		// Token: 0x040003BB RID: 955
		internal Quaternion rot;
	}
}
