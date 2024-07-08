using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSolve
{
    SolverAI _aI;
    Map _map;

    public void Initialise(SolverAI aI, Map map)
    {
        _aI = aI;
        _map = map;
    }
    void IsCoveredCell(int x, int y, int z, List<Cell> listToAddTo)
    {
        if (x >= 0 && y >= 0 && z >= 0 && x < _map.CreatedWidth && y < _map.CreatedHeight && z < _map.CreatedDepth)
        {
            Cell cell = _map.Cells[x, y, z];
            if (!cell._uncovered)
                listToAddTo.Add(cell);
        }
    }
    List<Cell> GetNearbyCoveredCells(Vector3 worldPoint)
    {
        int x = (int)worldPoint.x;
        int y = (int)worldPoint.y;
        int z = (int)worldPoint.z;
        List<Cell> nearbyCells = new List<Cell>();

        for (int i = 0; i < _map.NearbyCellArray.Length; i++)
            IsCoveredCell(x + _map.NearbyCellArray[i]._x, y + _map.NearbyCellArray[i]._y, z + _map.NearbyCellArray[i]._z, nearbyCells);

        return nearbyCells;
    }
    public bool Solve()
    {
        bool solved = false;
        for (int i = 0; i < _map.UncoveredCells.Count; i++)
        {
            List<Cell> nearbys = GetNearbyCoveredCells(_map.UncoveredCells[i]._worldPoint);
            if (nearbys.Count == _map.UncoveredCells[i]._numberOfAdjMines)
            {
                for (int j = 0; j < nearbys.Count; j++)
                {
                    if (!nearbys[j]._marked)
                    {
                        solved = true;
                        _map.ToggleMark(nearbys[j]);
                        if (_aI._delayedSolve)
                            return true;
                    }
                }
            }
            else
            {
                int markCount = 0;
                for (int j = 0; j < nearbys.Count; j++)
                {
                    if (nearbys[j]._marked)
                        markCount++;
                }
                if (markCount == _map.UncoveredCells[i]._numberOfAdjMines)
                {
                    for (int j = 0; j < nearbys.Count; j++)
                    {
                        if (!nearbys[j]._marked)
                        {
                            if (_map.Uncover(nearbys[j]._worldPoint, true))
                            {
                                solved = true;
                                if (_aI._delayedSolve)
                                    return true;
                            }
                        }
                    }
                }
            }
        }
        return solved;
    }
}
