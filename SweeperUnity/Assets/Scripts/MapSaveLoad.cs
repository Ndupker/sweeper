using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public enum MapDimentionsBombs
{
    _Unknown,
    _12x22_99,
    _6x6x6_20,
    _4x4x4_6
}
public class MapSaveLoad : MonoBehaviour
{
    public class MapData
    {
        public string name;
        public uint width;
        public uint height;
        public uint depth;
        public Vector3[] initialCells;
        public uint mineCount;
        public bool[,,] isMineList;
    }
    public Map _map;

    public MapData[] _loadedMaps;

    public bool _save = false;
    public bool _load = false;

    public string SaveMap()
    {
        MapType type = _map._mapType;
        List<byte> data = new List<byte>();

        data.Add((byte)_map.CreatedWidth);
        data.Add((byte)_map.CreatedHeight);
        if(type == MapType._3D)
            data.Add((byte)_map.CreatedDepth);
        data.Add((byte)_map.InitialUncoveredCells.Count);
        for (int i = 0; i < _map.InitialUncoveredCells.Count; i++)
        {
            data.Add((byte)_map.InitialUncoveredCells[i]._worldPoint.x);
            data.Add((byte)_map.InitialUncoveredCells[i]._worldPoint.y);
            if (type == MapType._3D)
                data.Add((byte)_map.InitialUncoveredCells[i]._worldPoint.z);
        }
        int id = 0;
        int mineCount = 0;
        BitArray bits = new BitArray(8);
        byte[] b = new byte[1];
        for (int i = 0; i < _map.CreatedWidth; i++)
        {
            for (int j = 0; j < _map.CreatedHeight; j++)
            {
                for (int k = 0; k < _map.CreatedDepth; k++)
                {
                    bits[id] = _map.Cells[i, j, k]._isMine;
                    if (_map.Cells[i, j, k]._isMine)
                        mineCount++;
                    id++;
                    if (id == 8)
                    {
                        bits.CopyTo(b, 0);
                        data.Add(b[0]);
                        id = 0;
                    }
                }
            }
        }
        if (mineCount != _map.CreatedMineCount)
        {
            Debug.LogError("Mine counts not equal? Cancelling save.");
            return null;
        }
        if (id > 0)
        {
            bits.CopyTo(b, 0);
            data.Add(b[0]);
        }
        string mapFolder = _map._mapType == MapType._2D ? _map.CreatedWidth + "x" + _map.CreatedHeight + "-" + mineCount : _map.CreatedWidth + "x" + _map.CreatedHeight + "x" + _map.CreatedDepth + "-" + mineCount;

        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");
        string parentFolder = _map._mapType == MapType._2D ? "2D" : "3D";

        if (!Directory.Exists("Assets/Resources/" + parentFolder))
            Directory.CreateDirectory("Assets/Resources/" + parentFolder);
        if (!Directory.Exists("Assets/Resources/" + parentFolder  + "/ " + mapFolder))
            Directory.CreateDirectory("Assets/Resources/" + parentFolder + "/" + mapFolder);

        string fileName = DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00")
            + "-" + DateTime.Now.Second.ToString("00") + "_" + DateTime.Now.Day.ToString("00")
            + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Year;
        using (BinaryWriter writer = new BinaryWriter(File.Open("Assets/Resources/" + parentFolder + "/" + mapFolder + "/" + fileName + ".bytes", FileMode.Create)))
        {
            for (int i = 0; i < data.Count; i++)
            {
                writer.Write(data[i]);
            }
            writer.Close();
        }
        data.Clear();
        return fileName;
    }

    public void LoadAll(MapType mapType, MapDimentionsBombs mapDimentions)
    {
        string location = "";
        switch(mapType)
        {
            case MapType._2D:
                location = "2D";
                break;
            case MapType._3D:
                location = "3D";
                break;
        }
        switch (mapDimentions)
        {
            case MapDimentionsBombs._12x22_99:
                location += "/12x22-99";
                break;
            case MapDimentionsBombs._6x6x6_20:
                location += "/6x6x6-20";
                break;
            case MapDimentionsBombs._4x4x4_6:
                location += "/4x4x4-6";
                break;
        }

        TextAsset[] texts = Resources.LoadAll<TextAsset>(location);
        _loadedMaps = new MapData[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            _loadedMaps[i] = LoadAndStoreFromBytes(mapType, texts[i]);
        }
        _map.Load(mapType, mapDimentions, _loadedMaps[UnityEngine.Random.Range(0, _loadedMaps.Length)]);
    }

    MapData LoadAndStoreFromBytes(MapType type, TextAsset text)
    {
        MapData mapData = new MapData();
        mapData.name = text.name;
        byte[] bytes = text.bytes;
        mapData.width = bytes[0];
        mapData.height = bytes[1];
        int ID = 2;
        if (type == MapType._3D)
        {
            mapData.depth = bytes[ID];
            ID++;
        }
        else
            mapData.depth = 1;
        int initialCellCount = bytes[ID];
        mapData.initialCells = new Vector3[initialCellCount];
        ID++;
        for (int i = 0; i < initialCellCount; i++)
        {
            mapData.initialCells[i].x = bytes[ID];
            mapData.initialCells[i].y = bytes[ID + 1];
            if (type == MapType._3D)
            {
                mapData.initialCells[i].z = bytes[ID + 2];
                ID += 3;
            }
            else
                ID += 2;
        }
        mapData.mineCount = 0;
        mapData.isMineList = new bool[mapData.width, mapData.height, mapData.depth];
        BitArray bits = new BitArray(bytes);
        int bitID = ID * 8;
        for (int i = 0; i < mapData.width; i++)
        {
            for (int j = 0; j < mapData.height; j++)
            {
                for (int k = 0; k < mapData.depth; k++)
                {
                    mapData.isMineList[i, j, k] = bits[bitID];
                    if (mapData.isMineList[i, j, k])
                        mapData.mineCount++;
                    bitID++;
                }
            }
        }
        return mapData;
    }

    
    private void Update()
    {
        if(_save)
        {
            _save = false;
            SaveMap();
            _map._recreateBoard = true;
        }
        if (_load)
        {
            _load = false;
            LoadAll(MapType._2D, MapDimentionsBombs._12x22_99);
            //LoadMap("Assets/Resources/" + containedFolderToLoad + "/" + fileNameToLoad);
        }
    }

    #region LoadWithBinReader
    /*public*/ string containedFolderToLoad;
    /*public*/ string fileNameToLoad;
    private void LoadMapBinaryReader(string fileName)
    {
        if (File.Exists(fileName))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                uint width = reader.ReadByte();
                uint height = reader.ReadByte();
                uint depth = 1;
                int initialCellCount = reader.ReadByte();
                Vector3[] initialCells = new Vector3[initialCellCount];
                for (int i = 0; i < initialCellCount; i++)
                {
                    initialCells[i].x = reader.ReadByte();
                    initialCells[i].y = reader.ReadByte();
                }
                uint mineCount = 0;
                bool[,,] isMineList = new bool[width, height, depth];
                BitArray bits = new BitArray(reader.ReadBytes((int)(width * height)));
                int bitID = 0;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        for (int k = 0; k < depth; k++)
                        {
                            isMineList[i, j, k] = bits[bitID];
                            if (isMineList[i, j, k])
                                mineCount++;
                            bitID++;
                            /*if(bitID == 8)
                            {
                                ID++;
                                bits = new BitArray(new byte[] { bytes[ID] });
                            }*/
                        }
                    }
                }
                _map.Load(MapType._Unknown, MapDimentionsBombs._Unknown, width, height, 1, initialCells, mineCount, isMineList);
                reader.Close();
            }
        }
        else
            Debug.LogError("Binary file does not exist in ReadBinaryFile()!");
    }
    #endregion
}
