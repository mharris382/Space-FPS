using UnityEngine;
using System.Collections;

namespace SA
{
	public class BodypartShootableHook : ShootableHook
	{
		FPS_Controller controller;

		public override void Init()
		{
			base.Init();
			controller = GetComponentInParent<FPS_Controller>();
		}


		public override void Hit(Vector3 hitPosition, Vector3 hitDirection)
		{
			base.Hit(hitPosition, hitDirection);
			controller.OnHit(hitPosition, hitDirection);
		}

	}
}
