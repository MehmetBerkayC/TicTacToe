using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    // If you are using pointer interfaces, you need an EventSystem and a proper Raycaster on the Main Camera
    /// Raycasters 
    /// - Graphic Raycaster -> UI 
    /// - Physics 2D OR Physics Raycaster -> World
    // Use OnMouseDown but it might react to UI too depending on what you do with it
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.ClickedOnGridPositionRpc(x, y);
    }
}
