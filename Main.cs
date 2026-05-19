using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GHPC.Camera;
using GHPC.Mission;
using GHPC.Player;
using GHPC.State;
using GHPC.Vehicle;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(SuperM60.SuperM60Mod), "Super M60", "0.1.0", "Cyance")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace SuperM60
{
    public class SuperM60Mod : MelonMod
    {
        public static Vehicle[] vics;
        public static GameObject gameManager;
        public static CameraManager camManager;
        public static PlayerInput playerManager;

        private ModuleManager moduleManager = new ModuleManager("SuperM60");

        public IEnumerator GetVics(GameState _)
        {
            gameManager = GameObject.Find("_APP_GHPC_");
            camManager = gameManager.GetComponent<CameraManager>();
            playerManager = gameManager.GetComponent<PlayerInput>();
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            yield break;
        }

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("SuperM60Config");
            M60A3.Config(cfg);
            M60A1.Config(cfg);
            AMMO_105mm.Config(cfg);
        }

        public override void OnUpdate()
        {
            M60A3.Update();
        }

        public override void OnSceneWasLoaded(int idx, string scene_name)
        {
            foreach (string id in moduleManager.modules.Keys)
            {
                Module module = moduleManager.modules[id];
                bool dynamic_unloaded = module.TryUnloadDynamicAssets();
            }

            if (scene_name == "MainMenu2_Scene" || scene_name == "MainMenu2-1_Scene" || scene_name == "t64_menu")
            {
                moduleManager.LoadAllStaticAssets();
                AMMO_105mm.LoadAssets();
                AssetUtil.ReleaseVanillaAssets();
            }

            if (Util.menu_screens.Contains(scene_name)) return;

            moduleManager.LoadAllDynamicAssets();

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);

            Armour.Init();
            AMMO_105mm.Init();
            M60A1.Init();
            M60A3.Init();
        }
    }

    public class Module
    {
        private bool static_assets_loaded = false;
        private bool dynamic_assets_loaded = false;

        public bool TryLoadStaticAssets()
        {
            if (static_assets_loaded) return false;
            LoadStaticAssets();
            static_assets_loaded = true;
            return true;
        }

        public bool TryLoadDynamicAssets()
        {
            if (dynamic_assets_loaded) return false;
            LoadDynamicAssets();
            dynamic_assets_loaded = true;
            return true;
        }

        public bool TryUnloadDynamicAssets()
        {
            if (!dynamic_assets_loaded) return false;
            dynamic_assets_loaded = false;
            UnloadDynamicAssets();
            return true;
        }

        public virtual void LoadStaticAssets() { }
        public virtual void LoadDynamicAssets() { }
        public virtual void UnloadDynamicAssets() { }
    }

    public class ModuleManager
    {
        internal Dictionary<string, Module> modules = new Dictionary<string, Module>();
        private string mod_id;

        public ModuleManager(string mod_id)
        {
            this.mod_id = mod_id;
        }

        public void Add(string id, Module module)
        {
            modules.Add(id, module);
        }

        public void LoadAllDynamicAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool loaded = module.TryLoadDynamicAssets();
                if (loaded) MelonLogger.Msg(mod_id + " dynamic assets loaded from module: " + id);
            }
        }

        public void LoadAllStaticAssets()
        {
            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool static_loaded = module.TryLoadStaticAssets();
                if (static_loaded) MelonLogger.Msg(mod_id + " static assets loaded from module: " + id);
            }
        }
    }
}