using System.Collections.Generic;
namespace UnityEngine.UI
{

    /// <summary>
    /// 用来给image挖洞
    /// </summary>
    [AddComponentMenu("UI/Effects/RectHoleImage", 17)]
    [RequireComponent(typeof(Image))]
#if UNITY_5_1
    public class RectHoleImage : BaseVertexEffect
#else
    public class RectHoleImage: BaseMeshEffect
#endif
    {
        public List<RectTransform> holeList = new List<RectTransform>();
        protected RectHoleImage()
        { }
#if UNITY_5_1
        public override void ModifyVertices(List<UIVertex> verts)
#else
        public override void ModifyMesh(VertexHelper vh)
#endif
        {
            
            if(holeList.Count == 0)
            {
                return;
            }
            Image image = GetComponent<Image>();
            if (image.type != Image.Type.Simple && image.type != Image.Type.Sliced)
            {
                return;
            }
            List<UIVertex> stream = new List<UIVertex>();
            vh.GetUIVertexStream(stream);


            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return;
            }
            Rect myRect = rectTransform.rect;

            //所有的点的坐标
            List<Vector3> allPoint = new List<Vector3>();

            //已经有的点的坐标加进去 不重复
            foreach(var point in stream)
            {
                bool exists = allPoint.Exists(_point=> 
                    _point.x == point.position.x && _point.y == point.position.y
                );
                if (!exists){
                    allPoint.Add(point.position);
                }
            }

            //记录x方向所有的x坐标
            List<float> xLine = new List<float>();
            //记录y方向所有的y坐标
            List<float> yLine = new List<float>();

            foreach(var point in allPoint)
            {
                if(!xLine.Exists(x=>x == point.x))
                {
                    xLine.Add(point.x);
                }

                if (!yLine.Exists(y => y == point.y))
                {
                    yLine.Add(point.y);
                }
            }

            //记录一下原始的坐标用于后面来算uv坐标
            List<float> xOriginalLine = new List<float>(xLine);
            List<float> yOriginalLine = new List<float>(yLine);

            //把洞的xy也加进去
            foreach(var hole in holeList)
            {
                Rect holeRect = hole.rect;
                Vector3 holePosition = hole.transform.localPosition;
                Vector3 thisPosition = transform.localPosition;

                float holeX1 = holeRect.xMin + holePosition.x;
                float holeX2 = holeRect.xMax + holePosition.x;
                float holeY1 = holeRect.yMin + holePosition.y;
                float holeY2 = holeRect.yMax + holePosition.y;

                xLine.Add(holeX1);
                xLine.Add(holeX2);
                yLine.Add(holeY1);
                yLine.Add(holeY2);
            }

            //将洞加进去之后可能会有重复 这儿去除一下
            List<float> resultXLine = new List<float>();
            List<float> resultYLine = new List<float>();
            foreach(var x in xLine)
            {
                if(!resultXLine.Exists(reslutX => reslutX == x))
                {
                    resultXLine.Add(x);
                }
            }

            foreach(var y in yLine)
            {
                if(!resultYLine.Exists(resultY => resultY == y))
                {
                    resultYLine.Add(y);
                }
            }

            xLine = new List<float>(resultXLine);
            yLine = new List<float>(resultYLine);

            xLine.Sort();
            yLine.Sort();

            List<Triangle> triangleList = new List<Triangle>();
            Sprite sprite = image.overrideSprite;
            Vector2[] originalUvs = new Vector2[4];
            if (sprite != null)
            {
                originalUvs = sprite.uv;
            }
            else
            {
                for(int i = 0; i < 4; i++)
                {
                    originalUvs[i] = new Vector2(0, 0);
                }
            }

            
            float originalUvXMax = 0f;
            float originalUvXMin = 0f;
            float originalUvYMax = 0f;
            float originalUvYMin = 0f;

            //计算出最大的x 最小的x 最大的y 最小的y
            for (int i = 0; i < originalUvs.Length; i++)
            {
                if(i == 0)
                {
                    originalUvXMax = originalUvs[i].x;
                    originalUvXMin = originalUvs[i].x;
                    originalUvYMax = originalUvs[i].y;
                    originalUvYMin = originalUvs[i].y;
                }
                else
                {
                    originalUvXMax = originalUvs[i].x > originalUvXMax ? originalUvs[i].x : originalUvXMax;
                    originalUvXMin = originalUvs[i].x < originalUvXMin ? originalUvs[i].x : originalUvXMin;
                    originalUvYMax = originalUvs[i].y > originalUvYMax ? originalUvs[i].y : originalUvYMax;
                    originalUvYMin = originalUvs[i].y < originalUvYMin ? originalUvs[i].y : originalUvYMin;
                }
            }

            float originalUvXLength = originalUvXMax - originalUvXMin;
            float originalUvYLength = originalUvYMax - originalUvYMin;

            //算出在洞外的所有的三角形
            for (int i = 0; i < xLine.Count - 1; i++)
            {
                for(int j = 0; j < yLine.Count - 1; j++)
                {
                    Vector2 uv1, uv2, uv3, uv4;
                    if(image.type == Image.Type.Simple)
                    {
                        uv1 = new Vector2((xLine[i] - myRect.xMin) / myRect.width, (yLine[j] - myRect.yMin) / myRect.height);
                        uv2 = new Vector2((xLine[i + 1] - myRect.xMin) / myRect.width, (yLine[j] - myRect.yMin) / myRect.height);
                        uv3 = new Vector2((xLine[i] - myRect.xMin) / myRect.width, (yLine[j + 1] - myRect.yMin) / myRect.height);
                        uv4 = new Vector2((xLine[i + 1] - myRect.xMin) / myRect.width, (yLine[j + 1] - myRect.yMin) / myRect.height);
                    }
                    else
                    {
                        uv1 = GetVertexUV(new Vector2(xLine[i], yLine[j]), sprite, xOriginalLine, yOriginalLine);
                        uv2 = GetVertexUV(new Vector2(xLine[i + 1], yLine[j]), sprite, xOriginalLine, yOriginalLine);
                        uv3 = GetVertexUV(new Vector2(xLine[i], yLine[j + 1]), sprite, xOriginalLine, yOriginalLine);
                        uv4 = GetVertexUV(new Vector2(xLine[i + 1], yLine[j + 1]), sprite, xOriginalLine, yOriginalLine);
                    }

                    Vector2 resultUv1, resultUv2, resultUv3, resultUv4;
                    
                    resultUv1.x = uv1.x * originalUvXLength + originalUvXMin;
                    resultUv1.y = uv1.y * originalUvYLength + originalUvYMin;

                    resultUv2.x = uv2.x * originalUvXLength + originalUvXMin;
                    resultUv2.y = uv2.y * originalUvYLength + originalUvYMin;

                    resultUv3.x = uv3.x * originalUvXLength + originalUvXMin;
                    resultUv3.y = uv3.y * originalUvYLength + originalUvYMin;

                    resultUv4.x = uv4.x * originalUvXLength + originalUvXMin;
                    resultUv4.y = uv4.y * originalUvYLength + originalUvYMin;

                    //这是那个矩形的四个点
                    UIVertex u1 = new UIVertex()
                    {
                        color = image.color,
                        position = new Vector3(xLine[i],yLine[j]),
                        uv0 = resultUv1,
                    };

                    UIVertex u2 = new UIVertex()
                    {
                        color = image.color,
                        position = new Vector3(xLine[i + 1], yLine[j]),
                        uv0 = resultUv2,
                    };

                    UIVertex u3 = new UIVertex()
                    {
                        color = image.color,
                        position = new Vector3(xLine[i], yLine[j + 1]),
                        uv0 = resultUv3,
                    };

                    UIVertex u4 = new UIVertex()
                    {
                        color = image.color,
                        position = new Vector3(xLine[i + 1], yLine[j + 1]),
                        uv0 = resultUv4,
                    };

                    
                    float rectangleX = (u2.position.x - u1.position.x) / 2 + u1.position.x;
                    float rectangleY = (u3.position.y - u1.position.y) / 2 + u1.position.y;

                    //判断这个矩形是否在洞里面并且是不是在自己的范围内 如果满足条件则拆分成两个三角形加入到要渲染的三角形list
                    bool outRect = true;
                    if(image.type == Image.Type.Simple)
                    {
                        outRect = OutRect(new Vector4(xOriginalLine[0], xOriginalLine[1], yOriginalLine[0], yOriginalLine[1]), new Vector2(rectangleX, rectangleY));
                    }
                    else
                    {
                        outRect = OutRect(new Vector4(xOriginalLine[0], xOriginalLine[3], yOriginalLine[0], yOriginalLine[3]), new Vector2(rectangleX, rectangleY));
                    }
                    
                    if(!InHole(holeList, new Vector2(rectangleX, rectangleY)) && !outRect)
                    {
                        //将矩形拆成两个三角形
                        Triangle triangle1 = new Triangle(u1, u3, u4);
                        Triangle triangle2 = new Triangle(u1, u2, u4);
                        triangleList.Add(triangle1);
                        triangleList.Add(triangle2);
                    }
                    
                }
            }

            stream.Clear();
            foreach(var triangle in triangleList)
            {
                foreach(var vertext in triangle.vertexList)
                {
                    stream.Add(vertext);
                }
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(stream);

        }

        class Triangle
        {
            public List<UIVertex> vertexList = new List<UIVertex>();

            public Triangle(UIVertex u1, UIVertex u2, UIVertex u3)
            {
                this.vertexList.Add(u1);
                this.vertexList.Add(u2);
                this.vertexList.Add(u3);
            }
        }

        Vector2 GetVertexUV(Vector2 vertex, Sprite sprite, List<float> xLine, List<float> yLine)
        {
            Vector4 border = sprite.border;//X=left, Y=bottom, Z=right, W=top.
            Rect rect = sprite.rect;

            float xLeftL = border.x;
            float xCenterL = rect.xMax - rect.xMin - border.x - border.z;
            float xRightL = border.z;

            float yBottomL = border.y;
            float yCenterL = rect.yMax - rect.yMin - border.y - border.w;
            float yTopL = border.w;
            //Debug.Log(border.x + " " + border.z + " " + border.y + " " + border.w);
            //Debug.Log(rect.xMin + " " + rect.xMax + " " + rect.yMin + " " + rect.yMax);
            //Debug.Log("xl=>" + xLeftL + " xc=>" + xCenterL + " xr=>" + xRightL + " yb=>" + yBottomL + " yc=>" + yCenterL + " yt=>" + yTopL);

            xLine.Sort();
            yLine.Sort();

            float thisXLeftL = xLine[1] - xLine[0];
            float thisXCenterL = xLine[2] - xLine[1];
            float thisXRightL = xLine[3] - xLine[2];

            float thisYBottonL = yLine[1] - yLine[0];
            float thisYCenterL = yLine[2] - yLine[1];
            float thisYTopL = yLine[3] - yLine[2];

            Vector2 uvVertex = new Vector2();

            //计算uv坐标的x
            if (vertex.x >= xLine[0] && vertex.x <= xLine[1])
            {
                uvVertex.x = (vertex.x - xLine[0]) / thisXLeftL * xLeftL / (rect.xMax - rect.xMin);
            }
            else if(vertex.x > xLine[1] && vertex.x < xLine[2])
            {
                uvVertex.x = ((vertex.x - xLine[1]) / thisXCenterL * xCenterL + xLeftL) / (rect.xMax - rect.xMin);
            }
            else if (vertex.x >= xLine[2] && vertex.x <= xLine[3])
            {
                uvVertex.x = ((vertex.x - xLine[2]) / thisXRightL * xRightL + xLeftL + xCenterL) / (rect.xMax - rect.xMin);
            }

            //计算uv坐标的y
            if (vertex.y >= yLine[0] && vertex.y <= yLine[1])
            {
                uvVertex.y = (vertex.y - yLine[0]) / thisYBottonL * yBottomL / (rect.yMax - rect.yMin);
            }
            else if(vertex.y > yLine[1] && vertex.y < yLine[2])
            {
                uvVertex.y = ((vertex.y - yLine[1]) / thisYCenterL * yCenterL + yBottomL) / (rect.yMax - rect.yMin);
            }
            else if(vertex.y >= yLine[2] && vertex.y <= yLine[3])
            {
                uvVertex.y = ((vertex.y - yLine[2]) / thisYTopL * yTopL + yBottomL + yCenterL) / (rect.yMax - rect.yMin);
            }

            //Debug.Log(uvVertex.x + "  " + uvVertex.y);

            return uvVertex;
        }

        bool InHole(List<RectTransform> holeList, Vector2 point)
        {
            float rectangleX = point.x;
            float rectangleY = point.y;
            Vector3 selfPos = transform.localPosition;

            foreach (var hole in holeList)
            {
                Rect holeRect = hole.rect;
                Vector3 pos = hole.transform.localPosition;
                if (rectangleX > holeRect.xMin + pos.x 
                    && rectangleX < holeRect.xMax + pos.x
                    && rectangleY > holeRect.yMin + pos.y
                    && rectangleY < holeRect.yMax + pos.y)
                {
                    return true;
                }
            }

            return false;
        }

        bool OutRect(Vector4 rect, Vector2 point)
        {
            //x = xmin, y = xmax, z = ymin, w = ymax
            if(point.x < rect.x || point.x > rect.y || point.y < rect.z || point.y > rect.w)
            {
                return true;
            }
            return false;
        }
    }
}
