using UnityEngine;
using UnityEngine.UI;
using Bolt;

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

    // Player values.
    [SerializeField] [Range(0.0f, float.MaxValue)] float movementSpeed = 5.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float mouseSensitivity = 2.0f;
    [SerializeField] [Range(0.0f, 180.0f)] float fieldOfView = 90.0f;
    [SerializeField] [Range(0.0f, float.MaxValue)] float shotCooldown = 2.0f;

    // Player camera to instanciate.
    [SerializeField] GameObject cameraPrefab = null;

    // Sounds.
    [SerializeField] AudioClip hurt = null;
    [SerializeField] AudioClip shot = null;
    [SerializeField] AudioClip reloadDone = null;
    [SerializeField] AudioClip ding = null;
    float reloadSoundDuration = 0;
    bool playingReloadSound = false;
    bool isLocalPlayer = false;

    // Components.
    [SerializeField] CharacterController charaController = null;
    [SerializeField] AudioSource audioSource = null;
    RawImage screenOverlay = null;

    // Private properties.
    int Health
    {
        get
        {
            return _health;
        }
        set
        {
            if (_health > value) // Damaging the player.
            {
                // Play hurt sound.
                if (isLocalPlayer)
                {
                    audioSource.PlayOneShot(hurt, 1.0f);
                }
                else
                {
                    audioSource.PlayOneShot(ding, 1.0f);
                }

                // Highlight screen in appropriate color.
                if (isLocalPlayer)
                {
                    if (value == 2)
                    {
                        screenOverlay.color = Color.yellow;
                    }
                    else if (value == 1)
                    {
                        screenOverlay.color = Color.red;
                    }
                }
            }
            else if (isLocalPlayer)
            {
                if (value == 3)
                {
                    screenOverlay.color = Color.clear;
                }
            }
            _health = value;
        }
    }

    // Network related.
    Input input;
    GameManager gameManager = null;

    // Gameplay related.
    const int DEFAULT_HEALTH = 3;
    float reloadingTimer = 0;
    int _health = 0;
    #endregion

    #region Inherited
    // Monobehaviour inherited.
    private void Start()
    {
        _health = DEFAULT_HEALTH;
        gameManager = GameManager.GetGM();
        gameManager.RegisterPlayer(this);
        reloadSoundDuration = reloadDone.length;
    }
    private void OnGUI()
    {
        if (entity.HasControl)
        {
            // Display kill to death ratio.
            GUILayout.Label(Health.ToString());
        }
    }
    private void Update()
    {
        if (!playingReloadSound)
        {
            if (reloadingTimer - reloadSoundDuration < 0.05f)
            {
                audioSource.PlayOneShot(reloadDone, 0.5f);
                playingReloadSound = true;
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
        isLocalPlayer = true;
    }
    public override void OnEvent(DamagePlayer evnt)
    {
        gameManager.BroadcastDamagePlayerEvent(evnt.Target.NetworkId);
    }
    public override void SimulateController()
    {
        UpdateInputs();

        // Queue inputs.
        var cmd = PlayerCommand.Create();
        cmd.MouseX = input.mouseX;
        cmd.Left = input.left;
        cmd.Up = input.up;
        cmd.Right = input.right;
        cmd.Down = input.down;
        cmd.Firing = input.firing;
        entity.QueueInput(cmd);
    }
    public override void ExecuteCommand(Command command, bool resetState)
    {
        PlayerCommand cmd = command as PlayerCommand;

        // Process command.
        if (resetState) // Reset internal state order received from owner. (Note: unnecessary? resetState is for internal state, PlayerState is force updated?)
        {
            SetTransformValues(cmd.Result.Position, cmd.Result.Rotation);
        }
        else
        {
            if (cmd.IsFirstExecution) // Ran once at the start of every frame.
            {
                if (Health > 0) // Is alive.
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
                    Health = DEFAULT_HEALTH;
                    SetTransformValues(gameManager.FindRespawnPosition(this), Quaternion.identity);
                    cmd.Result.Position = transform.position;
                }
            }
            else // Subsequent executions for rollback only.
            {
                if (Health > 0)
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
    public void DmgPlayer(NetworkId target)
    {
        audioSource.PlayOneShot(shot, 0.5f);

        if (entity.NetworkId == target)
        {
            Health--;
        }
    }

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

        screenOverlay = cameraTransform.gameObject.GetComponentInChildren<RawImage>();
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
            // Start reloading sound timer.
            playingReloadSound = false;

            // Draw ray.
            BoltNetwork.Instantiate(BoltPrefabs.Ray, transform.position, transform.rotation);

            // Reset shooting cooldown.
            reloadingTimer = shotCooldown;

            // Perform raycast and dispatch DamagePlayer event.

            // BUG: raycast locally first to check against pillars.
            // TODO

            using (var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position + entity.transform.forward, entity.transform.forward), serverFrame))
            {
                if (hits.count > 0)
                {
                    DamagePlayer damageEvnt = DamagePlayer.Create(entity, EntityTargets.Everyone);
                    damageEvnt.Target = hits.GetHit(0).body.GetComponent<BoltEntity>();
                    damageEvnt.Send();
                }
            }
        }
    }
}
