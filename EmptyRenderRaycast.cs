using UnityEngine;
using UnityEngine.UI;
namespace Assets.game.Util
{
    public class EmptyRenderRaycast : Image {


        protected EmptyRenderRaycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        private Collider2D mCollider = null;



        protected override void Awake()
        {
            this.mCollider = this.GetComponent<Collider2D>();
            if (null == this.mCollider)
            {
                //Debugger.Log("not has Collider2D");
            }
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            Vector3 worldPoint;
            bool isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(this.rectTransform, sp, eventCamera, out worldPoint);

            if (isInside)
            {
                if (mCollider != null)
                {
                    isInside = this.mCollider.OverlapPoint(worldPoint);
                }
                
            }
            return isInside;
        }


    }
}
