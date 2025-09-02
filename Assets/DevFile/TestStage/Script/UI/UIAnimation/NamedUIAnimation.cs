using System;
using UnityEngine;

[Serializable]
public struct NamedUIAnimation
{
    public string name;            // 인스펙터에서 보기 편한 이름
    public UIAnimation animation;  // 실행할 애니메이션
}
