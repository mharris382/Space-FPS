using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SA
{
	public class FxManager : MonoBehaviour
	{
		public Animator dashAnimator;

		public static FxManager singleton;

		private void Awake()
		{
			singleton = this;
		}

		public void PlayDashFx()
		{
			dashAnimator.Play("Play");
		}
	}
}
