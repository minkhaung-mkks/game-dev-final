using UnityEngine;
using UnityEngine.EventSystems;

// Grows the button a little on hover and shrinks it on press.
// Uses unscaled time so it still animates while the game is paused (timeScale = 0).
public class MenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float speed = 14f;

    private Vector3 baseScale;
    private float target = 1f;
    private bool hovering;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Reset so the button isn't stuck big when a menu is hidden mid-hover
        hovering = false;
        target = 1f;
        transform.localScale = baseScale;
    }

    private void Update()
    {
        Vector3 goal = baseScale * target;
        transform.localScale = Vector3.Lerp(transform.localScale, goal, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        target = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        target = 1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        target = pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        target = hovering ? hoverScale : 1f;
    }
}
