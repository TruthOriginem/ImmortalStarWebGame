using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InterfaceTools
{
    public interface ISpawnPoolFrame
    {
        void Spawn(Transform prefab);
        void Despawn(Transform prefab);
        SpawnPool GetSpawnPool();
    }
    public interface ISpawnPoolItem
    {
        void OnSpawned();
        void OnDespawned();
    }
}