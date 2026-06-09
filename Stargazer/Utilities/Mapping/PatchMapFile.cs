using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Stargazer.Mapping;
using UnityEngine;

namespace Stargazer.Mapping;

public static class PatchMapFile
{
    private const string FolderPath = @"C:\Map";
    private const string ResourcePrefix = "Stargazer.Resources.Maps.";
    private const string BinaryMagic = "RFS_PATH_MAP_V2";

    public static string GetCurrentMapName()
    {
        if (GameObject.Find("PolusShip(Clone)") != null) return "Polus";
        if (GameObject.Find("SkeldShip(Clone)") != null) return "Skeld";
        if (GameObject.Find("MiraShip(Clone)") != null) return "Mira";
        if (GameObject.Find("Airship(Clone)") != null) return "Airship";
        if (GameObject.Find("FungleShip(Clone)") != null) return "Fungle";
        if (GameObject.Find("Submerged(Clone)") != null) return "Submerged";

        if (ShipStatus.Instance)
        {
            string name = ShipStatus.Instance.gameObject.name.Replace("(Clone)", "");

            if (name.Contains("Polus")) return "Polus";
            if (name.Contains("Skeld")) return "Skeld";
            if (name.Contains("Mira")) return "Mira";
            if (name.Contains("Airship")) return "Airship";
            if (name.Contains("Fungle")) return "Fungle";
            if (name.Contains("Submerged")) return "Submerged";
        }

        return "Unknown";
    }

    public static bool TryLoadCurrentMap(out RandomizationUtils.CachedPathMapFileData data, out string source)
    {
        string mapName = GetCurrentMapName();

        if (TryLoadEmbeddedBinary(mapName, out data))
        {
            source = "embedded .map";
            return true;
        }

        string path = GetExternalBinaryPath(mapName);

        if (File.Exists(path))
        {
            data = ReadFromBinaryFile(path);
            source = path;
            return true;
        }

        source = null;
        data = null;
        return false;
    }

    public static void SaveCurrentMap(RandomizationUtils.CachedPathMapFileData data)
    {
        Directory.CreateDirectory(FolderPath);

        string path = GetExternalBinaryPath(data.MapName);
        WriteToBinaryFile(path, data);
    }

    private static string GetExternalBinaryPath(string mapName)
    {
        return Path.Combine(FolderPath, mapName + ".map");
    }

    private static bool TryLoadEmbeddedBinary(string mapName, out RandomizationUtils.CachedPathMapFileData data)
    {
        data = null;

        string resourceName = ResourcePrefix + mapName + ".map";
        Assembly assembly = Assembly.GetExecutingAssembly();

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Debug.LogWarning("[PatchMapFile] Embedded resource not found: " + resourceName);
            return false;
        }

        data = ReadFromBinaryStream(stream);
        return true;
    }

    private static void WriteToBinaryFile(string path, RandomizationUtils.CachedPathMapFileData data)
    {
        using FileStream file = File.Create(path);
        using BinaryWriter writer = new(file);

        writer.Write(BinaryMagic);

        writer.Write(data.MapName ?? "");
        writer.Write(data.CellSize);
        writer.Write(data.BoundsMin.x);
        writer.Write(data.BoundsMin.y);

        writer.Write(data.Nodes.Count);
        foreach (var node in data.Nodes)
        {
            writer.Write(node.Cell.x);
            writer.Write(node.Cell.y);
            writer.Write(node.Position.x);
            writer.Write(node.Position.y);
        }

        writer.Write(data.Links.Count);
        foreach (var link in data.Links)
        {
            writer.Write(link.From.x);
            writer.Write(link.From.y);
            writer.Write(link.To.x);
            writer.Write(link.To.y);
        }

        writer.Write(data.DebugCells.Count);
        foreach (var debug in data.DebugCells)
        {
            writer.Write((byte)debug.State);
            writer.Write(debug.Position.x);
            writer.Write(debug.Position.y);
        }
    }

    private static RandomizationUtils.CachedPathMapFileData ReadFromBinaryFile(string path)
    {
        using FileStream file = File.OpenRead(path);
        return ReadFromBinaryStream(file);
    }

    private static RandomizationUtils.CachedPathMapFileData ReadFromBinaryStream(Stream stream)
    {
        using BinaryReader reader = new(stream);

        string magic = reader.ReadString();
        if (magic != BinaryMagic)
            throw new Exception("Wrong path map format: " + magic);

        RandomizationUtils.CachedPathMapFileData data = new();

        data.MapName = reader.ReadString();
        data.CellSize = reader.ReadSingle();
        data.BoundsMin = new Vector2(reader.ReadSingle(), reader.ReadSingle());

        int nodeCount = reader.ReadInt32();
        data.Nodes = new List<RandomizationUtils.CachedPathNode>(nodeCount);

        for (int i = 0; i < nodeCount; i++)
        {
            Vector2Int cell = new(reader.ReadInt32(), reader.ReadInt32());
            Vector2 position = new(reader.ReadSingle(), reader.ReadSingle());

            data.Nodes.Add(new RandomizationUtils.CachedPathNode(cell, position));
        }

        int linkCount = reader.ReadInt32();
        data.Links = new List<RandomizationUtils.CachedPathLink>(linkCount);

        for (int i = 0; i < linkCount; i++)
        {
            Vector2Int from = new(reader.ReadInt32(), reader.ReadInt32());
            Vector2Int to = new(reader.ReadInt32(), reader.ReadInt32());

            data.Links.Add(new RandomizationUtils.CachedPathLink(from, to));
        }

        int debugCount = reader.ReadInt32();
        data.DebugCells = new List<RandomizationUtils.SpawnDebugCell>(debugCount);

        for (int i = 0; i < debugCount; i++)
        {
            var state = (RandomizationUtils.SpawnCellState)reader.ReadByte();
            Vector2 position = new(reader.ReadSingle(), reader.ReadSingle());

            data.DebugCells.Add(new RandomizationUtils.SpawnDebugCell(position, state));
        }

        return data;
    }
}