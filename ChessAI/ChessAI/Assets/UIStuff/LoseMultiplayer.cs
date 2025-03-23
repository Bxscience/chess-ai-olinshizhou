using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMultiplayer : MonoBehaviour
{
    public void LoseToMultiplayer() 
    {
        SceneManager.LoadScene("Game");
    }
}

