using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LuaInterface;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ScrollRectExtend : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    readonly List<NumItem> _items = new List<NumItem>();
    private LuaFunction _notifyScrollNumChange = null;
    // Use this for initialization
    public GameObject itemgo;
    public RectTransform content;
    public GridLayoutGroup glg;
    public float itemx;
    public int _count = 0;
    public enum Direction
    {
        Left,
        Right,
        None
    }

    public Direction dir = Direction.None;
    [NoToLua]
    public void Awake()
    {
  
        itemx = glg.cellSize.x + glg.spacing.x;
        //Refresh();

    }
    [NoToLua]
    public void Refresh()
    {
      
        for (int i = 20 - 1; i >= 0; i--)
        {
           var go = GameObject.Instantiate(itemgo);
            go.transform.SetParent(content);
            go.SetActive(true);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }

        for (int i = 1; i <= content.childCount ; i++)
        {
            var item = content.GetChild(i).GetComponent<NumItem>();
            item.Init(i,this);
            item.OnEndDrag(content.anchoredPosition.x);
            _items.Add(item);

        }
    }

    [NoToLua]
    public void UpdataItems(int count)
    {

    }




    private float speed = 0;
    [NoToLua]
    public  void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!IsDrag(eventData.delta.x))
        {
            return;
        }


        if (_count == 0)
        {
            return;
        }
        var _x = content.anchoredPosition.x % itemx;
        Vector2 deltVector2 = eventData.position - startpos;
        speed = deltVector2.x/eventData.clickTime;
        float deltx;
        if (_x != 0)
        {

            //右滑动
            if (deltVector2.x >= 0)
            {
                if(_x >0)
                {
                    deltx = itemx - _x;
                }
                else
                {
                    deltx = -_x;
                }
                
                dir = Direction.Right;
            }
            else
            {
                if (_x > 0)
                {
                    deltx = ( -_x);
                }
                else
                {
                    deltx = (-itemx - _x);
                }
                
    
                dir = Direction.Left;
            }
            

        }
        else
        {
          
            deltx = 0;
            if (deltVector2.x > 0)
            {

                dir = Direction.Right;
            }
            else
            {
              
                dir = Direction.Left;
            }

        }

        content.DOAnchorPosX(content.anchoredPosition.x + deltx, 0.3f);
        //content.DOLocalMoveX(content.anchoredPosition.x + deltx, 0.3f);
        //content.anchoredPosition = new Vector2(content.anchoredPosition.x + deltx,content.anchoredPosition.y);
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].OnEndDrag(content.anchoredPosition.x + deltx);
        }
        _anim = true;


    }
    [NoToLua]
    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!IsDrag(eventData.delta.x))
        {
            return;
        }

        content.anchoredPosition = content.anchoredPosition + new Vector2(eventData.delta.x, 0);
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].OnDrag(eventData.delta.x);
        }

        
    }




    [NoToLua]
    public bool IsDrag(float deltx)
    {
        float x = content.anchoredPosition.x + deltx;
        if (x >= 2f * itemx - content.rect.width && x <=  1*itemx + 1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public Vector2 startpos = Vector2.zero;
    [NoToLua]
    public void OnBeginDrag(PointerEventData eventData)
    {
        startpos = eventData.position;
    }

    public bool _anim = true;
    public float _anim_speed = 1f;
    public float anim_s = 0;
    [NoToLua]
    void LataUpdate()
    {
        //if (_anim)
        //{

        //    for (int i = 0; i < items.Count; i++)
        //    {

        //        items[i].OnEndDrag(content.anchoredPosition.x);

        //    }
        //    _anim = false;

        //}
    }

    public void RegistEvent(LuaFunction cb)
    {
        _notifyScrollNumChange = cb;
    }

    public void SetScrollNum(int num)
    {
        Vector2 v2 = new Vector2();
        v2.y = content.anchoredPosition.y;
        v2.x = -(num-2)*itemx;

        content.DOAnchorPosX(v2.x, 0.2f).OnComplete(() =>
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.OnEndDrag(v2.x);


            }
        }
        );

    }
    public void ReSetScrollCount(int count)
    {

        _count = count;
        var delt = _items.Count - count;

        var itemscount = _items.Count;

        if (delt >= 0)
        {
            for (int i = 0; i < delt; i++)
            {
                _items[i+ count].gameObject.SetActive(false);
            }
            for (int i = 0; i < count; i++)
            {
                _items[i].gameObject.SetActive(true);
            }

        }
        else
        {
            for (int i = 1; i <= -delt; i++)
            {
                var go = GameObject.Instantiate(itemgo);
                go.transform.SetParent(content);
                go.SetActive(true);
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                var item = go.GetComponent<NumItem>();
                item.Init(i + itemscount, this);
                _items.Add(item);


            }
        }
        //content.anchoredPosition = new Vector2(0,content.anchoredPosition.y);
        //content.DOAnchorPosX(0, 0.1f).OnComplete(()=>
        //{
        //    for (int i = 0; i < _items.Count; i++)
        //    {
        //        var item = _items[i];
        //        item.OnEndDrag(0);


        //    }
        //}
        //);
       



    }
    [NoToLua]
    public  void OnNotifyScrollNumChange(int arg0)
    {
        if (_notifyScrollNumChange == null)
        {   
            Debug.LogError("请注册数字滚动完成事件");
            return;
        }
        _notifyScrollNumChange.BeginPCall();
        _notifyScrollNumChange.Push(arg0);
        _notifyScrollNumChange.PCall();
        _notifyScrollNumChange.EndPCall();
    }
}
