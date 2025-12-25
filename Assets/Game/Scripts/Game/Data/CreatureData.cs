using System.Collections;
using System.Collections.Generic;
using ThangDD;
using UnityEngine;


[CreateAssetMenu(fileName = "New Creature", menuName = "Idle Planet/Creature Data")]
public class CreatureData : ScriptableObject
{
    [Header("Basic Info")]
    public int id;
    public new string name;
    public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Category")]
    public CreatureSize size;
    public CreatureEra era;

    [Header("Economy")]
    [Tooltip("Base price to purchase this creature")]
    public BigNumber basePrice;
    [Tooltip("Price multiplier per upgrade level")]
    public float priceScalePerLevel = 1.15f;

    [Header("Productivity")]
    [Tooltip("Base stardust generation per second at level 1")]
    public BigNumber baseProductivity;
    [Tooltip("Productivity multiplier per upgrade level")]
    public float productivityScalePerLevel = 1.1f;

    [Header("Requirements")]
    [Tooltip("Other creatures that must be owned first")]
    public List<CreatureData> requiredCreatures;
    [Tooltip("Days that must be passed first")]
    public int requiredDayNumber;

    /// <summary>
    /// Calculate the price for a specific level
    /// </summary>
    public BigNumber GetPriceForLevel(int level)
    {
        return basePrice * Mathf.Pow(priceScalePerLevel, level - 1);
    }

    /// <summary>
    /// Calculate the productivity (stardust/s) for a specific level
    /// </summary>
    public BigNumber GetProductivityForLevel(int level)
    {
        return baseProductivity * Mathf.Pow(productivityScalePerLevel, level - 1);
    }

    /// <summary>
    /// Calculate total cost to upgrade from level A to level B
    /// </summary>
    public BigNumber GetTotalUpgradeCost(int fromLevel, int toLevel)
    {
        BigNumber totalCost = BigNumber.Zero;
        for (int i = fromLevel + 1; i <= toLevel; i++)
        {
            totalCost += GetPriceForLevel(i);
        }
        return totalCost;
    }
}

public enum CreatureSize
{
    Tiny,
    Small,
    Medium,
    Large,
    Huge,
    Colossal
}

public enum CreatureEra
{
    Ancient,
    Prehistoric,
    Medieval,
    Industrial,
    Modern,
    Futuristic
}

