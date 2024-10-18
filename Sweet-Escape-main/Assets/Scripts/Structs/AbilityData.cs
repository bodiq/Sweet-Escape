using System;
using Abilities;
using Enums;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct AbilityData
    {
        public CharacterAbilities AbilityType;
        public ICharacterAbility Ability;
        public Sprite AbilityIcon;
        public string AbilityName;
        public string AbilityDescription;
    }
}