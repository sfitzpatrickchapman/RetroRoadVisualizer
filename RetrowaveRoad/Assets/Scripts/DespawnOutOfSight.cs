using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DespawnOutOfSight : MonoBehaviour
{
    public bool isEnabled = true;

    private void OnBecameInvisible()
    {
        if (isEnabled) Destroy(gameObject);
    }
}
