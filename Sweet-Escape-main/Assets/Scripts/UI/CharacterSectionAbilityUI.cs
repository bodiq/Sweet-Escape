using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSectionAbilityUI : MonoBehaviour
{
    [SerializeField] private GameObject availableAbility;
    [SerializeField] private GameObject unavailableAbility;
    [SerializeField] private Image abilityImage;
    [SerializeField] private TextMeshProUGUI descriptionAbilityText;
    [SerializeField] private TextMeshProUGUI levelLockedAbility;

    private AbilityData _abilityData;
    private bool _isInitialized;

    public void Initialize(AbilityData abilityData, int level)
    {
        availableAbility.SetActive(true);
        unavailableAbility.SetActive(false);
        abilityImage.sprite = abilityData.AbilityIcon;
        descriptionAbilityText.text = abilityData.AbilityDescription;
        _isInitialized = true;
        _abilityData = abilityData;
    }
}
