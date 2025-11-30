using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004B RID: 75
public class PlayerWeapons : MonoBehaviour
{
	// Token: 0x060001AA RID: 426 RVA: 0x00022AD4 File Offset: 0x00020ED4
	public void QuickSetup(bool isLocal)
	{
		this.firstPersonWeapons = this.firstPersonAudioSource.transform;
		this.totalWeapons.Clear();
		this.PrepareWepaons(this.primaryWeapons);
		this.PrepareWepaons(this.secondaryWeapons);
		this.PrepareWepaons(this.specialWeapons);
		if (isLocal)
		{
			this.defaultSwayPosition = this.firstPersonWeapons.localPosition;
			this.mainPlayerCamera = this.playerCamera.GetComponent<Camera>();
			this.mainPlayerCamera.fieldOfView = GameSettings.defaultFOV;
		}
	}

	// Token: 0x060001AB RID: 427 RVA: 0x00022B5C File Offset: 0x00020F5C
	private void PrepareWepaons(List<PlayerWeapons.WeaponSet> tmpWeapons)
	{
		for (int i = 0; i < tmpWeapons.Count; i++)
		{
			tmpWeapons[i].firstPersonWeapon.playerWeapons = this;
			tmpWeapons[i].firstPersonWeapon.wSettings = tmpWeapons[i];
			tmpWeapons[i].firstPersonWeapon.audioSource = this.firstPersonAudioSource;
			tmpWeapons[i].firstPersonWeapon.gameObject.SetActive(false);
			tmpWeapons[i].firstPersonWeapon.playerNetwork = this.playerNetwork;
			tmpWeapons[i].firstPersonWeapon.isThirdPerson = false;
			if (tmpWeapons[i].thirdPersonWeapon != null)
			{
				tmpWeapons[i].thirdPersonWeapon.playerWeapons = this;
				tmpWeapons[i].thirdPersonWeapon.wSettings = tmpWeapons[i];
				tmpWeapons[i].thirdPersonWeapon.audioSource = this.thirdPersonAudioSource;
				tmpWeapons[i].thirdPersonWeapon.gameObject.SetActive(false);
				tmpWeapons[i].thirdPersonWeapon.soldierAnimation = this.soldierAnimation;
				tmpWeapons[i].thirdPersonWeapon.playerNetwork = this.playerNetwork;
				tmpWeapons[i].thirdPersonWeapon.isThirdPerson = true;
			}
			this.totalWeapons.Add(tmpWeapons[i]);
		}
	}

	// Token: 0x060001AC RID: 428 RVA: 0x00022CC4 File Offset: 0x000210C4
	private void Update()
	{
		if (GameSettings.menuOpened)
		{
			this.isFiring = false;
			return;
		}
		if (this.previousWeaponIndex != GameSettings.switchWeaponIndex)
		{
			if (GameSettings.switchWeaponIndex > 3 || GameSettings.switchWeaponIndex < 1)
			{
				GameSettings.switchWeaponIndex = 1;
			}
			this.previousWeaponIndex = GameSettings.switchWeaponIndex;
			this.GetWeaponToSelect(this.previousWeaponIndex);
		}
		if (this.currentSelectedWeapon)
		{
			if (GameSettings.mobileReloading && !this.currentSelectedWeapon.isReloading)
			{
				this.currentSelectedWeapon.ReloadRemote();
			}
			if (GameSettings.mobileReloading)
			{
				GameSettings.mobileReloading = false;
			}
			if (this.currentSelectedWeapon.wSettings.fireType == PlayerWeapons.FireType.MachineGun)
			{
				if (GameSettings.mobileFiring)
				{
					this.currentSelectedWeapon.Fire(false);
				}
				else
				{
					this.isFiring = false;
				}
			}
			else
			{
				if (GameSettings.mobileFiring)
				{
					this.currentSelectedWeapon.Fire(false);
					GameSettings.mobileFiring = false;
					this.isSingleFiring = true;
				}
				else
				{
					this.isSingleFiring = false;
				}
				this.isFiring = false;
			}
			if (this.currentSelectedWeapon.wSettings.aimType != PlayerWeapons.AimType.None && GameSettings.mobileAiming)
			{
				this.isAimed = !this.isAimed;
				if (this.isAimed && this.currentSelectedWeapon.wSettings.aimType == PlayerWeapons.AimType.CameraOnly && this.currentSelectedWeapon.wSettings.scopeTexture != null)
				{
					GameSettings.currentScopeTexture = this.currentSelectedWeapon.wSettings.scopeTexture;
				}
				else
				{
					GameSettings.currentScopeTexture = null;
				}
			}
			if (GameSettings.mobileAiming)
			{
				GameSettings.mobileAiming = false;
			}
			if (this.currentSelectedWeapon.isReloading)
			{
				this.isAimed = false;
			}
			if (this.isAimed)
			{
				this.mainPlayerCamera.fieldOfView = Mathf.Lerp(this.mainPlayerCamera.fieldOfView, this.currentSelectedWeapon.wSettings.aimFOV, Time.deltaTime * 19f);
				if (this.currentSelectedWeapon.aimOffset != Vector3.zero)
				{
					this.currentSelectedWeapon.thisT.localPosition = Vector3.Lerp(this.currentSelectedWeapon.thisT.localPosition, this.currentSelectedWeapon.aimOffset, Time.deltaTime * 19f);
				}
			}
			else
			{
				this.mainPlayerCamera.fieldOfView = Mathf.Lerp(this.mainPlayerCamera.fieldOfView, GameSettings.defaultFOV, Time.deltaTime * 19f);
				if (this.currentSelectedWeapon.aimOffset != Vector3.zero)
				{
					this.currentSelectedWeapon.thisT.localPosition = Vector3.Lerp(this.currentSelectedWeapon.thisT.localPosition, this.currentSelectedWeapon.defaultPosition, Time.deltaTime * 19f);
				}
				if (GameSettings.currentScopeTexture != null)
				{
					GameSettings.currentScopeTexture = null;
				}
			}
			GameSettings.currentFOV = ((!this.isAimed) ? GameSettings.defaultFOV : this.currentSelectedWeapon.wSettings.aimFOV);
			GameSettings.isWeaponInAction = (this.isFiring || this.currentSelectedWeapon.isReloading || this.isSingleFiring);
			if (GameSettings.isWeaponInAction)
			{
				GameSettings.singleWeaponTimeAction = Time.time;
			}
		}
	}

	// Token: 0x060001AD RID: 429 RVA: 0x00023028 File Offset: 0x00021428
	public void GetWeaponToSelect(int type)
	{
		if (type == 1)
		{
			this.SwitchWeapon(this.primaryWeapons[this.selectedPrimary].firstPersonWeapon, true);
		}
		if (type == 2)
		{
			this.SwitchWeapon(this.secondaryWeapons[this.selectedSecondary].firstPersonWeapon, true);
		}
		if (type == 3)
		{
			this.SwitchWeapon(this.specialWeapons[this.selectedSpecial].firstPersonWeapon, true);
		}
	}

	// Token: 0x060001AE RID: 430 RVA: 0x000230A1 File Offset: 0x000214A1
	private void FixedUpdate()
	{
		this.DoWeaponBobbing();
	}

	// Token: 0x060001AF RID: 431 RVA: 0x000230AC File Offset: 0x000214AC
	private void SwitchWeapon(FPSWeapon tmpWeapon, bool firstPerson)
	{
		if (tmpWeapon != null && tmpWeapon != this.currentSelectedWeapon)
		{
			if (this.currentSelectedWeapon)
			{
				this.currentSelectedWeapon.gameObject.SetActive(false);
				this.currentSelectedWeapon = null;
			}
			this.isAimed = false;
			this.currentSelectedWeapon = tmpWeapon;
			this.currentSelectedWeapon.gameObject.SetActive(true);
			this.currentSelectedWeapon.Deploy();
			if (firstPerson)
			{
				for (int i = 0; i < this.totalWeapons.Count; i++)
				{
					if (this.totalWeapons[i].firstPersonWeapon == tmpWeapon)
					{
						this.globalWeaponIndex = i;
						this.currentSelectedWeapon.weaponIndex = i;
					}
				}
			}
		}
	}

	// Token: 0x060001B0 RID: 432 RVA: 0x0002317C File Offset: 0x0002157C
	private void DoWeaponBobbing()
	{
		if (this.fpsController.isMoving && this.fpsController.isGrounded)
		{
			this.currentBobbingSpeed = this.bobbingSpeed;
			this.currentBobbingSpeed *= this.fpsController.speed;
			float num = Mathf.Sin(this.timer);
			this.timer += this.currentBobbingSpeed;
			if (this.timer > 6.2831855f)
			{
				this.timer -= 6.2831855f;
			}
			if (num != 0f)
			{
				float num2 = num * this.bobbingAmount;
				float num3 = (float)(Mathf.Abs(1) + Mathf.Abs(1));
				num3 = Mathf.Clamp(num3, 0f, 1f);
				num2 = num3 * num2;
				this.firstPersonWeapons.localPosition = new Vector3(this.defaultSwayPosition.x, this.defaultSwayPosition.y, this.defaultSwayPosition.z - num2);
			}
		}
		else
		{
			this.timer = 0f;
			this.firstPersonWeapons.localPosition = Vector3.Lerp(this.firstPersonWeapons.localPosition, this.defaultSwayPosition, Time.deltaTime * 5f);
		}
	}

	// Token: 0x060001B1 RID: 433 RVA: 0x000232B5 File Offset: 0x000216B5
	public void SwitchWeaponRemote()
	{
		this.SwitchWeapon(this.totalWeapons[this.globalWeaponIndex].thirdPersonWeapon, false);
	}

	// Token: 0x060001B2 RID: 434 RVA: 0x000232D4 File Offset: 0x000216D4
	public void FireRemote()
	{
		if (this.currentSelectedWeapon)
		{
			this.currentSelectedWeapon.Fire(true);
		}
	}

	// Token: 0x04000419 RID: 1049
	public Transform playerCamera;

	// Token: 0x0400041A RID: 1050
	public GameObject concreteParticles;

	// Token: 0x0400041B RID: 1051
	public GameObject metalParticles;

	// Token: 0x0400041C RID: 1052
	public GameObject woodParticles;

	// Token: 0x0400041D RID: 1053
	public GameObject bloodParticles;

	// Token: 0x0400041E RID: 1054
	public AudioSource firstPersonAudioSource;

	// Token: 0x0400041F RID: 1055
	public AudioSource thirdPersonAudioSource;

	// Token: 0x04000420 RID: 1056
	public GameObject grenadePrefab;

	// Token: 0x04000421 RID: 1057
	public GameObject grenadeExplosionParticles;

	// Token: 0x04000422 RID: 1058
	public List<PlayerWeapons.WeaponSet> primaryWeapons;

	// Token: 0x04000423 RID: 1059
	public List<PlayerWeapons.WeaponSet> secondaryWeapons;

	// Token: 0x04000424 RID: 1060
	public List<PlayerWeapons.WeaponSet> specialWeapons;

	// Token: 0x04000425 RID: 1061
	[HideInInspector]
	public List<PlayerWeapons.WeaponSet> totalWeapons = new List<PlayerWeapons.WeaponSet>();

	// Token: 0x04000426 RID: 1062
	public int selectedPrimary;

	// Token: 0x04000427 RID: 1063
	public int selectedSecondary;

	// Token: 0x04000428 RID: 1064
	public int selectedSpecial;

	// Token: 0x04000429 RID: 1065
	private Vector3 defaultSwayPosition;

	// Token: 0x0400042A RID: 1066
	private float bobbingSpeed = 0.0135f;

	// Token: 0x0400042B RID: 1067
	private float bobbingAmount = 0.0175f;

	// Token: 0x0400042C RID: 1068
	private float timer;

	// Token: 0x0400042D RID: 1069
	private float currentBobbingSpeed;

	// Token: 0x0400042E RID: 1070
	private Transform firstPersonWeapons;

	// Token: 0x0400042F RID: 1071
	[HideInInspector]
	public FPSWeapon currentSelectedWeapon;

	// Token: 0x04000430 RID: 1072
	[HideInInspector]
	public int globalWeaponIndex = -1;

	// Token: 0x04000431 RID: 1073
	[HideInInspector]
	public bool isFiring;

	// Token: 0x04000432 RID: 1074
	private bool isSingleFiring;

	// Token: 0x04000433 RID: 1075
	[HideInInspector]
	public SoldierAnimation soldierAnimation;

	// Token: 0x04000434 RID: 1076
	[HideInInspector]
	public PlayerNetwork playerNetwork;

	// Token: 0x04000435 RID: 1077
	[HideInInspector]
	public FPSController fpsController;

	// Token: 0x04000436 RID: 1078
	[HideInInspector]
	public Camera mainPlayerCamera;

	// Token: 0x04000437 RID: 1079
	public bool isAimed;

	// Token: 0x04000438 RID: 1080
	private int previousWeaponIndex;

	// Token: 0x0200004C RID: 76
	public enum FireType
	{
		// Token: 0x0400043A RID: 1082
		MachineGun,
		// Token: 0x0400043B RID: 1083
		Pistol,
		// Token: 0x0400043C RID: 1084
		Shotgun,
		// Token: 0x0400043D RID: 1085
		SniperRifle,
		// Token: 0x0400043E RID: 1086
		Grenade,
		// Token: 0x0400043F RID: 1087
		RPG
	}

	// Token: 0x0200004D RID: 77
	public enum AimType
	{
		// Token: 0x04000441 RID: 1089
		None,
		// Token: 0x04000442 RID: 1090
		CameraOnly,
		// Token: 0x04000443 RID: 1091
		CameraAndIronsights
	}

	// Token: 0x0200004E RID: 78
	[Serializable]
	public class WeaponSet
	{
		// Token: 0x060001B3 RID: 435 RVA: 0x000232F4 File Offset: 0x000216F4
		public WeaponSet(FPSWeapon fpw, FPSWeapon tpw)
		{
			this.firstPersonWeapon = fpw;
			this.thirdPersonWeapon = tpw;
			this.weaponCost = 0;
			this.timeToDeploy = 1f;
			this.reloadTime = 1f;
			this.fireRate = 0.05f;
			this.bulletsPerClip = 30;
			this.reserveBullets = 150;
			this.headDamage = 30;
			this.torsoDamage = 15;
			this.limbsDamage = 10;
			this.aimType = PlayerWeapons.AimType.None;
			this.aimFOV = GameSettings.defaultFOV;
			this.aimObject = null;
			this.scopeTexture = null;
			this.obfuscatedPrice = 0;
			this.showThis = true;
		}

		// Token: 0x04000444 RID: 1092
		public FPSWeapon firstPersonWeapon;

		// Token: 0x04000445 RID: 1093
		public FPSWeapon thirdPersonWeapon;

		// Token: 0x04000446 RID: 1094
		public int weaponCost;

		// Token: 0x04000447 RID: 1095
		public PlayerWeapons.FireType fireType;

		// Token: 0x04000448 RID: 1096
		public float timeToDeploy;

		// Token: 0x04000449 RID: 1097
		public float reloadTime;

		// Token: 0x0400044A RID: 1098
		public float fireRate;

		// Token: 0x0400044B RID: 1099
		public int bulletsPerClip;

		// Token: 0x0400044C RID: 1100
		public int reserveBullets;

		// Token: 0x0400044D RID: 1101
		public int headDamage;

		// Token: 0x0400044E RID: 1102
		public int torsoDamage;

		// Token: 0x0400044F RID: 1103
		public int limbsDamage;

		// Token: 0x04000450 RID: 1104
		public AudioClip fireSound;

		// Token: 0x04000451 RID: 1105
		public AudioClip reloadSound;

		// Token: 0x04000452 RID: 1106
		public AudioClip takeInSound;

		// Token: 0x04000453 RID: 1107
		public PlayerWeapons.AimType aimType;

		// Token: 0x04000454 RID: 1108
		public float aimFOV;

		// Token: 0x04000455 RID: 1109
		public Transform aimObject;

		// Token: 0x04000456 RID: 1110
		public Sprite scopeTexture;

		// Token: 0x04000457 RID: 1111
		[HideInInspector]
		public int obfuscatedPrice;

		// Token: 0x04000458 RID: 1112
		[HideInInspector]
		public bool showThis;
	}
}
