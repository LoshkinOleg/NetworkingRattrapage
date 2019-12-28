using UnityEngine;
using Bolt;
using Bolt.LagCompensation;
using TMPro;

public class PlayerController : EntityEventListener<IPlayerState>
{
    #region Fields
    struct Input
    {
        public float mouseX;
        public bool left;
        public bool up;
        public bool right;
        public bool down;
        public bool firing;
    }

    // Public properties.
    public int Health => health;
    public bool IsAlive
    {
        get
        {
            return health > 0;
        }
    }
    public int PlayerId { get; set; }

    // Player values.
    [SerializeField] [Range(0.0f, float.MaxValue)] float movementSpeed = 5.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float mouseSensitivity = 2.0f;
    [SerializeField] [Range(0.0f, 180.0f)] float fieldOfView = 90.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float shotCooldown = 2.0f;
    [SerializeField] [Range(1, System.Int16.MaxValue)] int defaultHealth = 3;

    // Player camera to instanciate.
    [SerializeField] GameObject cameraPrefab = null;

    // Components.
    [SerializeField] CharacterController charaController = null;

    // Network related.
    Input input;
    bool isRegistered = false;

    // Gameplay related.
    float reloadingTimer = 0;
    int health = 0;
    #endregion

    #region Inherited
    // Monobehaviour inherited.
    private void Start()
    {
        health = defaultHealth;
    }
    private void OnGUI()
    {
        DisplayDebugData();
    }
    private void Update()
    {
        if (!isRegistered)
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.RegisterPlayer(this);
            }
        }
    }

    // Bolt inherited.
    public override void Attached()
    {
        // Sync the transforms.
        state.SetTransforms(state.StateTransform, entity.transform);
    }
    public override void ControlGained()
    {
        SetupCamera();
    }
    public override void OnEvent(DamagePlayer evnt)
    {
        DmgPlayer(evnt.Target);
    }
    public override void SimulateController()
    {
        UpdateInputs();

        // Queue inputs.
        var cmd = MoveCommand.Create();
        cmd.MouseX = input.mouseX;
        cmd.Left = input.left;
        cmd.Up = input.up;
        cmd.Right = input.right;
        cmd.Down = input.down;
        entity.QueueInput(cmd);
    }
    public override void ExecuteCommand(Command command, bool resetState)
    {
        MoveCommand cmd = command as MoveCommand;

        // Process command.
        if (resetState) // Reset internal state order received from owner. (Note: unnecessary? resetState is for internal state, PlayerState is force updated?)
        {
            SetTransformValues(cmd.Result.Position, cmd.Result.Rotation);
        }
        else
        {
            if (cmd.IsFirstExecution) // Ran once at the start of every frame.
            {
                if (IsAlive)
                {
                    Move();
                    Rotate();

                    cmd.Result.Position = transform.position;
                    cmd.Result.Rotation = transform.rotation;

                    Shoot(cmd.ServerFrame);
                }
                else // Respawn.
                {
                    // Note: it is executed once at the start of frame, meaning upon respawning, the rollback will apply previous inputs?
                    SetTransformValues(GameManager.Instance.FindRespawnPoint(this).transform.position, Quaternion.identity);
                    cmd.Result.Position = transform.position;
                }
            }
            else // Subsequent executions for rollback only.
            {
                if (IsAlive)
                {
                    Move();
                    Rotate();

                    cmd.Result.Position = transform.position;
                    cmd.Result.Rotation = transform.rotation;
                }
            }
        }
    }
    #endregion

    // Public methods.

    // Private methods.
    void UpdateInputs()
    {
        var horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
        var vertical = UnityEngine.Input.GetAxisRaw("Vertical");

        input.mouseX = UnityEngine.Input.GetAxisRaw("Mouse X");
        input.left = horizontal < 0.0f;
        input.up = vertical > 0.0f;
        input.right = horizontal > 0.0f;
        input.down = vertical < 0.0f;
        input.firing = UnityEngine.Input.GetButtonDown("Fire1");
    }
    void SetupCamera()
    {
        var cameraTransform = Instantiate(cameraPrefab).transform;
        cameraTransform.SetPositionAndRotation(transform.position + transform.up, transform.rotation);
        cameraTransform.SetParent(transform);
        cameraTransform.gameObject.GetComponent<Camera>().fieldOfView = fieldOfView;
    }
    void DmgPlayer(BoltEntity targetEntity)
    {
        BoltLog.Info("My entity is: " + entity.ToString() + "; Target is: " + targetEntity.ToString());

        if (entity == targetEntity)
        {
            BoltLog.Info(entity.NetworkId + ": Damaging self.");
            health--;
            BoltLog.Info(entity.NetworkId + ": Health updated: " + health.ToString());
        }
    }
    void DisplayDebugData()
    {
        if (entity.IsControlled)
        {
            GUILayout.Label("Health: " + health.ToString());
            GUILayout.Label("ShootingCD: " + reloadingTimer);
        }
    }
    void SetTransformValues(Vector3 position, Quaternion rotation)
    {
        entity.transform.position = position;
        entity.transform.rotation = rotation;
    }
    void Move()
    {
        var playerTransform = entity.transform;

        // Move.
        var newMovement = Vector3.zero;
        if (input.left) newMovement -= playerTransform.right;
        if (input.up) newMovement += playerTransform.forward;
        if (input.right) newMovement += playerTransform.right;
        if (input.down) newMovement -= playerTransform.forward;
        newMovement.Normalize();

        charaController.Move(newMovement * movementSpeed * BoltNetwork.FrameDeltaTime);
    }
    void Rotate()
    {
        entity.transform.rotation *= Quaternion.Euler(0, input.mouseX * mouseSensitivity, 0);
    }
    void Shoot(int serverFrame)
    {
        reloadingTimer -= BoltNetwork.FrameDeltaTime;

        if (input.firing && reloadingTimer < 0)
        {
            // Reset shooting cooldown.
            reloadingTimer = shotCooldown;

            using (var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position + entity.transform.forward, entity.transform.forward), serverFrame))
            {
                if (hits.count > 0)
                {
                    DamagePlayer damageEvnt = DamagePlayer.Create(GlobalTargets.Everyone);
                    damageEvnt.Target = hits.GetHit(0).body.GetComponent<BoltEntity>();
                    BoltLog.Info("Sending damage event with target: " + damageEvnt.Target.ToString());
                    damageEvnt.Send(); // Problem: Sends this only to the copies of THIS entity, not other PlayerControllers....
                    // Sln: make THIS entity across all machines damage the defined entity.
                }
                else
                {
                    BoltLog.Info("Missed");
                }
            }
        }
    }
}
