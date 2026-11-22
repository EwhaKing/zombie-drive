// PlayerInventory.cs  (빈 오브젝트에 붙이고 최초 씬에 하나만 배치)
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int gold;
    readonly Dictionary<string, int> materials = new();
    readonly Dictionary<string, int> furniture = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);   // 씬이 바뀌어도 유지 (루트 오브젝트여야 함)
    }

    public int GetMaterial(string id) => materials.TryGetValue(id, out var n) ? n : 0;
    public void AddMaterial(string id, int n) => materials[id] = GetMaterial(id) + n;
    public void ConsumeMaterial(string id, int n) => materials[id] = GetMaterial(id) - n;
    public void AddFurniture(string id) => furniture[id] = (furniture.TryGetValue(id, out var n) ? n : 0) + 1;
}