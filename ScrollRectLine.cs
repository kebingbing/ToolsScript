using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LuaInterface;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ScrollRectLine : ScrollRect
{

    protected override void SetContentAnchoredPosition(Vector2 position)
    {

        Vector2 temp = position;
        temp.x = Mathf.Floor(temp.x);
        temp.y = Mathf.Floor(temp.y);
        content.anchoredPosition = temp;
    }
}
