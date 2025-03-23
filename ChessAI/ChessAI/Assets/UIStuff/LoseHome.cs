using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseHome : MonoBehaviour
{
    public void LoseToHome() 
    {
        SceneManager.LoadScene("HomePage");
    }
}
