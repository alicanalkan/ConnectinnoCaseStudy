using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace ConnectinnoGames.Scripts.Object_Pooling
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private List<PoolInfo> poolList = new List<PoolInfo>();

        private void Awake() 
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            foreach (var t in poolList)
            {
                FillPool(t);
            }
        }

        /// <summary>
        /// Fills the pool according to the given pool info
        /// </summary>
        /// <param name="info"></param>
        private void FillPool(PoolInfo info)
        {
            for (var i = 0; i < info.amount; i++)
            {
                var poolObj = Instantiate(info.prefab, transform);
                poolObj.name += $" {i}";
                poolObj.SetActive(false);
                info.pooledObjects.Enqueue(poolObj);
                poolObj.transform.position = Vector3.zero;

            }
        }

        /// <summary>
        /// Gets pool object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameObject GetPoolObject(PoolObjectType type)
        {
            var selectedPool = GetPoolByType(type);

            GameObject poolObj;
            if (selectedPool.pooledObjects.Count > 0)
            {
                poolObj = selectedPool.pooledObjects.Dequeue();
            }
            else
            {
                poolObj = Instantiate(selectedPool.prefab, transform);
                poolObj.name += $" {selectedPool.pooledObjects.Count}";
                poolObj.transform.position = Vector3.zero;
            }
            poolObj.transform.localScale = Vector3.one;
            poolObj.SetActive(true);

            return poolObj;
        }

        /// <summary>
        /// Retrieves the object back in its pool by type.
        /// </summary>
        /// <param name="poolObj"></param>
        /// <param name="type"></param>
        public void DestroyObject(GameObject poolObj, PoolObjectType type)
        {
            poolObj.SetActive(false);
            poolObj.transform.localScale = Vector3.one;

            if (poolObj.transform.parent != transform)
            {
                poolObj.transform.SetParent(transform);
            }
            
            var selectedPool = GetPoolByType(type);
            
            if(!selectedPool.pooledObjects.Contains(poolObj))
                selectedPool.pooledObjects.Enqueue(poolObj);
        }

        /// <summary>
        /// Gets the pool based on the type of object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private PoolInfo GetPoolByType(PoolObjectType type)
        {
            foreach (var poolInfo in poolList)
            {
                if (type == poolInfo.type)
                    return poolInfo;
            }

            return null;
        }

    }

    [Serializable]
    public class PoolInfo
    {
        public PoolObjectType type;
        public int amount = 0;
        public GameObject prefab;
        
        public readonly Queue<GameObject> pooledObjects = new Queue<GameObject>();
    }
}