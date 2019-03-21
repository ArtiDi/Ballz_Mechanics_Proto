using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartLevelScript : MonoBehaviour
{
    
    void Start()
    {
        
    }

    public void RestartLevel()
    {
        //TODO:
        // Save progress
        // Restart current level

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
