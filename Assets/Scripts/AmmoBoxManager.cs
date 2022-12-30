using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AmmoBoxManager : MonoBehaviour
{
    [SerializeField]
    int AmmoAmount = 10;

    [SerializeField]
    AudioClip PickUpSound;

    [SerializeField]
    GameObject PickUpSoundPlayerPrefab;

    public int GetAmmos()
    {
        PlayPickUpSound();
        return AmmoAmount;
    }

    void PlayPickUpSound()
    {
        GameObject soundPlayer = Instantiate<GameObject>(PickUpSoundPlayerPrefab, transform.position, new Quaternion(0, 0, 0, 0));
        soundPlayer.GetComponent<AudioSource>().PlayOneShot(PickUpSound);
    }
}
