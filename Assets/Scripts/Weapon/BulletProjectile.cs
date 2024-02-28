using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : NetworkBehaviour
{
    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;

    [SerializeField] float speed = 100f;

    private Rigidbody bulletRigidBody;

    public Vector3 direction;

    private void Awake()
    {
        bulletRigidBody = GetComponent<Rigidbody>();

        Destroy(gameObject, 10f);
    }

    private void Start()
    {
        bulletRigidBody.velocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BulletTarget>() != null)
        {
            //hit target
            Instantiate(vfxHitRed, transform.position, Quaternion.identity);
        }
        else
        {
            //hit something else
            Instantiate(vfxHitGreen, transform.position, Quaternion.identity);

        }
        Destroy(gameObject);
    }
}
