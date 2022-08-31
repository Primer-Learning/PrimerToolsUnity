using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PrimerColor
{   
    /*
    These are the main colors I use in the vids. I don't own the colors,
    so you're free to use them without constraint.

    That said, if your goal is to publish, you're probably better off creating your own
    color palette to give your content a distinct feel. If you decide to do that,
    coolors.co is a super useful tool for that.

    Main colors: https://coolors.co/2f3336-ffffff-3e7ea0-ff9500-e7e247-d63b50-698f3f-db5abb-5e666c
    Complementary colors: https://coolors.co/ffa06d-073b8f-6d9d37-9fe3da-d0aaff-092d13
    */

    // Defining colors as Vector4 because UnityEngine.Color stores values on a 0-1 scale
    // rather than a 0-255 scale

    // Main colors
    static Vector4 celadonBlue = new Color (62, 126, 160, 255);
    static Vector4 yellowOrangeColorWheel = new Vector4 (255, 148, 0, 255);
    static Vector4 corn = new Vector4 (231, 226, 71, 255);
    static Vector4 amaranth = new Vector4 (214, 59, 80, 255);
    static Vector4 maximumGreen = new Vector4 (105, 143, 63, 255);
    static Vector4 superPink = new Vector4 (219, 90, 186, 255);
    static Vector4 onyx = new Vector4 (47, 51, 54, 255);
    static Vector4 blackCoral = new Vector4 (94, 102, 108, 255);
    static Vector4 white = new Vector4 (243, 242, 240, 255);
    static Vector4 black = new Vector4 (0, 0, 0, 255);

    public static Color Blue = UnityColorFromVector4(celadonBlue);
    public static Color Orange = UnityColorFromVector4(yellowOrangeColorWheel);
    public static Color Yellow = UnityColorFromVector4(corn);
    public static Color Red = UnityColorFromVector4(amaranth);
    public static Color Green = UnityColorFromVector4(maximumGreen);
    public static Color Purple = UnityColorFromVector4(superPink);
    public static Color Gray = UnityColorFromVector4(onyx);
    public static Color LightGray = UnityColorFromVector4(blackCoral);
    public static Color White = UnityColorFromVector4(white); 
    public static Color Black = UnityColorFromVector4(black);

    // Complementary colors
    static Vector4 atomicTangerine = new Vector4(255, 160, 109, 255);
    static Vector4 darkCornFlowerBlue = new Vector4(7, 59, 143, 255);
    static Vector4 oliveDrab3 = new Vector4(109, 157, 55, 255);
    static Vector4 middleBlueGreen = new Vector4(159, 227, 218, 255);
    static Vector4 mauve = new Vector4(208, 170, 255, 255);
    static Vector4 darkGreen = new Vector4(9, 45, 19, 255);

    // Perhaps not the most useful names, but it does make it inconvenient to use 
    // them as main colors
    public static Color BlueComplement = UnityColorFromVector4(atomicTangerine);
    public static Color OrangeComplement = UnityColorFromVector4(darkCornFlowerBlue);
    public static Color YellowComplement = UnityColorFromVector4(oliveDrab3);
    public static Color RedComplement = UnityColorFromVector4(middleBlueGreen);
    public static Color GreenComplement = UnityColorFromVector4(mauve);
    public static Color PurpleComplement = UnityColorFromVector4(darkGreen);

    public static Dictionary<Color, Color> ComplementaryColorMap = new Dictionary<Color, Color>() {
        {Blue, BlueComplement},
        {Orange, OrangeComplement},
        {Yellow, YellowComplement},
        {Red, RedComplement},
        {Green, GreenComplement},
        {Purple, PurpleComplement},
        {Gray, White},
        {White, Gray}
    };
    public static List<Color> BlobColors = new List<Color>() {
        PrimerColor.Blue,
        PrimerColor.Green,
        PrimerColor.Yellow,
        PrimerColor.Orange,
        PrimerColor.Red,
        PrimerColor.Purple
    };

    public static Color UnityColorFromVector4(Vector4 sc) {
        return sc / 255;
    }
}