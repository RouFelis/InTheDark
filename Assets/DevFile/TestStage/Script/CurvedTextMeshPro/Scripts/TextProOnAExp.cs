//MIT License

//Copyright(c) 2019 Antony Vitillo(a.k.a. "Skarredghost")

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using UnityEngine;
using System.Collections;
using TMPro;

namespace ntw.CurvedTextMeshPro
{
    /// <summary>
    /// 텍스트를 지수 함수(n^x) 곡선에 따라 휘게 그려주는 클래스
    /// </summary>
    [ExecuteInEditMode]
    public class TextProOnAExp : TextProOnACurve
    {
        /// <summary>
        /// 포물선의 곡률 계수 (a값, 작을수록 더 평평하고, 클수록 곡선이 가파름)
        /// </summary>
        [SerializeField]
        [Tooltip("포물선의 곡률 계수 (작을수록 평평한 곡선)")]
        private float m_parabolaWidth = 0.01f;

        /// <summary>
        /// 이전 프레임의 곡률 값 (변경 여부 확인용)
        /// </summary>
        private float m_oldParabolaWidth = float.MaxValue;

        /// <summary>
        /// 파라미터가 변경되었는지 확인
        /// </summary>
        protected override bool ParametersHaveChanged()
        {
            bool changed = m_parabolaWidth != m_oldParabolaWidth;
            m_oldParabolaWidth = m_parabolaWidth;
            return changed;
        }

        /// <summary>
        /// 텍스트 메시의 한 문자에 대한 변환 행렬 계산
        /// </summary>
        protected override Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
        {
            float x0 = charMidBaselinePos.x;

            // 포물선 위치 계산: y = a * x^2
            float y0 = m_parabolaWidth * x0 * x0;

            // 여러 줄 텍스트일 경우 줄마다 Y위치를 다르게 보정
            float lineOffset = textInfo.lineInfo[0].lineExtents.max.y * textInfo.characterInfo[charIdx].lineNumber;
            Vector2 newMidBaselinePos = new Vector2(x0, y0 - lineOffset);

            // 기울기(도함수): y' = 2 * a * x
            float slope = 2f * m_parabolaWidth * x0;
            float angle = Mathf.Atan(slope) * Mathf.Rad2Deg;

            // 이동 + 회전 변환 행렬 반환
            return Matrix4x4.TRS(
                new Vector3(newMidBaselinePos.x, newMidBaselinePos.y, 0),
                Quaternion.AngleAxis(angle, Vector3.forward),
                Vector3.one
            );
        }

    }
}