using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDetectorTrigger : MonoBehaviour
{
    [SerializeField]
    TurretController ParentTurret;

  

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TankController>())
        {
            ParentTurret.SetTarget(other.GetComponent<TankController>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TankController>())
        {
            ParentTurret.SetTarget(null);
        }
    }

}
