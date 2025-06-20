using TMPro;
using UnityEngine;

public class PlayerHUD : Singleton<PlayerHUD>
{
    public TMP_Text PrisonText;
    public Canvas EndScreen;
    public TMP_Text PlayerWonText;

    private void Start()
    {
        GameManager.Instance.GameHasWinner.AddListener(ShowGameEnd);
        if (GameManager.Instance.WinningTeam == GameManager.Instance.m_Teams[0])
            PlayerWonText.enabled = true;
    }
    public void ShowPrisonText(bool _show)
    {
        PrisonText.enabled = _show;
    }

    private void ShowGameEnd()
    {
        EndScreen.enabled = true;
    }
}
