﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Unit : WorldObject
{
    protected bool moving, rotating;
    private Vector3 destination;
    private Quaternion targetRotation;
    public float moveSpeed, rotateSpeed;

    protected override void Awake() {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(rotating) TurnToTarget();
        else if(moving) MakeMove();
    }

    protected override void OnGUI() {
        base.OnGUI();
    }

    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);
        if (player && player.human && currentlySelected) {
            if(hoverObject.name == "Ground") player.hud.SetCursorState(CursorState.Move);
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);
        if(player && player.human && currentlySelected) {
            if(hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
                float x = hitPoint.x;
                float y = hitPoint.y + player.SelectedObject.transform.position.y;
                float z = hitPoint.z;
                Vector3 destination = new Vector3(x, y, z);
                StartMove(destination);
            }
        }
    }

    public void StartMove(Vector3 destination) {
        this.destination = destination;
        targetRotation = Quaternion.LookRotation(destination - transform.position);
        rotating = true;
        moving = false;
    }

    private void TurnToTarget() {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
        Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, 
            -targetRotation.z, -targetRotation.w);
        if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
            rotating = false;
            moving = true;
        }
        CalculateBounds();
    }

    private void MakeMove() {
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
        if(transform.position == destination) moving = false;
        CalculateBounds();
    }
}
