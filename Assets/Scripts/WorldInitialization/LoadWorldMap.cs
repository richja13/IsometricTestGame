using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadWorldMap : MonoBehaviour
{
    [SerializeField]
    private string key;

    AsyncOperationHandle<GameObject> opHandle;

    public static bool MapLoaded;

    private void Awake()
    {
        MapLoaded = false;
        StartCoroutine("LoadWalls");
    }

    public IEnumerator LoadWalls()
    {
        opHandle = Addressables.LoadAssetAsync<GameObject>(key);
        yield return opHandle;

        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject obj = opHandle.Result;
            Instantiate(obj, transform);
            MapLoaded = true;
        }
    }
}
