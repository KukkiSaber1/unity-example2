using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene3 : MonoBehaviour
{
    //Load scene
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
    }

    //Quit Game
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player has quit the game");
    }
}
