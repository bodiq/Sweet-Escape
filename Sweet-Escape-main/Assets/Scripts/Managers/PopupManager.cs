using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Structs;
using UnityEngine;

public class PopupManager : UIScreen
{
    [Serializable]
    public struct ButtonPair
    {
        public PopupTypes Key;
        public PopupMessage Value;
    }
    
    [SerializeField] private List<ButtonPair> popups = new();
    [SerializeField] private GameObject darkBackground;

    public void ActivatePopupMessage(PopupTypes popupType, string customMessage = null)
    {
        foreach (var popup in popups.Where(popup => popup.Key == popupType))
        {
            popup.Value.ShowPopupMessage(customMessage);
            break;
        }
    }

    public void ActivateAbilityDescriptionPopup(AbilityData abilityData)
    {
        foreach (var popup in popups)
        {
            if (popup.Key == PopupTypes.AbilityDescription)
            {
                TurnBackground(true);
                popup.Value.ShowAbilityDescriptionMessage(abilityData);
            }
        }
    }
    
    public void TurnBackground(bool isActive)
    {
        darkBackground.SetActive(isActive);
    }

    public void TurnOffAllPopups()
    {
        foreach (var message in popups)
        {
            message.Value.TurnOffPopup();
        }
    }
}
