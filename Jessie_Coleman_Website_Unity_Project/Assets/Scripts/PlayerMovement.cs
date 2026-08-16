using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAnimatorController playerAnimatorController;
    [SerializeField] private string moveXParameter = "moveX";
    [SerializeField] private string moveYParameter = "moveY";
    [SerializeField] private string isMovingParameter = "isMoving";

    [Header("Rail Grind")]
    [SerializeField] private float railGrindSpeed = 6f;
    [SerializeField] private float railTopOffset = 0.6f;
    [SerializeField] private float railSpinSpeed = 12f;
    [SerializeField] private float railExitSidePush = 1.25f;
    [SerializeField] private float railExitForwardPush = 0.5f;
    [SerializeField] private Vector3 railDirectionLocal = Vector3.right;

    [Header("Bouncy")]
    [SerializeField] private float bounceHeight = 5f;
    [SerializeField] private float bounceDuration = 0.8f;
    [SerializeField] private float bouncySquashAmount = 0.65f;
    [SerializeField] private float bouncyStretchAmount = 1.25f;
    [SerializeField] private float bouncyAnimationDuration = 0.45f;

    [Header("Bouncy Sound")]
    [SerializeField] private AudioSource bounceAudioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField, Range(0f, 1f)] private float bounceSoundVolume = 1f;

    private static readonly Vector2[] railSpinDirections =
    {
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
        new Vector2(1f, 0f),
        new Vector2(1f, -1f),
        new Vector2(0f, -1f),
        new Vector2(-1f, -1f),
        new Vector2(-1f, 0f),
        new Vector2(-1f, 1f)
    };

    public bool IsRailGrinding => isRailGrinding;

    private bool isRailGrinding;
    private bool isBouncing;

    private Transform currentRail;
    private Vector3 railCenter;
    private Vector3 railMoveDirection;
    private Vector3 railSideDirection;

    private float railHalfLength;
    private float railDistance;
    private float railCooldown;
    private float railSpinTimer;

    private int railSpinDirectionIndex;

    private Vector3 lastMoveDirection = Vector3.forward;

    private Coroutine bounceRoutine;

    private readonly Dictionary<Transform, Vector3> bouncyBaseScales =
        new Dictionary<Transform, Vector3>();

    private readonly Dictionary<Transform, Coroutine> bouncyRoutines =
        new Dictionary<Transform, Coroutine>();

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (playerAnimatorController == null)
        {
            playerAnimatorController = GetComponent<PlayerAnimatorController>();

            if (playerAnimatorController == null)
            {
                playerAnimatorController = GetComponentInChildren<PlayerAnimatorController>();
            }
        }

        if (bounceAudioSource == null)
        {
            bounceAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (railCooldown > 0f)
        {
            railCooldown -= Time.deltaTime;
        }

        if (isRailGrinding)
        {
            UpdateRailGrind();
            return;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        if (movement.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = movement.normalized;
        }

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bouncy"))
        {
            StartBounce(other.transform);
            return;
        }

        TryStartRailGrind(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        if (other.CompareTag("Bouncy"))
        {
            StartBounce(other.transform);
            return;
        }

        TryStartRailGrind(other);
    }

    private void StartBounce(Transform bouncyObject)
    {
        if (isRailGrinding)
        {
            return;
        }

        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
        }

        bounceRoutine = StartCoroutine(BounceRoutine());

        PlayBounceSound();

        if (bouncyObject != null)
        {
            StartBouncyJellyAnimation(bouncyObject);
        }
    }

    private void PlayBounceSound()
    {
        if (bounceAudioSource == null || bounceSound == null)
        {
            return;
        }

        bounceAudioSource.PlayOneShot(bounceSound, bounceSoundVolume);
    }

    private IEnumerator BounceRoutine()
    {
        isBouncing = true;

        float startY = transform.position.y;
        float timer = 0f;

        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(timer / bounceDuration);
            float height = 4f * bounceHeight * normalizedTime * (1f - normalizedTime);

            Vector3 position = transform.position;
            position.y = startY + height;
            transform.position = position;

            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = startY;
        transform.position = finalPosition;

        isBouncing = false;
        bounceRoutine = null;
    }

    private void StartBouncyJellyAnimation(Transform bouncyObject)
    {
        if (!bouncyBaseScales.ContainsKey(bouncyObject))
        {
            bouncyBaseScales.Add(bouncyObject, bouncyObject.localScale);
        }

        Vector3 baseScale = bouncyBaseScales[bouncyObject];

        if (bouncyRoutines.TryGetValue(bouncyObject, out Coroutine existingRoutine))
        {
            if (existingRoutine != null)
            {
                StopCoroutine(existingRoutine);
            }

            bouncyRoutines.Remove(bouncyObject);
        }

        bouncyObject.localScale = baseScale;

        Coroutine newRoutine = StartCoroutine(BouncyJellyRoutine(bouncyObject, baseScale));
        bouncyRoutines[bouncyObject] = newRoutine;
    }

    private IEnumerator BouncyJellyRoutine(Transform bouncyObject, Vector3 baseScale)
    {
        Vector3 squashScale = new Vector3(
            baseScale.x * bouncyStretchAmount,
            baseScale.y * bouncySquashAmount,
            baseScale.z * bouncyStretchAmount);

        Vector3 stretchScale = new Vector3(
            baseScale.x * 0.9f,
            baseScale.y * bouncyStretchAmount,
            baseScale.z * 0.9f);

        float squashDuration = bouncyAnimationDuration * 0.25f;
        float stretchDuration = bouncyAnimationDuration * 0.3f;
        float settleDuration = bouncyAnimationDuration * 0.45f;

        float timer = 0f;

        while (timer < squashDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / squashDuration);
            bouncyObject.localScale = Vector3.Lerp(baseScale, squashScale, t);

            yield return null;
        }

        timer = 0f;

        while (timer < stretchDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / stretchDuration);
            bouncyObject.localScale = Vector3.Lerp(squashScale, stretchScale, t);

            yield return null;
        }

        timer = 0f;

        while (timer < settleDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / settleDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            bouncyObject.localScale = Vector3.Lerp(stretchScale, baseScale, t);

            yield return null;
        }

        bouncyObject.localScale = baseScale;
        bouncyRoutines.Remove(bouncyObject);
    }

    private void TryStartRailGrind(Collider railCollider)
    {
        if (isRailGrinding || isBouncing || railCooldown > 0f)
        {
            return;
        }

        if (!railCollider.CompareTag("Rail"))
        {
            return;
        }

        currentRail = railCollider.transform;
        railCenter = railCollider.bounds.center;

        Vector3 baseRailDirection = currentRail.TransformDirection(railDirectionLocal.normalized);
        baseRailDirection.y = 0f;

        if (baseRailDirection.sqrMagnitude < 0.001f)
        {
            baseRailDirection = currentRail.right;
            baseRailDirection.y = 0f;
        }

        baseRailDirection.Normalize();
        railMoveDirection = baseRailDirection;

        if (Vector3.Dot(lastMoveDirection, railMoveDirection) < 0f)
        {
            railMoveDirection = -railMoveDirection;
        }

        railSideDirection = Vector3.Cross(Vector3.up, railMoveDirection).normalized;
        railHalfLength = GetRailHalfLength(railCollider, railMoveDirection);

        railDistance = Vector3.Dot(transform.position - railCenter, railMoveDirection);
        railDistance = Mathf.Clamp(railDistance, -railHalfLength, railHalfLength);

        railSpinTimer = 0f;
        railSpinDirectionIndex = 0;
        isRailGrinding = true;

        if (playerAnimatorController != null)
        {
            playerAnimatorController.enabled = false;
        }

        SnapToRailTop();
        SetRailSpinAnimation();
    }

    private void UpdateRailGrind()
    {
        if (currentRail == null)
        {
            StopRailGrind(false);
            return;
        }

        railDistance += railGrindSpeed * Time.deltaTime;

        SnapToRailTop();
        UpdateRailSpinAnimation();

        if (railDistance >= railHalfLength)
        {
            railDistance = railHalfLength;
            SnapToRailTop();
            StopRailGrind(true);
        }
    }

    private void SnapToRailTop()
    {
        Vector3 railPosition = railCenter + railMoveDirection * railDistance;
        transform.position = railPosition + Vector3.up * railTopOffset;
    }

    private void UpdateRailSpinAnimation()
    {
        railSpinTimer += Time.deltaTime;

        float stepTime = 1f / Mathf.Max(railSpinSpeed, 0.01f);

        while (railSpinTimer >= stepTime)
        {
            railSpinTimer -= stepTime;
            railSpinDirectionIndex++;

            if (railSpinDirectionIndex >= railSpinDirections.Length)
            {
                railSpinDirectionIndex = 0;
            }
        }

        SetRailSpinAnimation();
    }

    private void SetRailSpinAnimation()
    {
        if (animator == null)
        {
            return;
        }

        Vector2 spinInput = railSpinDirections[railSpinDirectionIndex];

        animator.SetBool(isMovingParameter, true);
        animator.SetFloat(moveXParameter, spinInput.x);
        animator.SetFloat(moveYParameter, spinInput.y);
    }

    private float GetRailHalfLength(Collider railCollider, Vector3 direction)
    {
        Bounds bounds = railCollider.bounds;
        Vector3 extents = bounds.extents;
        Vector3 absDirection = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));

        float halfLength = Vector3.Dot(extents, absDirection);

        if (halfLength < 0.1f)
        {
            halfLength = 2f;
        }

        return halfLength;
    }

    private void StopRailGrind(bool pushOffRail)
    {
        isRailGrinding = false;
        currentRail = null;
        railCooldown = 0.35f;

        if (pushOffRail)
        {
            transform.position += railSideDirection * railExitSidePush + railMoveDirection * railExitForwardPush;
        }

        if (animator != null)
        {
            animator.SetBool(isMovingParameter, false);
        }

        if (playerAnimatorController != null)
        {
            playerAnimatorController.enabled = true;
        }
    }
}