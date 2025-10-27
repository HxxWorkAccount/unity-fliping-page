using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(FlipEffect))]
public class FlipEffectEditor : Editor
{
    private SerializedProperty m_propRadius;
    private SerializedProperty m_propImage;
    private SerializedProperty m_propCylinderPos;
    private SerializedProperty m_propScrollAngle;

    void OnEnable() {
        m_propRadius      = serializedObject.FindProperty("radius");
        m_propImage       = serializedObject.FindProperty("m_image");
        m_propCylinderPos = serializedObject.FindProperty("cylinderPos");
        m_propScrollAngle = serializedObject.FindProperty("scrollAngle");
    }

    public override void OnInspectorGUI() {
        FlipEffect flipEffect                          = target as FlipEffect;
        bool                           enabledProgress = false;
        if (flipEffect) {
            FlipEffectProgress progressCom = flipEffect.GetComponent<FlipEffectProgress>();
            enabledProgress                = progressCom && progressCom.enabled;
        }
        serializedObject.Update();
        if (enabledProgress)
            EditorGUI.BeginDisabledGroup(true);
        DrawDefaultInspector();
        ClampProp();
        if (enabledProgress) {
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("Disable FlipEffectProgress to Modify Properties", MessageType.Info);
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI() {
        serializedObject.Update();

        FlipEffect flipEffect = target as FlipEffect;
        if (flipEffect == null)
            return;
        Image image = m_propImage.objectReferenceValue as Image;
        if (image == null || image.rectTransform == null)
            return;
        FlipEffectProgress progressCom     = flipEffect.GetComponent<FlipEffectProgress>();
        bool               enabledProgress = progressCom && progressCom.enabled;

        /* 初始化把手 */
        GetHandleTransform(out Vector3 pos, out Quaternion rot);
        EditorGUI.BeginChangeCheck();
        Vector3    newPos = Handles.PositionHandle(pos, rot);
        Quaternion newRot = Handles.RotationHandle(rot, pos);

        /* 监听变化并应用 */
        if (!enabledProgress && EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(flipEffect, "Move FlipEffect Handle");  // 添加一个撤销记录（可选）
            SetPropByHandleTransform(newPos, newRot);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(flipEffect);  // 标记对象已更改
        } else {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void GetHandleTransform(out Vector3 pos, out Quaternion rot) {
        Vector2 cylinderPos = m_propCylinderPos.vector2Value;
        float   radius      = m_propRadius.floatValue;
        float   scrollAngle = m_propScrollAngle.floatValue;
        Image   image       = (Image)m_propImage.objectReferenceValue;

        pos = image.rectTransform.position;
        // pos = image.rectTransform.localPosition;
        pos.x += image.rectTransform.rect.width * (cylinderPos.x - 0.5f) / 10f;
        pos.y += image.rectTransform.rect.height * (cylinderPos.y - 0.5f) / 10f;
        pos.z -= radius / 5f;
        rot = Quaternion.identity;
        rot *= Quaternion.Euler(0f, 0f, -scrollAngle);
    }

    private void SetPropByHandleTransform(in Vector3 newPos, in Quaternion newRot) {
        Image   image    = (Image)m_propImage.objectReferenceValue;
        Vector3 imagePos = image.rectTransform.position;
        // Vector3 imagePos = image.rectTransform.localPosition;

        m_propRadius.floatValue = (imagePos.z - newPos.z) * 5f;

        m_propScrollAngle.floatValue = MapAngle(-newRot.eulerAngles.z);

        m_propCylinderPos.vector2Value = new Vector2(                               //
            (newPos.x - imagePos.x) * 10f / image.rectTransform.rect.width + 0.5f,  //
            (newPos.y - imagePos.y) * 10f / image.rectTransform.rect.height + 0.5f  //
        );

        ClampProp();
    }

    private void ClampProp() {
        Vector2 cylinderPos            = m_propCylinderPos.vector2Value;
        cylinderPos.x                  = Mathf.Clamp01(cylinderPos.x);
        cylinderPos.y                  = Mathf.Clamp01(cylinderPos.y);
        m_propCylinderPos.vector2Value = cylinderPos;

        m_propRadius.floatValue = Mathf.Max(0.01f, m_propRadius.floatValue);

        m_propScrollAngle.floatValue = Mathf.Clamp(m_propScrollAngle.floatValue, -180f, 180f);
    }

    private float MapAngle(float angle) {
        float backup = angle;
        while (angle < -360f)
            angle += 360f;
        while (angle > 360f)
            angle -= 360f;
        if (angle > 180f)
            angle = angle - 360f;  // 180..360 -> -180..0
        else if (angle < -180f)
            angle = angle + 360f;  // -360..-180 -> 180..0
        // Debug.Log($"Map Angle: {backup} -> {angle}");
        return angle;
    }
}
