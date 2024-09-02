using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Review : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Text text;
    
    private Action<Review> onPointerDown;
    private Action<Review> onPointerUp;
    
    
    public void Initialize(string review, Action<Review> onMouseDown, Action<Review> onMouseUp)
    {
        text.text = review;
        onPointerDown = onMouseDown;
        onPointerUp = onMouseUp;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke(this);
    }
}
