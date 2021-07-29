using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputFrameCount : MonoBehaviour
{
    public int inputframeCount;

    public void ProcessInputFrameCount(bool[]inputs,bool[] previousInputs)
    {
        if ((inputs[(int)EnumData.Inputs.Down] && previousInputs[(int)EnumData.Inputs.Down] != inputs[(int)EnumData.Inputs.Down])||
            (inputs[(int)EnumData.Inputs.Left] && previousInputs[(int)EnumData.Inputs.Left] != inputs[(int)EnumData.Inputs.Left])||
            (inputs[(int)EnumData.Inputs.Right] && previousInputs[(int)EnumData.Inputs.Right] != inputs[(int)EnumData.Inputs.Right])||
            (inputs[(int)EnumData.Inputs.Up] && previousInputs[(int)EnumData.Inputs.Up] != inputs[(int)EnumData.Inputs.Up]))
        {
            inputframeCount = 0;
        }
        if (inputs[(int)EnumData.Inputs.Down] || inputs[(int)EnumData.Inputs.Left] || inputs[(int)EnumData.Inputs.Right] || inputs[(int)EnumData.Inputs.Up])
        {
            inputframeCount++;
        }
        if ((!inputs[(int)EnumData.Inputs.Down] && previousInputs[(int)EnumData.Inputs.Down] != inputs[(int)EnumData.Inputs.Down])||
            (!inputs[(int)EnumData.Inputs.Left] && previousInputs[(int)EnumData.Inputs.Left] != inputs[(int)EnumData.Inputs.Left]) ||
            (!inputs[(int)EnumData.Inputs.Right] && previousInputs[(int)EnumData.Inputs.Right] != inputs[(int)EnumData.Inputs.Right]) ||
            (!inputs[(int)EnumData.Inputs.Up] && previousInputs[(int)EnumData.Inputs.Up] != inputs[(int)EnumData.Inputs.Up]))
        {
            inputframeCount = 0;
        }
    }
}
