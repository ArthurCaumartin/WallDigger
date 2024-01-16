using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private TerrainManager _terrainManager;
    [SerializeField] private CanvasManager _canvasManager;
    [SerializeField] private GameObject _player;


    void Awake()
    {
        instance = this;
    }

    public void KillPlayer()
    {
        _player.GetComponent<PlayerMovement>().enabled = false;
        Debug.LogWarning("Player is dèd");

        _canvasManager.PlayerDead();
    }

    public void ResetGame()
    {
        _canvasManager.ResetGame();
        _player.GetComponent<PlayerMovement>().enabled = true;
        _player.transform.position = Vector3.zero;

        _terrainManager.CreateNewTerrain();
    }
}
