using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BulletController : MonoBehaviour
{
    #region Variable
    enum MoveSystemEnum { ByForce, ByTranslate}
    [SerializeField]
    private MoveSystemEnum MoveSystem = new MoveSystemEnum();

    [SerializeField]
    private int Damages = 1;

    [SerializeField]
    private float MaxDistance = 1;

    [SerializeField]
    private float MoveSpeed = 20;

    bool moving = true;

    [SerializeField]
    private float bounceForce = 20;

    Vector3 StartPosition;

    Rigidbody _rigidBody;
    #endregion Variable

    void OnEnable()
    {
        StartPosition = transform.position;
        _rigidBody = GetComponent<Rigidbody>();
        moving = true;
    }

    private void Update()
    {
        if (moving) // the bullet has been shot and a force is applied to it
        {
            if (MoveSystem == MoveSystemEnum.ByForce)
            {
                //apply a force to the bullet to move it
                _rigidBody.AddForce(transform.up * MoveSpeed);
            }
            else if (MoveSystem == MoveSystemEnum.ByTranslate)
            {
                transform.Translate(Vector3.up * (Time.deltaTime * MoveSpeed));
            }
        }

        // get the distance between
        if (GetDistance() >= MaxDistance) DestroySelf();
    }

    float GetDistance() // returns the distance between the Bullet and its starting position 
    {
        float distance = Vector3.Distance(StartPosition, transform.position);

        return distance;
    }

    public int GetDamages() // returns the damages from the bullet to apply to the target
    {
        return Damages;
    }

    void OnCollisionEnter(Collision collision) // when the bullet collides with an object
    {    
        // Apply an Explosion Force at the collision point
        ApplyExplosionForce(collision);

        // destroy the bullet after it to collide with anything
        if (collision.transform.tag != "NoBulletCollision")
        DestroySelf();
    }

    void ApplyExplosionForce(Collision collision) // Apply an Explosion Force at the collision point
    {
        // get the rigidbody of the collided object to apply the explosion force to
        Rigidbody otherRigidBody = collision.transform.GetComponent<Rigidbody>();

        // apply the explosion force to the target rigidbody if relevant
        if (otherRigidBody != null)       
            otherRigidBody.AddExplosionForce(bounceForce, collision.contacts[0].point, 5);
    }

    void DestroySelf() // Destroy the bullet by itself
    {
        Destroy(gameObject);
    }
}
