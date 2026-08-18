using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dice : MonoBehaviour, IPointerClickHandler
{
    private bool _isSelectable;

    public bool IsSelectable => _isSelectable;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float stopDuration = 0.5f;

    private Quaternion initialRotation;
    private bool isRolling;
    private Coroutine stopCoroutine;
    private readonly Dictionary<int, Vector3> faceDirections = new()
    {
        { 1, Vector3.down },
        { 2, Vector3.forward },
        { 3, Vector3.right },
        { 4, Vector3.left },
        { 5, Vector3.back },
        { 6, Vector3.up }
    };
    public void Enable()
    {
        _isSelectable = true;
    }

    public void Disable()
    {
        _isSelectable = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isSelectable)
            return;

        Disable();
    }
    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    private void Update()
    {
        if (!isRolling)
            return;

        transform.Rotate(
            Random.insideUnitSphere * rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    public void StartRolling()
    {
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            stopCoroutine = null;
        }

        isRolling = true;
    }
    public void StopRolling(int face)
    {
        if (!faceDirections.TryGetValue(face, out Vector3 localDirection))
        {
            Debug.LogError($"Invalid dice face: {face}");
            return;
        }

        isRolling = false;

        // جهت وجه موردنظر در دنیای اولیه
        Vector3 worldDirection = initialRotation * localDirection;

        // Rotation لازم برای اینکه آن وجه رو به بالا قرار بگیرد
        Quaternion targetRotation =
            Quaternion.FromToRotation(worldDirection, Vector3.up)
            * initialRotation;

        if (stopCoroutine != null)
            StopCoroutine(stopCoroutine);

        stopCoroutine = StartCoroutine(
            RotateToTarget(targetRotation)
        );
    }
    private IEnumerator RotateToTarget(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;

        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / stopDuration);

            // حرکت نرم
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.rotation = targetRotation;

        stopCoroutine = null;
    }


}