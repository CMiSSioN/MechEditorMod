using BattleTech;
using BattleTech.Framework;
using CustomUnits;
using Harmony;
using HBS.Data;
using HBS.Util;
using HybridDSP.Net.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MechEditor {
  public class Settings {
    public bool debugLog { get; set; }
    public int httpServerPort { get; set; }
    public Settings() {
      debugLog = true;
      httpServerPort = 64080;
    }
  }
  class MechEditorAPIHandler : IHTTPRequestHandler {
    public void SendResponce(ref JObject content, ref HTTPServerResponse response) {
      using (Stream ostr = response.Send()) using (TextWriter tw = new StreamWriter(ostr)) {
        tw.Write(content.ToString(Formatting.Indented));
      }
    }
    public JObject listItems<T>(SimGameState sim, string propertyName) where T : new() {
      JObject content = new JObject();
      if (sim.DataManager == null) {
        content["error"] = new JObject();
        content["error"]["id"] = "NODATAMANAGER";
        content["error"]["string"] = "No DataManager avaible";
        return content;
      }
      try {
        IDataItemStore<string, T> list = Traverse.Create(sim.DataManager).Property<IDataItemStore<string, T>>(propertyName).Value;
        content[propertyName] = new JArray();
        Log.M.TWL(0, "listing " + propertyName + "(" + list.Count + ")");
        foreach (var item in list) {
          Log.M.WL(1, item.Key);
          //IJsonTemplated template = item.Value as IJsonTemplated;
          //if (template == null) { continue; }
          //try {
            (content[propertyName] as JArray).Add(item.Key);
          //}catch(Exception) {
            //continue;
          //}
        }
        return content;
      } catch (Exception e) {
        content["error"] = new JObject();
        content["error"]["id"] = "EXCEPTION";
        content["error"]["string"] = e.ToString();
        return content;
      }
    }
    public JObject listItems(SimGameState sim, string propertyName) {
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
    }
    public JObject addMechFromFile(SimGameState sim, ref HTTPServerRequest request) {
      JObject jrequest = null;
      JObject result = new JObject();
      try {
        using (Stream ostr = request.GetRequestStream()) using (TextReader tr = new StreamReader(ostr)) {
          jrequest = JObject.Parse(tr.ReadToEnd());
        }
        MechDef def = new MechDef();
        def.FromJSON(jrequest["mechDef"].ToString());
        def.DataManager = sim.DataManager;
        def.Refresh();
        if(sim.DataManager.MechDefs.Exists(def.Description.Id)) {
          Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Remove(def.Description.Id);
        }
        Traverse.Create(sim.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Add(def.Description.Id,def);
        int bayIndex = (int)jrequest["bayIndex"];
        if (def.IsChassisFake()) {
          bayIndex += sim.VehicleShift();
        }
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
    public JObject listHangar(SimGameState sim) {
      int max = sim.GetMaxActiveMechs();
      JObject result = new JObject();
      result["baysCount"] = max;
      result["mechBays"] = new JArray();
      result["vehicleBays"] = new JArray();
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
        int vehicleIndex = bayIndex + sim.VehicleShift();
        if (sim.ActiveMechs.TryGetValue(vehicleIndex, out mechDef)) {
          vehicleSlot["empty"] = false;
          vehicleSlot["active"] = true;
          vehicleSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        } else if (sim.ReadyingMechs.TryGetValue(vehicleIndex, out mechDef)) {
          vehicleSlot["empty"] = false;
          vehicleSlot["active"] = false;
          vehicleSlot["definition"] = JObject.Parse(mechDef.ToJSON());
        } else {
          vehicleSlot["empty"] = false;
          vehicleSlot["active"] = false;
          vehicleSlot["definition"] = new JObject();
        }
        (result["mechBays"] as JArray).Add(mechSlot);
        (result["vehicleBays"] as JArray).Add(vehicleSlot);
      }
      return result;
    }
    public JObject GetAnswer(ref HTTPServerRequest request) {
      SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
      JObject content = null;
      if (sim == null) {
        content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "NOSIMGAME";
        content["error"]["string"] = "SimGame not started";
        return content;
      }
      if(UnityGameInstance.BattleTechGame.Combat != null) {
        content = new JObject();
        content["error"] = new JObject();
        content["error"]["id"] = "INCOMBAT";
        content["error"]["string"] = "You are in combat";
        return content;
      }
      if (request.URI.StartsWith("/list/")) {
        string listname = Path.GetFileName(request.URI);
        content = listItems(sim, listname);
      } else
      if (request.URI.StartsWith("/listhangar")) {
        content = listHangar(sim);
      } else
      if (request.URI.StartsWith("/addmechfromfile")) {
        content = addMechFromFile(sim, ref request);
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
        response.ContentType = "application/json";
        response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_OK;
        Log.M.TWL(0, "Incoming URI:" + request.URI);
        JObject content = GetAnswer(ref request);
        SendResponce(ref content, ref response);
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
    public static HTTPServer server { get; set; }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.M.TWL(0, "FinishedLoading", true);
      try {
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
      Core.settings = JsonConvert.DeserializeObject<MechEditor.Settings>(settingsJson);
      Log.InitLog();
      Log.M.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      try {
        var harmony = HarmonyInstance.Create("ru.mission.customvoices");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.LogWrite(e.ToString(), true);
      }
    }
  }
}
