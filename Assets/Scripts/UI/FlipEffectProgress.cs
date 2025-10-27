using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(FlipEffect))]
public class FlipEffectProgress : MonoBehaviour
{
    [Range(0f, 1f)]
    public float progress = 0f;
    [SerializeField]
    private AnimationCurve m_angleLerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private FlipEffect m_flipEffect;
    private float      m_initRadius;
    private float      m_initScrollAngle;
    private int        m_horizontalDir = 0;  // 横向滚动方向，1 表示从左到右，-1 表示从右到左，0 表示非横向滚动
    private int        m_verticalDir   = 0;  // 纵向滚动方向，同理同上

    void OnEnable() {
        m_flipEffect      = GetComponent<FlipEffect>();
        m_initRadius      = m_flipEffect.radius;
        m_initScrollAngle = m_flipEffect.scrollAngle;
    }

    void OnDisable() {
#if UNITY_EDITOR
        m_flipEffect.radius      = m_initRadius;
        m_flipEffect.scrollAngle = m_initScrollAngle;
#endif
    }

    void Update() {
        UpdateProgress();
    }

    private void UpdateProgress() {
        if (!m_flipEffect.enabled)
            return;

        var imageRect = m_flipEffect.image.rectTransform.rect;

        /*==--- 根据 scrollAngle 确定方向 ---==*/
        float ratio        = imageRect.width / imageRect.height;
        float imageRad     = Mathf.Atan2(imageRect.width, imageRect.height);
        float absScrollRad = Mathf.Abs(Mathf.Deg2Rad * m_initScrollAngle);
        m_horizontalDir    = 0;
        m_verticalDir      = 0;
        if (absScrollRad < imageRad)
            m_verticalDir = 1;
        else if (absScrollRad > (Mathf.PI - imageRad))
            m_verticalDir = -1;
        if (m_verticalDir == 0)
            m_horizontalDir = m_initScrollAngle > 0 ? 1 : -1;
        // Debug.Log($"ratio: {ratio}, imageRad: {imageRad}, absScrollRad: {absScrollRad}, HorizontalDir: {m_horizontalDir}, VerticalDir: {m_verticalDir}");

        /*==--- 进度计算 ---==*/
        float rectLength        = Mathf.Abs(m_horizontalDir) * imageRect.width + Mathf.Abs(m_verticalDir) * imageRect.height;
        float totalLength       = m_initRadius + rectLength;
        float currentLength     = Mathf.Clamp01(progress) * totalLength;
        float targetScrollAngle = m_horizontalDir != 0 ? m_horizontalDir * 90f : (m_verticalDir - 1) * 90f;

        /* 根据进度更新 FlipEffect */
        Vector2 cylinderPos = new(m_initScrollAngle > 0 ? 0f : 1f, absScrollRad <= Mathf.PI / 2 ? 0f : 1f);
        if (currentLength < rectLength) { /* 移动 cylinderPos */
            float rectProgress = currentLength / rectLength;
            // 更新半径
            m_flipEffect.radius = m_initRadius;
            // 更新 cylinderPos
            if (m_horizontalDir != 0) {
                cylinderPos.x = m_horizontalDir < 0 ? 1f - rectProgress : rectProgress;
            } else {
                cylinderPos.y = m_verticalDir < 0 ? 1f - rectProgress : rectProgress;
            }
            // 更新方向
            m_flipEffect.scrollAngle = Mathf.Lerp(m_initScrollAngle, targetScrollAngle, m_angleLerpCurve.Evaluate(rectProgress));
        } else { /* 缩小 radius */
            // 更新半径
            m_flipEffect.radius = m_initRadius - (currentLength - rectLength);
            // 更新 cylinderPos
            if (m_horizontalDir != 0)
                cylinderPos.x = m_horizontalDir >= 0 ? 1f : 0f;
            else
                cylinderPos.y = m_verticalDir >= 0 ? 1f : 0f;
            // 更新方向
            m_flipEffect.scrollAngle = targetScrollAngle;
        }
        m_flipEffect.cylinderPos = cylinderPos;
    }
}
