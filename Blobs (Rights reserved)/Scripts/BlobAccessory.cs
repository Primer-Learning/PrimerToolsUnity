using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobAccessory : PrimerObject
{
    internal PrimerBlob blob;
    internal AccessoryType accessoryType;

    // Constructor that loads and instantiates the proper prefab
    // also that sets the color or texture based on the blob color and type
    public static BlobAccessory NewAccessory(PrimerBlob blob, AccessoryType accessoryType, bool highQuality = true, bool colorMatch = false) {
        GameObject go = null;
        string parentBoneName = null;
        string baseTextureName = null;
        switch (accessoryType)
        {
            case AccessoryType.beard:
                if (highQuality) { go = Instantiate(Resources.Load<GameObject>("beard_high_res")); }
                else { go = Instantiate(Resources.Load<GameObject>("beard_low_res")); }
                parentBoneName = "bone_neck";
                break;
            case AccessoryType.glasses:
                go = Instantiate(Resources.Load<GameObject>("glasses"));
                parentBoneName = "bone_neck";
                break;
            case AccessoryType.sunglasses:
                go = Instantiate(Resources.Load<GameObject>("sunglasses"));
                parentBoneName = "bone_neck";
                break;

            // Textured accessories
            case AccessoryType.beanie:
                go = Instantiate(Resources.Load<GameObject>("beanie"));
                parentBoneName = "bone_neck";
                baseTextureName = "beanie textures/texture_beanie";
                break;
            case AccessoryType.detectiveHat:
                go = Instantiate(Resources.Load<GameObject>("detective_hat"));
                parentBoneName = "bone_neck";
                baseTextureName = "detective hat textures/texture_detective_hat";
                break;
            case AccessoryType.eyePatch:
                go = Instantiate(Resources.Load<GameObject>("eye_patch"));
                parentBoneName = "bone_neck";
                baseTextureName = "eyepatch textures/texture_eyepatch";
                break;
            case AccessoryType.froggyHat:
                go = Instantiate(Resources.Load<GameObject>("froggy_hat"));
                parentBoneName = "bone_neck";
                baseTextureName = "froggy hat textures/texture_froggy_hat";
                break;
            case AccessoryType.propellerHat:
                go = Instantiate(Resources.Load<GameObject>("propeller_cap"));
                parentBoneName = "bone_neck";
                baseTextureName = "propeller cap textures/texture_propeller_cap";
                break;
            case AccessoryType.starShades:
                go = Instantiate(Resources.Load<GameObject>("star_shades"));
                parentBoneName = "bone_neck";
                baseTextureName = "starshades textures/texture_starshades";
                break;
            case AccessoryType.wizardHat:
                go = Instantiate(Resources.Load<GameObject>("wizard_hat"));
                parentBoneName = "bone_neck";
                baseTextureName = "wizard hat textures/texture_wizard_hat";
                break;
            // Textured accessories with no color options
            case AccessoryType.monocle:
                go = Instantiate(Resources.Load<GameObject>("monocle"));
                parentBoneName = "bone_neck";
                baseTextureName = "texture_monocle";
                colorMatch = false;
                break;
            case AccessoryType.magnifyingGlass:
                go = Instantiate(Resources.Load<GameObject>("magnifying_glass"));
                parentBoneName = "bone_arm.r_end";
                baseTextureName = "texture_magnifying_glass";
                colorMatch = false;
                break;
            case AccessoryType.fairSign:
                go = Instantiate(Resources.Load<GameObject>("fair_sign"));
                parentBoneName = "bone_neck";
                baseTextureName = "TEXTURE_woodensign_base_fair";
                colorMatch = false;
                break;
            case AccessoryType.cheaterSign:
                go = Instantiate(Resources.Load<GameObject>("cheater_sign"));
                parentBoneName = "bone_neck";
                baseTextureName = "TEXTURE_woodensign_base_fair";
                colorMatch = false;
                break;
            case AccessoryType.none:
                return null;
            default:
                Debug.LogError("Accessory type not recognized");
                return null;
        }

        BlobAccessory ba = go.AddComponent<BlobAccessory>();
        ba.blob = blob;
        ba.accessoryType = accessoryType;
        ba.transform.parent = blob.transform.FindDeepChild(parentBoneName);
        ba.transform.localPosition = Vector3.zero;
        ba.transform.localRotation = Quaternion.identity;
        ba.transform.localScale = Vector3.one;
        ba.SetColor(colorMatch, baseTextureName);
        return ba;
    }

    internal void SetColor(bool colorMatch = false, string textureName = null) {
        if (texturedAccessoryTypes.Contains(accessoryType)) {
            // If !colormatch, look for a default texture. If it doesn't exist, colormatch anyway
            if (!colorMatch) {
                Texture texture = Resources.Load<Texture>(textureName);
                if (texture != null) {
                    SetTexture(textureName);
                }
                else {
                    colorMatch = true;
                }
            }
            if (colorMatch) {
                string extension = null;
                // try {
                    extension = complementaryTextureNameExtensions[blob.color];
                // }
                // catch {}
                textureName += extension;
                Texture texture = Resources.Load<Texture>(textureName);
                if (texture != null) {
                    SetTexture(textureName);
                }
                else {
                    SetColor(PrimerColor.ComplementaryColorMap[blob.color]);
                }
            }
        }
        else {
            if (colorMatch) {
                if (firstMatOnlyAccessories.Contains(accessoryType)) {
                    base.SetColor(PrimerColor.ComplementaryColorMap[blob.color], onlyFirstMaterial: true);
                }
                else {
                    base.SetColor(PrimerColor.ComplementaryColorMap[blob.color]);
                }
            }
        }
    }
    internal void SetTexture(string textureName) {
        SetTexture(Resources.Load<Texture>(textureName));
    }
    internal void SetTexture(Texture texture) {
        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        // if (mr.Length != 1) { Debug.LogWarning($"Unexpected number of MeshRenderers in {this.gameObject.name}"); }
        Material mat = mr[0].material;
        mat.mainTexture = texture;
    }
    static List<AccessoryType> texturedAccessoryTypes = new List<AccessoryType>() {
        AccessoryType.beanie,
        AccessoryType.detectiveHat,
        AccessoryType.eyePatch,
        AccessoryType.froggyHat,
        AccessoryType.propellerHat,
        AccessoryType.starShades,
        AccessoryType.wizardHat
    };
    static List<AccessoryType> firstMatOnlyAccessories = new List<AccessoryType>() {
        AccessoryType.glasses,
        AccessoryType.sunglasses
    };
    static Dictionary<Color, string> complementaryTextureNameExtensions = new Dictionary<Color, string>() {
        {PrimerColor.Blue, "_orange"},
        {PrimerColor.Orange, "_blue"},
        {PrimerColor.Yellow, "_green"},
        {PrimerColor.Red, "_teal"},
        {PrimerColor.Green, "_purple"},
        {PrimerColor.Purple, "_dark_green"},
        {PrimerColor.White, ""},
        {PrimerColor.Gray, ""},
    };
    internal static Dictionary<AccessoryType, float> SignHeights = new Dictionary<AccessoryType, float>() {
        {AccessoryType.froggyHat, 0.2f},
        {AccessoryType.beanie, 0.34f},
        {AccessoryType.propellerHat, 0.3f},
        {AccessoryType.wizardHat, 0.43f}
    };
    // static Dictionary<Color, string> beanieDict = new Dictionary<Color, string>() {
    //     {PrimerColor.Blue, "texture_beanie_orange"},
    //     {PrimerColor.Orange, "texture_beanie_blue"},
    //     {PrimerColor.Yellow, "texture_beanie_green"},
    //     {PrimerColor.Red, "texture_beanie_teal"},
    //     {PrimerColor.Green, "texture_beanie_purple"},
    //     {PrimerColor.Purple, "texture_beanie_dark_green"},
    // };
    // static Dictionary<Color, string> detectiveHatDict = new Dictionary<Color, string>() {
    //     // {PrimerColor.Blue, "texture_detective_hat"},
    //     {PrimerColor.Blue, "texture_detective_orange"},
    //     {PrimerColor.Orange, "texture_detective_hat_blue"},
    //     {PrimerColor.Purple, "texture_detective_hat_dark_green"},
    //     {PrimerColor.Yellow, "texture_detective_hat_green"},
    //     {PrimerColor.Red, "texture_detective_hat_teal"},
    //     {PrimerColor.Green, "texture_detective_hat_purple"},
    // };
    // static Dictionary<AccessoryType, Dictionary<Color, string>> textureDictDict = new Dictionary<AccessoryType, Dictionary<Color, string>>() {
    //     {AccessoryType.beanie, beanieDict},
    //     {AccessoryType.detectiveHat, detectiveHatDict},
    //     // {AccessoryType.eyePatch, eyePatchDict},
    //     // {AccessoryType.froggyHat, froggyHatDict},
    //     // {AccessoryType.propellerHat, propellerHatDict},
    //     // {AccessoryType.starShades, starShadesDict},
    //     // {AccessoryType.wizardHat, wizardHatDict}
    // };
}

public enum AccessoryType {
    none,
    beard,
    glasses,
    sunglasses,
    froggyHat,
    beanie,
    detectiveHat,
    eyePatch,
    propellerHat,
    starShades,
    wizardHat,
    monocle,
    magnifyingGlass,
    fairSign,
    cheaterSign
}