using HarmonyLib;

using System;
using System.Reflection;

using UnityEngine;

using DMT;



public class FancyADS : IHarmony
{
    public static FancyADS Instance { get; private set;}

    private float zoom;
    private float baseFOV;

    public void Start()
    {
        
        Instance = this;

        Debug.Log(" Loading Patch: " + GetType().ToString());

        var harmony = new Harmony(GetType().ToString());

        harmony.PatchAll(Assembly.GetExecutingAssembly());

    }
    public void SetZoom(float fov)
    {
        this.zoom = this.baseFOV/fov;
    }
    
    public void SetBaseFOV(float fov)
    {
        this.baseFOV = fov;
    }

    public float GetZoom() 
    {
        return this.zoom;
    }
}


[HarmonyPatch(typeof(PlayerMoveController), "Update")]
public class FancyADS_Update
{  
    static void Prefix(float __state, ref float ___aimingSensitivity, ref float ___defaultSensitivity)
    {
        __state = ___aimingSensitivity;
        var zoom = FancyADS.Instance.GetZoom();
        var ads  = GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomMouseSensitivity);

        //Debug.Log($"zoom: {zoom} aim: {___aimingSensitivity} ads: {ads} def: {___defaultSensitivity}");
        ___aimingSensitivity = (ads * ___defaultSensitivity) / zoom;
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
        float baseFOV = (_actionData.invData.holdingEntity as EntityPlayerLocal).playerCamera.fieldOfView;
        //float num = itemActionDataZoom.CurrentZoom;
        FancyADS.Instance.SetZoom(((EntityPlayerLocal)_actionData.invData.holdingEntity).cameraTransform.GetComponent<Camera>().fieldOfView);
    }
}

[HarmonyPatch(typeof(EntityPlayerLocal), "updateCameraPosition")]
public class FancyADS_updateCameraPosition
{
    static void Prefix(bool _bLerpPosition)
    {
        FancyADS.Instance.SetBaseFOV((float)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV));
    }
}