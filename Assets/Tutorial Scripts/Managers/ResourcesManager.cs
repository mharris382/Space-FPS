using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SA
{
	[CreateAssetMenu]
	public class ResourcesManager : ScriptableObject
	{
		public Item[] items;
		[System.NonSerialized]
		Dictionary<string, Item> itemsDictionary = new Dictionary<string, Item>();

		public void Init()
		{
			for (int i = 0; i < items.Length; i++)
			{
				itemsDictionary.Add(items[i].name, items[i]);
			}
		}

		Item GetItem(string id)
		{
			Item retVal = null;
			itemsDictionary.TryGetValue(id, out retVal);
			return retVal;
		}

		public RuntimeWeapon GetWeaponInstance(string id)
		{
			Item item = GetItem(id);
			if ((item is Weapon) == false) return null;

			Weapon w = (Weapon)item;

			GameObject go = Instantiate(w.fps_prefab);
			RuntimeWeapon retVal = go.GetComponent<RuntimeWeapon>();
			retVal.Init(w);
			go.SetActive(false);
			return retVal;
		}
	}
}
