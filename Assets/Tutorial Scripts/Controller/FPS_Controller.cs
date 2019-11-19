using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
	public class FPS_Controller : MonoBehaviour , IShootable
	{
		public bool isLocal = true;
		bool aiming;
		bool shooting;
		new Rigidbody rigidbody;

		public int hitTimes;

		public float velocityDownSpeed = 1;
		public float normalMovementSpeed = 2;
		public float aimSpeed = 2;
		public float torqueSpeed = 2;
		public float rotationSpeed = 2;
		public float movementDownWeapon = -0.1f;
		public float dashSpeed = 40;
		public float dashResetTime = 2;
		public GameObject dashParticles;

		float dashTimer;
		bool canDash;
		bool weaponDownDash;
		float weaponDownDashTimer;
		public float maxWeaponDownDashTimer = .4f;
		public float lerpADSSpeed = 18;
		public float lerpADSAimingSpeed = 30;


		float lookAngle = 0;
		float tiltAngle = 0;
		float pivotAngle = 0;
		float moveAmount;

		public bool isDead;

		public float fov_normal = 40;

		Transform mTransform;
		public Transform tiltTransform;
		public Transform pivotTransform;
		public Transform gunParent;

		public string mainWeapon = "PlasmaRifle";
		public string secondaryWeapon = "Revolver";
		RuntimeWeapon weapon1;
		RuntimeWeapon weapon2;
		public RuntimeWeapon currentWeapon;
		float delta;
		float fixedDelta;

		Rigidbody[] ragdollRigidbodies;
		Collider[] ragdollColliders;
		Animator animator;

		private void Start()
		{
			mTransform = this.transform;
			rigidbody = GetComponent<Rigidbody>();

			if (isLocal)
			{
				rigidbody.useGravity = false;
				rigidbody.angularDrag = 0;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				RuntimeWeapon mw = Settings.resourcesManager.GetWeaponInstance(mainWeapon);
				RuntimeWeapon sw = Settings.resourcesManager.GetWeaponInstance(secondaryWeapon);
				Equip(mw, 0);
				weapon2 = sw;

			}
			else
			{
				//TODO: setup client references
				animator = GetComponentInChildren<Animator>();
				ragdollColliders = animator.transform.GetComponentsInChildren<Collider>();
				ragdollRigidbodies = animator.transform.GetComponentsInChildren<Rigidbody>();
				DisableRagdoll();
			}

		}

		void EnableRagdoll()
		{
			for (int i = 0; i < ragdollRigidbodies.Length; i++)
			{
				ragdollRigidbodies[i].isKinematic = false;
				ragdollColliders[i].isTrigger = false;
			}
			animator.transform.parent = null;
			animator.enabled = false;
		}

		void DisableRagdoll()
		{
			for (int i = 0; i < ragdollRigidbodies.Length; i++)
			{
				ragdollRigidbodies[i].isKinematic = true;
				ragdollColliders[i].isTrigger = true;
				ragdollRigidbodies[i].useGravity = false;
			}
		}

		private void FixedUpdate()
		{
			fixedDelta = Time.fixedDeltaTime;
			if (isLocal)
			{
				HandeLocalController();
				HandleADS(aiming);
			}
		}

		private void Update()
		{
			delta = Time.deltaTime;

			if (isLocal)
			{
				aiming = Input.GetMouseButton(1);
				shooting = Input.GetMouseButton(0);

				HandleRecoil();
				if (shooting)
				{
					bool isShoot = currentWeapon.Shoot();
					if (isShoot)
					{
						recoilFlag = true;
						HandleShooting(aiming);
					}
				}

				HandleSpread();

				if (!canDash)
				{
					dashTimer += delta;
					if (dashTimer > dashResetTime)
					{
						dashTimer = 0;
						canDash = true;
					}
				}

				if (weaponDownDash)
				{
					weaponDownDashTimer += delta;
					if (weaponDownDashTimer > maxWeaponDownDashTimer)
					{
						weaponDownDashTimer = 0;
						weaponDownDash = false;
					}
				}

				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					if (weapon1 != null)
						Equip(weapon1, 0);
				}

				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					if (weapon2 != null)
						Equip(weapon2, 1);
				}
			}
		}

		void HandeLocalController()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				if (Cursor.visible)
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
			}

			float horizontal = Input.GetAxis("Horizontal");
			float vertical = Input.GetAxis("Vertical");
			float upDown = Input.GetAxis("UpDown");
			float pivot = Input.GetAxis("Pivot");
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = Input.GetAxis("Mouse Y");

			moveAmount = Mathf.Clamp01((Mathf.Abs(horizontal) + Mathf.Abs( vertical)));

			

			//Handle move
			Vector3 moveDirection = tiltTransform.forward * vertical;
			moveDirection += tiltTransform.right * horizontal;
			moveDirection += tiltTransform.up * upDown;

			pivotAngle = (pivot * torqueSpeed) / fixedDelta;
			tiltAngle = (mouseY * rotationSpeed) / fixedDelta;
			lookAngle = (mouseX * rotationSpeed) / fixedDelta;

			tiltTransform.Rotate(0, lookAngle, 0);
			tiltTransform.Rotate(-tiltAngle, 0, 0);
			tiltTransform.Rotate(0, 0, -pivotAngle);

			if (mouseX != 0 || mouseY != 0 || Vector3.Equals(Vector3.zero, moveDirection) == false)
			{
				currentSpread += .1f;
			}

			Move(moveDirection);

			if (Input.GetButtonDown("Jump"))
			{
				if (canDash)
				{
					if (moveAmount == 0)
					{
						moveDirection = tiltTransform.forward;
					}

					canDash = false;
					weaponDownDash = true;
					dashParticles.SetActive(false);
					dashParticles.transform.rotation = Quaternion.LookRotation(-moveDirection);
					dashParticles.SetActive(true);
					FxManager.singleton.PlayDashFx();
					rigidbody.AddForce(moveDirection * dashSpeed, ForceMode.Impulse);
				}
			}
		}

		void Move(Vector3 moveDirection)
		{
			float actualSpeed = normalMovementSpeed;
			if (aiming)
				actualSpeed = aimSpeed;

			rigidbody.AddForce(moveDirection * actualSpeed);
	
			Vector3 velocity = rigidbody.velocity;
			Vector3 targetVelocity = Vector3.Lerp(velocity, Vector3.zero, fixedDelta * velocityDownSpeed);
			rigidbody.velocity = targetVelocity;
		}


		void HandleADS(bool aiming)
		{
			Vector3 tp = currentWeapon.def_pos;
			float fov = fov_normal;
			float adsSpeed = lerpADSSpeed;

			if (aiming)
			{
				tp = Vector3.zero;
				fov = currentWeapon.fov_ads;
				adsSpeed = lerpADSAimingSpeed;
			}
			else
			{
				if (moveAmount > 0.1f)
				{
					tp.y += movementDownWeapon;
				}
			}

			if (weaponDownDash)
			{
				tp.y = -1;
				currentWeapon.transform.localPosition = tp;
				fov = fov_normal;
			}
			else
			{
				Vector3 actualPos = Vector3.Lerp(currentWeapon.transform.localPosition, tp, fixedDelta * adsSpeed);
				//Vector3 actualPos = tp;
				currentWeapon.transform.localPosition = actualPos;
			}

			float fv = Mathf.Lerp(Camera.main.fieldOfView, fov, delta * 11);
			Camera.main.fieldOfView = fv;

		}

		void Equip(RuntimeWeapon weapon, int pos)
		{
			if (isLocal == false)
			{
				return;
			}
			else
			{
				if (currentWeapon != null)
				{
					currentWeapon.gameObject.SetActive(false);
				}

				if (pos == 0)
				{
					if (weapon1 != null)
					{
						//TODO: drop previous weapon
					}

					weapon1 = weapon;
				}
				else
				{
					if (weapon2 != null)
					{
						//TODO: drop previous weapon
					}

					weapon2 = weapon;
				}

				weapon.transform.parent = gunParent;
				weapon.transform.localPosition = Vector3.zero;
				weapon.transform.localRotation = Quaternion.identity;
				weapon.gameObject.SetActive(true);

				currentWeapon = weapon;
			}
		}

		public GameObject impactFx;

		[SerializeField]
		float currentSpread;
		[SerializeField]
		float spreadResetSpeed = 0.03f;

		void HandleShooting(bool aiming)
		{
			RaycastHit hit;
			
			float actualSpread = currentSpread;

			if (aiming)
			{
				actualSpread *= .02f;
			}

			Vector3 randPos = Random.insideUnitCircle * currentSpread;
			Vector3 shootOrigin = gunParent.position + gunParent.TransformDirection(randPos);
			Ray ray = new Ray(shootOrigin, gunParent.forward);

			if (Physics.Raycast(ray, out hit, 100))
			{
				IShootable shootable = hit.transform.GetComponent<IShootable>();
				if (shootable != null)
				{
					shootable.OnHit(hit.point, gunParent.forward);
				}

				impactFx.SetActive(false);
				impactFx.transform.position = hit.point;
				impactFx.transform.LookAt(shootOrigin);
				impactFx.SetActive(true);
			}

			currentSpread += currentWeapon.spreadValue;
		}

		void HandleSpread()
		{
			currentSpread = Mathf.Lerp(currentSpread, 0, spreadResetSpeed * delta);
			currentSpread = Mathf.Clamp(currentSpread, 0, currentWeapon.maxSpreadValue);
		}

		#region Recoil
		[SerializeField]
		float recoilValue;
		[SerializeField]
		float recoilResetSpeed = 1;
		[SerializeField]
		float recoilSpeed = 3;
		[SerializeField]
		float maxRecoil = 15;
		[SerializeField]
		float recoilRate = .1f;
		bool recoilFlag;

		void HandleRecoil()
		{
			if (currentWeapon == null)
			{
				Debug.Log("no current weapon");
				return;
			}

			Quaternion targetRotation = Quaternion.identity;
			float lerpSpeed = delta * recoilResetSpeed;

			if (recoilFlag)
			{
				targetRotation = Quaternion.Euler(-recoilValue, 0, 0);
				lerpSpeed = delta * recoilSpeed;

				if (Time.realtimeSinceStartup - currentWeapon.lastShot > recoilRate)
				{
					recoilValue += currentWeapon.weaponKick;
					recoilValue = Mathf.Clamp(recoilValue, -maxRecoil, maxRecoil);
					recoilFlag = false;
				}
			}
			else
			{
				recoilValue = currentWeapon.weaponKick;
			}

			gunParent.localRotation = Quaternion.Slerp(gunParent.localRotation, targetRotation, lerpSpeed);
		}
		#endregion

		public void OnHit(Vector3 hitPosition, Vector3 hitDirection)
		{
			if (!isDead)
			{
				hitTimes++;
				if (hitTimes > 0)
				{
					isDead = true;
					EnableRagdoll();
					this.enabled = false;

					if (currentWeapon != null)
					{
						currentWeapon.OnDrop();
					}
				}
			}
		}

	}
}
