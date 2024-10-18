using Audio;
using UnityEngine;
using AudioType = Audio.AudioType;

public class CoinUIAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        GameManager.Instance.OnGetCoin += CoinSpin;
    }

    public void CoinSpin()
    {
        animator.Play("CoinUISpinning");
        AudioManager.Instance.PlaySFX(AudioType.CollectCoinUI);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGetCoin -= CoinSpin;
    }
}
