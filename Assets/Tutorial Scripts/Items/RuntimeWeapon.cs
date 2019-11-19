using UnityEngine;
using System.Collections;

namespace SA
{
	public class RuntimeWeapon : MonoBehaviour
	{
		Weapon baseWeapon;
		public Vector3 def_pos;
		ParticleSystem[] particles;
		public float fov_ads { get { return baseWeapon.fov_ads; } }

		public void Init(Weapon w)
		{
			baseWeapon = w;
			particles = GetComponentsInChildren<ParticleSystem>();
		}

		int _magazineAmmo;
		public int magazineAmmo {
			get { return _magazineAmmo; }
		}

		float _lastShot;
		public float lastShot { get { return _lastShot; } }
		public float weaponKick { get { return baseWeapon.weaponKick; } }
		public float spreadValue { get { return baseWeapon.spreadValue; } }
		public float maxSpreadValue { get { return baseWeapon.maxSpreadValue; } }

		public bool Shoot()
		{
			if (Time.realtimeSinceStartup - _lastShot > baseWeapon.fireRate)
			{
				_lastShot = Time.realtimeSinceStartup;
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Play();
				}

				return true;
			}

			return false;
		}
	}
}
