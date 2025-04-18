using UnityEngine;
using ExtraMath;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController cc;
    [SerializeField] private Transform camBody;
    [SerializeField] private Camera cam;
    [HideInInspector] public Transform Camera { get { return camBody; } }
    private PlayerInput input;

    [Header("Stats")]
    [SerializeField] private float accelGravity;
    [SerializeField] private float accel = 10f;
    [SerializeField] private float maxBaseSpeed = 7f;
    [SerializeField] private float maxSprintSpeed = 14f;
    [SerializeField] private float jumpPower = 3f;

    private Vector3 vel = new();
    private RaycastHit ground;
    private RaycastHit ledgeGrabDir;
    private RaycastHit ledge;
    private bool onGround;
    private bool canClimb = true;
    private bool ledgegrabReset = false;

    private InputAction mouseInput = new();
    private InputAction move = new();
    private InputAction jump = new();
    private InputAction sprint = new();

    private void Start()
    {
        input = GetComponent<PlayerInput>();

        //Cursor.lockState = CursorLockMode.Locked;

        jump = input.actions.FindAction("Jump");
        move = input.actions.FindAction("Move");
        mouseInput = input.actions.FindAction("Look");
        sprint = input.actions.FindAction("Sprint");

        StartCoroutine(CameraZooming());
    }

    private void Update()
    {
        Looking();

        Vector2 movementInput = move.ReadValue<Vector2>();

        //resets velocity
        vel = cc.velocity;

        //check if we're on the ground
        CheckOnGround(); 

        if (onGround)
        {
            // are we trying to move?
            if (Mathf.Abs(movementInput.x) >= 0.1f || Mathf.Abs(movementInput.y) >= 0.1f) 
            {
                StraightRedirect(movementInput);
                Traverse(movementInput);
            }
            else
            {
                SlowDownStandby();
            }

            CheckJump();
        }

        //apply gravity
        vel += accelGravity * Time.deltaTime * Vector3.down;

        if (ledgegrabReset)
        {
            Vector3 pull = ledgeGrabDir.normal;
            pull.y = 0;
            pull = -2 * pull.normalized;
            vel += pull;
            ledgegrabReset = false;
        }
        else
        {
            //check for ledge grabs
            if (canClimb && 
                jump.triggered &&
                Physics.Raycast(transform.position + (2f * Vector3.up) + transform.forward, Vector3.down, out ledge, 2f)
                )
            {
                if (Physics.Raycast(transform.position, transform.forward, out ledgeGrabDir, 2f) && ledge.distance is >= 1e-6f and <= 1.25f)
                {
                    StartCoroutine(ClimbSequence());
                }
                else
                {
                    vel.y += 5f;
                }
            }
        }

        //apply velocity
        cc.Move(vel * Time.deltaTime); 
    }

    private IEnumerator ClimbSequence()
    {
        canClimb = false;
        vel = 5f * (3.03f - ledge.distance) * Vector3.up;
        yield return new WaitForSeconds(.2f);
        ledgegrabReset = true;
        canClimb = true;
    }

    private IEnumerator CameraZooming()
    {
        float rTime = 0f;
        float targetFOV = 90f;
        float previousFOV = 90f;
        while (true)
        {
            while(rTime <= 0.1f)
            {
                cam.fieldOfView = Mathf.Lerp(previousFOV, targetFOV, rTime * 10f);
                rTime += Time.deltaTime;
                yield return null;
            }
            previousFOV = targetFOV;
            targetFOV = Mathf.Clamp(cc.velocity.magnitude/2f, 0f, 10f) + 90f;
            rTime = 0f;
            yield return null;
        }
    }

    private void CheckJump()
    {
        if (jump.triggered)
        {
            vel += (jumpPower + (.1f * vel.magnitude)) * Vector3.up;
        }
    }

    private void SlowDownStandby()
    {
        if (new Vector2(vel.x, vel.z).magnitude >= 1f)
        {
            // simulate traction
            vel -= ResistiveVector(vel, 7f);
        }
        else
        {
            // stop when too slow
            vel = Vector3.zero; 
        }
    }

    private Vector3 ResistiveVector(Vector3 v, float intensity)
    {
        return intensity * (ExMath.Logistic(75f - v.magnitude) + 1f) / 2f * Time.deltaTime * v;
    }

    private void Traverse(Vector2 movementInput)
    {
        Vector3 acceleratedVel = vel + (accel * Time.deltaTime * ((Vector3.ProjectOnPlane(transform.forward, ground.normal).normalized * movementInput.y)
                                                                  + (Vector3.ProjectOnPlane(transform.right, ground.normal).normalized * movementInput.x)).normalized);

        float currentMaxSpeed = (sprint.ReadValue<float>() > 0.5 ? maxSprintSpeed : maxBaseSpeed);
        // will we go too fast?
        if (acceleratedVel.magnitude <= currentMaxSpeed) 
        {
            // accelerate
            vel = acceleratedVel; 
        }
        else
        {
            vel -= ResistiveVector(vel.normalized * currentMaxSpeed, 3f);
        }
    }

    private void StraightRedirect(Vector2 movementInput)
    {
        // magical vectors bending
        float inputDeviation = Vector3.Angle(Vector3.forward, new Vector3(movementInput.x, 0f, movementInput.y)) * Mathf.Deg2Rad * Mathf.Sign(movementInput.x);
        float sinID = -Mathf.Sin(inputDeviation);
        float cosID = Mathf.Cos(inputDeviation);
        Vector3 forwardMove = Vector3.ProjectOnPlane(new Vector3((transform.forward.x * cosID) - (transform.forward.z * sinID), 0f, (transform.forward.x * sinID) + (transform.forward.z * cosID)), ground.normal);

        //go very straight
        vel = Vector3.Project(vel, forwardMove); 
    }

    private void Looking()
    {
        // read mouse input
        Vector2 mause = .2f * mouseInput.ReadValue<Vector2>();
        // turns your body
        transform.localEulerAngles += new Vector3(0f, mause.x);
        // turns your head
        Vector3 camrot = camBody.localEulerAngles - new Vector3(mause.y, 0f);
        // stops breaking your neck
        if (camrot.x is <= 85f or >= 270f) 
        {
            camBody.localEulerAngles = camrot;
        }
        if (camBody.localEulerAngles.x is > 85f and < 270f)
        {
            camBody.localEulerAngles = camBody.localEulerAngles.x < 177.5f ? 85f * Vector3.right : 270f * Vector3.right;
        }
    }

    private void CheckOnGround()
    {
        onGround = Physics.SphereCast(transform.position, 0.45f, Vector3.down, out RaycastHit hit, .61f);
        if (onGround)
        {
            if (!Physics.Raycast(transform.position + .9f * Vector3.down, Vector3.down, .2f))
            {
                Vector3 overGround = hit.normal;
                overGround.y = 0;
                overGround = -overGround.normalized + 3 * Vector3.up;
                Physics.Raycast(hit.point + 1e-3f * overGround, Vector3.down, out ground, 1f, LayerMask.GetMask("Default"));
            }
            else
            {
                ground = hit;
            } 
        }
    }
}
