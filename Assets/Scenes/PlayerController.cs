
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float rotationSpeed = 12f;
    public float gravity = -20f;

    [Header("Kick")]
    public float kickDistance = 0.3f;
    public float lungeDuration = 0.15f;
    public float kickDuration = 1f;
    public float kickHitDelay = 0.3f;

    [Header("Hit Detection")]
    public float hitRadius = 0.4f;
    public float hitDistance = 1.5f;
    public LayerMask hitLayers;

    [Header("Stamina")]
    public StaminaSystem staminaSystem;

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;

    // This is a BOOLEAN, not a coroutine
    private bool isKicking;

    private int speedHash;
    private int kickHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        speedHash = Animator.StringToHash("Speed");
        kickHash = Animator.StringToHash("Kick");

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Automatically find StaminaSystem
        if (staminaSystem == null)
        {
            staminaSystem = GetComponent<StaminaSystem>();
        }

        if (staminaSystem == null)
        {
            Debug.LogError(
                "StaminaSystem is missing! Add it to your Player."
            );
        }
    }

    void Update()
    {
        if (animator == null)
            return;

        if (isKicking)
            return;

        MovePlayer();

        // Press E to kick
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (staminaSystem == null)
                return;

            // Check and consume stamina
            if (!staminaSystem.UseStamina())
                return;

            // Start the kick coroutine
            StartCoroutine(Kick());
        }
    }

    void MovePlayer()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move;

        // Camera-relative movement
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            move = forward * z + right * x;
        }
        else
        {
            move = new Vector3(x, 0f, z);
        }

        if (move.magnitude > 1f)
            move.Normalize();

        bool isMoving = move.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // Rotate toward movement direction
        if (isMoving)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Blend Tree
        float animSpeed = 0f;

        if (isMoving)
        {
            animSpeed = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat(
            speedHash,
            animSpeed,
            0.1f,
            Time.deltaTime
        );

        ApplyGravity();
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    // KICK COROUTINE
    IEnumerator Kick()
    {
        isKicking = true;

        // Stop walking and running animations
        animator.SetFloat(speedHash, 0f);

        // Play kick animation
        animator.ResetTrigger(kickHash);
        animator.SetTrigger(kickHash);

        // Wait until impact frame
        yield return new WaitForSeconds(kickHitDelay);

        // Break box at impact
        CheckForBreakable();

        // Short forward lunge
        float timer = 0f;

        while (timer < lungeDuration)
        {
            float step = (kickDistance / lungeDuration)
                       * Time.deltaTime;

            controller.Move(transform.forward * step);

            ApplyGravity();

            timer += Time.deltaTime;

            yield return null;
        }

        // Wait for Kick animation state
        float waitTimer = 0f;
        bool kickStarted = false;

        while (waitTimer < 2f)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName("Kick"))
            {
                kickStarted = true;
                break;
            }

            waitTimer += Time.deltaTime;
            yield return null;
        }

        if (kickStarted)
        {
            // Wait until Kick animation finishes
            float exitTimer = 0f;

            while (exitTimer < kickDuration + 2f)
            {
                AnimatorStateInfo state =
                    animator.GetCurrentAnimatorStateInfo(0);

                if (!state.IsName("Kick") &&
                    !animator.IsInTransition(0))
                {
                    break;
                }

                exitTimer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(kickDuration);
        }

        // Reset animation trigger
        animator.ResetTrigger(kickHash);

        // Allow player movement again
        isKicking = false;
    }

    void CheckForBreakable()
    {
        Vector3 origin = transform.position
                       + Vector3.up * 1f;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            hitRadius,
            transform.forward,
            hitDistance,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            // Ignore player's own colliders
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                continue;
            }

            BreakableObject box =
                hit.collider.GetComponentInParent<BreakableObject>();

            if (box != null)
            {
                box.DestroyBox();
                break;
            }
        }
    }
}