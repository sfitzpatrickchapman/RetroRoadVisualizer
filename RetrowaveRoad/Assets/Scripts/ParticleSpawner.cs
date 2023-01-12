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
    private readonly int spawnFrequency = 50;

    void Update()
    {
        if ((int)Camera.main.transform.position.z % spawnFrequency == 0)
        {
            if (readyToSpawn)
            {
                SpawnParticles();
                readyToSpawn = false;
                prevSpawnFrame = curFrame;

                // Delete old particle effects
                if (transform.childCount > 2)
                    Destroy(GetComponent<Transform>().GetChild(0).gameObject);
            }
        }

        if (curFrame - prevSpawnFrame > 100)
            readyToSpawn = true;

        curFrame++;
    }

    void SpawnParticles()
    {
        GameObject curParticles = Instantiate(particlesPrefab);
        curParticles.transform.parent = transform;
        curParticles.SetActive(true);

        // Transform to camera z position plus offset
        Vector3 newParticlesPos = curParticles.transform.position;
        newParticlesPos.z = Camera.main.transform.position.z + spawnFrequency;
        curParticles.transform.position = newParticlesPos;
    }
}
