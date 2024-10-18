using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Audio.AudioType;

public class DoorTransition : UIScreen
{
    [SerializeField] private Animator animator;
    [SerializeField] private Image image;
    [SerializeField] private Sprite defaultDoorSprite;

    private const string DoorClosingStateName = "DoorClosing";
    private const string DoorOpeningStateName = "DoorOpening";

    private float _doorOpeningDuration;
    private float _doorClosingDuration;

    private WaitForSeconds _waitForOpeningDoor;
    private WaitForSeconds _waitForClosingDoor;

    private Coroutine _openingDoorCoroutine;
    private Coroutine _closingDoorCoroutine;

    protected override void Awake()
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == DoorClosingStateName)
            {
                _doorClosingDuration = clip.length;
            }
            else if (clip.name == DoorOpeningStateName)
            {
                _doorOpeningDuration = clip.length;
            }
        }

        _waitForOpeningDoor = new WaitForSeconds(_doorOpeningDuration);
        _waitForClosingDoor = new WaitForSeconds(_doorClosingDuration);
    }

    public void CloseDoor(bool isAutoOpen = false)
    {
        if (!isAutoOpen)
        {
            animator.Play(DoorClosingStateName);
            AudioManager.Instance.PlaySFX(AudioType.UIDoorClose);
        }
        else
        {
            _closingDoorCoroutine = StartCoroutine(StartClosingDoor());
        }
    }

    public void OpenDoor()
    {
        _openingDoorCoroutine = StartCoroutine(StartOpeningDoor());
    }

    private IEnumerator StartClosingDoor()
    {
        AudioManager.Instance.PlaySFX(AudioType.UIDoorClose);
        animator.Play(DoorClosingStateName);
        yield return _waitForClosingDoor;
        OpenDoor();
    }

    private IEnumerator StartOpeningDoor()
    {
        AudioManager.Instance.PlaySFX(AudioType.UIDoorOpen);
        animator.Play(DoorOpeningStateName);
        yield return _waitForOpeningDoor;
        image.sprite = defaultDoorSprite;
        TurnOff();
    }
}
