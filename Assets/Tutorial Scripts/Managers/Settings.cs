using UnityEngine;
using System.Collections;

namespace SA
{
	public static class Settings 
	{
		static ResourcesManager _resourcesManager;
		public static ResourcesManager resourcesManager {
			get {
				if (_resourcesManager == null)
				{
					_resourcesManager = Resources.Load("ResourcesManager") as ResourcesManager;
					_resourcesManager.Init();
				}

				return _resourcesManager;
			}
		}

	}
}
