using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.DevUtils
{
    /// <summary>
    /// Generic object pool. Attach to a GameObject, assign a prefab, and use
    /// Get/Return instead of Instantiate/Destroy.
    /// </summary>
    public class SimplePool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 5;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool (or create one if empty).
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = Instantiate(prefab, transform);
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool immediately.
        /// </summary>
        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        /// <summary>
        /// Return an object to the pool after a delay (ideal for VFX).
        /// </summary>
        public void Return(GameObject obj, float delay)
        {
            StartCoroutine(DelayedReturn(obj, delay));
        }

        private IEnumerator DelayedReturn(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(obj);
        }
    }
}
