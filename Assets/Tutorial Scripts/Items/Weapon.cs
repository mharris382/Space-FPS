using UnityEngine;
using System.Collections;

namespace SA
{
	[CreateAssetMenu]
	public class Weapon : Item
	{
		public GameObject fps_prefab;
		[SerializeField]
		float _fireRate = .2f;
		public float fireRate { get { return _fireRate; } }

		[SerializeField]
		float _weaponKick = 5;
		public float weaponKick { get { return _weaponKick; } }

		[SerializeField]
		float _spreadValue;
		public float spreadValue { get { return _spreadValue; } }
		[SerializeField]
		float _maxSpreadValue = .5f;
		public float maxSpreadValue { get { return _maxSpreadValue; } }


		public float fov_ads = 35;
	}
}
