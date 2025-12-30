using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThangDD;
using System;
using Newtonsoft.Json;
using UnityEngine.Events;

public class DataManager : Singleton<DataManager>
{
    const string PLAYER_DATA = "PLAYER_DATA";

    public List<CreatureData> allCreatures;

    public UnityEvent<PlayerData> onPlayerDataChanged;

    PlayerData playerData;

    BigNumber stardustPerSecond = 1;

    #region Unity Methods
    void Start()
    {
        LoadData();
        StartCoroutine(GameSecondTick());
    }
    #endregion

    #region Public Methods
    public void SubcribeOnPlayerDataChanged(UnityAction<PlayerData> action)
    {
        onPlayerDataChanged.AddListener(action);
    }
    public void UnsubcribeOnPlayerDataChanged(UnityAction<PlayerData> action)
    {
        onPlayerDataChanged.RemoveListener(action);
    }
    #endregion

    #region Data Handlers
    public BigNumber GetStarDust()
    {
        return playerData.starDust;
    }
    public void AddStarDust(BigNumber amount)
    {
        playerData.starDust += amount;
        SaveData();

        onPlayerDataChanged.Invoke(playerData);
    }
    public bool SpendStarDust(BigNumber amount)
    {
        if (playerData.starDust >= amount)
        {
            playerData.starDust -= amount;
            SaveData();

            onPlayerDataChanged.Invoke(playerData);
            return true;
        }
        return false;
    }
    public List<CreatureData> GetAllCreatures()
    {
        return allCreatures;
    }
    public int GetOwnedItemLevel(int itemId)
    {
        if (playerData.ownedItems.ContainsKey(itemId))
            return playerData.ownedItems[itemId];
        return 0;
    }
    public void UpgradeOwnedItem(int itemId)
    {
        if (playerData.ownedItems.ContainsKey(itemId))
        {
            BigNumber price = allCreatures[itemId - 1].GetPriceForLevel(playerData.ownedItems[itemId]);
            if (!SpendStarDust(price))
                return;
            playerData.ownedItems[itemId]++;
            SaveData();
            CalculateStardustPerSecond();

            onPlayerDataChanged.Invoke(playerData);
        }
    }
    public int GetTotalDateDifferenceInDays()
    {
        DateTime currentDate = DateTime.Now.Date;
        TimeSpan difference = currentDate - playerData.firstTimeActive;
        SaveData();
        return difference.Days;
    }
    #endregion

    #region Helper Methods
    void CalculateStardustPerSecond()
    {
        stardustPerSecond = 1;
        foreach (var creature in allCreatures)
        {
            int level = GetOwnedItemLevel(creature.id);
            if (level > 0)
            {
                stardustPerSecond += creature.GetProductivityForLevel(level);
            }
        }
    }

    IEnumerator GameSecondTick()
    {
        while (true)
        {
            AddStarDust(stardustPerSecond);
            yield return new WaitForSeconds(1f);
        }
    }
    #endregion

    #region Data Save & Load
    void LoadData()
    {
        playerData = new PlayerData();
        foreach (var creature in allCreatures)
        {
            playerData.ownedItems[creature.id] = 0;
        }
        if (PlayerPrefs.HasKey(PLAYER_DATA))
            playerData = JsonConvert.DeserializeObject<PlayerData>(PlayerPrefs.GetString(PLAYER_DATA));

        CalculateStardustPerSecond();
    }
    void SaveData()
    {
        PlayerPrefs.SetString(PLAYER_DATA, JsonConvert.SerializeObject(playerData));
    }
    #endregion
}

[Serializable]
public class PlayerData
{
    public BigNumber starDust;
    public Dictionary<int, int> ownedItems;
    public DateTime firstTimeActive;

    public PlayerData()
    {
        starDust = 0;
        ownedItems = new Dictionary<int, int>();
        firstTimeActive = DateTime.Now.Date;
    }
}