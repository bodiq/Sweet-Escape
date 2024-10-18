using System;
using Enums;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct SkinData
    {
        public Sprite skinIcon;
        public string skinName;
        public Sprite characterSkinSprite;
        public Sprite characterSkinMenuSprite;
        public SkinEnum skinEnum;
        public RuntimeAnimatorController defaultAnimatorController;
        public RuntimeAnimatorController shieldedAnimatorController;
    }
}