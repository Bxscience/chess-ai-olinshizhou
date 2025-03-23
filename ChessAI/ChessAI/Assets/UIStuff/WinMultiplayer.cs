using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMultiplayer : MonoBehaviour
{
    public void WinToMultiplayer() 
    {
        SceneManager.LoadScene("Game");
    }
}

