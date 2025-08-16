using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEditor;
using System.Text.RegularExpressions;

public struct BoardStateRefs
{
    public string b_newKey;
    public string  b_oldKey;
    public int  b_input;
}

public class MoraJaiSolver : MonoBehaviour
{
    [SerializeField] List<MoraTile> board;
    [SerializeField] List<Image> cornerTiles, cornerTilesBorders, solveButtons;
    [SerializeField] List<string> solveColors = new(){"red", "red", "red", "red"};
    [SerializeField] GameObject playSolveButtons, colorPicker, helpNumbers, settingsMenu, playAfterSolveIndicator;
    [SerializeField] TextMeshProUGUI sequenceText, solvedText;
    [SerializeField] int attempts, attemptsPerFrame;
    Dictionary<string, Action<int>> tileActions;
    Dictionary<string, Color> tileColors;
    Dictionary<string, BoardStateRefs> newStateOldState;
    HashSet<string> uniqueBoardStateKeys;
    List<string> startState, setState;
    Queue<string> stateQueue;
    MoraTile selectedTile;
    public bool isPlaying, isSolving, isSolved;
    bool playAfterSolve = true, bufferReset;
    int input, selectedCornerTile;
    string solvedKey;   

    void Awake()
    {  
        InstantiateDictionary();   
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectedTile != null)
            {
                selectedTile = null;
            }

            if (colorPicker.activeSelf) colorPicker.SetActive(false);
            else if (settingsMenu.activeSelf) settingsMenu.SetActive(false);
            else settingsMenu.SetActive(true);
        }   
    }

    void UpdateSetState(int _tileToUpdate, string _newColor)
    {
        setState[_tileToUpdate] = _newColor;
    }
    void SaveStartState()
    {
        uniqueBoardStateKeys = new();
        newStateOldState = new();
        attempts = 0;

        isSolved = false;

        stateQueue = new();

        startState = new();
        foreach(MoraTile _tile in board)
        {
            startState.Add(_tile.tileColor);
        }

        string newKey = StateToKey(startState);
        stateQueue.Enqueue(newKey); 

        uniqueBoardStateKeys.Add(newKey);

        BoardStateRefs boardStateRefs = new()
        {
            b_newKey = newKey,
            b_oldKey = "",
            b_input = 0
        };

        newStateOldState.Add(newKey, boardStateRefs);
    }
    void SaveBoardState(int _newInput, string oldKey, string newStateKey)
    {
        stateQueue.Enqueue(newStateKey);
        uniqueBoardStateKeys.Add(newStateKey);

        BoardStateRefs boardStateRefs = new()
        {
            b_newKey = newStateKey,
            b_oldKey = oldKey,
            b_input = _newInput
        };

        newStateOldState.Add(newStateKey, boardStateRefs);
    }

    void ResetGame(bool _hardReset = true)
    {
        Debug.Log("reset");
        bufferReset = false;

        if (_hardReset)
        {
            isSolved = false;
            isSolving = false;
            isPlaying = false;
            solvedText.text = "";
            helpNumbers.SetActive(false);
            sequenceText.text = "";
            playSolveButtons.SetActive(true);
        }


        foreach(Image _img in cornerTiles) _img.color = tileColors["grey"];
        ResetBoard(setState);
    }

    public void ResetButton()
    {
        if (isSolving) bufferReset = true;
        else ResetGame(true);
    }

    void ResetBoard(List<string> stateToResetTo)
    {
        for(int i = 0; i < board.Count; i++)
        {
            board[i].tileColor = stateToResetTo[i];
        }
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

    public void Settings()
    {
        if (settingsMenu.activeSelf) settingsMenu.SetActive(false);
        else settingsMenu.SetActive(true);
    }

    public void ChangeAttemptsPerFrame(TMP_InputField _inputField)
    {
        string digitsOnly = Regex.Replace(_inputField.text, "[^0-9]", "");
        int newAttempts;

        // If empty or only zeros, return "1"
        if (string.IsNullOrEmpty(digitsOnly)) newAttempts = 1;
        else
        {
            newAttempts = int.Parse(digitsOnly);
            if (newAttempts == 0) newAttempts = 1;
        }

        _inputField.text = newAttempts.ToString();
        attemptsPerFrame = newAttempts;
    }

    public void TogglePlayAfterSolve()
    {
        if (playAfterSolve) playAfterSolve = false;
        else playAfterSolve = true;

        playAfterSolveIndicator.SetActive(playAfterSolve);
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
        if(isSolved) return;

        if (isPlaying || isSolving)
        {
            string _color = board[_tileNumber - 1].tileColor;
            input = _tileNumber;

            HandleTileAction(_color, input);
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
            if(CheckCornerTiles())
            {
                isSolved = true;
                solvedText.text = "Solved!";
            }
        }
        else if(!isSolving && !isSolved)
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
        else if(selectedCornerTile != -1)
        {
            solveColors[selectedCornerTile] = color;
            cornerTilesBorders[selectedCornerTile].color = tileColors[color];
            UpdateSetState(8 + selectedCornerTile + 1, color);
            selectedCornerTile = -1;
        }
        colorPicker.SetActive(false);
    }

    IEnumerator AttemptSolve()
    {
        isSolving = true;
        attempts = 0;

        while (stateQueue.Count > 0 && !isSolved)
        {
            int statesThisFrame = 0;

            while (stateQueue.Count > 0 && statesThisFrame < attemptsPerFrame && !isSolved)
            {
                attempts++;
                TryAllTilesForBoardState(stateQueue.Dequeue());
                statesThisFrame++;
            }

            if (bufferReset) break;
            yield return null;
        }

        if (isSolved)
        {
            yield return RetrieveInputs(solvedKey);
            solvedText.text = "Solved!";
            helpNumbers.SetActive(true);
            if (playAfterSolve)
            {
                ResetGame(false);
                isPlaying = true;
                isSolved = false;
            }
        }
        else if (!bufferReset)
        {
            solvedText.text = "No solution.";
        }
        else
        {
            ResetGame(true);
        }

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
        string sequenceString = "";
        for(int i = 0; i < solvedInputs.Count; i++)
        {
            if(i + 1 >= solvedInputs.Count) sequenceString += solvedInputs[i].ToString();
            else sequenceString += solvedInputs[i] + ", ";
        }
        sequenceText.text = sequenceString;
    }

    void TryAllTilesForBoardState(string _stateKey)
    {
        List<string> _boardState = KeyToState(_stateKey);
        
        for (int i = 0; i < 9; i++)
        {
            if (isSolved) break;

            int _newInput = i + 1;

            ResetBoard(_boardState);
            ClickTile(_newInput);
            CheckSolved(-1, isPlaying);

            IsRepeatingBoardState(_newInput, _stateKey);

            if (!isSolved)
            {
                ResetBoard(_boardState);
            }
            else
            {
                List<string> boardState = new();
                foreach (MoraTile _tile in board)
                {
                    boardState.Add(_tile.tileColor);
                }
                solvedKey = StateToKey(boardState);
                break;
            }

        }  
    }

    void HandleTileAction(string _tileColor, int _input)
    {
        tileActions[_tileColor].Invoke(_input);
        CorrectBoardColors();
        if (isPlaying) CheckSolved(-1, true);
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

    }
    //Swaps color with the tile above
    void HandleYellowTile(int _input)
    {
        if(_input == 1 || _input == 2 || _input == 3) return;

        SwapTile(_input, _input - 3);

    }
    //Swaps color with the tile below
    void HandleVioletTile(int _input)
    {
        if(input == 7 || input == 8 || input == 9) return;

        SwapTile(input, input + 3);
    }
    //Behaves like the center tile (no effect if blue)
    void HandleBlueTile(int _input)
    {
        string centerColor = board[4].tileColor;
        if(centerColor != "blue") HandleTileAction(centerColor, _input);
    }
    public bool CheckCornerTiles()
    {
        bool solved = true;

        for(int i = 0; i < cornerTiles.Count; i++)
        {
            if(cornerTiles[i].color != tileColors[solveColors[i]]) solved = false;
        }

        return solved;
    }

    public bool CheckSolved(int _corner, bool _playing)
    {
        bool solved = true;

        for (int i = 0; i < cornerTiles.Count; i++)
        {
            if (true)
            {
                int cornerToCheck = 0;
                switch (i)
                {
                    case 1:
                        cornerToCheck = 2;
                        break;
                    case 2:
                        cornerToCheck = 6;
                        break;
                    case 3:
                        cornerToCheck = 8;
                        break;
                }
                if (board[cornerToCheck].tileColor != solveColors[i])
                {
                    solved = false;
                    cornerTiles[i].color = tileColors["grey"];
                    if (_playing && _corner == i) ResetGame(false);
                }
                else if (_corner == i || !_playing) cornerTiles[i].color = tileColors[solveColors[i]];
            }
        }

        if (solved)
        {
            Debug.Log("Puzzle solved!");
            if(isSolving) isSolved = true;
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

    bool IsRepeatingBoardState(int _newInput, string oldKey)
    {
        List<string> _boardState = new();
        foreach (MoraTile _tile in board)
        {
            _boardState.Add(_tile.tileColor);
        }

        string newKey = StateToKey(_boardState);

        bool isUnique = !uniqueBoardStateKeys.Contains(newKey);
        
        if(isUnique) SaveBoardState(_newInput, oldKey, newKey);
        return isUnique;
    }

    string StateToKey(List<string> state) => string.Join(",", state);
    List<string> KeyToState(string key)
    {
        return key.Split(',').ToList();
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
