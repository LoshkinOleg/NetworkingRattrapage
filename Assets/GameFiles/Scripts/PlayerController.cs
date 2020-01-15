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

    // Prefabs.
    [SerializeField] GameObject cameraPrefab = null;
    [SerializeField] GameObject rayPrefab = null;

    // Sounds.
    [SerializeField] AudioClip shot = null;
    [SerializeField] AudioClip hurt = null;
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
    EventRelayer evntRelayer = null;

    // Gameplay related.
    const int DEFAULT_HEALTH = 3;
    float reloadingTimer = 0;
    int _health = 0;
    int killCount = 0;
    int deathCount = 0;

    // Bot related.
    bool runBackAndForth = false;
    float backAndForthTimer = 0.0f;
    float backAndForthTimerDefaultValue = 1.0f;
    #endregion

    #region Inherited
    // Monobehaviour inherited.
    private void Start()
    {
        _health = DEFAULT_HEALTH;
        gameManager = GameManager.Get();
        gameManager.RegisterPlayer(this);
        evntRelayer = EventRelayer.Get();
        reloadSoundDuration = reloadDone.length;
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

        if (runBackAndForth)
        {
            backAndForthTimer -= Time.deltaTime;
            if (backAndForthTimer < -backAndForthTimerDefaultValue)
            {
                backAndForthTimer = backAndForthTimerDefaultValue;
            }
        }
    }
    private void OnGUI()
    {
        if (entity.HasControl)
        {
            GUILayout.Label("Kills: " + killCount + "; Deaths: " + deathCount);
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

                    Shoot();
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
    public void ProcessEvent(Bolt.Event evnt, EventType type)
    {
        switch (type)
        {
            case EventType.SHOT:
                {
                    ShotEvent shotEvnt = evnt as ShotEvent;
                    audioSource.PlayOneShot(shot);

                    if (shotEvnt.Shooter == entity)
                    {
                        if (entity.HasControl)
                        {
                            // Perform raycast and dispatch DamagePlayer event.
                            RaycastHit hit;
                            if (Physics.Raycast(new Ray(entity.transform.position + entity.transform.forward, entity.transform.forward), out hit, 100))
                            {
                                if (hit.transform.tag == "Player")
                                {
                                    using (var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position + entity.transform.forward, entity.transform.forward), BoltNetwork.ServerFrame))
                                    {
                                        if (hits.count > 0)
                                        {
                                            // Send damage event with this entity as Sender and hit target as Target entity.
                                            evntRelayer.Send(EventType.DAMAGE, entity, hits[0].body.gameObject.GetComponent<BoltEntity>());
                                        }
                                    }
                                }
                            }
                        }
                        // Instanciate a visual ray.
                        Instantiate(rayPrefab, transform.position, transform.rotation);
                    }
                }
                break;
            case EventType.DAMAGE:
                {
                    DamageEvent damageEvnt = evnt as DamageEvent;
                    if (entity.HasControl)
                    {
                        if (damageEvnt.Target == entity)
                        {
                            audioSource.Stop();
                            audioSource.PlayOneShot(hurt);
                            TakeDamage(damageEvnt.Sender);
                        }
                        else if (damageEvnt.Sender == entity)
                        {
                            audioSource.PlayOneShot(ding);
                        }
                    }
                }
                break;
            case EventType.KILL:
                {
                    KillEvent killEvnt = evnt as KillEvent;
                    if (killEvnt.Killer == entity)
                    {
                        killCount++;
                    }
                }
                break;
        }
    }

    // Private methods.
    void TakeDamage(BoltEntity shooter)
    {
        if (--Health < 1)
        {
            evntRelayer.Send(EventType.KILL, shooter);
            deathCount++;
        }
    }
    void UpdateInputs()
    {
        if (runBackAndForth)
        {
            input.mouseX = 0.0f;
            input.left = false;
            input.right = false;
            input.up = backAndForthTimer > 0.0f ? true : false;
            input.down = backAndForthTimer > 0.0f ? false : true;
            input.firing = false;
        }
        else
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

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            runBackAndForth = !runBackAndForth;
        }
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
    void Shoot()
    {
        reloadingTimer -= BoltNetwork.FrameDeltaTime;

        if (input.firing && reloadingTimer < 0)
        {
            // Send shot event.
            evntRelayer.Send(EventType.SHOT, entity);

            // Start reloading sound timer.
            playingReloadSound = false;

            // Reset shooting cooldown.
            reloadingTimer = shotCooldown;
        }
    }
}
