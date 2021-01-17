using HarmonyLib;

using System;
using System.Reflection;

using UnityEngine;

using DMT;



public class FancyADS : IHarmony
{
    public static FancyADS Instance { get; private set;}

    public float zoom {get; private set;}

    private float _bfov;

    public float baseFOV
    {
        get => this._bfov;
        set 
        {
            this._bfov = value;
            this.SetZoom();
        }
    }

    public void baseFOVUpdate()
    {
        this.baseFOV = (float)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
    }
    
    private float _cfov;
    public float currentFOV 
    {
        get => this._cfov;
        set 
        {
            this._cfov = value;
            this.SetZoom();
        }
    }

    public float ads { get; private set;}
    public void adsUpdate() 
    {
        this.ads = GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomMouseSensitivity);
    }

    public void Start()
    {
        
        Instance = this;

        Debug.Log(" Loading Patch: " + GetType().ToString());

        var harmony = new Harmony(GetType().ToString());

        harmony.PatchAll(Assembly.GetExecutingAssembly());

    }
    private void SetZoom()
    {
        this.zoom = this._bfov/this._cfov;
    }
}


[HarmonyPatch(typeof(PlayerMoveController), "Update")]
public class FancyADS_Update
{  
    static void Prefix(float __state, ref float ___aimingSensitivity, ref float ___defaultSensitivity)
    {
        __state = ___aimingSensitivity;
        var fancyADS = FancyADS.Instance;

        // TODO find a better way to debug with some sort of flag
        //Debug.Log($"zoom: {fancyADS.zoom} baseFOV: {fancyADS.baseFOV} currentFOV: {fancyADS.currentFOV} aim: {___aimingSensitivity} ads: {fancyADS.ads} def: {___defaultSensitivity}");
        ___aimingSensitivity = (fancyADS.ads * ___defaultSensitivity) / (float)Math.Pow(fancyADS.zoom, 2.0);
    }

    static void Postfix(float __state, ref float ___aimingSensitivity)
    {
        ___aimingSensitivity = __state;
    }
}

[HarmonyPatch(typeof(ItemActionZoom), "ConsumeScrollWheel")]
public class FancyADS_ConsumeScrollWheel 
{
    static void Postfix(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput, ItemActionZoom __instance)
    {
        FancyADS.Instance.currentFOV = ((EntityPlayerLocal)_actionData.invData.holdingEntity).cameraTransform.GetComponent<Camera>().fieldOfView;
    }
}

[HarmonyPatch(typeof(XUiC_OptionsVideo), "applyChanges")]
public class FancyADS_Video_applyChanges
{
    static void Postfix()
    {
       FancyADS.Instance.baseFOVUpdate();
    }
}

[HarmonyPatch(typeof(XUiC_OptionsControls), "applyChanges")]
public class FancyADS_Controls_applyChanges
{
    static void Postfix()
    {
        FancyADS.Instance.adsUpdate();
    }
}

[HarmonyPatch(typeof(WorldStaticData), "Init")]
public class FancyADS_Init
{
    static void Postfix() 
    {
        FancyADS.Instance.adsUpdate();
        FancyADS.Instance.baseFOVUpdate();
        Debug.Log($"ads: {FancyADS.Instance.ads} fov: {FancyADS.Instance.baseFOV}");
    }
}