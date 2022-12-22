using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoxManager : MonoBehaviour
{
    [SerializeField]
    int AmmoAmount = 10;

    public int GetAmmos()
    {
        return AmmoAmount;
    }
}
