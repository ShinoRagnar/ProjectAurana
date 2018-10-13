using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class ME_ParticleTrails : MonoBehaviour, NeedsDisabling
{
    public GameObject TrailPrefab;
    public bool update = true;
    public bool turnOff = false;

    private ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    private ListDictionary<uint, GameObject> hashTrails = new ListDictionary<uint, GameObject>();

    void Start()
    {
        if(ps == null) { 
            ps = GetComponent<ParticleSystem>();
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

    void OnEnable()
    {
        //  InvokeRepeating("ClearEmptyHashes", 1, 1);
        Enable();
    }

    void OnDisable()
    {
        Disable();
     //   CancelInvoke("ClearEmptyHashes");
    }
    public void Enable()
    {
        update = true;
        turnOff = false;
    }

    public void Disable()
    {
        //Debug.Log("Disabled");
        update = false;
        UpdateTrail();
        foreach(GameObject g in hashTrails)
        {
            Destroy(g);
        }
    }

    void Update()
    {
        if (!turnOff)
        {
            turnOff = UpdateTrail();
        }

    }

    bool UpdateTrail()
    {
        int count = ps.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            if (!hashTrails.Contains(particles[i].randomSeed))
            {
                var go = Instantiate(TrailPrefab, transform.position, new Quaternion());
                //go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.parent = Global.References[SceneReferenceNames.NodeClone];//Global.instance.X_CLONES;

                hashTrails.Add(particles[i].randomSeed, go);
                var trail = go.GetComponent<LineRenderer>();
                trail.widthMultiplier *= particles[i].startSize;
            }
            else
            {
                var go = hashTrails[particles[i].randomSeed];
                if (go != null)
                {
                    var trail = go.GetComponent<LineRenderer>();
                    
                    if (update) { 
                        if(trail.enabled == false)
                        {
                            trail.enabled = true;
                        }
                        trail.startColor *= particles[i].GetCurrentColor(ps);
                        trail.endColor *= particles[i].GetCurrentColor(ps);

                        if (ps.main.simulationSpace == ParticleSystemSimulationSpace.World)
                            go.transform.position = particles[i].position;
                        if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                            go.transform.position = ps.transform.TransformPoint(particles[i].position);
                    }
                    else
                    {
                        if (trail.enabled == true)
                        {
                            trail.enabled = false;

                        }
                    }

                }
            }
        }
        if (!update)
        {
            return true;
        }
        return false;
    }

    /*void ClearEmptyHashes()
    {
        hashTrails = hashTrails.Where(h => h.Value != null).ToDictionary(h => h.Key, h => h.Value);
    }*/
}
