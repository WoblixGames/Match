using UnityEngine;
using TMPro;
public class MoveCounter : MonoBehaviour
{
    public TextMeshProUGUI moveCounterText;
    public int moveCounter = 3; // Başlangıç hamle sayısı

    void Start()
    {
        if (moveCounterText == null)
        {
            Debug.LogError("moveCounterText referansı atanmadı! Lütfen Unity Editor'de bu bileşeni atayın.");
            return;
        }
        
        UpdateMoveCounterUI();
    }

    // Hamle yapıldığında bu fonksiyonu çağırın
    public void DecreaseMoveCounter()
    {
        Debug.Log("Oyuncu bir hamle yaptı.");
        if (moveCounter > 0)
        {
            
            moveCounter--;
            UpdateMoveCounterUI();
        }
    }

    // UI'da hamle sayısını güncelleyen fonksiyon
    void UpdateMoveCounterUI()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = "Moves Left: " + moveCounter.ToString();
        }
        else
        {
            Debug.LogError("moveCounterText referansı null. UI güncellenemedi.");
        }
    }
}
