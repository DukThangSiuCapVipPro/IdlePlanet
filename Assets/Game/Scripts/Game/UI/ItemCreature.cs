using System.Collections;
using System.Collections.Generic;
using ThangDD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCreature : MonoBehaviour
{
    public Image icon;
    public TMP_Text txtName;
    public TMP_Text txtDescription;
    public TMP_Text txtUpgradePrice;
    public SmoothButton buttonUpgrade;
    public GameObject lockObject;
    public SmoothButton buttonBuy;
    public TMP_Text txtBuyPrice;

    CreatureData creatureData;

    public void Initialize(CreatureData creatureData)
    {
        this.creatureData = creatureData;

        buttonUpgrade.onClick.RemoveAllListeners();
        buttonUpgrade.onClick.AddListener(OnClickUpgrade);
        buttonBuy.onClick.RemoveAllListeners();
        buttonBuy.onClick.AddListener(OnClickBuy);

        DataManager.Instance.UnsubcribeOnPlayerDataChanged(OnPlayerDataChanged);
        DataManager.Instance.SubcribeOnPlayerDataChanged(OnPlayerDataChanged);

        UpdateUI();
    }

    void UpdateUI()
    {
        DataManager dataManager = DataManager.Instance;
        int ownedLevel = dataManager.GetOwnedItemLevel(creatureData.id);
        icon.sprite = creatureData.icon;
        txtName.text = creatureData.name;
        txtDescription.text = creatureData.description;

        icon.sprite = creatureData.icon;
        txtName.text = $"{creatureData.name} Lv.{ownedLevel}";
        txtDescription.text = creatureData.description;
        txtUpgradePrice.text = creatureData.GetPriceForLevel(ownedLevel).ToString();
        txtBuyPrice.text = creatureData.GetPriceForLevel(0).ToString();
        buttonUpgrade.interactable = dataManager.GetStarDust() >= creatureData.GetPriceForLevel(ownedLevel);

        lockObject.SetActive(ownedLevel == 0);
        bool enoughDays = DataManager.Instance.GetTotalDateDifferenceInDays() >= creatureData.requiredDayNumber;
        buttonBuy.interactable = ownedLevel == 0 && enoughDays && DataManager.Instance.GetStarDust() >= creatureData.GetPriceForLevel(0);
        if (!enoughDays)
            txtBuyPrice.text = $"{creatureData.requiredDayNumber - DataManager.Instance.GetTotalDateDifferenceInDays()} days left";
    }
    void OnPlayerDataChanged(PlayerData playerData)
    {
        UpdateUI();
    }

    void OnClickUpgrade()
    {
        DataManager.Instance.UpgradeOwnedItem(creatureData.id);
        UpdateUI();
    }
    void OnClickBuy()
    {
        DataManager.Instance.UpgradeOwnedItem(creatureData.id);
        UpdateUI();
    }
}
