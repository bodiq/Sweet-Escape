using UnityEngine;
using UnityEngine.UI;

public class CharacterLevelStar : MonoBehaviour
{
    [SerializeField] private GameObject fullStar;
    [SerializeField] private Image emptyStar;
    [SerializeField] private Animator starAnimator;

    [SerializeField] private Sprite defaultPureSprite;

    private const string StarAnimationState = "CharacterLevelStarAnim";

    public void EmptyStar()
    {
        fullStar.SetActive(false);
        emptyStar.gameObject.SetActive(true);
        ResetStarToDefault();
    }

    public void FullStar(bool animateStar = false)
    {
        if (!animateStar)
        {
            fullStar.SetActive(true);
            emptyStar.gameObject.SetActive(false);
        }
        else
        {
            starAnimator.enabled = true;
            starAnimator.Play(StarAnimationState);
        }
    }

    private void ResetStarToDefault()
    {
        starAnimator.enabled = false;
        emptyStar.sprite = defaultPureSprite;
    }
}
