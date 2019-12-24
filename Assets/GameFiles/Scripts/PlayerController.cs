using UnityEngine;
using Bolt;

public class PlayerController : EntityBehaviour<IPlayerState>
{
    struct Input
    {
        public float mouseX;
        public bool left;
        public bool up;
        public bool right;
        public bool down;
    }
    struct State
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // Player values.
    [SerializeField] [Range(0.0f, float.MaxValue)] float acceleration = 5.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float maxSpeed = 10.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float mouseSensitivity = 2.0f;
    [SerializeField] [Range(0.0f, 180.0f)] float fieldOfView = 90.0f;

    // Player camera to instanciate.
    [SerializeField] GameObject cameraPrefab = null;

    // Network related.
    Input input;
    State playerState;

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
        cameraTransform.SetPositionAndRotation(transform.position + transform.up, transform.rotation);
        cameraTransform.SetParent(transform);
        cameraTransform.gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;

        playerState.position = transform.position;
        playerState.rotation = transform.rotation;
    }
    public override void SimulateController()
    {
        input.mouseX = UnityEngine.Input.GetAxisRaw("Mouse X");

        var horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
        var vertical = UnityEngine.Input.GetAxisRaw("Vertical");
        input.left = horizontal < 0.0f;
        input.up = vertical > 0.0f;
        input.right = horizontal > 0.0f;
        input.down = vertical < 0.0f;

        var cmd = MovementCommand.Create();
        cmd.MouseX = input.mouseX;
        cmd.Left = input.left;
        cmd.Up = input.up;
        cmd.Right = input.right;
        cmd.Down = input.down;
        entity.QueueInput(cmd);
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        MovementCommand cmd = command as MovementCommand;

        // Process command.
        if (resetState) // Ran on controller only.
        {
            transform.position = cmd.Result.Position;
            transform.rotation = cmd.Result.Rotation;
            playerState.position = cmd.Result.Position;
            playerState.rotation = cmd.Result.Rotation;
        }
        else // ran on controller and owner.
        {
            // Move.
            var newMovement = Vector3.zero;
            if (input.left) newMovement -= transform.right;
            if (input.up) newMovement += transform.forward;
            if (input.right) newMovement += transform.right;
            if (input.down) newMovement -= transform.forward;
            newMovement.Normalize();

            newMovement *= acceleration * BoltNetwork.FrameDeltaTime;
            newMovement = Vector3.ClampMagnitude(newMovement, maxSpeed);

            playerState.position += newMovement;

            // Rotate.
            playerState.rotation *= Quaternion.Euler(new Vector3(0, input.mouseX, 0) * mouseSensitivity);

            // Apply movement and rotation.
            cmd.Result.Position = playerState.position;
            cmd.Result.Rotation = playerState.rotation;
            transform.position = playerState.position;
            transform.rotation = playerState.rotation;
        }
    }
}
