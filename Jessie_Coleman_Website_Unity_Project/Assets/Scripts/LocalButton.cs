using UnityEngine;
using System.Collections;

public class LocalButton : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private CapsuleCollider playerCapsuleCollider;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Cart")]
    [SerializeField] private Transform cartSeatPosition;
    [SerializeField] private GameObject cart;

    [Header("Enter Cart Animation")]
    [SerializeField] private float enterCartDuration = 0.75f;
    [SerializeField] private float enterCartArcHeight = 2f;

    [Header("Exit Cart")]
    [SerializeField] private float exitUpwardForce = 6f;

    private bool inCart;
    private bool isAnimating;

    private Transform originalPlayerParent;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (player != null)
        {
            if (playerCapsuleCollider == null)
            {
                playerCapsuleCollider = player.GetComponent<CapsuleCollider>();
            }

            if (playerRigidbody == null)
            {
                playerRigidbody = player.GetComponent<Rigidbody>();
            }

            originalPlayerParent = player.transform.parent;
        }
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    private void FaceCamera()
    {
        if (targetCamera == null)
        {
            return;
        }

        transform.LookAt(
            transform.position + targetCamera.transform.rotation * Vector3.forward,
            targetCamera.transform.rotation * Vector3.up);
    }

    public void ButtonClicked()
    {
        if (isAnimating || player == null)
        {
            return;
        }

        if (!inCart)
        {
            StartCoroutine(EnterCart());
        }
        else
        {
            ExitCart();
        }
    }

    private IEnumerator EnterCart()
    {
        if (cartSeatPosition == null)
        {
            yield break;
        }

        isAnimating = true;

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCapsuleCollider != null)
        {
            playerCapsuleCollider.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }

        Vector3 startPosition = player.transform.position;
        Quaternion startRotation = player.transform.rotation;

        float timer = 0f;

        while (timer < enterCartDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / enterCartDuration);

            Vector3 position = Vector3.Lerp(
                startPosition,
                cartSeatPosition.position,
                t);

            float arc = 4f * enterCartArcHeight * t * (1f - t);

            position.y += arc;

            player.transform.position = position;

            player.transform.rotation = Quaternion.Slerp(
                startRotation,
                cartSeatPosition.rotation,
                t);

            yield return null;
        }

        player.transform.SetParent(cartSeatPosition);
        cart.tag = "Player";
        cart.GetComponent<ShoppingCart>().enabled = true;

        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;        

        inCart = true;
        isAnimating = false;
    }

    private void ExitCart()
    {
        cart.tag = "Untagged";
        cart.GetComponent<ShoppingCart>().enabled = false;
        player.transform.SetParent(originalPlayerParent);
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.linearVelocity = Vector3.zero;

            playerRigidbody.AddForce(
                Vector3.up * exitUpwardForce,
                ForceMode.Impulse);
        }

        if (playerCapsuleCollider != null)
        {
            playerCapsuleCollider.enabled = true;
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        inCart = false;
    }
}