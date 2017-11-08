using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
	/// <summary>
	/// 字体渐变效果
	/// </summary>
	[AddComponentMenu("UI/Effects/Gradient")]
	public class Gradient : BaseMeshEffect
	{
		[SerializeField]
		private Color topColor = Color.white;

		/// <summary>
		/// 顶部颜色
		/// </summary>
		public Color TopColor
		{
			get { return topColor; }
			set { topColor = value; }
		}

		/// <summary>
		/// 底部颜色
		/// </summary>
		[SerializeField]
		private Color bottomColor = Color.black;
		public Color BottomColor
		{
			get { return bottomColor; }
			set { bottomColor = value; }
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive() || vh.currentVertCount == 0)
			{
				return;
			}

			var vertexList = new List<UIVertex>();
			vh.GetUIVertexStream(vertexList);
			int count = vertexList.Count;

			ApplyGradient(vertexList, 0, count);
			vh.Clear();
			vh.AddUIVertexTriangleStream(vertexList);
		}

		private void ApplyGradient(List<UIVertex> vertexList, int start, int end)
		{
			float bottomY = vertexList[0].position.y;
			float topY = vertexList[0].position.y;
			for (int i = start; i < end; ++i)
			{
				float y = vertexList[i].position.y;
				if (y > topY)
				{
					topY = y;
				}
				else if (y < bottomY)
				{
					bottomY = y;
				}
			}

			float uiElementHeight = topY - bottomY;
			for (int i = start; i < end; ++i)
			{
				UIVertex uiVertex = vertexList[i];
				uiVertex.color = Color.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);
				vertexList[i] = uiVertex;
			}
		}
	}
}
