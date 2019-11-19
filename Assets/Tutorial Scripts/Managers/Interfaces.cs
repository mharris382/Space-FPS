using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
	public interface IShootable {
		void OnHit(Vector3 hitPosition, Vector3 hitDirection);
	}

}