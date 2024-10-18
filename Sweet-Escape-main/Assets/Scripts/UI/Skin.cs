using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skin : MonoBehaviour
{
    [SerializeField] private Image skinSprite;
    [SerializeField] private TextMeshProUGUI skinName;
    [SerializeField] private GameObject availableObject;
    [SerializeField] private GameObject unavailableObject;
    [SerializeField] private GameObject selectedIcon;
    [SerializeField] private GameObject unselectedIcon;
    [SerializeField] private Button button;

    private SkinEnum _skinEnum;
    private Sprite _skinCharacterSprite;
    private RuntimeAnimatorController _animatorController;

    public void Initialize(Sprite sprite, string name, bool isAvailable, bool isSelected, SkinEnum skinEnum, Sprite characterSkin, RuntimeAnimatorController animatorController)
    {
        skinSprite.sprite = sprite;
        skinName.text = name;
        _skinEnum = skinEnum;
        _skinCharacterSprite = characterSkin;
        _animatorController = animatorController;
        
        if (isAvailable)
        {
            availableObject.SetActive(true);
            unavailableObject.SetActive(false);
        }
        else
        {
            availableObject.SetActive(false);
            unavailableObject.SetActive(true);
        }

        if (isSelected)
        {
            selectedIcon.SetActive(true);
            unselectedIcon.SetActive(false);
        }
        else
        {
            selectedIcon.SetActive(false);
            unselectedIcon.SetActive(true);
        }
        
        button.onClick.AddListener(SelectSkin);
    }

    public void TurnOffSelected()
    {
        selectedIcon.SetActive(false);
        unselectedIcon.SetActive(true);
    }

    public void TurnOnSelected()
    {
        selectedIcon.SetActive(true);
        unselectedIcon.SetActive(false);
    }
    
    private void SelectSkin()
    {
        TurnOnSelected();
        
        UIManager.Instance.CharactersSection.SkinSelection.ChangeSelectedSkin(_skinEnum, this, _skinCharacterSprite, _animatorController);
    }
}
