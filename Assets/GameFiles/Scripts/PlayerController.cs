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
        public bool firing;
    }

    // Player values.
    [SerializeField] [Range(0.0f, float.MaxValue)] float movementSpeed = 5.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float mouseSensitivity = 2.0f;
    [SerializeField] [Range(0.0f, 180.0f)] float fieldOfView = 90.0f;

    // Player camera to instanciate.
    [SerializeField] GameObject cameraPrefab = null;

    // Components.
    [SerializeField] CharacterController charaController = null;

    // Network related.
    Input input;

    // Bolt inherited.
    public override void Attached()
    {
        // Sync the transforms.
        state.SetTransforms(state.StateTransform, entity.transform);
    }
    public override void ControlGained()
    {
        // Instanciate a camera for the player.
        var cameraTransform = Instantiate(cameraPrefab).transform;
        cameraTransform.SetPositionAndRotation(transform.position + transform.up, transform.rotation);
        cameraTransform.SetParent(transform);
        cameraTransform.gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;

        entity.transform.position = transform.position;
        entity.transform.rotation = transform.rotation;
    }
    public override void SimulateController()
    {
        // Queue inputs.
        var horizontal  = UnityEngine.Input.GetAxisRaw("Horizontal");
        var vertical    = UnityEngine.Input.GetAxisRaw("Vertical");

        input.mouseX    = UnityEngine.Input.GetAxisRaw("Mouse X");
        input.left      = horizontal < 0.0f;
        input.up        = vertical > 0.0f;
        input.right     = horizontal > 0.0f;
        input.down      = vertical < 0.0f;
        input.firing    = UnityEngine.Input.GetButtonDown("Fire1");

        var cmd = MoveCommand.Create();
        cmd.MouseX  = input.mouseX;
        cmd.Left    = input.left;
        cmd.Up      = input.up;
        cmd.Right   = input.right;
        cmd.Down    = input.down;
        entity.QueueInput(cmd);

        if (UnityEngine.Input.GetKeyDown(KeyCode.M))
        {
            BoltLog.Info("Sessions count: " + BoltNetwork.SessionList.Count);
            foreach (var item in BoltNetwork.SessionList)
            {
                BoltLog.Info(item.Value.HostName);
            }
        }
    }
    
    public override void ExecuteCommand(Command command, bool resetState)
    {
        MoveCommand cmd = command as MoveCommand;

        // Process command.
        if (resetState) // Ran on controller only.
        {
            entity.transform.position = cmd.Result.Position;
            entity.transform.rotation = cmd.Result.Rotation;
        }
        else // Ran on controller and owner (same host in this game's case).
        {
            var playerTransform = entity.transform;

            // Move.
            var newMovement = Vector3.zero;
            if (input.left) newMovement     -= playerTransform.right;
            if (input.up) newMovement       += playerTransform.forward;
            if (input.right) newMovement    += playerTransform.right;
            if (input.down) newMovement     -= playerTransform.forward;
            newMovement.Normalize();

            charaController.Move(newMovement * movementSpeed * BoltNetwork.FrameDeltaTime);
            cmd.Result.Position = playerTransform.position;

            // Rotate.
            entity.transform.rotation *= Quaternion.Euler(0,input.mouseX * mouseSensitivity, 0);
            cmd.Result.Rotation = playerTransform.rotation;

            // Shoot.
            if (cmd.IsFirstExecution)
            {
                if (input.firing)
                {
                    var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, entity.transform.forward), cmd.ServerFrame);
                    if (hits.count > 0)
                    {
                        BoltLog.Info("Hit something");
                    }
                }
            }
        }
    }
}
