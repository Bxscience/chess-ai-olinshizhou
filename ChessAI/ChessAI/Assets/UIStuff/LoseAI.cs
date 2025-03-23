using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseAI : MonoBehaviour
{
    public void LoseToAI() 
    {
        SceneManager.LoadScene("Game");
    }
}
