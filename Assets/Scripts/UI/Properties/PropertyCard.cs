using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EstateBuilder.Models;

public class PropertyCard : MonoBehaviour
{
    public Image           houseImage; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;
    public Button          buyButton;

    public void Setup(PropertyData data, System.Action<PropertyData> onBuy)
    {
        nameText.text        = data.name;
        priceText.text       = $"₱{data.base_price:N0}";
        descriptionText.text = data.description;
        buyButton.onClick.AddListener(() => onBuy(data));

        if (!string.IsNullOrEmpty(data.image_ref)) 
        {
            // Extract just the name (e.g., "modern_home") from the database string
            string[] pathParts = data.image_ref.Split('/');
            string fileName = pathParts[pathParts.Length - 1];

            // Loads the Sprite from Assets/Sprites/Resources/Properties
            Sprite loadedSprite = Resources.Load<Sprite>("Properties/" + fileName);
            
            if (loadedSprite != null)
            {
                houseImage.sprite = loadedSprite;
            }
            else
            {
                Debug.LogWarning("Could not find image: " + fileName);
            }
        }
    }
}