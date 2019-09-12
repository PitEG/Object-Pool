using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PitGan {
	public class ObjectPooler : MonoBehaviour {

		public ObjectPool[] objectsToPool;
		private Dictionary<int, ObjectPool> objectPools;

		private static GameObject instance;
		public static ObjectPooler objectPoolerInstance;

		public static GameObject Instance {
			get { return instance; }
		}

		//gets by gameobject hashcode
		public GameObject Get(GameObject gameObject) {
			int key = gameObject.GetHashCode();
			if (!objectPools.ContainsKey(key)) {
				Debug.Log("object is not pooled");
				return null;
			}
			ObjectPool pool = objectPools[key];
			GameObject nextObject = pool.Get();
			return nextObject;
		}

		private void Awake() {
			instance = gameObject;
			objectPoolerInstance = this;
			objectPools = new Dictionary<int, ObjectPool>();
			PoolObjects();
		}

		private void PoolObjects() {
			for (int i = 0; i < objectsToPool.Length; i++) {
				ObjectPool currentObjectPool = objectsToPool[i];
				GameObject currentObject = currentObjectPool.gameObject;

				//skip object of the current index if it's empty
				if (currentObject == null) {
					continue;
				}
				int key = currentObject.GetHashCode();
				if (objectPools.ContainsKey(key)) {
					throw new System.ArgumentException(
						"You are pooling an object/prefab twice. " +
						"Please remove one.");
				}
				objectPools.Add(key, currentObjectPool);	

				//set parent game object
				Transform parent = transform;
				if (currentObjectPool.ownFolder) {
					parent = CreateParentObject(
						currentObjectPool.folderName).transform;
				}

				//create the instances
				currentObjectPool.Pool = 
					new GameObject[currentObjectPool.poolSize];
				for (int j = 0; j < currentObjectPool.poolSize; j++) {
					GameObject instance = 
						CreateInstance(currentObject, parent);
					currentObjectPool.Pool[j] = instance;
				}
			}
		}

		private GameObject CreateParentObject(string name) {
			GameObject parent = new GameObject(name);
			parent.transform.SetParent(transform);
			return parent;
		}

		private GameObject CreateInstance(
			GameObject gameObject, Transform parent) {

			GameObject instance = Instantiate(gameObject, parent);
			instance.SetActive(false);
			return instance;
		}

		[System.Serializable] //so that the user can see it in the inpspector
		public class ObjectPool {
			public GameObject gameObject;
			private GameObject[] pool;
			private int currentObjectIndex;
			public int poolSize;
			public bool ownFolder;
			public string folderName;

			public GameObject[] Pool {
				get { return this.pool; }
				set { pool = value; }
			}

			public int CurrentObjectIndex {
				get { return currentObjectIndex; }
				set {
					currentObjectIndex = value;
					if (currentObjectIndex == poolSize) {
						currentObjectIndex = 0;
					}
				}
			}

			public GameObject Get() {
				return pool[CurrentObjectIndex++];
			}
		}
	}
}
