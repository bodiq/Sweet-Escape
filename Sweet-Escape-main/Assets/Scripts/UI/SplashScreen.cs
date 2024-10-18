using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup ninevaLogo;
    [SerializeField] private Animator creepyAnimator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<GameObject> ninevaWords = new();
    [SerializeField] private RectTransform rectTransform;

    private List<Vector3> _wordsInitialPos = new();

    private Coroutine _animationCoroutine;

    private float _animationDurationAnim;
    
    private const float HalfDuration = 0.5f;
    private const float FallingDuration = 1.5f;
    
    private WaitForSeconds _waitForAnimation;
    private WaitForSeconds _waitForSceneChange;
    
    private readonly WaitForSeconds _waitHalfSeconds = new(HalfDuration);
    private readonly WaitForSeconds _waitForNextButtonStarts = new(0.05f);

    private List<Tweener> _ninevaWordsTweeners = new();
    
    private void Awake()
    {
        var anims = creepyAnimator.runtimeAnimatorController.animationClips;

        foreach (var anim in anims)
        {
            if (anim.name == "CreapyCreamsSplashCreams")
            {
                _animationDurationAnim = anim.length;
            }
        }
        creepyAnimator.gameObject.SetActive(false);
        _waitForAnimation = new WaitForSeconds(_animationDurationAnim);
        _waitForSceneChange = new WaitForSeconds(3f);
        
        foreach (var t in ninevaWords)
        {
            _wordsInitialPos.Add(t.transform.localPosition);
            t.transform.localPosition = new Vector3(0f, t.transform.localPosition.y + rectTransform.rect.height);
        }
    }

    private void Start()
    {
        _animationCoroutine = StartCoroutine(StartAnimation());
    }
    
    private IEnumerator StartAnimation()
    {
        audioSource.Play();
        yield return _waitHalfSeconds;
        
        creepyAnimator.gameObject.SetActive(true);
        creepyAnimator.Play("SplashAnimation");

        yield return _waitForAnimation;

        for (var i = 0; i < ninevaWords.Count; i++)
        {
            _ninevaWordsTweeners.Add(ninevaWords[i].transform.DOLocalMove(_wordsInitialPos[i], FallingDuration).SetEase(Ease.OutBounce));
            yield return _waitForNextButtonStarts;
        }

        yield return _waitForSceneChange;

        SceneManager.LoadSceneAsync(1);
    }

    private void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        if (!_ninevaWordsTweeners.IsNullOrEmpty())
        {
            foreach (var tweeners in _ninevaWordsTweeners)
            {
                tweeners?.Kill();
            }
        }
    }
}
