using UnityEngine;

public class SkinShopUI : MonoBehaviour
{
    public void TryBuyOrEquipSkin(int skinIndex, int price)
    {
        if (ProfileManager.instance == null || ProfileManager.instance.activeProfile == null)
        {
            Debug.LogError("No active profile found! Cannot buy skin.");
            return;
        }

        PlayerProfileData profile = ProfileManager.instance.activeProfile;

        // SCENARIO 1: The player already owns this skin. Just equip it!
        if (profile.unlockedSkins != null && profile.unlockedSkins.Contains(skinIndex))
        {
            profile.selectedSkinIndex = skinIndex;
            ProfileManager.instance.SaveActiveProfileJSON();
            Debug.Log("Equipped Skin: " + skinIndex);
        }
        // SCENARIO 2: They don't own it, but they have enough coins to buy it!
        else if (profile.totalCoins >= price)
        {
            profile.totalCoins -= price;              // Deduct coins
            
            if (profile.unlockedSkins == null)
            {
                profile.unlockedSkins = new System.Collections.Generic.List<int>();
            }
            profile.unlockedSkins.Add(skinIndex);     // Unlock it
            
            profile.selectedSkinIndex = skinIndex;    // Equip it immediately
            ProfileManager.instance.SaveActiveProfileJSON();
            Debug.Log("Bought and Equipped Skin: " + skinIndex);
        }
        // SCENARIO 3: They are broke.
        else
        {
            Debug.LogWarning("Not enough coins to buy skin " + skinIndex + "!");
        }
    }
}
