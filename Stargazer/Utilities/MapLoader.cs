using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Stargazer.Utilities;

public class MapLoader
{
    private class Out<T>
    {
        public T Value { get; set; }
    }

    public static ShipStatus Skeld { get; private set; }
    public static AirshipStatus Airship { get; private set; }
    public static FungleShipStatus Fungle { get; private set; }

    public static IEnumerator LoadMaps()
    {
        while (AmongUsClient.Instance == null) yield return null;

        if (!Skeld)
        {
            Out<ShipStatus> o = new();
            yield return LoadMap(MapNames.Skeld, o);
            Skeld = o.Value;
        }

        if (!Airship)
        {
            Out<AirshipStatus> o = new();
            yield return LoadMap(MapNames.Airship, o);
            Airship = o.Value;
        }

        if (!Fungle)
        {
            Out<FungleShipStatus> o = new();
            yield return LoadMap(MapNames.Fungle, o);
            Fungle = o.Value;
        }
    }

    private static IEnumerator LoadMap<T>(MapNames map, Out<T> shipStatus) where T : ShipStatus
    {
        AssetReference reference = AmongUsClient.Instance.ShipPrefabs._items[(int) map];

        if (reference.IsValid())
        {
            shipStatus.Value = reference.OperationHandle.Result.Cast<GameObject>().GetComponent<T>();
        }
        else
        {
            AsyncOperationHandle<GameObject> asset = reference.LoadAsset<GameObject>();
            yield return asset;

            shipStatus.Value = asset.Result.GetComponent<T>();
        }
    }
}