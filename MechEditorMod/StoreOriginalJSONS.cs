using BattleTech;
using Harmony;
using HBS.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using static BattleTech.Data.DataManager;

namespace MechEditor {
  /*  [HarmonyPatch(typeof(TurretDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class TurretDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this TurretDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this TurretDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(TurretDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(TurretDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(TurretChassisDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class TurretChassisDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this TurretChassisDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this TurretChassisDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(TurretChassisDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(TurretChassisDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(MechDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class MechDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this MechDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this MechDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(MechDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(MechDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id,__state); }
      }
    }
    [HarmonyPatch(typeof(VehicleDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class VehicleDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this VehicleDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this VehicleDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(VehicleDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(VehicleDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(AmmunitionBoxDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class AmmunitionBoxDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this AmmunitionBoxDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this AmmunitionBoxDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(AmmunitionBoxDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(AmmunitionBoxDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(HeatSinkDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class HeatSinkDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this HeatSinkDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this HeatSinkDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(HeatSinkDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(HeatSinkDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(UpgradeDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class UpgradeDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this UpgradeDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this UpgradeDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(UpgradeDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(UpgradeDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }
    [HarmonyPatch(typeof(JumpJetDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class JumpJetDef_fromJSON_StoreOriginal {
      private static Dictionary<string, string> originals = new Dictionary<string, string>();
      public static string getOriginal(this JumpJetDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
      public static void setOriginal(this JumpJetDef def, string json) { if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json); } }
      public static void Prefix(JumpJetDef __instance, string json, ref string __state) {
        __state = json;
      }
      public static void Postfix(JumpJetDef __instance, string json, ref string __state) {
        if (originals.ContainsKey(__instance.Description.Id)) { originals[__instance.Description.Id] = __state; } else { originals.Add(__instance.Description.Id, __state); }
      }
    }*/
  [HarmonyPatch()]
  public static class WeaponDef_OnLoadedWithJSON {
    private static Dictionary<Type, Dictionary<string, string>> originals = new Dictionary<Type, Dictionary<string, string>>();
    public static string getOriginal(this MechComponentDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if(origs.TryGetValue(def.Description.Id,out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this MechDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this VehicleDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this TurretDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this ChassisDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this VehicleChassisDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this TurretChassisDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this AmmunitionDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.Description.Id, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static string getOriginal(this HardpointDataDef def) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        return def.ToJSON();
      }
      if (origs.TryGetValue(def.ID, out string result) == false) {
        return def.ToJSON();
      }
      return result;
    }
    public static void setOriginal(this AmmunitionDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this MechComponentDef def, string json) {
      if(originals.TryGetValue(def.GetType(),out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id,json); }
    }
    public static void setOriginal(this HardpointDataDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.ID)) { origs[def.ID] = json; } else { origs.Add(def.ID, json); }
    }
    public static void setOriginal(this MechDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this VehicleDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this TurretDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this ChassisDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this VehicleChassisDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static void setOriginal(this TurretChassisDef def, string json) {
      if (originals.TryGetValue(def.GetType(), out Dictionary<string, string> origs) == false) {
        origs = new Dictionary<string, string>();
        originals.Add(def.GetType(), origs);
      }
      if (origs.ContainsKey(def.Description.Id)) { origs[def.Description.Id] = json; } else { origs.Add(def.Description.Id, json); }
    }
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(StringDataLoadRequest<WeaponDef>), "OnLoadedWithText");
    }
    public static void Prefix(object __instance, string text, ref string __state) {
      Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText prefix");
      __state = text;
    }
    public static void Postfix(object __instance, string text, ref string __state) {
      object resource = Traverse.Create(__instance).Field("resource").GetValue();
      if ((resource as MechComponentDef) != null) {
        (resource as MechComponentDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as MechComponentDef).Description.Id + " postfix");
      }else
      if ((resource as HardpointDataDef) != null) {
        (resource as HardpointDataDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as HardpointDataDef).ID + " postfix");
      } else
      if ((resource as MechDef) != null) {
        (resource as MechDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as MechDef).Description.Id + " postfix");
      } else
      if ((resource as VehicleDef) != null) {
        (resource as VehicleDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as VehicleDef).Description.Id + " postfix");
      } else
      if ((resource as TurretDef) != null) {
        (resource as TurretDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as TurretDef).Description.Id + " postfix");
      } else
      if ((resource as ChassisDef) != null) {
        (resource as ChassisDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as ChassisDef).Description.Id + " postfix");
      } else
      if ((resource as VehicleChassisDef) != null) {
        (resource as VehicleChassisDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as VehicleChassisDef).Description.Id + " postfix");
      } else
      if ((resource as TurretChassisDef) != null) {
        (resource as TurretChassisDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as TurretChassisDef).Description.Id + " postfix");
      }else
      if ((resource as AmmunitionDef) != null) {
        (resource as AmmunitionDef).setOriginal(text);
        Log.M.TWL(0, "StringDataLoadRequest.OnLoadedWithText " + (resource as AmmunitionDef).Description.Id + " postfix");
      }
    }
  }
  public static class OriginalsStore<T> where T : class, IJsonTemplated, new() {
    public static void Prefix(object __instance, string json, ref string __state) {
      try {
        Log.M.TWL(0, __instance.GetType().ToString() + ".OnLoadedWithJSON prefix " + typeof(T).ToString());
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(object __instance, string json, ref string __state) {
      try {
        Log.M.TWL(0, __instance.GetType().ToString() + ".OnLoadedWithJSON postfix " + typeof(T).ToString());
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class OriginalsStoreHelper {

  }
}