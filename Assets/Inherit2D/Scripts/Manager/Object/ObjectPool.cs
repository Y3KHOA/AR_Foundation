using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lớp này tạo ra một lưới động trong không gian 2D, cho phép các ô vuông được tạo ra và di chuyển theo vị trí của camera.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private List<GameObject> pool;

    public GameObject prefab;
    public int poolSize = 5;

    public List<GameObject> _pool { get => pool; set => pool = value; }

    // Start is called before the first frame update
    void Awake()
    {
        _pool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _pool.Add(obj);
        }
    }

    public GameObject GetObjectFromPool()
    {
        foreach (GameObject go in _pool)
        {
            if (!go.activeInHierarchy)
            {
                return go;
            }
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _pool.Add(obj);
        return obj;
    }

    public void ReturnGameObjetToPool(GameObject obj)
    {
        obj.transform.SetParent(transform);
        obj.SetActive(false);
    }
}
