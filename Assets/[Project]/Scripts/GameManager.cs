using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject _player;


    void Awake()
    {
        instance = this;
    }

    public void KillPlayer()
    {
        _player.GetComponent<PlayerMovement>().enabled = false;
        Debug.LogWarning("Player is dèd");
    }
}
