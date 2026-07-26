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

        // The player already owns this skin. Just equip it!
        if (profile.unlockedSkins != null && profile.unlockedSkins.Contains(skinIndex))
        {
            profile.selectedSkinIndex = skinIndex;
            ProfileManager.instance.SaveActiveProfileJSON();
            Debug.Log("Equipped Skin: " + skinIndex);
        }
        // They don't own it, but they have enough coins to buy it!
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
        //  They are broke.
        else
        {
            Debug.LogWarning("Not enough coins to buy skin " + skinIndex + "!");
        }
    }

    public void TryBuyTheme(string themeID, int price)
    {
        if (ProfileManager.instance == null || ProfileManager.instance.activeProfile == null)
        {
            return;
        }

        PlayerProfileData profile = ProfileManager.instance.activeProfile;

        // : The player already owns this theme.
        if (profile.unlockedThemes != null && profile.unlockedThemes.Contains(themeID))
        {
            Debug.Log("You already own the theme: " + themeID);
        }
        // They don't own it, but they have enough coins to buy it!
        else if (profile.totalCoins >= price)
        {
            profile.totalCoins -= price;              // Deduct coins
            
            if (profile.unlockedThemes == null)
            {
                profile.unlockedThemes = new System.Collections.Generic.List<string>();
            }
            profile.unlockedThemes.Add(themeID);     // Unlock it (but do NOT equip it!)
            
            ProfileManager.instance.SaveActiveProfileJSON();
            Debug.Log("Bought Theme: " + themeID);
        }
        // They are broke.
        else
        {
            Debug.LogWarning("Not enough coins to buy theme " + themeID + "!");
        }
    }
}
