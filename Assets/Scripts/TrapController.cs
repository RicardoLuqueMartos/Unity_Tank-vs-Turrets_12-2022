using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : BaseController
{
    #region Variables
    [Header("Trap Controller")]

    [SerializeField]
    bool ForceFire = false;

    #endregion Variables

    private void Update()
    {
        if (ForceFire) HandleFire();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other != null 
            && other.gameObject != null
            && tankController != null
            && tankController.gameObject != null
            && other.gameObject == tankController.gameObject)
            
            HandleFire();
    }

    void HandleFire()
    {
    //    Debug.Log("HandleFire");

        Fire();
    }
}
