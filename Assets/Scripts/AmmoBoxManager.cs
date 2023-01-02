using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AmmoBoxManager : MonoBehaviour
{
    [SerializeField]
    int ammoAmount = 10;

    [SerializeField]
    AudioClip pickUpSound;

    [SerializeField]
    GameObject pickUpSoundPlayerPrefab;

    public int GetAmmos()
    {
        PlayPickUpSound();
        return ammoAmount;
    }

    void PlayPickUpSound()
    {
        GameObject soundPlayer = Instantiate<GameObject>(pickUpSoundPlayerPrefab, transform.position, new Quaternion(0, 0, 0, 0));
        soundPlayer.GetComponent<AudioSource>().PlayOneShot(pickUpSound);
    }
}
