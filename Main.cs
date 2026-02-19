using System.Collections;
using System.Linq;
using GHPC.Camera;
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
            if (Util.menu_screens.Contains(scene_name)) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);

            Armour.Init();
            AMMO_105mm.Init();
            M60A1.Init();
            M60A3.Init();
        }
    }
}
