using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EstateBuilder.Models;
using EstateBuilder.Database;

public class ChooseHouseUI : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform  content;
    public Button     closeBtn;
    public GameObject notEnoughCashPanel;

    private bool isReady = false;
    private Transform currentPlot; 

    void Start()
    {
        closeBtn.onClick.AddListener(ClosePanel);
        isReady = true;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (!isReady) return;
        LoadProperties();
    }

    public void OpenPanel(Transform plotTransform)
    {
        currentPlot = plotTransform;
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    void LoadProperties()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<PropertyData> properties = new PropertyRepository().GetAllProperties();

        foreach (PropertyData property in properties)
        {
            GameObject card = Instantiate(cardPrefab, content);
            card.GetComponent<PropertyCard>().Setup(property, OnBuyClicked);
        }
    }

    void OnBuyClicked(PropertyData property)
    {
        int playerId = 1;
        int currentDay = DatabaseManager.Instance.GetDB()
                        .ExecuteScalar<int>("SELECT game_day FROM Player WHERE player_id = 1");

        bool success = new PropertyRepository()
            .BuyProperty(playerId, property.property_id, property.base_price, currentDay);

        if (success)
        {
            TerrainPlacer.Instance.Place(property, currentPlot);
            PlayerManager.Instance.RefreshCash(); // ADD THIS LINE
            ClosePanel();
        }
        else
        {
            notEnoughCashPanel.SetActive(true); // ADD THIS LINE
        }
    }
}