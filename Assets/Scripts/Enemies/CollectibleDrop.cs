using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleDrop : MonoBehaviour
{
    [SerializeField] private List<GameObject> Collectibles = new List<GameObject>();

    void SpawnCollectible()
    {
        int rand = Random.Range(0, Collectibles.Count);

        GameObject drop = Collectibles[rand];
    }
}
