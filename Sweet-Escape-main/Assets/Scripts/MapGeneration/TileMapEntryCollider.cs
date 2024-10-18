using System.Collections;
using Audio;
using Enums;
using UnityEngine;
using AudioType = Audio.AudioType;

public class TileMapEntryCollider : MonoBehaviour, ITrigger
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private BoxCollider2D _boxCollider2D;

    private bool isEntered;

    public Transform Transform => transform;

    private void OnEnable()
    {
        ResetCollider();
        GameManager.Instance.OnPlayerRespawn += ResetCollider;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawn -= ResetCollider;
    }

    private void ResetCollider()
    {
        isEntered = false;
        animator.enabled = false;
        spriteRenderer.sprite = defaultSprite;
        _boxCollider2D.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Trigger");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Trigger(default);
        }
    }

    public void Trigger(Player _)
    {
        if (!isEntered)
        {
            animator.enabled = true;
            GameManager.Instance.RefreshSlimePlayer(TilemapManager.Instance.CurrentTilemap.Tilemap);

            TilemapManager.Instance.OnRoomChanged();

            gameObject.layer = LayerMask.NameToLayer("Default");
            if (animator)
            {
                animator.Play("Close");
                AudioManager.Instance.PlaySFX(AudioType.DoorClose);
            }

            isEntered = true;

            StartCoroutine(ActivateCollider());

            if (DailyBoxManager.AvailableDailyMissionsTypes.Contains(DailyMissions.GoThroughRooms))
            {
                UIManager.Instance.LeaderboardDailySectionManager.DailyBoxManager.onDailyMissionsRefresh?.Invoke(DailyMissions.GoThroughRooms, null, 1);
            }
        }
    }

    private IEnumerator ActivateCollider()
    {
        yield return null;
        _boxCollider2D.isTrigger = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerRespawn -= ResetCollider;
    }
}