using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinHome : MonoBehaviour
{
    public void WinToHome() 
    {
        SceneManager.LoadScene("HomePage");
    }
}
