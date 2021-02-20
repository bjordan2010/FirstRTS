using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankFactory : Building
{
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        actions = new string[] { "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", 
            "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", "Tank", "Tank"};        
    }

    public override void PerformAction(string actionToPerform) {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}
