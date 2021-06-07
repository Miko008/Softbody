using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
   
    public void Exit()
    {
        Application.Quit();
    }
    
    public void PauseTime()
    {
        if(Time.timeScale == 0)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

    public void Reset()
    {
        MeshVolume.Obj.ResetMesh();
    }

}
