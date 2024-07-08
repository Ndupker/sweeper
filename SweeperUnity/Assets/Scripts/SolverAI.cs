using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Optimisations
//Reset to before nearby cells in 5x5 grid
//once they are all solved then move on

//In the border cells:
//store a list in each cell that is the list of cells to be checked up to that point
//then only check if that list is valid

//In the number cells near border:
//store all the effects of non border cells (max, min, bombs)
//store them in the cell data then only check the in range border cells (pre calc these)

public class SolverAI : MonoBehaviour
{
    public Map _map;
    public bool _solve = false;
    float _nextTime = 0;
    public bool _delayedSolve = false;
    public float _solveDelay = 0.1f;
    public bool _useV1 = false;

    public bool _hasSolved = false;

    BasicSolve _basicSolve = null;
    BorderCellSolveV1 _borderSolveV1 = null;
    //BorderCellSolveV2 _borderSolveV2 = null;
    private void Start()
    {
        _basicSolve = new BasicSolve();
        _basicSolve.Initialise(this, _map);
        _borderSolveV1 = new BorderCellSolveV1();
        _borderSolveV1.Initialise(this, _map);
        //_borderSolveV2 = new BorderCellSolveV2();
        //_borderSolveV2.Initialise(this, _map);
    }
    void Update()
    {
        if (_solve && _nextTime < Time.time)
        {
            _nextTime = Time.time + _solveDelay;
            //_solve = false;
            Solve(_map.CreatedMineCount);
        }
    }

    public void Solve(uint mineCount)
    {
        _hasSolved = false;
        if (!_map.Generated)
            _map.Generate(new Vector2(Random.Range(0, _map.CreatedWidth - 1), Random.Range(0, _map.CreatedHeight) - 1));
        bool hasSolvedThisLoop = false;
        while(_map.UncoveredCells.Count != (_map.Cells.Length - mineCount) && _map.MarkedCells.Count != mineCount)
        {
            hasSolvedThisLoop = _basicSolve.Solve();

            if (!hasSolvedThisLoop)
            {
                //if(_useV1)
                    hasSolvedThisLoop = _borderSolveV1.Solve();
                //else
                //    hasSolvedThisLoop = _borderSolveV2.Solve();
            }

            if (!hasSolvedThisLoop)
            {
                break;
            }
        }
        if (!hasSolvedThisLoop)
            _solve = false;
        if(_map.UncoveredCells.Count == (_map.Cells.Length - mineCount) || _map.MarkedCells.Count == mineCount)
            _hasSolved = true;
    }

    

    

    
    

    #region Old Code

    #region Attempts to get Ordered border list
    /*
    public List<Cell> _endCells;
    public bool _isLoop = false;
    Cell _otherEndcell = null;
    void GenerateChain()
    {
        if (_endCells != null && _endCells.Count > 0)
            return;
        //List<Cell> chain = new List<Cell>();
        //chain.Add(_map.NearUnsolvedCells[0][0]);
        _endCells = new List<Cell>();
        List<List<Cell>> metaList = new List<List<Cell>>();
        GetEnd(metaList, new List<int>(), new List<float>(), _map.NearUnsolvedCells[0][0], new List<Cell>());
        _map.NearUnsolvedCells[0][0]._yellow = true;
        //get only one which is the closest one with the most connections to the "other" end (the end you didn't start from)
        //if another route has got there sooner or in the same amount of moves then disregard
        if (_isLoop)
        {
            _endCells.Clear();
            _endCells.Add(_map.NearUnsolvedCells[0][0]);
            _endCells.Add(_otherEndcell);
        }
        else if (_map.NearUnsolvedCells[0][0]._borderConnections.Count == 1)
        {
            _endCells.Add(_map.NearUnsolvedCells[0][0]);
        }
    }

    void GetEnd(List<List<Cell>> metaList, List<int> directions, List<float> pathValues, Cell startCell, List<Cell> currentChain, float pathValue = 0, int direction = -1)
    {
        currentChain.Add(startCell);
        if (direction >= 0)
        {
            if (!startCell._directionAndDistanceCount.ContainsKey(direction))
                startCell._directionAndDistanceCount.Add(direction, currentChain.Count);
            else if (startCell._directionAndDistanceCount[direction] < currentChain.Count)
                startCell._directionAndDistanceCount[direction] = currentChain.Count;
            else
                return;
        }
        bool foundConnection = false;
        for (int i = 0; i < startCell._borderConnections.Count; i++)
        {
            if ((currentChain.Count > (_map.NearUnsolvedCells[0].Count / 2f)) && currentChain[0] == startCell._borderConnections[i])
            {
                _isLoop = true;
                _otherEndcell = startCell;
            }
            if (!currentChain.Contains(startCell._borderConnections[i]))
            {
                int tempDir = direction;
                if (direction == -1)
                    tempDir = i;
                List<Cell> clone = new List<Cell>(currentChain);
                pathValue += 1 + (1 - Vector2.Distance(startCell._worldPoint, startCell._borderConnections[i]._worldPoint));
                GetEnd(metaList, directions, pathValues, startCell._borderConnections[i], clone, tempDir);
                foundConnection = true;
            }
        }
        if (!foundConnection)
        {
            metaList.Add(currentChain);
            _endCells.Add(startCell);
            directions.Add(direction);
            pathValues.Add(pathValue);
        }
        if (direction == -1)
        {
            List<Cell> newEndCells = new List<Cell>();
            List<int> newEndCellIDs = new List<int>();
            for (int i = 0; i < startCell._borderConnections.Count; i++)
            {
                int MaxID = 0;
                int biggestListCount = 0;
                float biggestPathValue = 0;
                for (int j = 0; j < directions.Count; j++)
                {
                    if (i == directions[j])
                    {
                        if (newEndCells.Contains(_endCells[j]))
                            continue;
                        //here I was gonna check if the path was "too similar" to an already worked out one
                        //as if how many cells exist in both so as to create one that has the most cells NOT in common with an already decided end
                        {

                        }
                        if (metaList[i].Count >= biggestListCount)
                        {
                            if (biggestListCount == metaList[i].Count)
                            {
                                if (pathValue <= biggestPathValue)
                                    continue;

                            }
                            biggestPathValue = pathValues[j];
                            biggestListCount = metaList[i].Count;
                            MaxID = j;
                        }
                    }
                }
                newEndCells.Add(_endCells[MaxID]);
                newEndCellIDs.Add(MaxID);
            }
            _endCells = newEndCells;
        }
    }*/
    #endregion

    #endregion
}
