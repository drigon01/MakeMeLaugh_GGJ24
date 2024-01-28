using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatterOnImpact : MonoBehaviour
{
    public GameObject SplatterPrefab;
    
    public void OnCollisionEnter(Collision other)
    {
        var splatter = Instantiate(SplatterPrefab, transform.position, Quaternion.LookRotation(-other.GetContact(0).normal));
        splatter.transform.parent = other.transform;
        Destroy(gameObject);
    }
}
