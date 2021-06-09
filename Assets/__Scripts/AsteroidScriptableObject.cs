using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Asteroid", menuName = "ScriptableObjects/Asteroid")]
public class AsteroidScriptableObject : ScriptableObject
{
    public List<GameObject> asteroids = new List<GameObject>();

    public List<ParticleSystem> explosions = new List<ParticleSystem>();

    public int size3Score;
    public int size2Score;
    public int size1Score;

    public GameObject PickRandomModel()
    {
        return asteroids[Random.Range(0, asteroids.Count)];
    }

    public ParticleSystem PickRandomExplosion()
    {
        return explosions[Random.Range(0, explosions.Count)];
    }
}
