using UnityEngine;
using System.Collections;

namespace SA
{
	public class ShootableHook : MonoBehaviour, IShootable
	{
		new Rigidbody rigidbody;

		private void Start()
		{
			Init();
		}

		public virtual void Init()
		{
			rigidbody = GetComponentInChildren<Rigidbody>();
		}

		public virtual void Hit(Vector3 hitPosition, Vector3 hitDirection)
		{
			rigidbody.AddForceAtPosition(hitDirection * 25, hitPosition, ForceMode.Impulse);
		}

		public void OnHit(Vector3 hitPosition, Vector3 hitDirection)
		{
			Hit(hitPosition, hitDirection);
		}
	}
}
