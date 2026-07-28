using UnityEngine;

public enum MilestoneMetric
{
    LifetimeCoins,
    LifetimeDistance,
    LifetimeSeconds,
    TotalDailyRewards,       
    HighestSingleRunDistance, 
    HighestSingleRunCoins,    
    SkinsBought,              
    ThemesBought   
}

[CreateAssetMenu(
    fileName = "MilestoneData",
    menuName = "EndlessRunner/Milestone Definition"
)]
public class MilestoneData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string title;

    [TextArea]
    public string description;

    [Header("Goal")]
    public MilestoneMetric metric;
    public float targetValue;

    [Header("Rewards")]
    public int rewardCoins;
    public int permanentCoinBonusPercent;
}
