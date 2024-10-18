using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPlate : MonoBehaviour
{
    [SerializeField] private Image mainPlate;
    [SerializeField] private TextMeshProUGUI rankNumber;
    [SerializeField] private TextMeshProUGUI pointsCount;
    [SerializeField] private TextMeshProUGUI userName;

    public void SetData(Sprite plate, int rank, int points, string usernameText)
    {
        mainPlate.sprite = plate;
        rankNumber.text = rank.ToString();
        pointsCount.text = points.ToString();
        userName.text = usernameText;
    }
}
