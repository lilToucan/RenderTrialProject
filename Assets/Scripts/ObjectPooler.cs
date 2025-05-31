using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] int numerToSpawnAtStart;

    List<GameObject> pooledObjects = new();
    AfterImageTrail afterImage;

    private void Awake()
    {
        CreateObjectsToPool(numerToSpawnAtStart);

        afterImage = GetComponent<AfterImageTrail>();
    }

    private void OnEnable()
    {
        afterImage.GetPooledGameobject += GetObject;
    }

    private void OnDisable()
    {
        afterImage.GetPooledGameobject -= GetObject;
    }

    private void CreateObjectsToPool(int num)
    {
        for (int i = 0; i < num; i++)
        {
            var obj = new GameObject();
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>();
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (obj.activeInHierarchy)
                continue;

            obj.SetActive(true);
            return obj;
        }

        CreateObjectsToPool(10);

        return GetObject();
    }
}
