using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeMultiplayer : MonoBehaviour
{
    public void GoToMultiplayer() 
    {
        SceneManager.LoadScene("Game");
    }
}

