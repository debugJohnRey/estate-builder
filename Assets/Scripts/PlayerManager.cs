using UnityEngine;
using System;
using EstateBuilder.Database;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    
    // UI elements will listen to this event
    public Action<double> OnCashChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshCash();
    }

    // Call this whenever money is spent or earned
    public void RefreshCash()
    {
        int playerId = 1;
        double currentCash = DatabaseManager.Instance.GetDB()
            .ExecuteScalar<double>("SELECT cash_balance FROM Player WHERE player_id = ?", playerId);
            
        OnCashChanged?.Invoke(currentCash);
    }
}