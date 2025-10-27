using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class FlipEffect : BaseMeshEffect
{
    public float radius = 100f;
    [SerializeField]
    private Image  m_image;
    public Vector2 cylinderPos;
    [Range(-180f, 180f)]
    public float scrollAngle;

    [SerializeField, Range(0, 5)]
    private int m_level = 4;

    public Image image => m_image;

    void Update() {
        UpdateMaterial();
    }

    /*====-------------- Flip --------------====*/

    void UpdateMaterial() {
        if (m_image.enabled && m_image.material) {
            var      imageRect = m_image.rectTransform.rect;
            Material mat       = m_image.material;
            mat.SetFloat("_Radius", radius);
            Vector2 imageLocalPos = new Vector2(m_image.rectTransform.localPosition.x, m_image.rectTransform.localPosition.y);
            Vector2 realCylinderPos =
                // imageLocalPos + m_image.rectTransform.anchoredPosition + imageRect.min
                imageLocalPos + imageRect.min
                + new Vector2(Mathf.Clamp01(cylinderPos.x) * imageRect.width, Mathf.Clamp01(cylinderPos.y) * imageRect.height);
            mat.SetVector("_CylinderPos", realCylinderPos);
            Vector2 scrollDir = new Vector2(Mathf.Sin(Mathf.Deg2Rad * scrollAngle), Mathf.Cos(Mathf.Deg2Rad * scrollAngle));
            mat.SetVector("_ScrollDir", scrollDir.normalized);
        }
    }

    /*====-------------- Tessellation --------------====*/

    public override void ModifyMesh(VertexHelper vh) {
        List<UIVertex> vertexList = new();
        vh.GetUIVertexStream(vertexList);
        LinearSubdivide(vertexList, m_level);
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    private void LinearSubdivide(List<UIVertex> vertexList, int level) {
        if (level <= 0 || vertexList.Count < 3)
            return;
        Debug.Assert(vertexList.Count % 3 == 0, "vertex count must be multiple of 3");

        UIVertex MiddleVertex(int a, int b) {
            UIVertex va = vertexList[a];
            UIVertex vb = vertexList[b];
            va.color    = Color.Lerp(va.color, vb.color, 0.5f);
            va.normal   = Vector3.Lerp(va.normal, vb.normal, 0.5f);
            va.position = Vector3.Lerp(va.position, vb.position, 0.5f);
            va.tangent  = Vector3.Lerp(va.tangent, vb.tangent, 0.5f);
            va.uv0      = Vector2.Lerp(va.uv0, vb.uv0, 0.5f);
            va.uv1      = Vector2.Lerp(va.uv1, vb.uv1, 0.5f);
            va.uv2      = Vector2.Lerp(va.uv2, vb.uv2, 0.5f);
            va.uv3      = Vector2.Lerp(va.uv3, vb.uv3, 0.5f);
            return va;
        }

        void DoSubdivide() {
            int length = vertexList.Count;
            for (int i = 0; i < length; i += 3) {
                /* 取中点，插入（保持统一顺序），原地执行 */
                UIVertex ab = MiddleVertex(0, 1);
                UIVertex bc = MiddleVertex(1, 2);
                UIVertex ca = MiddleVertex(2, 0);
                vertexList.Add(vertexList[0]);
                vertexList.Add(ab);
                vertexList.Add(ca);
                vertexList.Add(vertexList[1]);
                vertexList.Add(bc);
                vertexList.Add(ab);
                vertexList.Add(vertexList[2]);
                vertexList.Add(ca);
                vertexList.Add(bc);
                vertexList.Add(ab);
                vertexList.Add(bc);
                vertexList.Add(ca);
                vertexList.RemoveRange(0, 3);
            }
        }

        for (; level > 0; level--)
            DoSubdivide();
    }
}
