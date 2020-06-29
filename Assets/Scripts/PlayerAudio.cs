using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> Footsteps = new List<AudioClip>();

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void PlayFootStep()
    {
        int rand = Random.Range(0, Footsteps.Count);
        AudioClip clip = Footsteps[rand];

        source.PlayOneShot(clip);
    }
}
