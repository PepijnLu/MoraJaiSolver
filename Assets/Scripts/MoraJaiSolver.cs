using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public struct BoardStateRefs
{
    public string b_newKey;
    public string  b_oldKey;
    public int  b_input;
}

public class MoraJaiSolver : MonoBehaviour
{
    //Orange takes majority or does nothing when equal
    [SerializeField] List<MoraTile> board;
    [SerializeField] bool autoSolve;
    [SerializeField] bool yieldNull;
    [SerializeField] List<Image> solveButtons;
    List<List<string>> uniqueBoardStates;
    List<List<string>> handledBoardStates;
    List<string> startState, setState;
    [SerializeField] GameObject playSolveButtons;
    [SerializeField] bool oneSolveColor;
    [SerializeField] List<string> solveColors = new(){"red", "red", "red", "red"};
    [SerializeField] List<Image> cornerTiles;
    Dictionary<string, Action<int>> tileActions;
    Dictionary<string, Color> tileColors;
    [SerializeField] int attempts;
    //1 through 9
    int input;
    bool isSolved, isSolving, isPlaying;
    string solvedKey;
    List<int> solvedCorners = new();
    List<int> previousInputs;
    public static MoraJaiSolver instance;
    HashSet<string> uniqueBoardStateKeys;
    HashSet<string> handledBoardStateKeys;
    Dictionary<string, BoardStateRefs> newStateOldState;
    MoraTile selectedTile;
    int selectedCornerTile;
    [SerializeField] GameObject colorPicker;

    void Awake()
    {
        instance = this;   
        InstantiateDictionary();   
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(selectedTile != null)
            {
                selectedTile = null;
            }
        }   
    }

    void UpdateSetState(int _tileToUpdate, string _newColor)
    {
        setState[_tileToUpdate] = _newColor;
    }
    void SaveStartState()
    {
        startState = new();
        uniqueBoardStates = new();
        handledBoardStates = new();
        uniqueBoardStateKeys = new();
        handledBoardStateKeys = new();
        newStateOldState = new();
        attempts = 0;

        isSolved = false;
        foreach(MoraTile _tile in board)
        {
            startState.Add(_tile.tileColor);
        }

        uniqueBoardStates.Add(startState);
        string newKey = StateToKey(startState);
        uniqueBoardStateKeys.Add(newKey);

        BoardStateRefs boardStateRefs = new()
        {
            b_newKey = newKey,
            b_oldKey = "",
            b_input = 0
        };

        newStateOldState.Add(newKey, boardStateRefs);
    }
    void SaveBoardState(int _newInput, string oldKey)
    {
        List<string> boardState = new();
        foreach(MoraTile _tile in board)
        {
            boardState.Add(_tile.tileColor);
        }

        string newKey = StateToKey(boardState);

        //if (!uniqueBoardStateKeys.Contains(newKey))
        //{
        uniqueBoardStates.Add(boardState);
        uniqueBoardStateKeys.Add(newKey);
        BoardStateRefs boardStateRefs = new()
        {
            b_newKey = newKey,
            b_oldKey = oldKey,
            b_input = _newInput
        };

        newStateOldState.Add(newKey, boardStateRefs);
        //CorrectBoardColors();
        //}
    }

    public void Reset()
    {
        isSolved = false;
        isSolving = false;
        isPlaying = false;
        foreach(Image _img in cornerTiles) _img.color = tileColors["grey"];
        playSolveButtons.SetActive(true);
        ResetBoard(setState);
    }

    void ResetBoard(List<string> stateToResetTo)
    {
        for(int i = 0; i < board.Count; i++)
        {
            board[i].tileColor = stateToResetTo[i];
        }
        solvedCorners.Clear();
        foreach(Image _img in solveButtons)
        {
            _img.color = tileColors["white"];
        }
        CorrectBoardColors();
    }

    void InstantiateDictionary()
    {
        tileActions = new()
        {
            { "grey", HandleGreyTile },
            { "black", HandleBlackTile },
            { "green", HandleGreenTile },
            { "pink", HandlePinkTile },
            { "yellow", HandleYellowTile },
            { "violet", HandleVioletTile },
            { "white", HandleWhiteTile },
            { "red", HandleRedTile },
            { "orange", HandleOrangeTile },
            { "blue", HandleBlueTile }
        };  

        tileColors = new()
        {
            { "grey", new Color(.7f, .7f, .7f)},
            { "black", new Color(0, 0, 0) },
            { "green", new Color(0, 1, 0) },
            { "pink", new Color(1, .75f, .8f) },
            { "yellow", new Color(1, 1f, 0f) },
            { "violet", new Color(.56f, 0f, 1f) },
            { "white",  new Color(1, 1, 1) },
            { "red", new Color(1, 0, 0) },
            { "orange", new Color(1, .7f, 0) },
            { "blue", new Color(0, 0, 1) }
        };

        setState = new(){"grey", "green", "grey", "orange", "red", "orange", "white", "green", "black", "red", "red", "red", "red"};

        CorrectBoardColors();
    }

    public void Solve()
    {
        if(!isSolving) 
        {
            foreach(Image _img in cornerTiles) _img.color = tileColors["grey"];
            playSolveButtons.SetActive(false);
            colorPicker.SetActive(false);
            SaveStartState();
            StartCoroutine(AttemptSolve());
        }
    }

    public void Play()
    {
        foreach(Image _img in cornerTiles) _img.color = tileColors["grey"];
        colorPicker.SetActive(false);
        playSolveButtons.SetActive(false);
        isPlaying = true;
        SaveStartState();
    }

    void CorrectBoardColors()
    {
        foreach(MoraTile _tile in board)
        {
            _tile.image.color = tileColors[_tile.tileColor];
        }
    }

    public void ClickTile(int _tileNumber)
    {
        if (isPlaying || isSolving)
        {
            string _color = board[_tileNumber - 1].tileColor;
            input = _tileNumber;

            HandleTileAction(_color, input);
            if (!isSolved)
            {
                isSolved = CheckSolved(-1, isSolving);
            }
        }
            else
            {
                selectedTile = board[_tileNumber - 1];
                selectedCornerTile = 0;
                //Enable color picker
                colorPicker.SetActive(true);
            }
    }


    public void ClickCornerTile(int _corner)
    {
        if (isPlaying)
        {
            //Check if correct, if so, become that color
            CheckSolved(_corner, true);
        }
        else
        {
            selectedTile = null;
            selectedCornerTile = _corner;
            //Enable color picker
            colorPicker.SetActive(true);
        }
    }

    public void SelectColor(string color)
    {
        if(selectedTile != null)
        {
            selectedTile.tileColor = color;
            CorrectBoardColors();
            UpdateSetState(selectedTile.tileNumber - 1, color);
            selectedTile = null;
        }
        else if(selectedCornerTile != 0)
        {
            solveColors[selectedCornerTile] = color;
            cornerTiles[selectedCornerTile].color = tileColors[color];
            UpdateSetState(8 + selectedCornerTile + 1, color);
            selectedCornerTile = 0;
        }
        colorPicker.SetActive(false);
    }

    IEnumerator AttemptSolve()
    {
        Debug.Log("Attempt solve");
        isSolving = true;
        previousInputs = new();

        while(solvedCorners.Count < 4)
        {
            Debug.Log("Solving");
            List<List<string>> currentUnhandledUniqueBoardStates = new();
            foreach(List<string> _state in uniqueBoardStates)
            {
                if(!IsHandledBoardState(_state)) 
                {
                    currentUnhandledUniqueBoardStates.Add(_state);
                }
            }

            foreach(List<string> _state in currentUnhandledUniqueBoardStates)
            {
                //Debug.Log("New solve attempt");
                StartCoroutine(TryAllTilesForBoardState(_state));

                handledBoardStates.Add(_state); // keep for reference if needed

                string newKey = StateToKey(_state);
                handledBoardStateKeys.Add(newKey); // use for fast lookups

                attempts = handledBoardStateKeys.Count;
                
                if(isSolved) 
                {
                    yield return RetrieveInputs(solvedKey);
                    break;
                }
            }

            if(isSolved) break;
            if(yieldNull) yield return null;
        }

        Debug.Log("Solved");
        isSolving = false;
        yield return null;
    }

    IEnumerator RetrieveInputs(string _key)
    {
        List<int> solvedInputs = new();
        string currentKey = _key;

        while (true)
        {
            BoardStateRefs currentRef = newStateOldState[currentKey];

            if (string.IsNullOrEmpty(currentRef.b_oldKey)) break;

            solvedInputs.Insert(0, currentRef.b_input);

            currentKey = currentRef.b_oldKey;
            yield return null;
        }

        Debug.Log($"Solved it in: {solvedInputs.Count}");
        foreach (int _input in solvedInputs)
        {
            Debug.Log("Input: " + _input);
        }
    }

    IEnumerator TryAllTilesForBoardState(List<string> _boardState)
    {
        for(int i = 0; i < 9; i++)
        {
            int _newInput = i + 1;
            ResetBoard(_boardState);
            string oldKey = StateToKey(_boardState);

            ClickTile(_newInput);
            bool isRepeating = IsRepeatingBoardState();
            if(!isRepeating)
            {
                SaveBoardState(_newInput, oldKey);
            }

            //if(!isSolved) isSolved = CheckSolved();

            if(!isSolved)
            {
                ResetBoard(_boardState);
            }
            else
            {
                List<string> boardState = new();
                foreach(MoraTile _tile in board)
                {
                    boardState.Add(_tile.tileColor);
                }
                solvedKey = StateToKey(boardState);
                break;
            }

        }  

        if(yieldNull) yield return null;
    }

    void HandleTileAction(string _tileColor, int _input)
    {
        tileActions[_tileColor].Invoke(_input);
    }

    //Red makes black -> red, white -> black
    void HandleRedTile(int _input)
    {
        foreach(MoraTile _tile in board)
        {
            if(_tile.tileColor == "black")
            {
                _tile.tileColor = "red";
            }
            else if(_tile.tileColor == "white")
            {
                _tile.tileColor = "black";
            }
        }
        CorrectBoardColors();
    }

    //White just turns grey
    void HandleWhiteTile(int _input)
    {
        List<int> adjacentIndexes = new();
        string originalColor = board[_input - 1].tileColor;
        board[_input - 1].tileColor = "grey";

        switch(_input)
        {
            case 1:
                adjacentIndexes.Add(1);
                adjacentIndexes.Add(3);
                break;
            case 2:
                adjacentIndexes.Add(0);
                adjacentIndexes.Add(2);
                adjacentIndexes.Add(4);
                break;
            case 3:
                adjacentIndexes.Add(1);
                adjacentIndexes.Add(5);
                break;
            case 4:
                adjacentIndexes.Add(0);
                adjacentIndexes.Add(4);
                adjacentIndexes.Add(6);
                break;
            case 5:
                adjacentIndexes.Add(1);
                adjacentIndexes.Add(3);
                adjacentIndexes.Add(5);
                adjacentIndexes.Add(7);
                break;
            case 6:
                adjacentIndexes.Add(2);
                adjacentIndexes.Add(4);
                adjacentIndexes.Add(8);
                break;
            case 7:
                adjacentIndexes.Add(3);
                adjacentIndexes.Add(7);
                break;
            case 8:
                adjacentIndexes.Add(4);
                adjacentIndexes.Add(6);
                adjacentIndexes.Add(8);
                break;
            case 9:
                adjacentIndexes.Add(5);
                adjacentIndexes.Add(7);
                break;
        }
        
        for(int i = 0; i < adjacentIndexes.Count; i++)
        {
            if(board[adjacentIndexes[i]].tileColor == originalColor) board[adjacentIndexes[i]].tileColor = "grey";
            else if(board[adjacentIndexes[i]].tileColor == "grey") board[adjacentIndexes[i]].tileColor = originalColor;
        }
        
        CorrectBoardColors();
    }

    //Grey does nothing
    void HandleGreyTile(int _input)
    {
        //Do nothing
    }

    //Green goes to opposite corner or side
    void HandleGreenTile(int _input)
    {
        //Return if in the center
        int _tileNumber = board[_input - 1].tileNumber;
        if(_tileNumber == 5) return;

        SwapTile(_tileNumber, 10 - _tileNumber);
    }
    //Takes adjacent majority color
    void HandleOrangeTile(int _input)
    {
        List<string> adjacentColors = new();

        switch(_input)
        {
            case 1:
                adjacentColors.Add(board[1].tileColor);
                adjacentColors.Add(board[3].tileColor);
                break;
            case 2:
                adjacentColors.Add(board[0].tileColor);
                adjacentColors.Add(board[2].tileColor);
                adjacentColors.Add(board[4].tileColor);
                break;
            case 3:
                adjacentColors.Add(board[1].tileColor);
                adjacentColors.Add(board[5].tileColor);
                break;
            case 4:
                adjacentColors.Add(board[0].tileColor);
                adjacentColors.Add(board[4].tileColor);
                adjacentColors.Add(board[6].tileColor);
                break;
            case 5:
                adjacentColors.Add(board[1].tileColor);
                adjacentColors.Add(board[3].tileColor);
                adjacentColors.Add(board[5].tileColor);
                adjacentColors.Add(board[7].tileColor);
                break;
            case 6:
                adjacentColors.Add(board[2].tileColor);
                adjacentColors.Add(board[4].tileColor);
                adjacentColors.Add(board[8].tileColor);
                break;
            case 7:
                adjacentColors.Add(board[3].tileColor);
                adjacentColors.Add(board[7].tileColor);
                break;
            case 8:
                adjacentColors.Add(board[4].tileColor);
                adjacentColors.Add(board[6].tileColor);
                adjacentColors.Add(board[8].tileColor);
                break;
            case 9:
                adjacentColors.Add(board[5].tileColor);
                adjacentColors.Add(board[7].tileColor);
                break;
        }

        string mostFrequentColor = GetMostFrequentOrNull(adjacentColors);
        if(mostFrequentColor != null)
        {
            board[input - 1].tileColor = mostFrequentColor;
            CorrectBoardColors();
        }
    }
    //Rotates the row
    void HandleBlackTile(int _input)
    {
        int startTile = 0;

        if(_input <= 3) startTile = 1;
        else if (_input <= 6) startTile = 4;
        else if (_input <= 9) startTile = 7;
        
        int startIndex = startTile - 1;

        string tile1color = board[startIndex].tileColor;
        string tile2color = board[startIndex + 1].tileColor;
        string tile3color = board[startIndex + 2].tileColor;

        board[startIndex].tileColor = tile3color;
        board[startIndex + 1].tileColor = tile1color;
        board[startIndex + 2].tileColor = tile2color;

        CorrectBoardColors();
    }
    //Rotates
    void HandlePinkTile(int _input)
    {
        int _tileIndex = _input - 1;

        switch(_tileIndex)
        {
            case 0: //works
                HandleRotation(new List<int>{1, 4, 3});
                break;
            case 1: //works
                HandleRotation(new List<int>{2, 5, 4, 3, 0});
                break;
            case 2: //works
                HandleRotation(new List<int>{5, 4, 1});
                break;
            case 3: //check
                HandleRotation(new List<int>{0, 1, 4, 7, 6});
                break;
            case 4: //check
                HandleRotation(new List<int>{1, 2, 5, 8, 7, 6, 3, 0});
                break;
            case 5: //check
                HandleRotation(new List<int>{8, 7, 4, 1, 2});
                break;
            case 6: //works
                HandleRotation(new List<int>{3, 4, 7});
                break;
            case 7: //works
                HandleRotation(new List<int>{6, 3, 4, 5, 8});
                break; 
            case 8: //works
                HandleRotation(new List<int>{7, 4, 5});
                break; 
        }

        CorrectBoardColors();
    }
    //Swaps color with the tile above
    void HandleYellowTile(int _input)
    {
        if(_input == 1 || _input == 2 || _input == 3) return;

        SwapTile(_input, _input - 3);

        CorrectBoardColors();
    }
    //Swaps color with the tile below
    void HandleVioletTile(int _input)
    {
        if(input == 7 || input == 8 || input == 9) return;

        SwapTile(input, input + 3);

        CorrectBoardColors();
    }
    //Behaves like the center tile (no effect if blue)
    void HandleBlueTile(int _input)
    {
        string centerColor = board[4].tileColor;
        if(centerColor != "blue") HandleTileAction(centerColor, _input);

        CorrectBoardColors();
    }
    public bool CheckSolved(int _corner = -1, bool _clickedOnCorner = false)
    {
        bool solved = true;

        if (_corner == -1 || _corner == 0)
        {
            if (board[0].tileColor != solveColors[0])
            {
                solved = false;
                cornerTiles[0].color = tileColors["grey"];
            }
            else if(_clickedOnCorner) cornerTiles[0].color = tileColors[solveColors[0]];
        }

        if (_corner == -1 || _corner == 1)
        {
            if (board[2].tileColor != solveColors[1])
            {
                solved = false;
                cornerTiles[1].color = tileColors["grey"];
            }
            else if(_clickedOnCorner) cornerTiles[1].color = tileColors[solveColors[1]];
        }
        if (_corner == -1 || _corner == 2)
        {
            if (board[6].tileColor != solveColors[2])
            {
                solved = false;
                cornerTiles[2].color = tileColors["grey"];
            }
            else if(_clickedOnCorner) cornerTiles[2].color = tileColors[solveColors[2]];
        }
        if (_corner == -1 || _corner == 3)
        {
            if (board[8].tileColor != solveColors[3])
            {
                solved = false;
                cornerTiles[3].color = tileColors["grey"];
            }
            else if(_clickedOnCorner) cornerTiles[3].color = tileColors[solveColors[3]];
        }
        
        return solved;
    }

    void SwapTile(int tile1, int tile2)
    {
        int tile1index = tile1 - 1;
        int tile2index = tile2 - 1;

        string tile1color = board[tile1index].tileColor;
        string tile2color = board[tile2index].tileColor;

        board[tile1index].tileColor = tile2color;
        board[tile2index].tileColor = tile1color;

        CorrectBoardColors();
    }

    bool IsRepeatingBoardState()
    {
        List<string> _boardState = new();
        foreach (MoraTile _tile in board)
        {
            _boardState.Add(_tile.tileColor);
        }

        return uniqueBoardStateKeys.Contains(StateToKey(_boardState));
    }

    string StateToKey(List<string> state) => string.Join(",", state);
    bool IsHandledBoardState(List<string> state)
    {
        string key = StateToKey(state);
        return handledBoardStateKeys.Contains(key);
    }

    string GetMostFrequentOrNull(List<string> items)
    {
        if (items == null || items.Count == 0)
            return null;

        var grouped = items
            .GroupBy(s => s)
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        if (grouped.Count < 1)
            return null;

        // Check for tie
        if (grouped.Count > 1 && grouped[0].Count == grouped[1].Count)
            return null;

        return grouped[0].Value;
    }

    void HandleRotation(List<int> _tilesToRotate)
    {
        Dictionary<int, string> tilesNeededColors = new();
        //First entry in list needs color from last entry in the list, all other entries take previous color
        for(int i = 0; i < _tilesToRotate.Count; i++)
        {
            int _currentTileIndex = _tilesToRotate[i];

            int _desiredColorTileIndex;

            if(i == 0) _desiredColorTileIndex = _tilesToRotate[_tilesToRotate.Count - 1];
            else _desiredColorTileIndex =  _tilesToRotate[i - 1];

            string _desiredColor = board[_desiredColorTileIndex].tileColor;
            
            tilesNeededColors.Add(_currentTileIndex, _desiredColor);
        }

        foreach(var kvp in tilesNeededColors)
        {
            board[kvp.Key].tileColor = kvp.Value;
        }
    }
}
