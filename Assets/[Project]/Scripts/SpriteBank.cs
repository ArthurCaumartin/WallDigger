using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBank : MonoBehaviour
{
    public static SpriteBank instance;
    [SerializeField] public List<Sprite> TileDurability;

    void Awake()
    {
        instance = this;
    }
}
