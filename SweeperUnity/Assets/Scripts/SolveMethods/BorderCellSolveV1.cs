using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderCellSolveV1
{
    SolverAI _aI;
    Map _map;

    public void Initialise(SolverAI aI, Map map)
    {
        _aI = aI;
        _map = map;
    }
    public bool Solve()
    {
        if (_map.NearUnsolvedCells.Count > 0)
        {
            List<Cell> borderCells = new List<Cell>();
            for (int i = 0; i < _map.NearUnsolvedCells.Count; i++)
                borderCells.AddRange(_map.NearUnsolvedCells[i]);
            _map.NearUnsolvedCells.Clear();
            for (int i = 0; i < borderCells.Count; i++)
                borderCells[i]._borderConnections.Clear();
            for (int i = 0; i < borderCells.Count; i++)
                _map.AddNearUnsolvedCell(borderCells[i]);

            if (_map.NearUnsolvedCells[0].Count > _map._maxBorder)
                _map._maxBorder = _map.NearUnsolvedCells[0].Count;

            if (_map.NearUnsolvedCells[0].Count < 50)
                return SolveBorderCells(_map.NearUnsolvedCells[0]);
        }
        return false;
    }

    void IncrementValuesForCell(int x, int y, int z, ref int minMines, ref int maxMines)
    {
        if (x < 0 || y < 0 || z < 0 || x >= _map.CreatedWidth || y >= _map.CreatedHeight || z >= _map.CreatedDepth)
        {
            maxMines--;
            return;
        }
        Cell cellToCheck = _map.Cells[x, y, z];
        if (cellToCheck.AIMarked)
            minMines++;
        if (cellToCheck.AIUncovered)
            maxMines--;
    }

    bool IsCellStatePossible(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= _map.CreatedWidth || y >= _map.CreatedHeight || z >= _map.CreatedDepth)
            return true;
        Cell cellToCheck = _map.Cells[x, y, z];
        if (cellToCheck._marked || !cellToCheck._uncovered || cellToCheck._numberOfAdjMines == 0)
            return true;
        int minMines = 0;
        int maxMines = _map.NearbyCellArray.Length;

        for (int i = 0; i < _map.NearbyCellArray.Length; i++)
            IncrementValuesForCell(x + _map.NearbyCellArray[i]._x, y + _map.NearbyCellArray[i]._y, z + _map.NearbyCellArray[i]._z, ref minMines, ref maxMines);

        return minMines <= cellToCheck._numberOfAdjMines && maxMines >= cellToCheck._numberOfAdjMines;

    }

    bool IsBombLayoutPossible(List<Cell> borderCells, int id)
    {
        for (int i = 0; i < (id + 1); i++)
        {
            for (int j = 0; j < _map.NearbyCellArray.Length; j++)
            {
                if (!IsCellStatePossible((int)(borderCells[i]._worldPoint.x + _map.NearbyCellArray[j]._x),
                    (int)(borderCells[i]._worldPoint.y + _map.NearbyCellArray[j]._y),
                    (int)(borderCells[i]._worldPoint.z + _map.NearbyCellArray[j]._z)))
                {
                    Map.CountNotPossible++;
                    return false;
                }
            }
        }
        Map.CountPossible++;
        return true;
    }

    bool SolveBorderCells(List<Cell> borderCells, int id = 0, List<bool[]> possibleCombinations = null, bool[] bombCells = null)
    {
        if ((DateTime.Now - _map._startTime).TotalSeconds > _map._maxGenerationTime)
        {
            _map._generationFailed = true;
            return false;
        }
        if (possibleCombinations == null)
            possibleCombinations = new List<bool[]>();
        if (bombCells == null)
            bombCells = new bool[borderCells.Count];
        borderCells[id]._fakeUncovered = false;
        borderCells[id]._fakeMarked = true;
        bombCells[id] = true;
        if (IsBombLayoutPossible(borderCells, id))
        {
            if (id == borderCells.Count - 1)
                possibleCombinations.Add((bool[])bombCells.Clone());
            else
                SolveBorderCells(borderCells, id + 1, possibleCombinations, bombCells);
        }
        borderCells[id]._fakeUncovered = true;
        borderCells[id]._fakeMarked = false;
        bombCells[id] = false;
        if (IsBombLayoutPossible(borderCells, id))
        {
            if (id == borderCells.Count - 1)
                possibleCombinations.Add((bool[])bombCells.Clone());
            else
                SolveBorderCells(borderCells, id + 1, possibleCombinations, bombCells);
        }
        borderCells[id]._fakeUncovered = false;
        borderCells[id]._fakeMarked = false;
        bool solvedSomething = false;
        if (_map._generationFailed)
            return false;

            //must be the end of the recursion
        if (id == 0 && possibleCombinations.Count > 0)
        {
            // have to clone it here as it will get altered otherwise by the uncovers/togglemarks below
            borderCells = new List<Cell>(borderCells);

            for (int i = 0; i < borderCells.Count; i++)
            {
                bool hasMine = possibleCombinations[0][i];
                bool failed = false;
                for (int j = 1; j < possibleCombinations.Count; j++)
                {
                    if (hasMine != possibleCombinations[j][i])
                    {
                        failed = true;
                        break;
                    }
                }
                if (!failed)
                {
                    solvedSomething = true;
                    if (hasMine)
                        _map.ToggleMark(borderCells[i]);
                    else
                        _map.Uncover(borderCells[i]._worldPoint, true);
                }
            }
        }
        return solvedSomething;
    }
}
