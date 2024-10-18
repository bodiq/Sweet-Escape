using System;
using System.Collections.Generic;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct CharacterPreset
    {
        public string name;
        public Sprite fullRectIcon;
        public AnimationClip mainMenuCharacterAnimationClip;
        public Player player;
        public List<AbilityData> abilityList;
        public List<SkinData> skinData;
        public int priceToUnlock;
        public Sprite characterSelectionIconSprite;
        public Color backgroundCharacterSelectionColor;
    }
}