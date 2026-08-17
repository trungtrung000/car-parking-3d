using UnityEngine;
using DG.Tweening;
using Random = System.Random;


public class Car : MonoBehaviour
{
    public Route route;

    public Transform bottomTransform;
    public Transform bodyTransform;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] ParticleSystem smokeFX;

    [SerializeField] Rigidbody rb;
    [SerializeField] float danceValue;
    [SerializeField] float durationMultiplier;

    [SerializeField] float lateralOffset;


    private void Start()
    {
        bodyTransform.DOLocalMoveY(danceValue, 0.1f)
                      .SetLoops(-1 , LoopType.Yoyo)
                      .SetEase(Ease.Linear);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Car ortherCar))
        {
            StopDancingAnim();
            rb.DOKill(false);

            //add explosion
            Vector3 hitpoint = collision.contacts[0].point;
            AddExplosionFroce(hitpoint);
            smokeFX.Play();

            Game.Instance.OnCarCollision?.Invoke();
        }
    }

    private void AddExplosionFroce(Vector3 point)
    {
        rb.AddExplosionForce(400f, point, 3f);
        rb.AddForceAtPosition( Vector3.up * 2f, point, ForceMode.Impulse);
        rb.AddTorque(new Vector3(GetRandomAngle(),GetRandomAngle(),GetRandomAngle()));
    }

    private float GetRandomAngle()
    {
        float angle = 10f;
        float rand = UnityEngine.Random.value;
        return rand > 0.5f ? angle : angle;

    }
    public void Move(Vector3[] path)
    {
        float yOffset = transform.position.y - bottomTransform.position.y;

        Vector3[] adjustedPath = new Vector3[path.Length];

        for (int i = 0; i < path.Length; i++)
        {
            Vector3 dir = (i < path.Length - 1)
                ? (path[i + 1] - path[i])
                : (path[i] - path[i - 1]);

            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            // vector vuông góc với hướng đi, nằm ngang (trái/phải)
            Vector3 right = Vector3.Cross(Vector3.up, dir);

            adjustedPath[i] = path[i] + Vector3.up * yOffset + right * lateralOffset;
        }

        rb.DOKill();

        rb.DOPath(adjustedPath, 2f * durationMultiplier * adjustedPath.Length)
            .SetLookAt(0f, Vector3.right)
            .SetEase(Ease.Linear);
    }

    public void StopDancingAnim()
    {
        bodyTransform.DOKill(true);
    }

    public void SetColor(Color color)
    {
        meshRenderer.sharedMaterials[0].color = color;
    }
}
