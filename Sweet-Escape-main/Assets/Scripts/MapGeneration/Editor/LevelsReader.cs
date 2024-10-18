using Extensions;
using MapGeneration;
using ScriptableObjects;
using UnityEditor;

public class LevelsReader
{
    [MenuItem("Custom menu/Read levels")]
    public static void ReadLevels()
    {
        var mapVariations = MapVariations.Instance;
        var directory = mapVariations.MapsDirectory;
        var assetsGUID = AssetDatabase.FindAssets("t:prefab", new[] { directory });
        mapVariations.InitializeWithEmptyValues();

        foreach (var assetGUID in assetsGUID)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);

            RoomInfo roomInfo = assetPath.ParseRoomInfo();

            AddLevelToConfig(assetPath, roomInfo, mapVariations);
        }
    }

    private static void AddLevelToConfig(string assetPath, RoomInfo roomInfo, MapVariations mapVariations)
    {
        var tilemapRoom = AssetDatabase.LoadAssetAtPath<TilemapRoom>(assetPath);
        var mapDataStruct = new MapDataStruct { tilemapRoom = tilemapRoom };
        mapVariations.Rooms[roomInfo.Biome][roomInfo.Difficulty].Add(mapDataStruct);
    }
}