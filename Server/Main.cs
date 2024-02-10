namespace TABG;

using System;
using System.Diagnostics;
using System.Text;
using ENet;
using TABGxGUI;
using UnityEngine;

public static class TABGServer
{
    public static Task StartServer(int players, GameMode gamemode, bool autostart)
    {
        string version = "0.1.0";
        bool isBeta = true;
        bool isAlpha = true;

        UnityEngine.Debug.Log("Starting TABG Community Server version " + version + " by incomprehensibl");

        if (isBeta || isAlpha)
        {
            UnityEngine.Debug.Log("You're running TABG Beta. Game-breaking bugs are possible!");
            UnityEngine.Debug.Log("Report bugs and request features at https://github.com/JIBSIL/TABGCommunityServer/issues");
        }

        Stopwatch timer = new Stopwatch();
        timer.Start();

        UnityEngine.Debug.Log("Booting Configuration...Skipped (DLL)");

        // create configuration here
        Configuration configuration = new()
        {
            Version = version,
            IsAlpha = isAlpha,
            IsBeta = isBeta
        };

        UnityEngine.Debug.Log("Booting server list... UNIMPLEMENTED");
        // BOOT SERVER LIST HERE

        UnityEngine.Debug.Log("Setting up servers...");
        ServerConcurrencyHandler serverConcurrencyHandler = new();
        configuration.ServerConcurrencyHandler = serverConcurrencyHandler;

        configuration.Initialized = true;

        UnityEngine.Debug.Log("Booting first server on port 9000...");
        ServerWrapper wrapper = serverConcurrencyHandler.CreateServer(players, gamemode, autostart);
        TABGxUI.Server = wrapper;

        timer.Stop();
        var timeTaken = timer.ElapsedMilliseconds;

        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("All systems online. Took " + timeTaken + "ms!");
        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("-----");

        // hang thread to prevent it from dying
        while(true) { }
    }
}