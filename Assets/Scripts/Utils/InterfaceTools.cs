using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public interface ICustomDropDown<T>
    {
        void Init(List<T> items);
        int CurrentIndex { get; set; }
        void OnIndexChanged(int index);
        T GetCurrentSelectedItem();
    }
}