using BepInEx;
using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TABG;
using Landfall.Network;
using System.Threading.Tasks;
using Open.Nat;

namespace TABGxGUI
{
    public class TABGxUI
    {
        // this is called in the Update loop for ingame init stuff
        public static bool ShouldInitialize = false;
        public static TunnelState TunnelOption = TunnelState.Uninitialized;
        public static GameObject HideIp;
        public static ServerWrapper Server;

        private static GameObject OptionsMenu;

        public static void CreateServer(float players, string gamemode, string autostart, string tunnel)
        {
            // fix args
            // options are prefixed with o_
            int o_players = (int)Math.Round(players);

            GameMode o_gamemode = GameMode.BattleRoyale;
            if(gamemode == "Brawl")
            {
                o_gamemode = GameMode.Brawl;
            } else if(gamemode == "Battle Royale")
            {
                o_gamemode = GameMode.BattleRoyale;
            }

            bool o_autostart = autostart == "ON" ? true : false;
            
            if(tunnel == "None (LAN)")
            {
                TunnelOption = TunnelState.None;
            } else if(tunnel == "UPnP (Internet accessible)")
            {
                TunnelOption = TunnelState.Upnp;
                TraverseNat();
            }

            Task.Run(() => TABGServer.StartServer(o_players, o_gamemode, o_autostart));
            System.Threading.Thread.Sleep(1000);

            MatchmakingHandler.Instance.UpdateMatchmakingState(Landfall.Network.JoinState.Searching);
            CreateKillServerMenu();

            ServerConnector.Instance.ConnectToServerIP("127.0.0.1", 9000, "");
            Task.Run(() => {
                System.Threading.Thread.Sleep(2500);
                ShouldInitialize = true;
            });
        }

        public static void AddDevtools()
        {
            GameObject objectToClone = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/OptionsMenu/Options/Item_AO");
            GameObject objectTransform = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/OptionsMenu/Options");
            GameObject newObject = UnityEngine.Object.Instantiate<GameObject>(objectToClone, objectTransform.transform);

            newObject.name = "HideIp";
            newObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("HIDE IP?");

            // set default to off
            newObject.GetComponent<OptionsButton>().valueNames[0] = "OFF";
            newObject.GetComponent<OptionsButton>().valueNames[1] = "ON";

            HideIp = newObject;
        }

        public static async void TraverseNat()
        {
            // this might include STUN or proxy NAT code sometime
            // for now it's just upnp

            try
            {
                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(10000);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, 9000, 9119, "TABG Community Server Port UDP"));

                Debug.Log("UPnP port mapping was successful!");
            } catch(Exception e)
            {
                Debug.LogException(e);
                Debug.Log("UPNP Provisioning failed! Notifying client...");
                Singleton<GlobalCanvasSingleton>.Instance.UIMessageBox.QueueMessage("Your router does not support UPnP, so you cannot use UPnP tunneling.", null);
            }
            //await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 9000, 9119, "TABG Community Server Port TCP"));
        }

        private static void DestroyOptionItem(string itemName)
        {
            UnityEngine.Object.Destroy(GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/" + itemName));
        }

        private static void OpenGUI()
        {
            // we can use admin menu state because it's unimplemented
            MenuState.CurrentMenuState = MenuState.TABGMenuState.AdminMenu;

            GameObject newObject2 = OptionsMenu;

            // set active
            newObject2.SetActive(true);
            newObject2.name = "CreateServerScreen";

            // remove controls and controls tab
            UnityEngine.Object.Destroy(GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Controls"));
            UnityEngine.Object.Destroy(GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/TABS/Controls"));

            GameObject gameOptions = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options");

            // remove unused options
            DestroyOptionItem("Item_CatchPhrases");
            DestroyOptionItem("Item_Music");
            DestroyOptionItem("Item_SensY");
            DestroyOptionItem("Item_SensX");
            DestroyOptionItem("Item_RenderDistance");
            DestroyOptionItem("Item_ShadowDistance");
            DestroyOptionItem("Item_Resolution");
            DestroyOptionItem("Item_Fullscreen");
            // set the texts
            (GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/TABS/Options").GetComponentsInChildren<TMPro.TextMeshProUGUI>())[0].GetTextInfo("New Server");


            // options
            (GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/TABS/Options").GetComponentsInChildren<TMPro.TextMeshProUGUI>())[0].GetTextInfo("New Server");
            var maxPlayers = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_MasterVolume");
            maxPlayers.name = "Item_MaxPlayers";
            maxPlayers.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("MAX PLAYERS");
            var dmgMulti = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_SFX");
            dmgMulti.name = "Item_DamageMultiplier";
            dmgMulti.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("DAMAGE MULTIPLIER");
            // hide it for now (not implemented)
            dmgMulti.SetActive(false);

            var gamemode = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_ShadowQuality");
            gamemode.name = "Item_Gamemode";
            gamemode.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("GAMEMODE");
            // set proper values
            string[] gamemodes = { "Battle Royale", "Brawl" };
            gamemode.GetComponent<OptionsButton>().valueNames = gamemodes;
            var adminCommands = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_AO");
            adminCommands.name = "Item_AdminCommands";
            adminCommands.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("ADMIN COMMANDS");
            // hide it for now (this will come in an update where there is a server console available to the host)
            adminCommands.SetActive(false);
            var autoStart = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_VSync");
            autoStart.name = "Item_AutomaticStart";
            autoStart.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("AUTOMATIC GAME START");
            var useTunnel = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_Display");
            // fix auto set
            UnityEngine.Object.Destroy(GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/Item_Display/OptionsDisplay"));
            useTunnel.name = "Item_UseTunnel";
            useTunnel.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("USE TUNNEL?");
            // set proper values, more coming soon
            string[] tunnelOptions = { "None (LAN)", "UPnP (Internet accessible)" };
            var tunnelOptionButton = useTunnel.GetComponent<OptionsButton>();
            tunnelOptionButton.valueNames = tunnelOptions;
            tunnelOptionButton.SetValue(0);
            // create button
            var startServer = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Options/ApplyScreen");
            var startServerButton = startServer.GetComponentsInChildren<UnityEngine.UI.Button>()[0];
            startServer.name = "StartServerButton";
            startServerButton.onClick.m_PersistentCalls.Clear();
            startServerButton.onClick.AddListener(() => {
                // get variables
                var val_maxPlayers = maxPlayers.GetComponent<OptionsButton>().GetFieldValue<Slider>("slider").value;
                var val_gamemode = gamemode.GetComponent<OptionsButton>().GetFieldValue<TextMeshProUGUI>("valueText").text;
                //var val_adminCommands = adminCommands.GetComponent<OptionsButton>().GetFieldValue<TextMeshProUGUI>("valueText").text;
                var val_autoStart = autoStart.GetComponent<OptionsButton>().GetFieldValue<TextMeshProUGUI>("valueText").text;
                var val_useTunnel = useTunnel.GetComponent<OptionsButton>().GetFieldValue<TextMeshProUGUI>("valueText").text;
                Debug.Log($"MAX PLAYERS: {val_maxPlayers}\nGAMEMODE: {val_gamemode}\nADMIN COMMANDS: {"disabled"}\nAUTOSTART: {val_autoStart}\nUSE TUNNEL? {val_useTunnel}");
                CreateServer(val_maxPlayers, val_gamemode, val_autoStart, val_useTunnel);
                Debug.Log("Creating server...");
                newObject2.SetActive(false);
                MenuState.CurrentMenuState = MenuState.TABGMenuState.Main;

            });
            startServer.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].GetTextInfo("START NEW SERVER (MAY TAKE A FEW MOMENTS)");

            // back button
            var backButtonOnclick = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen/Back").GetComponentsInChildren<UnityEngine.UI.Button>()[0].onClick;
            backButtonOnclick.m_Calls.Clear();
            backButtonOnclick.m_PersistentCalls.Clear();
            backButtonOnclick.AddListener(() => {
                Debug.Log("Closing Server menu!");
                newObject2.SetActive(false);
                MenuState.CurrentMenuState = MenuState.TABGMenuState.Main;
            });
        }

        public static void DeleteKillServerMenu()
        {
            GameObject killServerButton = GameObject.Find("/MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons/KillServer");
            if (killServerButton != null)
            {
                killServerButton.name = "unused";
                killServerButton.SetActive(false);
                UnityEngine.Object.Destroy(killServerButton);
            }
        }

        public static void CreateKillServerMenu()
        {
            GameObject x = GameObject.Find("/MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons/Quit");
            GameObject y = GameObject.Find("/MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons");

            RectTransform component = x.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(component.sizeDelta.x, component.sizeDelta.y + 60f);

            GameObject newObject = UnityEngine.Object.Instantiate<GameObject>(x, y.transform);
            // make server creator top button
            newObject.transform.SetSiblingIndex(1);
            newObject.name = "KillServer";

            var text = newObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0];
            text.GetTextInfo("KILL SERVER");

            var button = newObject.GetComponent<UnityEngine.UI.Button>();
            var onClickEvent = button.onClick;

            onClickEvent.m_Calls.Clear();
            onClickEvent.m_PersistentCalls.Clear();

            onClickEvent.AddListener(() =>
            {
                DeleteKillServerMenu();
                if (TABGxUI.Server != null)
                {
                    TABGxUI.Server.Server.Kill();
                    Debug.Log("Server killed");
                    Singleton<GlobalCanvasSingleton>.Instance.UIMessageBox.QueueMessage("Server has been terminated", null);
                } else
                {
                    Debug.Log("Wrapper was not found");
                    Singleton<GlobalCanvasSingleton>.Instance.UIMessageBox.QueueMessage("Error: Server was not found!", null);
                }
            });
        }

        public static void Start()
        {
            // test if server is already running
            if (Server != null)
            {
                CreateKillServerMenu();
            }

            // preload options menu
            GameObject x2 = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/OptionsMenu");
            GameObject y2 = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas");

            OptionsMenu = UnityEngine.Object.Instantiate<GameObject>(x2, y2.transform);

            // test if old options menu exists
            GameObject oldMenu = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/CreateServerScreen");
            GameObject oldOptions = GameObject.Find("MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons/CreateServer");

            if (oldMenu != null)
            {
                oldMenu.name = "unused";
                oldMenu.SetActive(false);
                UnityEngine.Object.Destroy(oldMenu);
            }

            if (oldOptions != null)
            {
                oldOptions.name = "unused2";
                oldOptions.SetActive(false);
                UnityEngine.Object.Destroy(oldOptions);
            }

            GameObject x = GameObject.Find("/MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons/Options");
            GameObject y = GameObject.Find("/MainMenuCamPivot/MainMenuCam/UICAM/Canvas/MainScreen/LowerLeftButtons");

            RectTransform component = x.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(component.sizeDelta.x, component.sizeDelta.y + 60f);

            GameObject newObject = UnityEngine.Object.Instantiate<GameObject>(x, y.transform);
            // make server creator top button
            newObject.transform.SetSiblingIndex(1);
            newObject.name = "CreateServer";

            var text = newObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0];
            text.GetTextInfo("CREATE SERVER");

            var button = newObject.GetComponent<UnityEngine.UI.Button>();
            var onClickEvent = button.onClick;

            onClickEvent.m_Calls.Clear();
            onClickEvent.m_PersistentCalls.Clear();

            onClickEvent.AddListener(() =>
            {
                Debug.Log("Server menu triggered!");
                OpenGUI();
            });
        }
    }
}