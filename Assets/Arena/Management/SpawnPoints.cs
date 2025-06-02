using System.Collections.Generic;
using UnityEngine;

namespace Arena.Management
{
    [CreateAssetMenu(fileName = "SpawnPoints", menuName = "Scriptable Objects/SpawnPoints")]
    public class SpawnPoints : ScriptableObject
    {
        public List<Vector3> spawnPointList = new();


    }
}
