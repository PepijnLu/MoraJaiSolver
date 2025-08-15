using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoreSolver : MonoBehaviour
{
    [SerializeField] string input;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(SolveCore(input));
        }   
    }

    IEnumerator SolveCore(string _stringInput)
    {
        List<int> coreValues = new();
        List<int> possibleCores = new();
        Dictionary<int, int> caseResults = new();
        char[] characters = _stringInput.ToCharArray();

        foreach (char c in characters)
        {
            coreValues.Add(CharToAlphabetIndex(c));
        }
        

        for(int i = 0; i < 6; i++)
        {
            float result = coreValues[0];

            switch(i)
            {
                case 0:
                    result = SubtractNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = DivideNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = MultiplyNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;

                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 0);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
                case 1:
                    result = SubtractNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = MultiplyNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = DivideNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;
                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 1);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
                case 2:
                    result = DivideNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = SubtractNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = MultiplyNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;
                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 2);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
                case 3:
                    result = DivideNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = MultiplyNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = SubtractNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;
                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 3);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
                case 4:
                    result = MultiplyNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = DivideNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = SubtractNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;
                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 4);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
                case 5:
                    result = MultiplyNumbers(result, coreValues[1]);
                    if(!IsWholeNumber(result)) break;
                    result = SubtractNumbers(result, coreValues[2]);
                    if(!IsWholeNumber(result)) break;
                    result = DivideNumbers(result, coreValues[3]);
                    if(!IsWholeNumber(result)) break;
                    if(IsWholeNumber(result)) 
                    {
                        if(!possibleCores.Contains((int)result))
                        {
                            possibleCores.Add((int)result);
                            caseResults.Add((int)result, 5);
                        }
                    }
                    else if(CountDigits(result) > 3) throw new System.Exception("Number bigger than 3");
                    break;
            }
        }

        int core = possibleCores.Min();
        Debug.Log($"Core found: {core}, using {caseResults[core]}");
        Debug.Log("Other possible values:");
        foreach(int _number in possibleCores)
        {
            Debug.Log($"Number: {_number}, using {caseResults[_number]}");
        }
        yield return null;
    }

    bool IsWholeNumber(float value)
    {
        return value >= 0f && Mathf.Approximately(value % 1f, 0f);
    }

    int CharToAlphabetIndex(char c)
    {
        c = char.ToLower(c); // Make it case-insensitive
        if (c < 'a' || c > 'z')
            return -1; // or throw an exception, or return 0 — your choice

        return c - 'a' + 1;
    }

    int CountDigits(float number)
    {
        if(number == 0) return 1;

        number = Mathf.Abs(number);
        return (number == 0) ? 1 : (int)Mathf.Floor(Mathf.Log10(number)) + 1;
    }

    float AddNumbers(float _input1, float _input2)
    {
        return (_input1 + _input2);
    }
    float SubtractNumbers(float _input1, float _input2)
    {
        return (_input1 - _input2);
    }

    float MultiplyNumbers(float _input1, float _input2)
    {
        return (_input1 * _input2);
    }

    float DivideNumbers(float _input1, float _input2)
    {
        return (_input1 / _input2);
    }
}
