using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Cell
{
    public bool _uncovered = false;
    public bool _isMine = false;
    public bool _marked = false;
    public bool _selected = false;
    public bool _positioned = false;
    public int _numberOfAdjMines = 0;
    public GameObject _model;
    public TMPro.TextMeshPro _text;
    public CellComponent _cellComp;
    public Vector3 _worldPoint;

    [System.NonSerialized]
    public List<Cell> _borderConnections;

    public bool _fakeMarked = false;
    public bool _fakeUncovered = false;
    public bool AIMarked
    {
        get { return (_marked || _fakeMarked); }
    }
    public bool AIUncovered
    {
        get { return (_uncovered || _fakeUncovered); }
    }
}
public enum MapType
{
    _Unknown,
    _2D,
    _3D
}

public class Map : MonoBehaviour, IMineMenu
{
    public class intTrio
    {
        public intTrio(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        public int _x;
        public int _y;
        public int _z;
    }

    intTrio[] _nearbyCellArray2D = null;
    intTrio[] _nearbyCellArray3D = null;
    void MakeCellArrays()
    {
        _nearbyCellArray2D = new intTrio[8];
        _nearbyCellArray2D[0] = new intTrio(-1, -1, 0);
        _nearbyCellArray2D[1] = new intTrio(0, -1, 0);
        _nearbyCellArray2D[2] = new intTrio(+1, -1, 0);
        _nearbyCellArray2D[3] = new intTrio(-1, 0, 0);
        _nearbyCellArray2D[4] = new intTrio(+1, 0, 0);
        _nearbyCellArray2D[5] = new intTrio(-1, +1, 0);
        _nearbyCellArray2D[6] = new intTrio(0, +1, 0);
        _nearbyCellArray2D[7] = new intTrio(+1, +1, 0);

        _nearbyCellArray3D = new intTrio[26];
        _nearbyCellArray3D[0] = new intTrio(-1, -1, 0);
        _nearbyCellArray3D[1] = new intTrio(0, -1, 0);
        _nearbyCellArray3D[2] = new intTrio(+1, -1, 0);
        _nearbyCellArray3D[3] = new intTrio(-1, 0, 0);
        _nearbyCellArray3D[4] = new intTrio(+1, 0, 0);
        _nearbyCellArray3D[5] = new intTrio(-1, +1, 0);
        _nearbyCellArray3D[6] = new intTrio(0, +1, 0);
        _nearbyCellArray3D[7] = new intTrio(+1, +1, 0);

        _nearbyCellArray3D[8] = new intTrio(0, 0, -1);
        _nearbyCellArray3D[9] = new intTrio(-1, -1, -1);
        _nearbyCellArray3D[10] = new intTrio(0, -1, -1);
        _nearbyCellArray3D[11] = new intTrio(+1, -1, -1);
        _nearbyCellArray3D[12] = new intTrio(-1, 0, -1);
        _nearbyCellArray3D[13] = new intTrio(+1, 0, -1);
        _nearbyCellArray3D[14] = new intTrio(-1, +1, -1);
        _nearbyCellArray3D[15] = new intTrio(0, +1, -1);
        _nearbyCellArray3D[16] = new intTrio(+1, +1, -1);

        _nearbyCellArray3D[17] = new intTrio(0, 0, +1);
        _nearbyCellArray3D[18] = new intTrio(-1, -1, +1);
        _nearbyCellArray3D[19] = new intTrio(0, -1, +1);
        _nearbyCellArray3D[20] = new intTrio(+1, -1, +1);
        _nearbyCellArray3D[21] = new intTrio(-1, 0, +1);
        _nearbyCellArray3D[22] = new intTrio(+1, 0, +1);
        _nearbyCellArray3D[23] = new intTrio(-1, +1, +1);
        _nearbyCellArray3D[24] = new intTrio(0, +1, +1);
        _nearbyCellArray3D[25] = new intTrio(+1, +1, +1);

        _nearbyCellArrayVector2D = new Vector3[_nearbyCellArray2D.Length];
        for (int i = 0; i < _nearbyCellArray2D.Length; i++)
        {
            _nearbyCellArrayVector2D[i] = new Vector3(_nearbyCellArray2D[i]._x, _nearbyCellArray2D[i]._y, _nearbyCellArray2D[i]._z);
        }

        _nearbyCellArrayVector3D = new Vector3[_nearbyCellArray3D.Length];
        for (int i = 0; i < _nearbyCellArray3D.Length; i++)
        {
            _nearbyCellArrayVector3D[i] = new Vector3(_nearbyCellArray3D[i]._x, _nearbyCellArray3D[i]._y, _nearbyCellArray3D[i]._z);
        }
    }

    public intTrio[] NearbyCellArray
    {
        get
        {
            if ( _nearbyCellArray2D == null)
                MakeCellArrays();
            if(_mapType == MapType._2D)
                return _nearbyCellArray2D;
            else
                return _nearbyCellArray3D;
        }
    }
    Vector3[] _nearbyCellArrayVector2D = null;
    Vector3[] _nearbyCellArrayVector3D = null;
    public Vector3[] NearbyCellArrayVector
    {
        get
        {
            if (_nearbyCellArray2D == null)
                MakeCellArrays();
            if (_mapType == MapType._2D)
                return _nearbyCellArrayVector2D;
            else
                return _nearbyCellArrayVector3D;
        }
    }

    public GameManager _gameManager;
    public MapType _mapType;
    public MapDimentionsBombs _dimentions;
    public uint _width;
    public uint _height;
    public uint _depth;
    public uint _mineCount;

    public uint CreatedWidth
    {
        get { return _createdWidth; }
    }
    private uint _createdWidth = 0;
    public uint CreatedHeight
    {
        get { return _createdHeight; }
    }
    private uint _createdHeight = 0;
    public uint CreatedDepth
    {
        get { return _createdDepth; }
    }
    private uint _createdDepth = 0;
    public uint CreatedMineCount
    {
        get { return _createdMineCount; }
    }
    private uint _createdMineCount = 0;

    public MapSaveLoad _mapSaveLoad;
    public Camera _camera;
    public GameObject _2DCellPrefab;
    public GameObject _3DCellPrefab;
    public Material _gray;
    public Material _lighterGray;
    public Material _red;
    public Material _magenta;
    public Material _yellow;
    public Material _blue;
    public Material _black;
    public Material _cyan;
    public Material _green;
    public Material _3DGray;
    public Material _3DYellow;
    public Material _3DSelectedGray;
    public Material _3DSelectedYellow;
    public Text _mineCountUI;
    public TMPro.TextMeshPro _vrMineCountUI;
    public GameObject _gameMenu;
    public ControllerCollider _leftVRController;
    public ControllerCollider _rightVRController;
    public ViewManager _viewManager;

    public GameObject _leftRaycastObj;
    public GameObject _rightRaycastObj;

    public Color[] _cellColours;

    public bool _canDie = true;

    public SolverAI _solverAI = null;

    GameObject _mapParent;
    GameObject _centerPoint;

    bool _allowVisualChange = true;

    int _recordCount = 0;
    float _lastGenerateTime = 0;
    public int _maxBorder = 0;
    public double _lastGenerationDuration = 0;
    bool _newData = true;
    //List<int> cachedRandoms = new List<int>();
    Vector2 _startPos;
    public bool _menuActive = false;
    List<double> _genTimes = new List<double>();

    public double _maxGenerationTime = 60;

    public uint _minesLeftToFind = 0;

    //3D
    private bool _rotating = false;

    bool _hasStoredControllerPosLeft = false;
    bool _hasStoredControllerPosRight = false;
    Vector3 _storedControllerPosLeft;
    Vector3 _storedControllerPosRight;

    public uint MinesLeftToFind
    {
        get { return _minesLeftToFind; }
        set { _minesLeftToFind = value;
            _mineCountUI.text = value.ToString("00");
            if(_vrMineCountUI != null)
                _vrMineCountUI.text = value.ToString("00");
        }
    }

    [System.NonSerialized]
    public bool _generationFailed = false;

    [System.NonSerialized]
    public DateTime _startTime;

    public int _uiSpacing = 20;

    public Cell[,,] Cells
    {
        get {return _cells; }
    }
    Cell[,,] _cells;
    public bool Generated
    {
        get { return _generated; }
    }
    bool _generated = false;

    public bool _recreateBoard = false;

    public bool _markAllMines = false;

    public bool _uncoverAllSpaces = false;

    public bool _startSaveLoop = false;

    public List<Cell> MarkedCells { get; private set; }
    public List<Cell> UncoveredCells { get; private set; }
    public List<List<Cell>> NearUnsolvedCells { get; private set; }
    public List<Cell> InitialUncoveredCells { get; private set; }
    public void Hide()
    {
        _menuActive = false;

        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _gameMenu.gameObject.SetActive(false);
        else
        {
            if(_vrMineCountUI != null)
                _vrMineCountUI.enabled = false;
            if(_leftRaycastObj != null)
                _leftRaycastObj.SetActive(true);
            if (_rightRaycastObj != null)
                _rightRaycastObj.SetActive(true);
            if(_leftVRController != null)
                _leftVRController.SetMapInteraction(false);
            if (_rightVRController != null)
                _rightVRController.SetMapInteraction(false);
        }
    }

    public void Show()
    {
        _menuActive = true;
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _gameMenu.gameObject.SetActive(true);
        else
        {
            if (_vrMineCountUI != null)
                _vrMineCountUI.enabled = true;
            if (_leftRaycastObj != null)
                _leftRaycastObj.SetActive(false);
            if (_rightRaycastObj != null)
                _rightRaycastObj.SetActive(false);
            if (_leftVRController != null)
                _leftVRController.SetMapInteraction(true);
            if (_rightVRController != null)
                _rightVRController.SetMapInteraction(true);
        }
    }

    public void HideMap()
    {
        DestroyCells();
        _cells = null;
    }

    private void Start()
    {
        //Hide();
    }

    public void CreateCells()
    {
        _gameMenu.gameObject.SetActive(true);

        if (_width > 100)
            _width = 100;
        else if (_width == 0)
            _width = 1;

        if (_height > 100)
            _height = 100;
        else if (_height == 0)
            _height = 1;

        if (_depth > 100)
            _depth = 100;
        else if (_depth == 0)
            _depth = 1;

        _maxBorder = 0;
        MinesLeftToFind = 0;
        MarkedCells = new List<Cell>();
        UncoveredCells = new List<Cell>();
        NearUnsolvedCells = new List<List<Cell>>();
        DestroyCells();
        _generated = false;
        _createdWidth = _width;
        _createdHeight = _height;
        _createdDepth = _depth;
        _createdMineCount = _mineCount;
        _cells = new Cell[_createdWidth, _createdHeight, _createdDepth];
        for(int i = 0; i < _createdWidth; i++)
        {
            for (int j = 0; j < _createdHeight; j++)
            {
                for (int k = 0; k < _createdDepth; k++)
                {
                    _cells[i, j, k] = new Cell();
                    _cells[i, j, k]._uncovered = false;
                    _cells[i, j, k]._isMine = false;
                    _cells[i, j, k]._numberOfAdjMines = 0;
                    _cells[i, j, k]._worldPoint = new Vector3(i, j, k);
                    _cells[i, j, k]._borderConnections = new List<Cell>();
                }
            }
        }


        _camera.transform.position = new Vector3(_createdWidth / 2f, _createdHeight / 2f, -10f);
        

        Visualise();
    }
    void DestroyCells()
    {
        if (_cells == null)
            return;
        for (int i = 0; i < _createdWidth; i++)
        {
            for (int j = 0; j < _createdHeight; j++)
            {
                for (int k = 0; k < _createdDepth; k++)
                {
                    if (_cells[i, j, k]._model != null)
                        Destroy(_cells[i, j, k]._model);
                }
            }
        }
    }
    public void Generate(Vector3 clearSpot)
    {
        _generated = true;
        AddMines(clearSpot);
        if (_generationFailed)
        {
            _generationFailed = false;
            _generated = false;
            CreateCells();
            return;
        }
        MinesLeftToFind = _createdMineCount;
        UpdateAdjMineNumbers();
        Visualise();
        Uncover(clearSpot, true);
        InitialUncoveredCells = new List<Cell>(UncoveredCells);
        /*for(int j = 0; j < NearUnsolvedCells[0].Count; j++)
        {
            if(_solverAI._endCells.Contains(NearUnsolvedCells[0][j]))
                NearUnsolvedCells[0][j]._model.GetComponent<Renderer>().sharedMaterial = _magenta;
            else if(NearUnsolvedCells[0][j]._yellow)
                NearUnsolvedCells[0][j]._model.GetComponent<Renderer>().sharedMaterial = _yellow;
            else
                NearUnsolvedCells[0][j]._model.GetComponent<Renderer>().sharedMaterial = _red;
        }*/

        
    }
    public void Load(MapType mapType, MapDimentionsBombs mapDimentions, MapSaveLoad.MapData mapdata)
    {
        Debug.Log("Loading map: " + mapdata.name);
        Load(mapType, mapDimentions, mapdata.width, mapdata.height, mapdata.depth, mapdata.initialCells, mapdata.mineCount, mapdata.isMineList);
    }
    public void Load(MapType mapType, MapDimentionsBombs mapDimentions, uint width, uint height, uint depth, Vector3[] initialCells, uint mineCount, bool[,,] isMineList)
    {
        _mapType = mapType;
        _dimentions = mapDimentions;
        _width = width;
        _height = height;
        _depth = depth;
        _mineCount = mineCount;
        CreateCells();
        _generated = true;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    Cells[i, j, k]._isMine = isMineList[i, j, k];
                }
            }
        }
        _allowVisualChange = true;
        UpdateAdjMineNumbers();
        Visualise();
        for (int i = 0; i < initialCells.Length; i++)
        {
            Uncover(initialCells[i], true);
        }
        InitialUncoveredCells = new List<Cell>(UncoveredCells);
        MinesLeftToFind = _mineCount;
    }

    void AddMines(Vector3 clearSpot)
    {
        List<Cell> possibleMines = new List<Cell>();
        for (uint i = 0; i < _createdWidth; i++)
        {
            for (uint j = 0; j < _createdHeight; j++)
            {
                for (uint k = 0; k < _createdDepth; k++)
                {
                    if (((i == clearSpot.x) || (i == (clearSpot.x - 1)) || (i == (clearSpot.x + 1))
                        ) &&
                        ((j == clearSpot.y) || (j == (clearSpot.y - 1)) || (j == (clearSpot.y + 1))
                        ) &&
                        ((k == clearSpot.z) || (k == (clearSpot.z - 1)) || (k == (clearSpot.z + 1))
                        ))
                    {
                        //_cells[x, y]._uncovered = true;
                        //do this after
                    }
                    else
                        possibleMines.Add(_cells[i, j, k]);
                }
            }
        }
        bool delayedSolve = _solverAI._delayedSolve;
        _solverAI._delayedSolve = false;
        _allowVisualChange = false;
        _startTime = DateTime.Now;
        int mineCount = 0;
        int loopCount = -1;
        //if (_newData)
        //    cachedRandoms.Clear();
        int logNum = 4;

        for (uint i = 0; i < _createdMineCount; i++)
        {
            loopCount++;
            if (possibleMines.Count == 0)
            {
                Debug.LogError("Could only create " + mineCount + " mines.");
                break;
            }
            /*if (!_newData)
            {
                if (loopCount >= cachedRandoms.Count)
                {
                    //Debug.LogError("loopCount: " + loopCount + " cachedRandoms.Count: " + cachedRandoms.Count);
                    break;
                }
                else if (cachedRandoms[loopCount] >= possibleMines.Count)
                {
                    //Debug.LogError("cachedRandoms[loopCount]: " + cachedRandoms[loopCount] + " possibleMines.Count: " + possibleMines.Count);
                    break;
                }
            }*/
            int id = UnityEngine.Random.Range(0, possibleMines.Count);//_newData ? UnityEngine.Random.Range(0, possibleMines.Count) : cachedRandoms[loopCount];
            //if (_newData)
            //    cachedRandoms.Add(id);
            Cell randomCell = possibleMines[id];
            //uint x = (uint)randomCell._worldPoint.x;//possibleMines[id] % _createdWidth;
            //uint y = (uint)randomCell._worldPoint.y;//(uint)Mathf.FloorToInt((float)possibleMines[id] / (float)_createdWidth);
            //uint z = (uint)randomCell._worldPoint.z;//(uint)0;

            randomCell._isMine = true;
            //_cells[x, y, z]._isMine = true;
            possibleMines.RemoveAt(id);

            UpdateAdjMineNumbers();
            Uncover(clearSpot, true);
            _solverAI.Solve(i + 1);

            /*if (_solverAI._endCells != null && _solverAI._endCells.Count > 0)
            {
                Debug.LogError("endCells " + _solverAI._endCells.Count);
                _allowVisualChange = true;
                for (int j = 0; j < _createdWidth; j++)
                {
                    for (int k = 0; k < _createdHeight; k++)
                    {
                        if (_cells[j, k]._isMarked)
                        {
                            ToggleMark(_cells[j, k]);
                            ToggleMark(_cells[j, k]);
                        }
                        if (_cells[j, k]._uncovered)
                            Uncover(_cells[j, k]._worldPoint, true);
                    }
                }
                for (int j = 0; j < _solverAI._endCells.Count; j++)
                    _solverAI._endCells[j]._model.GetComponent<Renderer>().sharedMaterial = _red;
                _solverAI._solve = false;
                break;
            }*/
            if (!_solverAI._hasSolved)
            {
                randomCell._isMine = false;
                //_cells[x, y, z]._isMine = false;
                i--;
            }
            else
                mineCount++;



            if (UncoveredCells.Count == (Cells.Length - mineCount) && MarkedCells.Count < mineCount)
                logNum = 1;
            else if (MarkedCells.Count == mineCount && UncoveredCells.Count < (Cells.Length - mineCount))
                logNum = 2;
            else if (MarkedCells.Count == mineCount && UncoveredCells.Count == (Cells.Length - mineCount))
                logNum = 3;
            else
                logNum = 4;

            UncoveredCells.Clear();
            MarkedCells.Clear();
            NearUnsolvedCells.Clear();
            MinesLeftToFind = _createdMineCount;
            for (int j = 0; j < _createdWidth; j++)
            {
                for (int k = 0; k < _createdHeight; k++)
                {
                    for (int l = 0; l < _createdDepth; l++)
                    {
                        _cells[j, k, l]._marked = false;
                        _cells[j, k, l]._uncovered = false;
                    }
                }
            }
            if (_generationFailed)
                break;
        }
        switch (logNum)
        {
            case 1: 
                Debug.LogError("1 Found all uncovered cells! but not all mines.");
                break;
            case 2:
                Debug.LogError("2 Found all mines! but not all uncovered cells.");
                break;
            case 3:
                Debug.LogError("3 Found all mines and uncovered cells.");
                break;
            case 4:
                Debug.LogError("4 Nope");
                break;
        }

        if (_generationFailed)
        {
            Debug.LogError("Generation Failed, went over second limit " + _maxGenerationTime + "s.");
            return;
        }
        Debug.LogError("Amount of placed mines: " + mineCount);
        TimeSpan span = DateTime.Now - _startTime;
        _lastGenerationDuration = span.TotalSeconds;
        Debug.LogError("Mines Took: " + span.TotalSeconds + "s with max border cells: " + _maxBorder);
        _genTimes.Add(span.TotalSeconds);
        double average = 0;
        for (int i = 0; i < _genTimes.Count; i++)
        {
            average += _genTimes[i];
        }
        average /= _genTimes.Count;
        Debug.LogError("Current Average: " + average + "s");

        _allowVisualChange = true;
        _solverAI._delayedSolve = delayedSolve;
    }
    void GetAdjacentMinesWithDepthOffset(int i, int j, int k, int kOffset)
    {
        int kWithOffset = k + kOffset;
        if (kWithOffset < 0 || kWithOffset >= _createdDepth)
            return;
        if(kOffset != 0)
        {
            if (_cells[i, j, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (i > 0 && j > 0)
        {
            if (_cells[i - 1, j - 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (j > 0)
        {
            if (_cells[i, j - 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (i < (_createdWidth - 1) && j > 0)
        {
            if (_cells[i + 1, j - 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }

        if (i > 0)
        {
            if (_cells[i - 1, j, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (i < (_createdWidth - 1))
        {
            if (_cells[i + 1, j, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }

        if (i > 0 && j < (_createdHeight - 1))
        {
            if (_cells[i - 1, j + 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (j < (_createdHeight - 1))
        {
            if (_cells[i, j + 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
        if (i < (_createdWidth - 1) && j < (_createdHeight - 1))
        {
            if (_cells[i + 1, j + 1, kWithOffset]._isMine)
                _cells[i, j, k]._numberOfAdjMines++;
        }
    }
    private void UpdateAdjMineNumbers()
    {
        for (int i = 0; i < _createdWidth; i++)
        {
            for (int j = 0; j < _createdHeight; j++)
            {
                for (int k = 0; k < _createdDepth; k++)
                {
                    if (_cells[i, j, k]._isMine)
                        continue;
                    _cells[i, j, k]._numberOfAdjMines = 0;
                    GetAdjacentMinesWithDepthOffset(i, j, k, -1);
                    GetAdjacentMinesWithDepthOffset(i, j, k, 0);
                    GetAdjacentMinesWithDepthOffset(i, j, k, 1);
                }
            }
        }
    }
    void AddIfUnsolved(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= CreatedWidth || y >= CreatedHeight || z >= _createdDepth)
            return;
        Cell cellToAdd = Cells[x, y, z];
        if (!cellToAdd._marked && !cellToAdd._uncovered)
        {
            AddNearUnsolvedCell(cellToAdd);

            /*if (cellToAdd._model != null && _allowVisualChange)
            {
                for (int i = 0; i < NearUnsolvedCells.Count; i++)
                {
                    for (int j = 0; j < NearUnsolvedCells[i].Count; j++)
                    {
                        switch (i)
                        {
                            case 0:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _red;
                                break;
                            case 1:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _magenta;
                                break;
                            case 2:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _yellow;
                                break;
                            case 3:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _blue;
                                break;
                            case 4:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _black;
                                break;
                            case 5:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _cyan;
                                break;
                            case 6:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _green;
                                break;
                            default:
                                NearUnsolvedCells[i][j]._model.GetComponent<Renderer>().sharedMaterial = _red;
                                break;
                        }
                    }
                }
            }*/
        }
    }
    void UpdateNearUnsolvedCells(Cell cell)
    {
        if (cell._numberOfAdjMines == 0)
            return;

        for (int i = 0; i < NearbyCellArray.Length; i++)
            AddIfUnsolved((int)(cell._worldPoint.x + NearbyCellArray[i]._x),
                    (int)(cell._worldPoint.y + NearbyCellArray[i]._y),
                    (int)(cell._worldPoint.z + NearbyCellArray[i]._z));
    }
    public bool Uncover(Vector3 clearSpot, bool KillIfMine)
    {
        if (clearSpot.x < 0 || clearSpot.x > _createdWidth - 1 || clearSpot.y < 0 || clearSpot.y > _createdHeight - 1 || clearSpot.z < 0 || clearSpot.z > _createdDepth - 1)
            return false;
        Cell cell = _cells[(int)clearSpot.x, (int)clearSpot.y, (int)clearSpot.z];
        if (cell._uncovered || cell._marked)
            return false;
        if (cell._isMine)
        {
            if(KillIfMine)
            {
                //BOOM
                if (_allowVisualChange)
                {
                    Debug.Log("BOOM!");
                    Hide();
                    _gameManager.GameLose();
                    if (ViewManager.instance._viewMode == ViewMode.VR)
                    {
                        //StartCoroutine(ControllerCollider.TriggerHaptics(OVRInput.Controller.LTouch, 1f, 1f));
                        //StartCoroutine(ControllerCollider.TriggerHaptics(OVRInput.Controller.RTouch, 1f, 1f));
                        _leftVRController.SetMapInteraction(false);
                        _rightVRController.SetMapInteraction(false);
                    }
                }
            }
            return false;
        }
        cell._uncovered = true;
        UncoveredCells.Add(cell);
        RemoveNearUnsolvedCell(cell);
        UpdateNearUnsolvedCells(cell);
        SetCellTextAndColour(cell);
        CheckForWin();
        if (cell._numberOfAdjMines > 0)
            return true;

        UncoverAroundSpot(clearSpot, false);
        return true;
    }

    void UncoverAroundSpot(Vector3 clearSpot, bool KillIfMine)
    {
        for(int i = 0; i < NearbyCellArrayVector.Length; i++)
            Uncover(clearSpot + NearbyCellArrayVector[i], KillIfMine);
    }

    public void AddNearUnsolvedCell(Cell cell)
    {
        List<int> listsToAdd = new List<int>();
        for (int i = 0; i < NearUnsolvedCells.Count; i++)
        {
            for (int j = 0; j < NearUnsolvedCells[i].Count; j++)
            {
                if (NearUnsolvedCells[i].Contains(cell))
                {
                    return;
                }
                if (Vector2.Distance(NearUnsolvedCells[i][j]._worldPoint, cell._worldPoint) < 1.5f)
                {
                    if(!listsToAdd.Contains(i))
                        listsToAdd.Add(i);
                    if (!NearUnsolvedCells[i][j]._borderConnections.Contains(cell))
                        NearUnsolvedCells[i][j]._borderConnections.Add(cell);
                    if (!cell._borderConnections.Contains(NearUnsolvedCells[i][j]))
                        cell._borderConnections.Add(NearUnsolvedCells[i][j]);
                }
            }
        }
        for (int i = listsToAdd.Count - 1; i >= 0; i--)
        {
            if(i == 0)
            {
                NearUnsolvedCells[listsToAdd[0]].Add(cell);
            }
            else
            {
                NearUnsolvedCells[listsToAdd[0]].AddRange(NearUnsolvedCells[listsToAdd[i]]);
                NearUnsolvedCells.RemoveAt(listsToAdd[i]);
            }
        }
        if(listsToAdd.Count == 0)
        {
            List<Cell> newList = new List<Cell>();
            newList.Add(cell);
            NearUnsolvedCells.Add(newList);
        }
    }

    void RemoveNearUnsolvedCell(Cell cell)
    {
        for (int i = 0; i < NearUnsolvedCells.Count; i++)
        {
            for (int j = 0; j < NearUnsolvedCells[i].Count; j++)
            {
                if(NearUnsolvedCells[i].Contains(cell))
                {
                    NearUnsolvedCells[i].Remove(cell);
                    cell._borderConnections.Clear();
                    if (NearUnsolvedCells[i].Count == 0)
                    {
                        NearUnsolvedCells.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }

    #region Visuals

    //TODO 3D Cell Prefab
    private void Visualise()
    {
        if(_mapParent == null)
        {
            _mapParent = new GameObject("Map");
            _mapParent.transform.position = Vector3.zero;
            _centerPoint = new GameObject("Center");
            _centerPoint.transform.position = new Vector3(_createdWidth/2f, _createdHeight / 2f, _createdDepth / 2f);
            _centerPoint.transform.parent = _mapParent.transform;
        }
        _mapParent.transform.position = Vector3.zero;
        _centerPoint.transform.position = new Vector3(_createdWidth / 2f, _createdHeight / 2f, _createdDepth / 2f);
        for (int i = 0; i < _createdWidth; i++)
        {
            for (int j = 0; j < _createdHeight; j++)
            {
                for (int k = 0; k < _createdDepth; k++)
                {
                    if (_mapType == MapType._2D)
                    {
                        if (_cells[i, j, k]._model == null)
                        {
                            _cells[i, j, k]._model = Instantiate(_2DCellPrefab);
                            _cells[i, j, k]._text = _cells[i, j, k]._model.GetComponentInChildren<TMPro.TextMeshPro>();
                            _cells[i, j, k]._model.transform.parent = _mapParent.transform;
                        }
                        _cells[i, j, k]._model.transform.position = new Vector3(i + 0.5f, j + 0.5f, k);
                        if (_cells[i, j, k]._uncovered)
                        {
                            //_cells[i, j]._model.GetComponent<Renderer>().sharedMaterial = _gray;
                            SetCellTextAndColour(_cells[i, j, k]);
                        }
                        else
                        {
                            _cells[i, j, k]._model.GetComponent<Renderer>().sharedMaterial = _lighterGray;
                        }
                    }
                    else if(_mapType == MapType._3D)
                    {
                        if (_cells[i, j, k]._model == null)
                        {
                            _cells[i, j, k]._model = Instantiate(_3DCellPrefab);
                            _cells[i, j, k]._text = _cells[i, j, k]._model.GetComponentInChildren<TMPro.TextMeshPro>();
                            _cells[i, j, k]._cellComp = _cells[i, j, k]._model.GetComponent<CellComponent>();
                            _cells[i, j, k]._cellComp._pos = new Vector3(i,j,k);
                            _cells[i, j, k]._model.transform.parent = _centerPoint.transform;
                        }
                        if (!_cells[i, j, k]._positioned)
                        {
                            _cells[i, j, k]._positioned = true;
                            _cells[i, j, k]._model.transform.position = new Vector3(i + 0.5f, j + 0.5f, k + 0.5f);
                        }
                        if (_cells[i, j, k]._uncovered)
                        {
                            //_cells[i, j]._model.GetComponent<Renderer>().sharedMaterial = _gray;
                            //SetCellTextAndColour(_cells[i, j, k]);
                        }
                        else
                        {
                            //_cells[i, j, k]._model.GetComponent<Renderer>().sharedMaterial = _lighterGray;
                        }
                    }
                }
            }
        }
    }

    void SetCellTextAndColour(Cell cell)
    {
        //TODO 3D version
        if (!_allowVisualChange)
            return;

        cell._text.text = (cell._isMine ? "M" :
                         (cell._numberOfAdjMines > 0 ? cell._numberOfAdjMines.ToString() : ""));
        if (!cell._isMine && cell._numberOfAdjMines > 0)
            cell._text.color = _cellColours[cell._numberOfAdjMines - 1];

        if (_mapType == MapType._3D)
        {
            cell._model.GetComponent<Renderer>().enabled = false;
            cell._model.GetComponent<Collider>().enabled = false;
        }
        else
        {
            if (cell._model != null)
                cell._model.GetComponent<Renderer>().sharedMaterial = _gray;
        }
    }
    #endregion
    public void ToggleMark(Cell cell)
    {
        if (cell._uncovered)
            return;
        cell._marked = !cell._marked;
        if (cell._text != null && _allowVisualChange)
            cell._text.text = cell._marked ? "M" : "";
        if (cell._marked)
        {
            if (!MarkedCells.Contains(cell))
                MarkedCells.Add(cell);
            RemoveNearUnsolvedCell(cell);
            if (_mapType == MapType._2D)
            {
                if (cell._model != null && _allowVisualChange)
                    cell._model.GetComponent<Renderer>().sharedMaterial = _lighterGray;
            }
            else
            {
                Set3DVisuals(cell);
            }
            MinesLeftToFind--;
        }
        else
        {
            if (MarkedCells.Contains(cell))
                MarkedCells.Remove(cell);
            AddNearUnsolvedCell(cell);
            MinesLeftToFind++;
            if (_mapType == MapType._2D)
            {
            }
            else
            {
                Set3DVisuals(cell);
            }
        }
        CheckForWin();
    }
    bool MarksMatchMineCount(Vector3 worldPoint)
    {
        return MarksMatchMineCountWithDepthOffset(worldPoint, -1) &&
            MarksMatchMineCountWithDepthOffset(worldPoint, 0) &&
            MarksMatchMineCountWithDepthOffset(worldPoint, +1);
    }
    bool MarksMatchMineCountWithDepthOffset(Vector3 worldPoint, int zOffset)
    {
        int x = (int)worldPoint.x;
        int y = (int)worldPoint.y;
        int z = (int)worldPoint.z;
        int zWithOffset = (int)worldPoint.z + zOffset;
        if (zWithOffset < 0 || zWithOffset >= _createdDepth)
            return true;
        int marks = 0;
        if(zOffset != 0)
        {
            if (_cells[x, y, zWithOffset]._marked)
                marks++;
        }
        if (x > 0 && y > 0)
        {
            if (_cells[x - 1, y - 1, zWithOffset]._marked)
                marks++;
        }
        if (y > 0)
        {
            if (_cells[x, y - 1, zWithOffset]._marked)
                marks++;
        }
        if (x < (_createdWidth - 1) && y > 0)
        {
            if (_cells[x + 1, y - 1, zWithOffset]._marked)
                marks++;
        }

        if (x > 0)
        {
            if (_cells[x - 1, y, zWithOffset]._marked)
                marks++;
        }
        if (x < (_createdWidth - 1))
        {
            if (_cells[x + 1, y, zWithOffset]._marked)
                marks++;
        }

        if (x > 0 && y < (_createdHeight - 1))
        {
            if (_cells[x - 1, y + 1, zWithOffset]._marked)
                marks++;
        }
        if (y < (_createdHeight - 1))
        {
            if (_cells[x, y + 1, zWithOffset]._marked)
                marks++;
        }
        if (x < (_createdWidth - 1) && y < (_createdHeight - 1))
        {
            if (_cells[x + 1, y + 1, zWithOffset]._marked)
                marks++;
        }
        return (marks == _cells[x, y, z]._numberOfAdjMines);

    }

    bool GetWorldPoint(out Vector3 worldPoint)
    {
        worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (worldPoint.x >= 0 && worldPoint.x < _createdWidth && worldPoint.y >= 0 && worldPoint.y < _createdHeight)
        {
            worldPoint.x = Mathf.FloorToInt(worldPoint.x);
            worldPoint.y = Mathf.FloorToInt(worldPoint.y);
            worldPoint.z = 0;
            return true;
        }
        return false;
    }
    bool _wasDown = false;
    float _timeDown = 0f;
    bool _inSaveLoop = false;
    Vector3 _initialPos = Vector3.zero;
    IEnumerator RepeatedlySaveMaps()
    {
        if (_inSaveLoop)
            yield break;
        _inSaveLoop = true;
        while (true)
        {
            while (!_generated)
            {
                _startPos = new Vector2(UnityEngine.Random.Range(0, CreatedWidth - 1), UnityEngine.Random.Range(0, CreatedHeight - 1));
                Generate(_startPos);
                yield return new WaitForSeconds(1f);
            }
            string logFileName = DateTime.Now.Day.ToString("00")
            + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Year + ".txt";
            string existingData = "";
            if (File.Exists("Assets/Logs/" + logFileName))
            {
                existingData = System.IO.File.ReadAllText("Assets/Logs/" + logFileName);
            }
            string name = _mapSaveLoad.SaveMap();
            if(name != null)
            {
                existingData += Environment.NewLine;
                existingData += "Name: " + name + Environment.NewLine;
                existingData += "    Max Border Cells: " + _maxBorder + Environment.NewLine;
                existingData += "    Duration: " + _lastGenerationDuration;
                System.IO.File.WriteAllText("Assets/Logs/" + logFileName, existingData);
            }
            yield return null;
            CreateCells();
            yield return new WaitForSeconds(1f);
        }
        //_inSaveLoop = false;
    }

    void CheckForWin()
    {
        if (_cells == null || !_allowVisualChange)
            return;
        for (int j = 0; j < _createdWidth; j++)
        {
            for (int k = 0; k < _createdHeight; k++)
            {
                for (int l = 0; l < _createdDepth; l++)
                {
                    if (_cells[j, k, l]._isMine && !_cells[j, k, l]._marked)
                        return;
                    if (!_cells[j, k, l]._isMine && !_cells[j, k, l]._uncovered)
                        return;
                }
            }
        }

        //WIN!
        Hide();
        _gameManager.GameWin();
        if (ViewManager.instance._viewMode == ViewMode.VR)
        {
            _leftVRController.SetMapInteraction(false);
            _rightVRController.SetMapInteraction(false);
        }
    }

    void Set3DVisuals(Cell cell)
    {
        if (cell._model == null || !_allowVisualChange)
            return;
        if (cell._marked)
            cell._model.GetComponent<Renderer>().sharedMaterial = cell._selected ? _3DSelectedYellow : _3DYellow;
        else
            cell._model.GetComponent<Renderer>().sharedMaterial = cell._selected ? _3DSelectedGray : _3DGray;
    }

    public void SetSelected(bool selected, Vector3 pos)
    {
        if (pos.x < 0 || pos.x > _createdWidth - 1 || pos.y < 0 || pos.y > _createdHeight - 1 || pos.z < 0 || pos.z > _createdDepth - 1)
            return;
        Cell cell = _cells[(int)pos.x, (int)pos.y, (int)pos.z];
        cell._selected = selected;
        Set3DVisuals(cell);
    }

    int Width
    {
        get { return Screen.width - _uiSpacing; }
    }

    //TODO 3D Camera controls and initial positioning
    private void Update()
    {
        //_camera.orthographicSize = 11;
        if (_mapType == MapType._2D && _createdHeight > 0 && _createdWidth > 0)
        {
            if (!_camera.orthographic)
                _camera.orthographic = true;
            if (Width * _createdHeight >= Screen.height * _createdWidth)
                _camera.orthographicSize = ((float)_createdHeight / 2f);
            else
                _camera.orthographicSize = ((float)Screen.height / (float)(Width)) * ((float)_createdWidth / 2f);
        }
        if (_mapType == MapType._3D)// && _createdHeight > 0 && _createdWidth > 0)
        {
            if(_camera.orthographic)
                _camera.orthographic = false;
        }

        if (_recreateBoard)
        {
            _recreateBoard = false;
            CreateCells();
            return;
        }
        if (_markAllMines)
        {
            _markAllMines = false;

            for (int j = 0; j < _createdWidth; j++)
            {
                for (int k = 0; k < _createdHeight; k++)
                {
                    for (int l = 0; l < _createdDepth; l++)
                    {
                        if (_cells[j, k, l]._isMine && !_cells[j, k, l]._marked)
                            ToggleMark(_cells[j, k, l]);
                    }
                }
            }
            return;
        }
        if (_uncoverAllSpaces)
        {
            _uncoverAllSpaces = false;

            for (int j = 0; j < _createdWidth; j++)
            {
                for (int k = 0; k < _createdHeight; k++)
                {
                    for (int l = 0; l < _createdDepth; l++)
                    {
                        if (!_cells[j, k, l]._isMine && !_cells[j, k, l]._uncovered)
                            Uncover(new Vector3(j, k, l), true);
                    }
                }
            }
            return;
        }
        if (_startSaveLoop)
        {
            _startSaveLoop = false;
            StartCoroutine(RepeatedlySaveMaps());
            return;
        }
        /*if (_mapType == MapType._3D && _viewManager._viewMode == ViewMode.VR)
        {
            if (_centerPoint == null || _mapParent == null)
            {
                _hasStoredControllerPosLeft = false;
                _hasStoredControllerPosRight = false;
            }
            else if (OVRInput.Get(OVRInput.RawButton.LHandTrigger, OVRInput.Controller.LTouch))
            {
                _hasStoredControllerPosRight = false;
                if (_hasStoredControllerPosLeft)
                {
                    Vector3 flatContVec = _leftVRController.transform.position;
                    flatContVec.y = _centerPoint.transform.position.y;
                    flatContVec -= _centerPoint.transform.position;

                    Vector3 flatPrevContVec = _storedControllerPosLeft;
                    flatPrevContVec.y = _centerPoint.transform.position.y;
                    flatPrevContVec -= _centerPoint.transform.position;

                    _centerPoint.transform.Rotate(Vector3.up, Vector3.SignedAngle(flatPrevContVec, flatContVec, Vector3.up));
                }
                _hasStoredControllerPosLeft = true;
                _storedControllerPosLeft = _leftVRController.transform.position;
            }
            //    Debug.Log("1 OVRInput.RawButton.LHandTrigger");
            //    rotate map?
            else if (OVRInput.Get(OVRInput.RawButton.RHandTrigger, OVRInput.Controller.RTouch))
            {
                _hasStoredControllerPosLeft = false;

                if (_hasStoredControllerPosRight)
                {
                    _mapParent.transform.position += _rightVRController.transform.position - _storedControllerPosRight;
                }
                _hasStoredControllerPosRight = true;
                _storedControllerPosRight = _rightVRController.transform.position;
            }
            else
            {
                _hasStoredControllerPosLeft = false;
                _hasStoredControllerPosRight = false;
            }
        }*/
        if (!_menuActive)
        {
            _rotating = false;
            //_hasStoredControllerPosLeft = false;
            //_hasStoredControllerPosRight = false;
            return;
        }
        if (_mapType == MapType._3D)
        {
            if (_viewManager._viewMode == ViewMode.Normal)
            {
                bool uncover = false;
                bool mark = false;

                RaycastHit hit;
                if (Input.GetMouseButtonDown(0))
                {
                    _wasDown = true;
                    _rotating = true;
                    if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        CellComponent cell = hit.collider.GetComponent<CellComponent>();
                        _initialPos = cell._pos;
                    }
                    else
                        _initialPos = new Vector3(-1, -1, -1);
                }
                if (_wasDown)
                {
                    _timeDown += Time.deltaTime;
                    if (_timeDown >= 0.4f)
                    {
                        _timeDown = 0;
                        mark = true;
                        _wasDown = false;
                    }
                    else if (!Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                    {
                        _timeDown = 0;
                        _rotating = false;
                        uncover = true;
                        _wasDown = false;
                    }
                }


                if (uncover)
                {
                    if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        CellComponent cell = hit.collider.GetComponent<CellComponent>();
                        if (cell != null && cell._pos == _initialPos)
                        {
                            UncoverAtPos(cell._pos);
                        }
                    }
                }

                if (_rotating)
                {
                    if (!Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                        _rotating = false;
                    else
                    {
                        float speed = 10f;
                        _centerPoint.transform.Rotate(Vector3.up, -(Input.GetAxis("Mouse X") * speed), Space.World);
                        _centerPoint.transform.Rotate(Vector3.right, Input.GetAxis("Mouse Y") * speed, Space.World);
                    }
                }

                if (!_generated)
                {
                    mark = false;
                    return;
                }

                if (mark || Input.GetMouseButtonDown(1))
                {
                    if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        CellComponent cell = hit.collider.GetComponent<CellComponent>();
                        if (cell != null && (Input.GetMouseButtonDown(1) || cell._pos == _initialPos))
                        {
#if UNITY_ANDROID
                            Handheld.Vibrate();
#endif
                            ToggleMark(_cells[(int)cell._pos.x, (int)cell._pos.y, (int)cell._pos.z]);
                        }
                    }
                }
            }
            /*else//VR
            {
                if (_centerPoint != null && _mapParent != null)
                {
                    if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger, OVRInput.Controller.LTouch))
                    {
                        CellComponent cell = _leftVRController.GetSelectedCell();
                        if (cell != null)
                        {
                            ToggleMark(_cells[(int)cell._pos.x, (int)cell._pos.y, (int)cell._pos.z]);
                        }
                    }
                    if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger, OVRInput.Controller.RTouch))
                    {
                        CellComponent cell = _rightVRController.GetSelectedCell();
                        if (cell != null)
                        {
                            UncoverAtPos(cell._pos);
                        }
                    }
                }
            }*/
        }
        else
        {
            bool uncover = false;
            bool mark = false;
            Vector3 worldPoint;
            if (Input.GetMouseButtonDown(0))
            {
                _wasDown = true;

                if (GetWorldPoint(out worldPoint))
                {
                    _initialPos = worldPoint;
                }
                else
                    _initialPos = new Vector3(-1, -1, -1);
            }
            if (_wasDown)
            {
                _timeDown += Time.deltaTime;
                if (_timeDown >= 0.4f)
                {
                    _timeDown = 0;
#if UNITY_ANDROID
                    mark = true;
#endif
                    _wasDown = false;
                }
                else if (!Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                {
                    _timeDown = 0;
                    uncover = true;
                    _wasDown = false;
                }
            }

            if (uncover)
            {
                if (GetWorldPoint(out worldPoint))
                {
                    if(worldPoint == _initialPos)
                        UncoverAtPos(worldPoint);
                }
            }
            if (!_generated)
            {
                mark = false;
                return;
            }

            if (mark || Input.GetMouseButtonDown(1))
            {
                if (GetWorldPoint(out worldPoint))
                {
                    if (worldPoint == _initialPos || Input.GetMouseButtonDown(1))
                    {
#if UNITY_ANDROID
                        Handheld.Vibrate();
#endif
                        ToggleMark(_cells[(int)worldPoint.x, (int)worldPoint.y, (int)worldPoint.z]);
                    }
                }
            }
        }
    }
    void UncoverAtPos(Vector3 worldPoint)
    {
        if (!_generated)
        {
            Generate(worldPoint);
        }
        else
        {
            Cell cell = _cells[(int)worldPoint.x, (int)worldPoint.y, (int)worldPoint.z];
            if (cell._uncovered)
            {
                if (MarksMatchMineCount(worldPoint))
                    UncoverAroundSpot(worldPoint, true);

            }
            else
                Uncover(worldPoint, true);
        }
    }

    //REMOVE
    public static int CountNotPossible = 0;
    public static int CountPossible = 0;
    //REMOVE
}
