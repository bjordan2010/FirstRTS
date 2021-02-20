using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Player : MonoBehaviour
{
    public HUD hud;
    public string username;
    public bool human;
    public WorldObject SelectedObject { get; set; }

    public int startMoney, startMoneyLimit, startPower, startPowerLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits;
    
    void Awake() {
        hud = GetComponentInChildren<HUD>();
        resources = InitResourceList();
        resourceLimits = InitResourceLimits();
    }

    // Start is called before the first frame update
    void Start() {
        AddStartResourceLimits();
        AddStartResources();
    }

    // Update is called once per frame
    void Update() {
        if (human) {
            hud.SetResourceValues(resources, resourceLimits);
        }
    }

    // Public Methods
    public void IncrementResourceLimit(ResourceType type, int amount) {
        resourceLimits[type] += amount;
    }

    public void AddResource(ResourceType type, int amount) {
        if (resources[type] + amount <= resourceLimits[type]) {
            resources[type] += amount;
        }    
    }

    public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation) {
        Units units = GetComponentInChildren<Units>();
        GameObject newUnit = (GameObject) Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
        newUnit.transform.parent = units.transform;
    }
    // End Public Methods

    // Private Methods
    private Dictionary<ResourceType, int> InitResourceList() {
        Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
        list.Add(ResourceType.Money, 0);
        list.Add(ResourceType.Power, 0);
        return list;
    }

    private Dictionary<ResourceType, int> InitResourceLimits() {
        Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
        list.Add(ResourceType.Money, 0);
        list.Add(ResourceType.Power, 0);
        return list;
    }

    private void AddStartResourceLimits() {
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
        IncrementResourceLimit(ResourceType.Power, startPowerLimit);
    }

    private void AddStartResources() {
        AddResource(ResourceType.Money, startMoney);
        AddResource(ResourceType.Power, startPower);
    }
    // End Private Methods
}
