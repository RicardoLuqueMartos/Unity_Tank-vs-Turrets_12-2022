using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyMeByBullets : MonoBehaviour
{
    [SerializeField]
    public TankController tankController;

    [SerializeField]
    protected int MaxLifePoint = 1; // -1 means infinite

    [SerializeField]
    protected int LifePoint = 1;

    [SerializeField]
    TurretHealthBar healthbar;

    [SerializeField]
    GameObject FXSpawnPoint;

    [SerializeField]
    GameObject FXPrefab;

    [SerializeField]
    GameObject SpawnedFX;
    enum HowDestroyEnum { DestroyObject, DisableComponent }
    [SerializeField]
    private HowDestroyEnum HowDestroy = new HowDestroyEnum();

    [SerializeField]
    AudioSource DestroyedSoundPlayer;

    [SerializeField]
    AudioClip DestroyedSound;

    private void OnEnable()
    {
        if (healthbar != null)
            healthbar.maxHealthPoints = MaxLifePoint;
    }

    public void SetAsAimed()
    {
        if (healthbar != null) healthbar.gameObject.SetActive(true);
    }

    public void NoMoreAimed()
    {
        if (healthbar != null) healthbar.gameObject.SetActive(false);
    }

    #region Damages & death
    void OnCollisionEnter(Collision collision) // object is collided by anther object, verify if the other is an ennemy bullet
    {
        // verify if the collision is an entering ennemy bullet
        if (collision.transform.GetComponent<BulletController>()
            && this.enabled == true)
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

        if (healthbar != null)
            healthbar.UpdateHealthBar(LifePoint);
    }

    void DestroySelf() // destroy itself and depending objects
    {
        if (HowDestroy == HowDestroyEnum.DestroyObject)
        {
            // destroy the object
            Destroy(gameObject);

        }
        else if (HowDestroy == HowDestroyEnum.DisableComponent)
        {
            // disable the object
            this.enabled = false;
        }

        InstantiateFXForDestruction();
        PlayDestructionSound();    
    }

    void InstantiateFXForDestruction()
    {
        if (FXPrefab != null)
        {
            // Instantiate the particle system at the impact position
            GameObject spawner = Instantiate<GameObject>(FXPrefab, FXSpawnPoint.transform.position,
               FXSpawnPoint.transform.rotation);
        }
    }

    void PlayDestructionSound()
    {
        if (DestroyedSoundPlayer != null)
        {
            DestroyedSoundPlayer.enabled = true;
            DestroyedSoundPlayer.Stop();
            DestroyedSoundPlayer.loop = false;
            DestroyedSoundPlayer.PlayOneShot(DestroyedSound);
        }
    }
    #endregion Damages & death
}
