using Controllers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject levelController;
    
    //Awake is always called before any Start functions
    void Awake()
    {
        //we only have a single level for this demo, this makes the game loading easy. We will just simply load that specific level.
        if (LevelController.instance == null)
            //Instantiate gameManager prefab
            Instantiate(levelController);
    }
}