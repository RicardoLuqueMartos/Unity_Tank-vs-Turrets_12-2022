using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMeByBullets : MonoBehaviour
{
    [SerializeField]
    public TankController tankController;

    [SerializeField]
    protected int MaxLifePoint = 1; // -1 means infinite

    [SerializeField]
    protected int LifePoint = 1;

    #region Damages & death
    void OnCollisionEnter(Collision collision) // object is collided by anther object, verify if the other is an ennemy bullet
    {
        // verify if the collision is an entering ennemy bullet
        if (collision.transform.GetComponent<BulletController>())
        {
            // Receive damages from the bullet
            ReceiveDamages(collision.transform.GetComponent<BulletController>().GetDamages());
        }
    }

    protected void ReceiveDamages(int damages) // the object receives damages from a colliding ennemy bullet
    {
        // verify if the object is not invincible
        if (MaxLifePoint != -1)
        {
            // kill / destroy self at 0 life point left
            if (LifePoint - damages <= 0)
            {
                LifePoint = 0;

                if (tankController == null)
                    tankController = FindObjectOfType<TankController>();
                              
                DestroySelf();
            }
            // apply damages amount
            else
            {
                LifePoint = LifePoint - damages;
            }
        }
    }

    void DestroySelf() // destroy itself and depending objects
    {
        Destroy(gameObject);      
    }

    #endregion Damages & death
}
