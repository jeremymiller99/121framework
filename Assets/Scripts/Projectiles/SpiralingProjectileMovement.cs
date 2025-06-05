using UnityEngine;

public class SpiralingProjectileMovement : ProjectileMovement
{
    public float start;
    public SpiralingProjectileMovement(float speed) : base(speed)
    {
        start = Time.time;
    }

    public override void Movement(Transform transform)
    {
        transform.Translate(new Vector3(displaySpeed * Time.deltaTime, 0, 0), Space.Self);
        transform.Rotate(0, 0, displaySpeed * Mathf.Sqrt(displaySpeed) * Time.deltaTime * 20.0f / (1 + Random.value + Time.time - start));
    }
}
