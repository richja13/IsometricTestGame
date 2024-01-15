using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadCharacters : MonoBehaviour
{
    const string key = "Characters";
    public static List<SpawningCharactersManager> characterList = new();
    public static bool CharactersLoaded;

    private void Awake()
    {
        CharactersLoaded = false;
        GetCharacters();
    }

    public async void GetCharacters()
    {
        var asset = Addressables.LoadAssetsAsync<SpawningCharactersManager>(key, obj =>
        {
            characterList.Add(obj);
        });

        try
        {
            while (!asset.IsDone)
                await Task.Yield();
        }
        catch
        {
            Debug.Log("Asset error");
        }

        CharactersLoaded = true;
    }
}
