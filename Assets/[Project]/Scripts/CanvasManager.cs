using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject deathTexte;

    public void PlayerDead()
    {
        deathTexte.SetActive(true);
    }

    public void ResetGame()
    {
        deathTexte.SetActive(false);
    }
}
