using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleDrop : MonoBehaviour
{
    [SerializeField] private List<GameObject> Collectibles = new List<GameObject>();

    public void SpawnCollectible()
    {
        int rand = Random.Range(0, Collectibles.Count);

        GameObject drop = Collectibles[rand];

        drop = Instantiate(drop, transform.position + Vector3.forward * 2, Quaternion.identity);
    }
}
