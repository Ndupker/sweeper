using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderCellSolveV2
{
    /*class BorderCell
    {
        public Cell _cell;
        public NumberCell[] _cellsToCheck;

        public bool _marked;
        public bool _uncovered;

        public int _checkedNumberCellCount = 0;
    }

    class NumberCell
    {
        public NumberCell()
        {
        }
        public NumberCell(NumberCell numberCell)
        {
            _cell = numberCell._cell;
            _minMineCount = numberCell._minMineCount;
            _maxMineCount = numberCell._maxMineCount;
            _numberOfAdjMines = numberCell._numberOfAdjMines;
        }

        public Cell _cell;

        public int _minMineCount;
        public int _maxMineCount;

        public int _numberOfAdjMines;
    }
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
                GetBorderCells(_map.NearUnsolvedCells[0]);
            return SolveBorderCells(GetBorderCells(_map.NearUnsolvedCells[0]));
        }
        return false;
    }
    List<BorderCell> GetBorderCells(List<Cell> borderCellCellObjects)
    {
        List<NumberCell> numberCells = new List<NumberCell>();
        List<Cell> numberCellCellObjects = new List<Cell>();

        List<BorderCell> borderCells = new List<BorderCell>();
        for (int i = 0; i < borderCellCellObjects.Count; i++)
        {
            BorderCell borderCell = new BorderCell();
            borderCell._cell = borderCellCellObjects[i];
            borderCell._uncovered = borderCellCellObjects[i]._uncovered;
            borderCell._marked = borderCellCellObjects[i]._marked;
            List<NumberCell> cellsToCheck = new List<NumberCell>();
            for (int j = 0; j < Map.NearbyCellArray.Length; j++)
            {
                int x = (int)(borderCellCellObjects[i]._worldPoint.x + Map.NearbyCellArray[j]._x);
                int y = (int)(borderCellCellObjects[i]._worldPoint.y + Map.NearbyCellArray[j]._y);
                if (x < 0 || y < 0 || x >= _map.CreatedWidth || y >= _map.CreatedHeight)
                    continue;
                Cell cellToCheck = _map.Cells[x, y];
                if(numberCellCellObjects.Contains(cellToCheck))
                {
                    cellsToCheck.Add(numberCells[numberCellCellObjects.IndexOf(cellToCheck)]);
                    continue;
                }
                NumberCell numberCell = GetNumberCell(cellToCheck, borderCellCellObjects);
                if (numberCell == null)
                    continue;
                numberCells.Add(numberCell);
                numberCellCellObjects.Add(numberCell._cell);

                cellsToCheck.Add(numberCell);
            }
            borderCell._cellsToCheck = new NumberCell[cellsToCheck.Count];
            for (int j = 0; j < cellsToCheck.Count; j++)
                borderCell._cellsToCheck[j] = new NumberCell(cellsToCheck[j]);
            //Debug.LogError("added _cellsToCheck length: " + borderCell._cellsToCheck.Length);
            borderCells.Add(borderCell);
        }
        return borderCells;
    }

    NumberCell GetNumberCell(Cell cellToCheck, List<Cell> borderCells)
    {
        if (cellToCheck._marked || !cellToCheck._uncovered || cellToCheck._numberOfAdjMines == 0)
            return null;
        int minMines = 0;
        int maxMines = 8;
        for (int i = 0; i < Map.NearbyCellArray.Length; i++)
        {
            int xx = (int)(cellToCheck._worldPoint.x + Map.NearbyCellArray[i]._x);
            int yy = (int)(cellToCheck._worldPoint.y + Map.NearbyCellArray[i]._y);
            if (xx < 0 || yy < 0 || xx >= _map.CreatedWidth || yy >= _map.CreatedHeight)
            {
                maxMines--;
                continue;
            }
            Cell nearCell = _map.Cells[xx, yy];
            IncrementValuesForCell(nearCell, ref minMines, ref maxMines);
            //Debug.LogError("minMines: " + minMines);
            //Debug.LogError("maxMines: " + maxMines);
        }
        NumberCell cell = new NumberCell();
        cell._cell = cellToCheck;
        cell._minMineCount = minMines;
        cell._maxMineCount = maxMines;
        cell._numberOfAdjMines = cellToCheck._numberOfAdjMines;
        return cell;
    }
    void IncrementValuesForCell(Cell cellToCheck, ref int minMines, ref int maxMines)
    {
        if (cellToCheck._marked)
            minMines++;
        if (cellToCheck._uncovered)
            maxMines--;
    }
    void IncrementValuesForCell(int x, int y, ref int minMines, ref int maxMines)
    {
        if (x < 0 || y < 0 || x >= _map.CreatedWidth || y >= _map.CreatedHeight)
        {
            maxMines--;
            return;
        }
        IncrementValuesForCell(_map.Cells[x, y], ref minMines, ref maxMines);
    }



    bool IsCellStatePossible(NumberCell numberCell, BorderCell changedBorderCell)
    {
        if (changedBorderCell._marked)
            numberCell._minMineCount++;
        if (changedBorderCell._uncovered)
            numberCell._maxMineCount--;
        return numberCell._minMineCount <= numberCell._numberOfAdjMines && numberCell._maxMineCount >= numberCell._numberOfAdjMines;
    }

    bool IsBombLayoutPossible(BorderCell changedBorderCell)
    {
        for (int j = 0; j < changedBorderCell._cellsToCheck.Length; j++)
        {
            changedBorderCell._checkedNumberCellCount++;
            if (!IsCellStatePossible(changedBorderCell._cellsToCheck[j], changedBorderCell))
            {
                Map.CountNotPossible++;
                //Debug.LogError("111111 bomb layout not possible");
                return false;
            }
        }
        Map.CountPossible++;
        //Debug.LogError("222222 bomb layout possible");
        return true;
    }

    void UndoMinMaxMines(BorderCell changedBorderCell)
    {
        for (int j = 0; j < changedBorderCell._checkedNumberCellCount; j++)
        {
            if (changedBorderCell._marked)
                changedBorderCell._cellsToCheck[j]._minMineCount--;
            if (changedBorderCell._uncovered)
                changedBorderCell._cellsToCheck[j]._maxMineCount++;
        }
        changedBorderCell._checkedNumberCellCount = 0;
    }

    bool SolveBorderCells(List<BorderCell> borderCells, int id = 0, List<bool[]> possibleCombinations = null, bool[] bombCells = null)
    {
        if ((DateTime.Now - _map._startTime).TotalSeconds > _map._maxGenerationTime)
        {
            _map._generationFailed = true;
            return false;
        }
        /*string start = "";
        for (int i = 0; i < borderCells.Count; i++)
        {
            for (int j = 0; j < borderCells[i]._cellsToCheck.Length; j++)
            {
                start += borderCells[i]._cellsToCheck[j]._minMineCount;
                start += borderCells[i]._cellsToCheck[j]._maxMineCount;
            }
        }*/
        /*
        if (possibleCombinations == null)
            possibleCombinations = new List<bool[]>();
        if (bombCells == null)
            bombCells = new bool[borderCells.Count];
        borderCells[id]._uncovered = false;
        borderCells[id]._marked = true;
        bombCells[id] = true;
        if (IsBombLayoutPossible(borderCells[id]))
        {
            if (id == borderCells.Count - 1)
                possibleCombinations.Add((bool[])bombCells.Clone());
            else
                SolveBorderCells(borderCells, id + 1, possibleCombinations, bombCells);
        }
        UndoMinMaxMines(borderCells[id]);
        borderCells[id]._uncovered = true;
        borderCells[id]._marked = false;
        bombCells[id] = false;
        if (IsBombLayoutPossible(borderCells[id]))
        {
            if (id == borderCells.Count - 1)
                possibleCombinations.Add((bool[])bombCells.Clone());
            else
                SolveBorderCells(borderCells, id + 1, possibleCombinations, bombCells);
        }
        UndoMinMaxMines(borderCells[id]);
        borderCells[id]._uncovered = false;
        borderCells[id]._marked = false;
        bool solvedSomething = false;
        //must be the end of the recursion

        if (_map._generationFailed)
            return false;
        if (id == 0 && possibleCombinations.Count > 0)
        {
            // have to clone it here as it will get altered otherwise by the uncovers/togglemarks below
            borderCells = new List<BorderCell>(borderCells);

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
                        _map.ToggleMark(borderCells[i]._cell);
                    else
                        _map.Uncover(borderCells[i]._cell._worldPoint, true);
                }
            }
        }
        */
        /*string end = "";
        for (int i = 0; i < borderCells.Count; i++)
        {
            for (int j = 0; j < borderCells[i]._cellsToCheck.Length; j++)
            {
                end += borderCells[i]._cellsToCheck[j]._minMineCount;
                end += borderCells[i]._cellsToCheck[j]._maxMineCount;
            }
        }
        Debug.LogError(start);
        Debug.LogError(end);*/
        /*
        return solvedSomething;
    }*/
}
