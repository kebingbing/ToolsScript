using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class ClickEventCompent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public delegate void VoidDelegate(GameObject go);

    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;


    public UnityAction onLongPress = null;
    public UnityAction<PointerEventData> MoveHorizontalLeft = null;
    public UnityAction<PointerEventData> MoveHorizontalRight = null;
    public UnityAction<PointerEventData> MoveVertical = null;

    public UnityAction<PointerEventData> MoveHorizontalEnd = null;
    public UnityAction<PointerEventData> MoveVerticalEnd = null;

    public UnityAction<PointerEventData> MoveHorizontalStart = null;
    public UnityAction<PointerEventData> MoveVerticalStart = null;

    private bool isPointerDown = false;
    private bool isBeginDrag = false;
    private bool isMoveHorizontal = true;
    //是否正在水平拖动
    private bool isMoveIngHorizontal = false;
    private bool isMoveIngVertical = false;

    private bool longPressTriggered = true;
    private float timePressStarted;
    public float delty = 0;
    public ScrollRect parentScrollRect;
    public float durationThreshold = 0.5f;

    void Start()
    {
        parentScrollRect = transform.GetComponentInParent<ScrollRect>();
    }

    private void Update()
    {
        //判断是否长按
        if (isPointerDown && longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                if (onLongPress != null)
                {
                    onLongPress.Invoke();
                }

                //onClick.Invoke(gameObject);
                longPressTriggered = false;

            }
        }

    }

    static public ClickEventCompent Get(GameObject go)
    {
        ClickEventCompent listener = go.GetComponent<ClickEventCompent>();
        if (listener == null) listener = go.AddComponent<ClickEventCompent>();
        return listener;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (longPressTriggered && !isBeginDrag)
            onClick.Invoke(gameObject);
        longPressTriggered = true;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
        //长按事件
        timePressStarted = Time.time;
        isPointerDown = true;

    }
    public  void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
    }
    public  void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
        //长按事件
        isPointerDown = false;
    }
    public  void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
        //长按事件
        isPointerDown = false;
        longPressTriggered = true;

    }

    public void OnDrag(PointerEventData eventData)
    {
        isPointerDown = false;

        if (eventData.position == DragStartPos)
        {
            return;
        }
        //Math.Abs(eventData.position.y - DragStartPos.y) - Math.Abs(eventData.position.x - DragStartPos.x) <= 0
        //左右滑动
        if (Math.Abs(eventData.delta.y) - Math.Abs(eventData.delta.x) <= 0 && !isMoveIngVertical)
        {
            //右滑动
            if (eventData.delta.x > 0)
            {
                if(MoveHorizontalRight != null)
                {
                    MoveHorizontalRight.Invoke(eventData);
                }
               

            }
            else if (eventData.delta.x < 0)
            {

                if (MoveHorizontalLeft != null)
                {
                    MoveHorizontalLeft.Invoke(eventData);
                }
                
            }
            isMoveIngHorizontal = true;

        }
        else if(Math.Abs(eventData.delta.y) - Math.Abs(eventData.delta.x) > 0 && !isMoveIngHorizontal)
        {
            isMoveHorizontal = false;
            isMoveIngHorizontal = false;
            isMoveIngVertical = true;

        }

        if (parentScrollRect != null && !isMoveIngHorizontal)
        {
            parentScrollRect.OnDrag(eventData);
        }


    }

    //拖动起始位置
    Vector2 DragStartPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        isBeginDrag = true;
        DragStartPos = eventData.position;
        if (parentScrollRect != null)
        {
            parentScrollRect.OnBeginDrag(eventData);
        }

        if (MoveHorizontalStart != null)
        {
            MoveHorizontalStart.Invoke(eventData);
        }

    }

    //拖动终止位置
    Vector2 DragEndPos;
    public void OnEndDrag(PointerEventData eventData)
    {
        DragEndPos = eventData.position;
        isBeginDrag = false;
        isMoveHorizontal = true;

        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }

        if (MoveHorizontalEnd != null && isMoveIngHorizontal)
        {
            MoveHorizontalEnd.Invoke(eventData);
        }

        isMoveIngVertical = false;
        isMoveIngHorizontal = false;

    }
}