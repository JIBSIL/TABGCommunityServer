using BepInEx;
using System;
using System.Reflection;
using Epic.OnlineServices.AntiCheatClient;
using HarmonyLib;
using Landfall;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mono.Cecil;
using System.Net;
using Landfall.Network;

namespace TABGxGUI
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        private bool privateIp = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TABGxUI.Start();
            TABGxUI.AddDevtools();
        }

        private void Update()
        {
            if (TABGxUI.HideIp != null)
            {
                if(TABGxUI.HideIp.GetComponent<OptionsButton>().GetFieldValue<TMPro.TextMeshProUGUI>("valueText").text == "ON")
                {
                    privateIp = true;
                } else
                {
                    privateIp = false;
                }
            }

            if (TABGxUI.ShouldInitialize == true)
            {
                TABGxUI.ShouldInitialize = false;
                MatchModifier matchModifier = TABGLootPresetDatabase.Instance.GetMatchModifier(5);

                string iptext = "IP: Unknown (internal error)";
                string ip = "unknown";

                if(TABGxUI.TunnelOption == TunnelState.None)
                {
                    // no tunnel, use public ip
                    ip = GetPublicIpAddress();
                    if (privateIp) ip = "X";
                    iptext = "IP: " + ip + ":9000";

                } else if (TABGxUI.TunnelOption == TunnelState.Upnp)
                {
                    ip = GetPublicIpAddress();
                    if (privateIp) ip = "X";
                    iptext = "IP: " + ip + ":9119";
                }

                Landfall.Network.ClientGameHandler.UIHandler.GetComponentInChildren<SpecialRoundUI>().PlaySpecialRound("Server Created!", iptext, matchModifier.Icon);

                // set the room name
                (GameObject.Find("InventoryUI(Clone)/ScreenSpaceCanvas/Canvas/CurrentRoomName").GetComponentsInChildren<TMPro.TextMeshProUGUI>())[0].GetTextInfo(iptext + " (Hosting)");
            }
        }

        // ip function ala https://stackoverflow.com/questions/3253701/get-public-external-ip-address
        private string GetPublicIpAddress()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }
    }
}
