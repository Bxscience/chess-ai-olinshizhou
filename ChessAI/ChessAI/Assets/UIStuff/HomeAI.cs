using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeAI : MonoBehaviour
{
    public void GoToAI() 
    {
        SceneManager.LoadScene("GameAI");
    }
}
