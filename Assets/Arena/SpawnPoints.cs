using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arena
{
    [CreateAssetMenu(fileName = "SpawnPoints", menuName = "Scriptable Objects/SpawnPoints")]
    public class SpawnPoints : ScriptableObject
    {
        public List<Vector3> spawnPointList = new();


    }
}
