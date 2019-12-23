using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class PlayerController : EntityBehaviour<IPlayerState>
{
    Vector3 movementInput = Vector3.zero;
    Vector2 mouseInput = Vector2.zero;

    public override void Attached()
    {
        base.Attached();

        state.SetTransforms(state.StateTransform, transform);
    }

    public override void SimulateController()
    {
        base.SimulateController();


        IMovementCommandInput cmd = MovementCommand.Create();
        cmd.Movement = movementInput;
        cmd.Mouse = mouseInput;
        entity.QueueInput(cmd);
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        base.ExecuteCommand(command, resetState);

        MovementCommand cmd = command as MovementCommand;

        if (resetState) // Controller only.
        {
            transform.position = cmd.Result.Position;
            transform.rotation = cmd.Result.Rotation;
        }
        else // Controller and owner.
        {
            transform.position = transform.position + cmd.Input.Movement * BoltNetwork.FrameDeltaTime;
            cmd.Result.Position = transform.position;

            transform.rotation *= Quaternion.Euler(cmd.Input.Mouse.y, cmd.Input.Mouse.x, 0);
            cmd.Result.Rotation = transform.rotation;
        }
    }

    private void Update()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");
        mouseInput.x = Input.GetAxisRaw("Mouse X");
        mouseInput.y = Input.GetAxisRaw("Mouse Y");
    }
}
