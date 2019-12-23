using UnityEngine;
using Bolt;

public class PlayerController : EntityBehaviour<IPlayerState>
{
    // Player values.
    [SerializeField] [Range(0.0f, float.MaxValue)] float movementSpeed = 10;
    [SerializeField] [Range(0.0f, float.MaxValue)] float mouseSensitivity = 2;
    [SerializeField] [Range(0.0f, 180.0f)] float fieldOfView = 90.0f;

    // Player camera to instanciate.
    [SerializeField] GameObject cameraPrefab = null;

    // Inputs.
    Vector3 movementInput = Vector3.zero;
    Vector2 mouseInput = Vector2.zero;

    // Bolt inherited.
    public override void Attached()
    {
        // Sync the transforms.
        state.SetTransforms(state.StateTransform, transform);
    }
    public override void ControlGained()
    {
        // Instanciate a camera for the player.
        var cameraTransform = Instantiate(cameraPrefab).transform;
        cameraTransform.SetPositionAndRotation(transform.position, transform.rotation);
        cameraTransform.SetParent(transform);
        cameraTransform.gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;
    }
    public override void SimulateController()
    {
        // Issue movement command.
        IMovementCommandInput cmd = MovementCommand.Create();
        cmd.Movement = movementInput;
        cmd.Mouse = mouseInput;
        entity.QueueInput(cmd);
    }
    public override void ExecuteCommand(Command command, bool resetState)
    {
        MovementCommand cmd = command as MovementCommand;

        // Process command.
        if (resetState) // Ran on controller only.
        {
            // Reset state to last acknowledged one.
            transform.position = cmd.Result.Position;
            transform.rotation = cmd.Result.Rotation;
        }
        else // ran on controller and owner.
        {
            // Move.
            transform.position = transform.position + cmd.Input.Movement * BoltNetwork.FrameDeltaTime;
            cmd.Result.Position = transform.position;

            // Rotate.
            transform.rotation *= Quaternion.Euler(0, cmd.Input.Mouse.x, 0);
            cmd.Result.Rotation = transform.rotation;
        }
    }

    // Monobehaviour inherited.
    void Update()
    {
        // Update inputs.
        movementInput = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized * movementSpeed;
        mouseInput.x = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
    }
}
