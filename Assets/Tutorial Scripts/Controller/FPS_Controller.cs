using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SA
{
    public class FPS_Controller : MonoBehaviour
	{
		public bool isLocal = true;
		new Rigidbody rigidbody;

		public float velocityDownSpeed  = 1;
		public float movementSpeed      = 2;
		public float torqueSpeed        = 2;
		public float rotationSpeed      = 2;

		float lookAngle = 0;
		float tiltAngle = 0;
		float pivotAngle = 0;

		public float fov_normal = 40;

		Transform mTransform;
		public Transform tiltTransform;
		public Transform pivotTransform;
		public Transform gunParent;

		public string mainWeapon = "PlasmaRifle";
		public string secondaryWeapon = "Revolver";
		RuntimeWeapon weapon1;
		RuntimeWeapon weapon2;
		RuntimeWeapon currentWeapon;
		float delta;

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
			}

		}

		private void Update()
		{
			delta = Time.deltaTime;

			if (isLocal)
			{
				HandeLocalController();
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
			bool aiming = Input.GetMouseButton(1);
			bool shooting = Input.GetMouseButton(0);

			//Handle move
			Vector3 moveDirection = tiltTransform.forward * vertical;
			moveDirection += tiltTransform.right * horizontal;
			moveDirection += tiltTransform.up * upDown;
			Move(moveDirection);

			pivotAngle = (pivot * torqueSpeed) / delta;
			tiltAngle = (mouseY * rotationSpeed) / delta;
			lookAngle = (mouseX * rotationSpeed) / delta;

			tiltTransform.Rotate(0, lookAngle, 0);
			tiltTransform.Rotate(-tiltAngle, 0, 0);
			tiltTransform.Rotate(0, 0, pivotAngle);

			if (mouseX != 0 || mouseY != 0 || Vector3.Equals(Vector3.zero, moveDirection) == false)
			{
				currentSpread += .1f;
			}

			HandleRecoil();
			HandleADS(aiming);
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

		void Move(Vector3 moveDirection)
		{
			rigidbody.AddForce(moveDirection * movementSpeed);

			Vector3 velocity = rigidbody.velocity;
			Vector3 targetVelocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * velocityDownSpeed);
			rigidbody.velocity = targetVelocity;
		}

		void HandleADS(bool aiming)
		{
			Vector3 tp = currentWeapon.def_pos;
			float fov = fov_normal;

			if (aiming)
			{
				tp = Vector3.zero;
				fov = currentWeapon.fov_ads;
			}

			Vector3 actualPos = Vector3.Lerp(currentWeapon.transform.localPosition, tp, delta * 9);
			currentWeapon.transform.localPosition = actualPos;

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

			Vector3 randPos = UnityEngine.Random.insideUnitCircle * currentSpread;
			Vector3 shootOrigin = gunParent.position + gunParent.TransformDirection(randPos);
			Ray ray = new Ray(shootOrigin, gunParent.forward);

			if (Physics.Raycast(ray, out hit, 100))
			{
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
	}
}
