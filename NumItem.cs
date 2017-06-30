using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NumItem : MonoBehaviour
{

    public int Index;
    public float v;
    public Text lable;
    public Transform node;
    public RectTransform parentrect;
    public RectTransform selfrect;
    public ScrollRectExtend SreExtend;

    public void Init(int index,ScrollRectExtend sreExtend)
    {
        lable = this.transform.Find("node/Text").GetComponent<Text>();
        node = this.transform.Find("node");
        Index = index;
        SreExtend = sreExtend;
        lable.text = index.ToString();
        parentrect = transform.parent.GetComponent<RectTransform>();
        selfrect = transform.GetComponent<RectTransform>();
    }

    public void OnDrag(float value)
    {
        v += value;

        float deltx = parentrect.anchoredPosition.x + selfrect.anchoredPosition.x - SreExtend.itemx* 1.5f;
        float scale = 1;

        if ((Mathf.Abs(deltx) - 20) < 0)
        {
            lable.color = Color.black;
        }
        else
        {
            lable.color = new Color(255,228,0);

        }

        if (Mathf.Abs(deltx) <= SreExtend.itemx * 3f)
        {
           scale =  1-Mathf.Abs(deltx)/ (SreExtend.itemx * 5f);
        }

        node.localScale = Vector3.one*scale;

    }

    public void OnEndDrag(float x)
    {
        float deltx = x + selfrect.anchoredPosition.x - SreExtend.itemx * 1.5f; ;

        if((Mathf.Abs(deltx) - 20) < 0)
        {
            Debug.Log(Index);
            SreExtend.OnNotifyScrollNumChange(Index);
            lable.color = Color.black;
        }
        else
        {
            lable.color = new Color(255, 228, 0);
        }
        
        float scale = 1;
        if (Mathf.Abs(deltx) <= 300)
        {
            scale = 1 - Mathf.Abs(deltx) / 500;
        }

        node.localScale = Vector3.one * scale;


    }

}
