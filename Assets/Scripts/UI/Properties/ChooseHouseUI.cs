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

    private bool isReady = false; // ← guard flag

    void Start()
    {
        closeBtn.onClick.AddListener(() => gameObject.SetActive(false));
        isReady = true;          // ← DatabaseManager is guaranteed ready by now
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (!isReady) return;    // ← skip if Start hasn't run yet
        LoadProperties();
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
        Debug.Log($"Buying {property.name}");
    }
}