using UnityEngine;

public class bullets_trace : MonoBehaviour
{
    [SerializeField] private SpriteRenderer Sprite;
    [SerializeField] public float DecayRate;
    private float DecayRateAtStart;
    private Vector3 FirstPosition;
    public Transform CurrectPosition;

    private void Start()
    {
        FirstPosition = transform.position;
        DecayRateAtStart = DecayRate;
    }

    private void FixedUpdate()
    {
        if (CurrectPosition != null)
        {
            transform.position = FirstPosition - (FirstPosition - CurrectPosition.position) / 2;
            transform.LookAt(new Vector3(transform.position.x, transform.position.y, transform.position.z + 1), transform.position - CurrectPosition.position);
            transform.localScale = new Vector2(0.05f, Vector2.Distance(FirstPosition, CurrectPosition.position));
        }

        if (DecayRate > 0)
        {
            Sprite.color = new Color(Sprite.color.r, Sprite.color.g, Sprite.color.b, 0.7f / DecayRateAtStart * DecayRate);
            DecayRate -= Time.deltaTime;
        }
        else
            Destroy(gameObject);
    }
}
