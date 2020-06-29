using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> Footsteps = new List<AudioClip>();
    [SerializeField] private List<AudioClip> SwordSwings = new List<AudioClip>();
    [SerializeField] private List<AudioClip> HitSounds = new List<AudioClip>();



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

    private void PlaySwordSwing()
    {
        int rand = Random.Range(0, SwordSwings.Count);
        AudioClip clip = SwordSwings[rand];

        source.PlayOneShot(clip);
    }
}
