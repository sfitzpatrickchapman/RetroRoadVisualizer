using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject particlesPrefab;
    private bool readyToSpawn = true;
    private int curFrame = 0;
    private int prevSpawnFrame;

    void Start()
    {
        SpawnParticles();
    }

    void Update()
    {
        //Debug.Log((int)Camera.main.transform.position.z + 50);

        if ((int)Camera.main.transform.position.z % 50 == 0)
        {
            if (readyToSpawn)
            {
                SpawnParticles();
                readyToSpawn = false;
                prevSpawnFrame = curFrame;
            }
        }

        if (curFrame - prevSpawnFrame > 100)
            readyToSpawn = true;

        curFrame++;
    }

    void SpawnParticles()
    {
        Debug.Log("Spawning particle system...");
        GameObject curParticles = Instantiate(particlesPrefab);
        curParticles.transform.parent = transform;
        curParticles.SetActive(true);

        // Transform to camera z position
        Vector3 newParticlesPos = curParticles.transform.position;
        newParticlesPos.z = Camera.main.transform.position.z;
        curParticles.transform.position = newParticlesPos;
    }
}
