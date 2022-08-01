using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.UI;
//using CustomComponents;
using Harmony;
using HBS.Data;
using HBS.Util;
using HybridDSP.Net.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static BattleTech.Data.DataManager;

namespace MechEditor {
  public static class ExternalModsHelper {
    public delegate bool d_IsChassisFake(MechDef mechdef);
    private static d_IsChassisFake i_IsChassisFake = null;
    public static bool IsChassisFake(this MechDef mechdef) {
      if (i_IsChassisFake == null) { return false; }
      return i_IsChassisFake(mechdef);
    }
    public static Transform FindRecursive(this Transform transform, string checkName) {
      foreach (Transform search in transform) {
        if (search.name == checkName)
          return search;
        Transform recursive = search.FindRecursive(checkName);
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          return recursive;
      }
      return (Transform)null;
    }
    public delegate void d_AddToFakeV(VehicleDef def);
    public delegate void d_AddToFakeVCh(VehicleChassisDef def);
    private static d_AddToFakeV i_AddToFakeV = null;
    private static d_AddToFakeVCh i_AddToFakeVCh = null;
    public static void AddToFake(this VehicleDef def) {
      if (i_AddToFakeV == null) { return; }
      i_AddToFakeV(def);
    }
    public static void AddToFake(this VehicleChassisDef def) {
      if (i_AddToFakeVCh == null) { return; }
      i_AddToFakeVCh(def);
    }
    public delegate int d_HangarShift(MechDef mechDef);
    private static d_HangarShift i_HangarShift = null;
    public static int HangarShift(this MechDef mechDef) {
      if (i_HangarShift == null) { return 0; }
      return i_HangarShift(mechDef);
    }
    private static MethodInfo i_FixMech = null;
    private static object AutoFixer_Shared = null;
    public static void FixMech(this MechDef def) {
      if (i_FixMech == null) { return; }
      i_FixMech.Invoke(AutoFixer_Shared, new object[] { new List<MechDef>() { def } });
    }
    public static void Init() {
      Log.M.TWL(0, "ExternalModsHelper.Init");
      AppDomain currentDomain = AppDomain.CurrentDomain;
      Assembly[] assems = currentDomain.GetAssemblies();
      foreach (Assembly assembly in assems) {
        if (assembly.FullName.StartsWith("CustomComponents")) {
          Type AutoFixer = assembly.GetType("CustomComponents.AutoFixer");
          Log.M.WL(1, "CustomComponents.AutoFixer "+ (AutoFixer == null?"not found":"found"));
          if (AutoFixer != null) {
            i_FixMech = AutoFixer.GetMethod("FixMechDef", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AutoFixer_Shared = AutoFixer.GetField("Shared", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetValue(null);
            Log.M.WL(1, "CustomComponents.AutoFixer.Shared " + (AutoFixer_Shared == null ? "not found" : "found"));
          }
        }
        if (assembly.FullName.StartsWith("CustomUnits,")) {
          MethodInfo mi_IsChassisFake = assembly.GetType("CustomUnits.FakeDatabase").GetMethod("IsVehicle", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(MechDef) }, null);
          Log.M.WL(1, "FakeDatabase.IsVehicle " + (mi_IsChassisFake == null ? "not found" : "found"));
          {
            var dm = new DynamicMethod("CACIsVehicle", typeof(bool), new Type[] { typeof(MechDef) });
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, mi_IsChassisFake);
            gen.Emit(OpCodes.Ret);
            i_IsChassisFake = (d_IsChassisFake)dm.CreateDelegate(typeof(d_IsChassisFake));
          }
          //MethodInfo mi_AddToFakeV = assembly.GetType("CustomUnits.BattleTechResourceLocator_RefreshTypedEntries_Patch").GetMethod("AddToFake", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(VehicleDef) }, null);
          //Log.M.WL(1, "CustomUnits.BattleTechResourceLocator_RefreshTypedEntries_Patch.AddToFake " + (mi_AddToFakeV == null ? "not found" : "found"));
          //{
          //  var dm = new DynamicMethod("CACAddToFakeV", null, new Type[] { typeof(VehicleDef) });
          //  var gen = dm.GetILGenerator();
          //  gen.Emit(OpCodes.Ldarg_0);
          //  gen.Emit(OpCodes.Call, mi_AddToFakeV);
          //  gen.Emit(OpCodes.Ret);
          //  i_AddToFakeV = (d_AddToFakeV)dm.CreateDelegate(typeof(d_AddToFakeV));
          //}
          //MethodInfo mi_AddToFakeVCh = assembly.GetType("CustomUnits.BattleTechResourceLocator_RefreshTypedEntries_Patch").GetMethod("AddToFake", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(VehicleChassisDef) }, null);
          //Log.M.WL(1, "CustomUnits.BattleTechResourceLocator_RefreshTypedEntries_Patch.AddToFake " + (mi_AddToFakeVCh == null ? "not found" : "found"));
          //{
          //  var dm = new DynamicMethod("CACAddToFakeVCh", null, new Type[] { typeof(VehicleChassisDef) });
          //  var gen = dm.GetILGenerator();
          //  gen.Emit(OpCodes.Ldarg_0);
          //  gen.Emit(OpCodes.Call, mi_AddToFakeVCh);
          //  gen.Emit(OpCodes.Ret);
          //  i_AddToFakeVCh = (d_AddToFakeVCh)dm.CreateDelegate(typeof(d_AddToFakeVCh));
          //}
          MethodInfo mi_VehicleShift = assembly.GetType("CustomUnits.CustomHangarHelper").GetMethod("GetHangarShift", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(MechDef) }, null);
          Log.M.WL(1, "CustomUnits.CustomHangarHelper.GetHangarShift " + (mi_VehicleShift == null ? "not found" : "found"));
          {
            var dm = new DynamicMethod("CACVehicleShift", typeof(int), new Type[] { typeof(MechDef) });
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, mi_VehicleShift);
            gen.Emit(OpCodes.Ret);
            i_HangarShift = (d_HangarShift)dm.CreateDelegate(typeof(d_HangarShift));
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(MechBayDragDropSlot))]
  [HarmonyPatch("OnAddItem")]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDraggableItem), typeof(bool) })]
  public static class MechBayDragDropSlot_OnAddItem {
    public static bool Prefix(MechBayDragDropSlot __instance, IMechLabDraggableItem item, bool validate, RectTransform ___mechUnitAnchor, ref MechBayMechUnitElement ___mechItem, ref bool __result) {
      try {
        if (!__instance.parentRow.isUnlocked || item == null) {
          __result = false;
          return false;
        }
        //Log.M.TWL(0, "MechBayDragDropSlot.OnAddItem", true);
        ___mechItem = item as MechBayMechUnitElement;
        //Log.M.WL(1, "___mechItem = item as MechBayMechUnitElement;");
        //Log.M.WL(1, "CachedRectTransform:"+(___mechItem.CachedRectTransform == null?"null":"not null"));
        if (___mechItem.CachedRectTransform == null) {
          Traverse.Create(___mechItem).Field<RectTransform>("cRTrans").Value = ___mechItem.GetComponent<RectTransform>();
        }
        ___mechItem.CachedRectTransform.SetParent((Transform)___mechUnitAnchor, false);
        //Log.M.WL(1, "___mechItem.CachedRectTransform.SetParent((Transform)___mechUnitAnchor, false);");
        ___mechItem.DropParent = (IMechLabDropTarget)__instance;
        //Log.M.WL(1, "___mechItem.DropParent = (IMechLabDropTarget)__instance;");
        ___mechItem.thisCanvasGroup.blocksRaycasts = true;
        //Log.M.WL(1, "___mechItem.thisCanvasGroup.blocksRaycasts = true;");
        ___mechItem.CachedRectTransform.localPosition = Vector3.zero;
        //Log.M.WL(1, "___mechItem.CachedRectTransform.localPosition = Vector3.zero;");
        ___mechItem.CachedRectTransform.localScale = Vector3.one;
        //Log.M.WL(1, "___mechItem.CachedRectTransform.localScale = Vector3.one;");
        ___mechItem.baySlot = __instance.slotIdx;
        //Log.M.WL(1, "___mechItem.baySlot = __instance.slotIdx;");
        __result = true;
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  public class Settings {
    public bool debugLog { get; set; }
    public int httpServerPort { get; set; }
    public Settings() {
      debugLog = true;
      httpServerPort = 64080;
    }
  }
  public class GoToMechBayTask: HTTPTask {
    public GoToMechBayTask(int id, JObject req, SimGameState sim, bool done = false):base(id,req,sim,done) {
      
    }
    public override void Update() {
      if (Traverse.Create(sim.RoomManager).Field<bool>("roomChanging").Value == true) { return; }
      if (Traverse.Create(sim.RoomManager).Field<DropshipLocation>("currRoomDropshipLocation").Value != DropshipLocation.MECH_BAY) { return; }
      if (sim.RoomManager.MechBayRoom.roomActive == false) { return; }
      responce["error"] = new JObject();
      responce["error"]["id"] = "SUCCESS";
      responce["error"]["string"] = "Success";
      done = true;
    }
    public override void Execute() {
      sim.ForceActiveDropshipRoom(DropshipMenuType.MechBay);
      this.executing = true;
    }
  }

  public class OpenInMechLab: HTTPTask {
    private MechBayMechInfoWidget mechInfoWidget;
    private bool OnMechLabClicked;
    private MechBayPanel mechBay;
    public OpenInMechLab(int id, JObject req, SimGameState sim, bool done = false) : base(id, req, sim, done) {
      OnMechLabClicked = false;
    }
    public override void Update() {
      if (mechInfoWidget == null) { return; }
      if (Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechRepresentationSimGame>("loadedMech").Value == null) { return; }
      if (OnMechLabClicked == false) { mechInfoWidget.OnMechLabClicked(); OnMechLabClicked = true; return; }
      if (mechBay.Initialized == false) { return; }
      done = true;
    }
    public override void Execute() {
      this.executing = true;
      responce = new JObject();
      try {
        //using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
        //jrequest = JObject.Parse(tr.ReadToEnd());
        //}
        if(sim.RoomManager.MechBayRoom.roomActive == false) {
          responce["error"] = new JObject();
          responce["error"]["id"] = "NOTINROOM";
          responce["error"]["string"] = "You are not in mechbay";
          done = true;
          return;
        }
        MechDef def = sim.DataManager.MechDefs.Get((string)request["mechDef"]);
        if (def == null) {
          responce["error"] = new JObject();
          responce["error"]["id"] = "NOTFOUND";
          responce["error"]["string"] = "Mech def not found in database";
          done = true;
          return;
        }
        this.mechBay = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechBayPanel>("mechBay").Value;
        Log.M.TWL(0, "mechBay found", true);
        MechBayRowGroupWidget bayGroupWidget = Traverse.Create(this.mechBay).Field<MechBayRowGroupWidget>("bayGroupWidget").Value;
        Log.M.TWL(0, "bayGroupWidget found", true);
        Transform editorBayTR = bayGroupWidget.transform.FindRecursive("editorBay");
        MechBayRowWidget editorBay = null;
        if (editorBayTR == null) {
          Log.M.TWL(0, "editorBay Instantiate", true);
          editorBayTR = GameObject.Instantiate(bayGroupWidget.Bays[0].gameObject).transform;
          Log.M.TWL(0, "editorBay Instantiate success", true);
          editorBayTR.gameObject.name = "editorBay";
          editorBayTR.SetParent(bayGroupWidget.Bays[0].gameObject.transform.parent.parent);
          editorBayTR.localScale = Vector3.one;
          LayoutElement layout = editorBayTR.gameObject.GetComponent<LayoutElement>();
          if (layout != null) { GameObject.Destroy(layout); };
          editorBayTR.position = new Vector3(0f, -100f, 0);
          Log.M.TWL(0, "editorBay SetData", true);
        }
        editorBay = editorBayTR.gameObject.GetComponent<MechBayRowWidget>();
        if (editorBay != null) {
          MechDef loadedMechDef = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechDef>("loadedMechDef").Value;
          if (loadedMechDef != def) { sim.RoomManager.MechBayRoom.RemoveMech(); }
          editorBay.SetData(mechBay, sim, "Edit", true, 10000, 10005);
          editorBay.SetMech(0, def, false, true, false);
          this.mechBay.OnButtonClicked(editorBay.MechList[0]);
          this.mechInfoWidget = Traverse.Create(mechBay).Field<MechBayMechInfoWidget>("mechInfoWidget").Value;
        } else {
          done = true;
        }
        responce["error"] = new JObject();
        responce["error"]["id"] = "SUCCESS";
        responce["error"]["string"] = "Success";
      } catch (Exception e) {
        responce["error"] = new JObject();
        responce["error"]["id"] = "EXCEPTION";
        responce["error"]["string"] = e.ToString();
        done = true;
      }
    }
  }
  public class HTTPTask {
    public int id { get; private set; }
    public SimGameState sim { get; private set; }
    public JObject request { get; private set; }
    public JObject responce { get; set; }
    public bool executing { get; protected set; }
    public bool done { get; protected set; }
    public virtual void Update() {
      done = true;
    }
    public void openMechInLab() {
    }
    public virtual void Execute() {
      executing = true;
    }
    public HTTPTask(int id,JObject req,SimGameState sim, bool done = false) {
      this.id = id;
      this.request = req;
      this.sim = sim;
      responce = new JObject();
      this.done = done;
      this.executing = false;
    }
  }

  [HarmonyPatch(typeof(BattleTech.UI.MainMenu))]
  [HarmonyPatch("OnVisibilityChange")]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MainMenu_OnVisibilityChange {
    private static bool MainMenuLoaded = false;
    public static bool isMainMenuLoaded(this UnityGameInstance gi) {
      return MainMenuLoaded;
    }
    public static void Postfix() {
      MainMenuLoaded = false;
    }
  }
  [HarmonyPatch(typeof(UnityGameInstance))]
  [HarmonyPatch("Update")]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class UnityHTTPQueue {
    private static int queue_index = 0;
    private static Dictionary<int, HTTPTask> tasks = new Dictionary<int, HTTPTask>();
    private static HashSet<int> tasksToCommit = new HashSet<int>();
    public static int AddTask(string jobId, JObject request, SimGameState sim) {
      HTTPTask task = null;
      if(jobId == "openmechlab") {
        task = new OpenInMechLab(queue_index, request, sim);
      } else if(jobId == "gotomechbay") { 
        task = new GoToMechBayTask(queue_index, request, sim);
      } else {
        task = new HTTPTask(queue_index, request, sim);
      }
      tasks.Add(queue_index, task);
      ++queue_index;
      return task.id;
    }
    public static bool isTaskDone(int id) {
      if(tasks.TryGetValue(id, out HTTPTask task)) {
        return task.done;
      }
      return true;
    }
    public static HTTPTask GetTask(int id) {
      if (tasks.TryGetValue(id, out HTTPTask task)) {
        return task;
      }
      return new HTTPTask(id,new JObject(), null, true);
    }
    public static void CommitTask(int id) {
      tasksToCommit.Add(id);
    }
    public static void Postfix(UnityGameInstance __instance) {
      foreach (var task in tasks) {
        if((task.Value.done == false)&&(task.Value.executing == false)) {
          task.Value.Execute();
        }
      }
      foreach (var task in tasks) {
        if ((task.Value.done == false) && (task.Value.executing == true)) {
          task.Value.Update();
        }
      }
      foreach (int id in tasksToCommit) {
        tasks.Remove(id);
      }
    }
  }
  class MechEditorAPIHandler : IHTTPRequestHandler {
    public void SendResponce(ref JObject content, ref HTTPServerResponse response) {
      response.ContentType = "application/json";
      response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
      using (Stream ostr = response.Send()) using (TextWriter tw = new StreamWriter(ostr)) {
        tw.Write(content.ToString(Formatting.Indented));
      }
    }
    public void SendResponce(string filename, ref HTTPServerResponse response) {
      string fullname = Core.BaseDirectroy + Path.DirectorySeparatorChar + filename.Replace('/', Path.DirectorySeparatorChar);
      Log.M.TWL(0, "base:"+Core.BaseDirectroy+" file:"+filename);
      Log.M.WL(1, fullname);
      if (File.Exists(fullname) == false) {
        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_NOT_FOUND;
        response.Send();
        return;
      }
      response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
      response.ContentType = GetMimeType(Path.GetExtension(fullname));
      FileInfo fi = new FileInfo(fullname);
      response.ContentLength = fi.Length;
      using (Stream output = response.Send()) {
        using (FileStream FS = new FileStream(fullname, FileMode.Open, FileAccess.Read, FileShare.Read)) {
          byte[] Buffer = new byte[1024];
          int Count = 0;
          int size = 0;
          while (FS.Position < FS.Length) {
            Count = FS.Read(Buffer, 0, Buffer.Length);
            output.Write(Buffer, 0, Count);
            size += Count;
          }
          Log.M.WL(1, "output size:"+ size);
        }
        output.Flush();
      }
    }
    public JObject listItems(string fieldName) {
      JObject content = new JObject();
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        content["error"] = new JObject();
        content["error"]["id"] = "NODATAMANAGER";
        content["error"]["string"] = "No DataManager available";
        return content;
      }
      try {
        object list = typeof(DataManager).GetField(fieldName,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(UnityGameInstance.BattleTechGame.DataManager);
        IEnumerable<string> keys = list.GetType().GetProperty("Keys", BindingFlags.Instance | BindingFlags.Public).GetMethod.Invoke(list, new object[] { }) as IEnumerable<string>;
        content[fieldName] = new JArray();
        Log.M.TWL(0, "listing " + fieldName);
        foreach (var item in keys) {
          Log.M.WL(1, item);
          (content[fieldName] as JArray).Add(item);
        }
        return content;
      } catch (Exception e) {
        content["error"] = new JObject();
        content["error"]["id"] = "EXCEPTION";
        content["error"]["string"] = e.ToString();
        return content;
      }
    }
    /*public JObject listItems(SimGameState sim, string fieldName) {

      FieldInfo[] fields = typeof(DataManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
      foreach (FieldInfo field in fields) {
        if (field.FieldType.IsGenericType == false) { continue; }
        Type[] typeArguments = field.FieldType.GetGenericArguments();
        if (typeArguments.Length == 0) { continue; }
        if (typeof(IJsonTemplated).IsAssignableFrom(typeArguments[0]) == false) { continue; }
        MethodInfo FromJSON = typeArguments[0].GetMethod("FromJSON", BindingFlags.Instance | BindingFlags.Public);
        if (FromJSON == null) { continue; };

      }


      if (propertyName == "ChassisDefs") { return listItems<ChassisDef>(sim, propertyName); }else
      if (propertyName == "VehicleChassisDefs") { return listItems<VehicleChassisDef>(sim, propertyName); } else
      if (propertyName == "TurretChassisDefs") { return listItems<TurretChassisDef>(sim, propertyName); } else
      if (propertyName == "TurretDefs") { return listItems<TurretDef>(sim, propertyName); } else
      if (propertyName == "BuildingDefs") { return listItems<BuildingDef>(sim, propertyName); } else
      if (propertyName == "AmmoDefs") { return listItems<AmmunitionDef>(sim, propertyName); } else
      if (propertyName == "AmmoBoxDefs") { return listItems<AmmunitionBoxDef>(sim, propertyName); } else
      if (propertyName == "JumpJetDefs") { return listItems<JumpJetDef>(sim, propertyName); } else
      if (propertyName == "HeatSinkDefs") { return listItems<HeatSinkDef>(sim, propertyName); } else
      if (propertyName == "UpgradeDefs") { return listItems<UpgradeDef>(sim, propertyName); } else
      if (propertyName == "WeaponDefs") { return listItems<WeaponDef>(sim, propertyName); } else
      if (propertyName == "MechDefs") { return listItems<MechDef>(sim, propertyName); } else
      if (propertyName == "VehicleDefs") { return listItems<VehicleDef>(sim, propertyName); } else
      if (propertyName == "PilotDefs") { return listItems<PilotDef>(sim, propertyName); } else
      if (propertyName == "AbilityDefs") { return listItems<AbilityDef>(sim, propertyName); } else
      if (propertyName == "DesignMaskDefs") { return listItems<DesignMaskDef>(sim, propertyName); } else
      if (propertyName == "PathingCapabilitiesDefs") { return listItems<PathingCapabilitiesDef>(sim, propertyName); } else
      if (propertyName == "MovementCapabilitiesDefs") { return listItems<MovementCapabilitiesDef>(sim, propertyName); } else
      if (propertyName == "HardpointDataDefs") { return listItems<HardpointDataDef>(sim, propertyName); } else
      if (propertyName == "LanceDefs") { return listItems<LanceDef>(sim, propertyName); } else
      if (propertyName == "SystemDefs") { return listItems<StarSystemDef>(sim, propertyName); } else
      if (propertyName == "CastDefs") { return listItems<CastDef>(sim, propertyName); } else
      if (propertyName == "GenderedOptionsListDefs") { return listItems<GenderedOptionsListDef>(sim, propertyName); } else
      if (propertyName == "AudioEventDefs") { return listItems<AudioEventDef>(sim, propertyName); } else
      if (propertyName == "DialogBucketDefs") { return listItems<DialogBucketDef>(sim, propertyName); } else
      if (propertyName == "SimGameEventDefs") { return listItems<SimGameEventDef>(sim, propertyName); } else
      if (propertyName == "SimGameStatDescDefs") { return listItems<SimGameStatDescDef>(sim, propertyName); } else
      if (propertyName == "LifepathNodeDefs") { return listItems<LifepathNodeDef>(sim, propertyName); } else
      if (propertyName == "SimGameStringLists") { return listItems<SimGameStringList>(sim, propertyName); } else
      if (propertyName == "ContractOverrides") { return listItems<ContractOverride>(sim, propertyName); } else
      if (propertyName == "Shops") { return listItems<ShopDef>(sim, propertyName); } else
      if (propertyName == "Factions") { return listItems<FactionDef>(sim, propertyName); } else
      if (propertyName == "Heraldries") { return listItems<HeraldryDef>(sim, propertyName); } else
      if (propertyName == "BaseDescriptionDefs") { return listItems<BaseDescriptionDef>(sim, propertyName); } else {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSUCHLIST";
        content["error"]["string"] = "No such property list";
        return content;
      }
    }*/
    public JObject addMechFromDatabase(SimGameState sim, ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject jrequest = null;
      JObject result = new JObject();
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        int bayIndex = (int)jrequest["bayIndex"];
        string mechdefId = (string)jrequest["mechDefId"];
        MechDef def = sim.DataManager.MechDefs.Get(mechdefId);
        //if (def.IsChassisFake()) {
          bayIndex += def.HangarShift();
        //}
        sim.AddMech(bayIndex, def, true, true, false);
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        result["error"] = new JObject();
        result["error"]["id"] = "REQUESTERROR";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject getAvaibleLists() {
      JObject result = new JObject();
      FieldInfo[] fields = typeof(DataManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
      result["lists"] = new JArray();
      foreach (FieldInfo field in fields) {
        if (field.FieldType.IsGenericType == false) { continue; }
        Type[] typeArguments = field.FieldType.GetGenericArguments();
        if (typeArguments.Length == 0) { continue; }
        if (typeof(IJsonTemplated).IsAssignableFrom(typeArguments[0]) == false) { continue; }
        (result["lists"] as JArray).Add(field.Name);
      }
      return result;
    }
    public JObject setDefToDataBase(string fieldName, ref HTTPServerRequest request) {
      if (fieldName == "chassisDefs") { return setDefToDataBase<ChassisDef>(fieldName, ref request); }else
      if (fieldName == "vehicleChassisDefs") { return setDefToDataBase<VehicleChassisDef>(fieldName, ref request); } else
      if (fieldName == "turretChassisDefs") { return setDefToDataBase<TurretChassisDef>(fieldName, ref request); } else
      if (fieldName == "turretDefs") { return setDefToDataBase<TurretDef>(fieldName, ref request); } else
      if (fieldName == "ammoDefs") { return setDefToDataBase<AmmunitionDef>(fieldName, ref request); } else
      if (fieldName == "ammoBoxDefs") { return setDefToDataBase<AmmunitionBoxDef>(fieldName, ref request); } else
      if (fieldName == "jumpJetDefs") { return setDefToDataBase<JumpJetDef>(fieldName, ref request); } else
      if (fieldName == "heatSinkDefs") { return setDefToDataBase<HeatSinkDef>(fieldName, ref request); } else
      if (fieldName == "upgradeDefs") { return setDefToDataBase<UpgradeDef>(fieldName, ref request); } else
      if (fieldName == "weaponDefs") { return setDefToDataBase<WeaponDef>(fieldName, ref request); } else
      if (fieldName == "mechDefs") { return setDefToDataBase<MechDef>(fieldName, ref request); } else
      if (fieldName == "hardpointDataDefs") { return setDefToDataBase<HardpointDataDef>(fieldName, ref request); } else 
      if (fieldName == "vehicleDefs") { return setDefToDataBase<VehicleDef>(fieldName, ref request); } else {
        JObject result = new JObject();
        result["error"] = new JObject();
        result["error"]["id"] = "NOTAVAIBLE";
        result["error"]["string"] = "not exists or read only";
        return result;
      }
    }
    public JObject reloadDefinition(string resourceType, string itemId) {
      if(Enum.TryParse<BattleTechResourceType>(resourceType, out BattleTechResourceType resType) == false){
        JObject result = new JObject();
        result["error"] = new JObject();
        result["error"]["id"] = "NOTAVAIBLE";
        result["error"]["string"] = resourceType + " is not valid resourceType";
        return result;
      }
      switch (resType) {
        case BattleTechResourceType.ChassisDef: return reloadDefinition<ChassisDef>(resType, itemId);
        case BattleTechResourceType.VehicleChassisDef: return reloadDefinition<VehicleChassisDef>(resType, itemId);
        case BattleTechResourceType.TurretChassisDef: return reloadDefinition<TurretChassisDef>(resType, itemId);
        case BattleTechResourceType.TurretDef: return reloadDefinition<TurretDef>(resType, itemId);
        case BattleTechResourceType.AmmunitionDef: return reloadDefinition<AmmunitionDef>(resType, itemId);
        case BattleTechResourceType.AmmunitionBoxDef: return reloadDefinition<AmmunitionBoxDef>(resType, itemId);
        case BattleTechResourceType.JumpJetDef: return reloadDefinition<JumpJetDef>(resType, itemId);
        case BattleTechResourceType.HeatSinkDef: return reloadDefinition<HeatSinkDef>(resType, itemId);
        case BattleTechResourceType.UpgradeDef: return reloadDefinition<UpgradeDef>(resType, itemId);
        case BattleTechResourceType.WeaponDef: return reloadDefinition<WeaponDef>(resType, itemId);
        case BattleTechResourceType.HardpointDataDef: return reloadDefinition<HardpointDataDef>(resType, itemId);
        case BattleTechResourceType.VehicleDef: return reloadDefinition<VehicleDef>(resType, itemId);
        case BattleTechResourceType.MechDef: return reloadDefinition<MechDef>(resType, itemId);
        default: {
          JObject result = new JObject();
          result["error"] = new JObject();
          result["error"]["id"] = "NOTAVAIBLE";
          result["error"]["string"] = resourceType + " is read only";
          return result;
        }
      }
    }
    public static string getListNameFromType<T>() where T : IJsonTemplated {
      if (typeof(T) == typeof(ChassisDef)) { return "chassisDefs"; }else
      if (typeof(T) == typeof(VehicleChassisDef)) { return "vehicleChassisDefs"; } else
      if (typeof(T) == typeof(TurretChassisDef)) { return "turretChassisDefs"; } else
      if (typeof(T) == typeof(TurretDef)) { return "turretDefs"; } else
      if (typeof(T) == typeof(AmmunitionDef)) { return "ammoDefs"; } else
      if (typeof(T) == typeof(AmmunitionBoxDef)) { return "ammoBoxDefs"; } else
      if (typeof(T) == typeof(JumpJetDef)) { return "jumpJetDefs"; } else
      if (typeof(T) == typeof(HeatSinkDef)) { return "heatSinkDefs"; } else
      if (typeof(T) == typeof(UpgradeDef)) { return "upgradeDefs"; } else
      if (typeof(T) == typeof(WeaponDef)) { return "weaponDefs"; } else
      if (typeof(T) == typeof(HardpointDataDef)) { return "hardpointDataDefs"; } else
      if (typeof(T) == typeof(VehicleDef)) { return "vehicleDefs"; }
      if (typeof(T) == typeof(MechDef)) { return "mechDefs"; }
      return string.Empty;
    }
    public static string getListNameFromResource(BattleTechResourceType resType){
      switch (resType) {
        case BattleTechResourceType.ChassisDef: return "chassisDefs";
        case BattleTechResourceType.VehicleChassisDef: return "vehicleChassisDefs";
        case BattleTechResourceType.TurretChassisDef: return "turretChassisDefs";
        case BattleTechResourceType.TurretDef: return "turretDefs";
        case BattleTechResourceType.AmmunitionDef: return "ammoDefs";
        case BattleTechResourceType.AmmunitionBoxDef: return "ammoBoxDefs";
        case BattleTechResourceType.JumpJetDef: return "jumpJetDefs";
        case BattleTechResourceType.HeatSinkDef: return "heatSinkDefs";
        case BattleTechResourceType.UpgradeDef: return "upgradeDefs";
        case BattleTechResourceType.WeaponDef: return "weaponDefs";
        case BattleTechResourceType.HardpointDataDef: return "hardpointDataDefs";
        case BattleTechResourceType.VehicleDef: return "vehicleDefs";
        case BattleTechResourceType.MechDef: return "mechDefs";
      }
      return string.Empty;
    }
    public JObject reloadDefinition<T>(BattleTechResourceType resourceType, string itemId) where T : IJsonTemplated, new() {
      JObject result = new JObject();
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        result["error"] = new JObject();
        result["error"]["id"] = "NODATAMANAGER";
        result["error"]["string"] = "No DataManager available";
        return result;
      }
      try {
        string fieldName = getListNameFromType<T>();
        if (string.IsNullOrEmpty(fieldName)) {
          result["error"] = new JObject();
          result["error"]["id"] = "NOTAVAIBLE";
          result["error"]["string"] = "Items for this resource type can't be reloaded";
          return result;
        }
        DictionaryStore<T> list = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<T>>(fieldName).Value;
        if (list.Exists(itemId) == false) {
          result["error"] = new JObject();
          result["error"]["id"] = "NOTAVAIBLE";
          result["error"]["string"] = "Item with this id is not loaded, so can't be reloaded";
          return result;
        }
        VersionManifestEntry entry = UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.EntryByID(itemId, resourceType);
        if (list.Exists(itemId) == false) {
          result["error"] = new JObject();
          result["error"]["id"] = "NOTAVAIBLE";
          result["error"]["string"] = "Item with this id is not in manifest";
          return result;
        }
        string content = string.Empty;
        using (FileStream arg = new FileStream(entry.FilePath, FileMode.Open, FileAccess.Read)) {
          StreamReader streamReader = new StreamReader((Stream)arg);
          content = streamReader.ReadToEnd();
        }
        T def = list.Get(itemId);
        def.FromJSON(content);
        if (typeof(T) == typeof(ChassisDef)) {
          (def as ChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as ChassisDef).Refresh();
          (def as ChassisDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(MechDef)) {
          (def as MechDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as MechDef).Refresh();
          (def as MechDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(VehicleDef)) {
          (def as VehicleDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as VehicleDef).Refresh();
          (def as VehicleDef).AddToFake();
          (def as VehicleDef).setOriginal(content);
          DictionaryStore<MechDef> mechs = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value;
          MechDef mechDef = mechs.Get(itemId);
          mechDef.FromJSON(content);
          mechDef.DataManager = UnityGameInstance.BattleTechGame.DataManager;
          mechDef.Refresh();
          mechDef.setOriginal(content);
        } else
        if (typeof(T) == typeof(VehicleChassisDef)) {
          (def as VehicleChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as VehicleChassisDef).Refresh();
          (def as VehicleChassisDef).AddToFake();
          (def as VehicleChassisDef).setOriginal(content);
          DictionaryStore<ChassisDef> chassis = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<ChassisDef>>("chassisDefs").Value;
          ChassisDef chassisDef = chassis.Get(itemId);
          chassisDef.FromJSON(content);
          chassisDef.DataManager = UnityGameInstance.BattleTechGame.DataManager;
          chassisDef.Refresh();
          chassisDef.setOriginal(content);
        } else
        if (typeof(T) == typeof(TurretDef)) {
          (def as TurretDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as TurretDef).Refresh();
          (def as TurretDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(TurretChassisDef)) {
          (def as TurretChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as TurretChassisDef).Refresh();
          (def as TurretChassisDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(HardpointDataDef)) {
          (def as HardpointDataDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(AmmunitionDef)) {
          (def as AmmunitionDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(AmmunitionBoxDef)) {
          (def as AmmunitionBoxDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as AmmunitionBoxDef).refreshAmmo(UnityGameInstance.BattleTechGame.DataManager);
          (def as AmmunitionBoxDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(WeaponDef)) {
          (def as WeaponDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as WeaponDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(HeatSinkDef)) {
          (def as HeatSinkDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as WeaponDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(JumpJetDef)) {
          (def as JumpJetDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as JumpJetDef).setOriginal(content);
        } else
        if (typeof(T) == typeof(UpgradeDef)) {
          (def as UpgradeDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as UpgradeDef).setOriginal(content);
        }
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        result["error"] = new JObject();
        result["error"]["id"] = "EXCEPTION";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }

    public JObject setDefToDataBase<T>(string fieldName, ref HTTPServerRequest request) where T : new() {
      JObject jrequest = null;
      JObject result = new JObject();
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        result["error"] = new JObject();
        result["error"]["id"] = "NODATAMANAGER";
        result["error"]["string"] = "No DataManager available";
        return result;
      }
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        DictionaryStore<T> list = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<T>>(fieldName).Value;
        string id = string.Empty;
        T def = Activator.CreateInstance<T>();
        if (typeof(T) == typeof(ChassisDef)) {
          (def as ChassisDef).FromJSON(jrequest.ToString());
          (def as ChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as ChassisDef).Refresh();
          (def as ChassisDef).setOriginal(jrequest.ToString());
          id = (def as ChassisDef).Description.Id;
        } else
        if (typeof(T) == typeof(MechDef)) {
          (def as MechDef).FromJSON(jrequest.ToString());
          (def as MechDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as MechDef).Refresh();
          (def as MechDef).setOriginal(jrequest.ToString());
          id = (def as MechDef).Description.Id;
        } else
        if (typeof(T) == typeof(VehicleDef)) {
          (def as VehicleDef).FromJSON(jrequest.ToString());
          (def as VehicleDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as VehicleDef).Refresh();
          (def as VehicleDef).AddToFake();
          (def as VehicleDef).setOriginal(jrequest.ToString());
          id = (def as VehicleDef).Description.Id;
          DictionaryStore<MechDef> mechs = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value;
          mechs.Remove(id);
          MechDef fakeDef = new MechDef();
          fakeDef.FromJSON(jrequest.ToString());
          fakeDef.Refresh();
          mechs.Add(id, fakeDef);
        } else
        if (typeof(T) == typeof(VehicleChassisDef)) {
          (def as VehicleChassisDef).FromJSON(jrequest.ToString());
          (def as VehicleChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as VehicleChassisDef).Refresh();
          (def as VehicleChassisDef).AddToFake();
          (def as VehicleChassisDef).setOriginal(jrequest.ToString());
          id = (def as VehicleChassisDef).Description.Id;
          DictionaryStore<ChassisDef> chassis = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<DictionaryStore<ChassisDef>>("chassisDefs").Value;
          chassis.Remove(id);
          ChassisDef fakeDef = new ChassisDef();
          fakeDef.FromJSON(jrequest.ToString());
          fakeDef.Refresh();
          chassis.Add(id, fakeDef);
        } else
        if (typeof(T) == typeof(TurretDef)) {
          (def as TurretDef).FromJSON(jrequest.ToString());
          (def as TurretDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as TurretDef).Refresh();
          (def as TurretDef).setOriginal(jrequest.ToString());
          id = (def as TurretDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(TurretChassisDef)) {
          (def as TurretChassisDef).FromJSON(jrequest.ToString());
          (def as TurretChassisDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as TurretChassisDef).Refresh();
          (def as TurretChassisDef).setOriginal(jrequest.ToString());
          id = (def as TurretChassisDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(HardpointDataDef)) {
          (def as HardpointDataDef).FromJSON(jrequest.ToString());
          (def as HardpointDataDef).setOriginal(jrequest.ToString());
          id = (def as HardpointDataDef).ID;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(AmmunitionDef)) {
          (def as AmmunitionDef).FromJSON(jrequest.ToString());
          (def as AmmunitionDef).setOriginal(jrequest.ToString());
          id = (def as AmmunitionDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(AmmunitionBoxDef)) {
          (def as AmmunitionBoxDef).FromJSON(jrequest.ToString());
          (def as AmmunitionBoxDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as AmmunitionBoxDef).refreshAmmo(UnityGameInstance.BattleTechGame.DataManager);
          (def as AmmunitionBoxDef).setOriginal(jrequest.ToString());
          id = (def as AmmunitionBoxDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(WeaponDef)) {
          (def as WeaponDef).FromJSON(jrequest.ToString());
          (def as WeaponDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as WeaponDef).setOriginal(jrequest.ToString());
          id = (def as WeaponDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(HeatSinkDef)) {
          (def as HeatSinkDef).FromJSON(jrequest.ToString());
          (def as HeatSinkDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as WeaponDef).setOriginal(jrequest.ToString());
          id = (def as HeatSinkDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(JumpJetDef)) {
          (def as JumpJetDef).FromJSON(jrequest.ToString());
          (def as JumpJetDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as JumpJetDef).setOriginal(jrequest.ToString());
          id = (def as JumpJetDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        } else
        if (typeof(T) == typeof(UpgradeDef)) {
          (def as UpgradeDef).FromJSON(jrequest.ToString());
          (def as UpgradeDef).DataManager = UnityGameInstance.BattleTechGame.DataManager;
          (def as UpgradeDef).setOriginal(jrequest.ToString());
          id = (def as UpgradeDef).Description.Id;
          if (list.Exists(id)) { list.Remove(id); };
        }
        list.Add(id, def);
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        result["error"] = new JObject();
        result["error"]["id"] = "EXCEPTION";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject getDefFromDatabase(string fieldName, string defId) {
      JObject content = new JObject();
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        content["error"] = new JObject();
        content["error"]["id"] = "NODATAMANAGER";
        content["error"]["string"] = "No DataManager available";
        return content;
      }
      try {
        IDataItemStore<string> list = Traverse.Create(UnityGameInstance.BattleTechGame.DataManager).Field<IDataItemStore<string>>(fieldName).Value;
        if(list == null) {
          content["error"] = new JObject();
          content["error"]["id"] = "NOSUCHLIST";
          content["error"]["string"] = "no list with such name";
          return content;
        }
        object item = list.Exists(defId)? list.GetObject(defId): null;
        if(item == null) {
          content["error"] = new JObject();
          content["error"]["id"] = "NOSUCHITEM";
          content["error"]["string"] = "no item with such id";
          return content;
        }
        if ((item as MechDef) != null) { content = JObject.Parse((item as MechDef).getOriginal()); }else
        if ((item as VehicleDef) != null) { content = JObject.Parse((item as VehicleDef).getOriginal()); }else
        if ((item as ChassisDef) != null) { content = JObject.Parse((item as ChassisDef).getOriginal()); }else
        if ((item as VehicleChassisDef) != null) { content = JObject.Parse((item as VehicleChassisDef).getOriginal()); }else
        if ((item as HardpointDataDef) != null) { content = JObject.Parse((item as HardpointDataDef).getOriginal()); } else
        if ((item as AmmunitionDef) != null) { content = JObject.Parse((item as AmmunitionDef).getOriginal()); } else
        if ((item as AmmunitionBoxDef) != null) { content = JObject.Parse((item as AmmunitionBoxDef).getOriginal()); } else
        if ((item as JumpJetDef) != null) { content = JObject.Parse((item as JumpJetDef).getOriginal()); } else
        if ((item as HeatSinkDef) != null) { content = JObject.Parse((item as HeatSinkDef).getOriginal()); } else
        if ((item as UpgradeDef) != null) { content = JObject.Parse((item as UpgradeDef).getOriginal()); } else
        if ((item as WeaponDef) != null) { content = JObject.Parse((item as WeaponDef).getOriginal()); } else
        if ((item as IJsonTemplated) != null) { content = JObject.Parse((item as IJsonTemplated).ToJSON()); } else {
          content["error"] = new JObject();
          content["error"]["id"] = "NOTJSON";
          content["error"]["string"] = "definition is not json";
          return content;
        }
        return content;
      } catch (Exception e) {
        content["error"] = new JObject();
        content["error"]["id"] = "EXCEPTION";
        content["error"]["string"] = e.ToString();
        return content;
      }
    }
    public JObject addMechFromFile(SimGameState sim, ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject jrequest = null;
      JObject result = new JObject();
      try {
        MechBayPanel mechBay = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechBayPanel>("mechBay").Value;
        if (mechBay.Initialized) {
          if (result == null) { result = new JObject(); }
          result["error"] = new JObject();
          result["error"]["id"] = "MECHBAYINITED";
          result["error"]["string"] = "Adding mech is not allowed while in mechlab";
          return result;
        }
        sim.RoomManager.ClearRooms(true);
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        MechDef def = new MechDef();
        def.FromJSON(jrequest["mechDef"].ToString());
        def.DataManager = sim.DataManager;
        def.Refresh();
        //((DefaultFixer)typeof(DefaultFixer).GetField("Shared", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).FixMech(def,sim);
        //sim.FixMech(def);
        def.FixMech();
        //DefaultFixer.Shared.FixMech(def, sim);
        //DefaultFixer.Shared.FixMechs();
        if (sim.DataManager.MechDefs.Exists(def.Description.Id)) {
          Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Remove(def.Description.Id);
        }
        Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Add(def.Description.Id,def);
        int bayIndex = (int)jrequest["bayIndex"];
        sim.AddMech(bayIndex, def, true, true, false);
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "REQUESTERROR";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject addVehiclemFromFile(SimGameState sim, ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject jrequest = null;
      JObject result = new JObject();
      try {
        MechBayPanel mechBay = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechBayPanel>("mechBay").Value;
        if (mechBay.Initialized) {
          if (result == null) { result = new JObject(); }
          result["error"] = new JObject();
          result["error"]["id"] = "MECHBAYINITED";
          result["error"]["string"] = "Adding vehicle is not allowed while in mechlab";
          return result;
        }
        sim.RoomManager.ClearRooms(true);
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        VehicleDef def = new VehicleDef();
        def.FromJSON(jrequest["vehicleDef"].ToString());
        def.DataManager = sim.DataManager;
        def.Refresh();
        //((DefaultFixer)typeof(DefaultFixer).GetField("Shared", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).FixMech(def,sim);
        //DefaultFixer.Shared.FixMech(def, sim);
        //DefaultFixer.Shared.FixMechs();
        if (sim.DataManager.MechDefs.Exists(def.Description.Id)) {
          Traverse.Create(sim.DataManager).Field<DictionaryStore<VehicleDef>>("VehicleDefs").Value.Remove(def.Description.Id);
        }
        Traverse.Create(sim.DataManager).Field<DictionaryStore<VehicleDef>>("VehicleDefs").Value.Add(def.Description.Id, def);
        MechDef fakeMechDef = new MechDef();
        fakeMechDef.FromJSON(jrequest["vehicleDef"].ToString());
        if (sim.DataManager.MechDefs.Exists(def.Description.Id)) {
          Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Remove(def.Description.Id);
        }
        Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Add(def.Description.Id, fakeMechDef);
        int bayIndex = (int)jrequest["bayIndex"]+ fakeMechDef.HangarShift();
        sim.AddMech(bayIndex, fakeMechDef, true, true, false);
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "REQUESTERROR";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject addAllEqupment(SimGameState sim, ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject jrequest = null;
      JObject result = null;
      MechBayPanel mechBay = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechBayPanel>("mechBay").Value;
      //if (mechBay.Initialized) {
      //  if (result == null) { result = new JObject(); }
      //  result["error"] = new JObject();
      //  result["error"]["id"] = "MECHBAYINITED";
      //  result["error"]["string"] = "Adding equipment is not allowed while in mechlab";
      //  return result;
      //}
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        int count = 0;
        if (jrequest["count"] != null) { count = (int)jrequest["count"]; }; if (count <= 0) { count = 10; }
        foreach(var item in sim.DataManager.AmmoBoxDefs) {
          string stat_name = sim.GetItemStatID(item.Value.Description, item.Value.GetType());
          if (sim.CompanyStats.ContainsStatistic(stat_name) == false) {
            sim.CompanyStats.AddStatistic<int>(stat_name, 0);
          }
          sim.CompanyStats.Set<int>(stat_name, count);
        }
        foreach (var item in sim.DataManager.UpgradeDefs) {
          string stat_name = sim.GetItemStatID(item.Value.Description, item.Value.GetType());
          if (sim.CompanyStats.ContainsStatistic(stat_name) == false) {
            sim.CompanyStats.AddStatistic<int>(stat_name, 0);
          }
          sim.CompanyStats.Set<int>(stat_name, count);
        }
        foreach (var item in sim.DataManager.HeatSinkDefs) {
          string stat_name = sim.GetItemStatID(item.Value.Description, item.Value.GetType());
          if (sim.CompanyStats.ContainsStatistic(stat_name) == false) {
            sim.CompanyStats.AddStatistic<int>(stat_name, 0);
          }
          sim.CompanyStats.Set<int>(stat_name, count);
        }
        foreach (var item in sim.DataManager.JumpJetDefs) {
          string stat_name = sim.GetItemStatID(item.Value.Description, item.Value.GetType());
          if (sim.CompanyStats.ContainsStatistic(stat_name) == false) {
            sim.CompanyStats.AddStatistic<int>(stat_name, 0);
          }
          sim.CompanyStats.Set<int>(stat_name, count);
        }
        foreach (var item in sim.DataManager.WeaponDefs) {
          string stat_name = sim.GetItemStatID(item.Value.Description, item.Value.GetType());
          if (sim.CompanyStats.ContainsStatistic(stat_name) == false) {
            sim.CompanyStats.AddStatistic<int>(stat_name, 0);
          }
          sim.CompanyStats.Set<int>(stat_name, count);
        }
        result = new JObject();
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
        return result;
      } catch (Exception e) {
        result = new JObject();
        result["error"] = new JObject();
        result["error"]["id"] = "REQUESTERROR";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject getMechBayMech(SimGameState sim, ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject jrequest = null;
      JObject result = null;
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        MechBayPanel mechBay = Traverse.Create(sim.RoomManager.MechBayRoom).Field<MechBayPanel>("mechBay").Value;
        string Id = string.Empty; if (jrequest["Id"]!=null) { Id = (string)jrequest["Id"]; }; if (string.IsNullOrEmpty(Id)) { Id = mechBay.mechLab.originalMechDef.Description.Id; }
        string Name = string.Empty; if (jrequest["Name"] != null) { Name = (string)jrequest["Name"]; }; if (string.IsNullOrEmpty(Name)) { Name = mechBay.mechLab.activeMechDef.Description.Name; }
        string UIName = string.Empty; if (jrequest["UIName"] != null) { UIName = (string)jrequest["UIName"]; }; if (string.IsNullOrEmpty(UIName)) { UIName = mechBay.mechLab.activeMechDef.Description.UIName; }
        string Details = string.Empty; if (jrequest["Details"] != null) { Details = (string)jrequest["Details"]; }; if (string.IsNullOrEmpty(Details)) { Details = mechBay.mechLab.activeMechDef.Description.Details; }
        string Icon = string.Empty; if (jrequest["Icon"] != null) { Icon = (string)jrequest["Icon"]; }; if (string.IsNullOrEmpty(Icon)) { Icon = mechBay.mechLab.activeMechDef.Description.Icon; }
        float Rarity = float.NaN; if (jrequest["Rarity"] != null) { Rarity = (float)jrequest["Rarity"]; }; if (float.IsNaN(Rarity)) { Rarity = mechBay.mechLab.originalMechDef.Description.Rarity; }

        if (mechBay.mechLab.Initialized == false) {
          if (result == null) { result = new JObject(); }
          result["error"] = new JObject();
          result["error"]["id"] = "MECHLABNOTINITED";
          result["error"]["string"] = "Mech lab not initialized";
          return result;
        }
        result = JObject.Parse(mechBay.mechLab.originalMechDef.getOriginal());
        MechLabMechInfoWidget mechInfoWidget = Traverse.Create(mechBay.mechLab).Field<MechLabMechInfoWidget>("mechInfoWidget").Value;
        DescriptionDef description = new DescriptionDef(Id, Name, Details, Icon, (int)mechInfoWidget.currentCBillValue, Rarity, true, mechBay.mechLab.originalMechDef.Description.Manufacturer, mechBay.mechLab.originalMechDef.Description.Model, UIName);
        LocationLoadoutDef[] Localtions = new LocationLoadoutDef[8] {
          mechBay.mechLab.headWidget.loadout,
          mechBay.mechLab.centerTorsoWidget.loadout,
          mechBay.mechLab.leftTorsoWidget.loadout,
          mechBay.mechLab.rightTorsoWidget.loadout,
          mechBay.mechLab.leftArmWidget.loadout,
          mechBay.mechLab.rightArmWidget.loadout,
          mechBay.mechLab.leftLegWidget.loadout,
          mechBay.mechLab.rightLegWidget.loadout
        };
        List<MechComponentRef> temp_inventory = new List<MechComponentRef>();
        for (int index = 0; index < mechBay.mechLab.activeMechInventory.Count; ++index) {
          MechComponentRef component = new MechComponentRef(mechBay.mechLab.activeMechInventory[index]);
          component.SimGameUID = string.Empty;
          component.SetGuid(string.Empty);
          component.hasPrefabName = false;
          component.prefabName = string.Empty;
          temp_inventory.Add(component);
        }
        MechComponentRef[] inventory = temp_inventory.ToArray();
        string inventory_string = JSONSerializationUtility.ToJSON<MechComponentRef[]>(inventory);
        string Locations_string = JSONSerializationUtility.ToJSON<LocationLoadoutDef[]>(Localtions);
        result["Description"] = JObject.Parse(description.ToJSON());
        result["inventory"] = JArray.Parse(inventory_string);
        result["Locations"] = JArray.Parse(Locations_string);
        return result;
      }catch(Exception e) {
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "REQUESTERROR";
        result["error"]["string"] = e.ToString();
        return result;
      }
    }
    public JObject listHangar(SimGameState sim) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      int max = sim.GetMaxActiveMechs();
      JObject result = new JObject();
      result["baysCount"] = max;
      result["mechBays"] = new JArray();
      //result["vehicleBays"] = new JArray();
      for (int bayIndex = 0; bayIndex < max; ++bayIndex) {
        JObject mechSlot = new JObject();
        JObject vehicleSlot = new JObject();
        mechSlot["index"] = bayIndex;
        vehicleSlot["index"] = bayIndex;
        MechDef mechDef = null;
        if(sim.ActiveMechs.TryGetValue(bayIndex, out mechDef)) {
          mechSlot["empty"] = false;
          mechSlot["active"] = true;
          mechSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        }else if(sim.ReadyingMechs.TryGetValue(bayIndex, out mechDef)) {
          mechSlot["empty"] = false;
          mechSlot["active"] = false;
          mechSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        } else {
          mechSlot["empty"] = false;
          mechSlot["active"] = false;
          mechSlot["definition"] = new JObject();
        }
        //int vehicleIndex = bayIndex + sim.VehicleShift();
        //if (sim.ActiveMechs.TryGetValue(vehicleIndex, out mechDef)) {
        //  vehicleSlot["empty"] = false;
        //  vehicleSlot["active"] = true;
        //  vehicleSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        //} else if (sim.ReadyingMechs.TryGetValue(vehicleIndex, out mechDef)) {
        //  vehicleSlot["empty"] = false;
        //  vehicleSlot["active"] = false;
        //  vehicleSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        //} else {
        //  vehicleSlot["empty"] = false;
        //  vehicleSlot["active"] = false;
        //  vehicleSlot["definition"] = new JObject();
        //}
        (result["mechBays"] as JArray).Add(mechSlot);
        //(result["vehicleBays"] as JArray).Add(vehicleSlot);
      }
      return result;
    }
    private static string GetMimeType(string ext) {
      switch (ext) {
        case ".html": return "text/html";
        case ".htm": return "text/html";
        case ".txt": return "text/plain";
        case ".jpe": return "image/jpeg";
        case ".jpeg": return "image/jpeg";
        case ".jpg": return "image/jpeg";
        case ".js": return "application/javascript";
        case ".png": return "image/png";
        case ".gif": return "image/gif";
        case ".bmp": return "image/bmp";
        case ".ico": return "image/x-icon";
      }
      return "application/octed-stream";
    }
    public JObject writeUISettings(ref HTTPServerRequest request) {
      JObject result = new JObject();
      JObject jrequest = null;
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        string settingsPath = Path.Combine(Core.BaseDirectroy, "ui", "settings.json");
        File.WriteAllText(settingsPath, jrequest.ToString(Formatting.Indented));
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "SUCCESS";
        result["error"]["string"] = "Success";
      } catch (Exception e) {
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "EXCEPTION";
        result["error"]["string"] = e.ToString();
      }
      return result;
    }

    public bool gotoMechBay(SimGameState sim, ref JObject error) {
      int id = UnityHTTPQueue.AddTask("gotomechbay", new JObject(), sim);
      int watchdog = 0;
      error = new JObject();
      while (UnityHTTPQueue.isTaskDone(id) == false) {
        System.Threading.Thread.Sleep(100);
        ++watchdog;
        if (watchdog > 3000) {
          error["error"] = new JObject();
          error["error"]["id"] = "TIMEOUT";
          error["error"]["string"] = "Operation timeout";
          return false;
        }
      }
      HTTPTask task = UnityHTTPQueue.GetTask(id);
      UnityHTTPQueue.CommitTask(id);
      JObject result = task.responce;
      if (result["error"] != null) {
        if ((string)result["error"]["id"] == "SUCCESS") { return true; }
      }
      error = result;
      return false;
    }
    public JObject openMechInLab(SimGameState sim,ref HTTPServerRequest request) {
      if (sim == null) {
        JObject content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      JObject result = new JObject();
      JObject jrequest = new JObject();
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        if(gotoMechBay(sim,ref result) == false) {
          return result;
        }
        int id = UnityHTTPQueue.AddTask("openmechlab", jrequest, sim);
        int watchdog = 0;
        while(UnityHTTPQueue.isTaskDone(id) == false) {
          System.Threading.Thread.Sleep(100);
          ++watchdog;
          if (watchdog > 3000) {
            result["error"] = new JObject();
            result["error"]["id"] = "TIMEOUT";
            result["error"]["string"] = "Operation timeout";
          }
        }
        HTTPTask task = UnityHTTPQueue.GetTask(id);
        UnityHTTPQueue.CommitTask(id);
        result = task.responce;
      } catch (Exception e) {
        if (result == null) { result = new JObject(); }
        result["error"] = new JObject();
        result["error"]["id"] = "EXCEPTION";
        result["error"]["string"] = e.ToString();
      }
      return result;
    }
    public JObject getBattleTechResourceTypes() {
      JObject result = new JObject();
      result["BattleTechResourceType"] = new JArray();
      foreach(object val in Enum.GetValues(typeof(BattleTechResourceType))) {
        (result["BattleTechResourceType"] as JArray).Add(val.ToString());
      }
      return result;
    }
    public JObject getManifest(string resource) {
      JObject result = new JObject();
      if(Enum.TryParse<BattleTechResourceType>(resource, out BattleTechResourceType resourceType) == false) {
        result["error"] = new JObject();
        result["error"]["id"] = "WRONGRESOURCE";
        result["error"]["string"] = "Wrong resource type";
        return result;
      }
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        result["error"] = new JObject();
        result["error"]["id"] = "NODATAMANAGER";
        result["error"]["string"] = "No DataManager available";
        return result;
      }
      result[resource] = new JArray();
      VersionManifestEntry[] manifest = UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.AllEntriesOfResource(resourceType, true);
      foreach(VersionManifestEntry entry in manifest) {
        JObject element = new JObject();
        element["Id"] = entry.Id;
        element["FileName"] = entry.FileName;
        element["FilePath"] = entry.FilePath;
        element["IsAssetBundled"] = entry.IsAssetBundled;
        element["AssetBundleName"] = entry.AssetBundleName;
        element["Loaded"] = UnityGameInstance.BattleTechGame.DataManager.Exists(resourceType, entry.Id);
        (result[resource] as JArray).Add(element);
      }
      return result;
    }
    internal class AllLoadingRequest {
      public MechEditorAPIHandler owner { get; private set; }
      public bool done { get; private set; }
      public void OnDataLoaded(LoadRequest loadRequest) { done = true; }
      public AllLoadingRequest(MechEditorAPIHandler owner) { this.owner = owner; done = false; }
    }
    private static HashSet<AllLoadingRequest> allLoadRequests = new HashSet<AllLoadingRequest>();
    public JObject requestAll(string resource) {
      JObject result = new JObject();
      if (UnityGameInstance.HasInstance == false) {
        result["error"] = new JObject();
        result["error"]["id"] = "GAMENOTSTARTED";
        result["error"]["string"] = "Game not started yet";
        return result;
      }
      if (UnityGameInstance.Instance.isMainMenuLoaded()) {
        result["error"] = new JObject();
        result["error"]["id"] = "NOMAINMENU";
        result["error"]["string"] = "Main menu not loaded";
        return result;
      }
      if (Enum.TryParse<BattleTechResourceType>(resource, out BattleTechResourceType resourceType) == false) {
        result["error"] = new JObject();
        result["error"]["id"] = "WRONGRESOURCE";
        result["error"]["string"] = "Wrong resource type";
        return result;
      }
      if (UnityGameInstance.BattleTechGame.DataManager == null) {
        result["error"] = new JObject();
        result["error"]["id"] = "NODATAMANAGER";
        result["error"]["string"] = "No DataManager available";
        return result;
      }
      AllLoadingRequest requestWaiter = new AllLoadingRequest(this);
      LoadRequest loadRequest = UnityGameInstance.BattleTechGame.DataManager.CreateLoadRequest(new Action<LoadRequest>(requestWaiter.OnDataLoaded), false);
      loadRequest.AddAllOfTypeBlindLoadRequest(resourceType, true);
      loadRequest.ProcessRequests(10U);
      int watchdog = 0;
      while(requestWaiter.done == false) {
        System.Threading.Thread.Sleep(100);
        ++watchdog;
        if (watchdog > 60*10) {
          result["error"] = new JObject();
          result["error"]["id"] = "TIMEOUT";
          result["error"]["string"] = "Request timeout";
          return result;
        }
      }
      result["error"] = new JObject();
      result["error"]["id"] = "SUCCESS";
      result["error"]["string"] = "Success";
      return result;
    }
    public JObject GetAnswer(ref HTTPServerRequest request) {
      SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
      JObject content = null;
      if(UnityGameInstance.BattleTechGame.Combat != null) {
        content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "INCOMBAT";
        content["error"]["string"] = "You are in combat";
        return content;
      }
      if (request.URI.StartsWith("/BattleTechResourceType")) {
        content = getBattleTechResourceTypes();
      } else
      if (request.URI.StartsWith("/Manifest/")) {
        string listname = Path.GetFileName(request.URI);
        content = getManifest(listname);
      } else
      if (request.URI.StartsWith("/LoadManifest/")) {
        string listname = Path.GetFileName(request.URI);
        content = requestAll(listname);
      } else
      if (request.URI.StartsWith("/list/")) {
        string listname = Path.GetFileName(request.URI);
        content = listItems(listname);
      } else
      if (request.URI.StartsWith("/listhangar")) {
        content = listHangar(sim);
      } else
      if (request.URI.StartsWith("/addmechfromfile")) {
        content = addMechFromFile(sim, ref request);
      } else
      if (request.URI.StartsWith("/addvehiclefromfile")) {
        content = addVehiclemFromFile(sim, ref request);
      } else
      if (request.URI.StartsWith("/addmechfromdb")) {
        content = addMechFromDatabase(sim, ref request);
      } else
      if (request.URI.StartsWith("/getmechlabmech")) {
        content = getMechBayMech(sim, ref request);
      } else
      if (request.URI.StartsWith("/addallequipment")) {
        content = addAllEqupment(sim, ref request);
      } else
      if (request.URI.StartsWith("/getavaiblelists")) {
        content = getAvaibleLists();
      } else
      if (request.URI.StartsWith("/uisettings")) {
        content = writeUISettings(ref request);
      } else
      if (request.URI.StartsWith("/openmechlab")) {
        content = openMechInLab(sim, ref request);
      } else
      if (request.URI.StartsWith("/get/")) {
        string itemname = Path.GetFileName(request.URI);
        string listname = Path.GetFileName(Path.GetDirectoryName(request.URI));
        content = getDefFromDatabase(listname, itemname);
      } else
      if (request.URI.StartsWith("/set/")) {
        string listname = Path.GetFileName(request.URI);
        content = setDefToDataBase(listname, ref request);
      }else
      if (request.URI.StartsWith("/reload/")) {
        string itemId = Path.GetFileName(request.URI);
        string resourceType = Path.GetFileName(Path.GetDirectoryName(request.URI));
        content = reloadDefinition(resourceType, itemId);
      } else {
        content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "UNKNOWN";
        content["error"]["string"] = "Unknown command";
      }
      if (content != null) { return content; } else { return new JObject(); }
    }
    public void HandleRequest(HTTPServerRequest request, HTTPServerResponse response) {
      try {
        response.Add("Access-Control-Allow-Origin","*");
        Log.M.TWL(0, "Incoming URI:" + request.URI);
        string URI = request.URI;
        if (URI == "/") { URI = "/ui/index.html"; }
        if (URI == "/ui/") { URI = "/ui/index.html"; }
        if (URI.StartsWith("/ui/")) {
          SendResponce(URI, ref response);
          return;
        }
        JObject content = GetAnswer(ref request);
        SendResponce(ref content, ref response);
        return;
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  class RequestHandlerFactory : IHTTPRequestHandlerFactory {
    public IHTTPRequestHandler CreateRequestHandler(HTTPServerRequest request) {
      return new MechEditorAPIHandler();
    }
  }

  public static class Core {
    public static Settings settings { get; set; }
    public static string BaseDirectroy { get; set; }
    public static HTTPServer server { get; set; }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.M.TWL(0, "FinishedLoading", true);
      try {
        ExternalModsHelper.Init();
        RequestHandlerFactory factory = new RequestHandlerFactory();
        server = new HTTPServer(factory, Core.settings.httpServerPort);
        server.Start();
        Log.M.TWL(0, "Http server created on port:"+Core.settings.httpServerPort, true);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.BaseDirectroy = directory;
      Core.settings = JsonConvert.DeserializeObject<MechEditor.Settings>(settingsJson);
      Log.InitLog();
      Log.M.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      try {
        var harmony = HarmonyInstance.Create("ru.mission.mecheditor");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        //Type JsonLoadRequest = typeof(JsonLoadRequest<>);
        //Type originalsStore = typeof(OriginalsStore<>);
        //FieldInfo[] fields = typeof(DataManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        /*foreach (FieldInfo field in fields) {
          if (field.FieldType.IsGenericType == false) { continue; }
          Type[] typeArguments = field.FieldType.GetGenericArguments();
          if (typeArguments.Length == 0) { continue; }
          if (typeArguments[0] == typeof(ContentPackDef)) { continue; }
          if (typeof(IJsonTemplated).IsAssignableFrom(typeArguments[0]) == false) { continue; }
          Type JsonLoadRequestTyped = JsonLoadRequest.M(new Type[] { typeArguments[0] });
          MethodInfo OnLoadedWithJSON = JsonLoadRequestTyped.GetMethod("OnLoadedWithJSON", BindingFlags.Instance | BindingFlags.NonPublic);
          if (OnLoadedWithJSON == null) { continue; };
          Type originalsStoreTyped = originalsStore.MakeGenericType(new Type[] { typeArguments[0] });
          MethodInfo prefix = originalsStoreTyped.GetMethod("Prefix",BindingFlags.Instance | BindingFlags.Public);
          MethodInfo postfix = originalsStoreTyped.GetMethod("Postfix", BindingFlags.Instance | BindingFlags.Public);
          harmony.Patch(OnLoadedWithJSON,new HarmonyMethod(prefix), new HarmonyMethod(postfix));
          Log.M.WL(0, JsonLoadRequestTyped.GetType().ToString() +"/"+ typeArguments[0].ToString()+ " - patched");
        }*/
      } catch (Exception e) {
        Log.LogWrite(e.ToString(), true);
      }
    }
  }
}
