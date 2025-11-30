using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Oxide.Core.Plugins;
using Network;
using UnityEngine;


namespace Oxide.Plugins
{
    [Info("Player Ping Report", "zhuhlia", "1.0.0")]
    [Description("Player report Ping lag")]
    public class PlayerPingReport : RustPlugin
    {
        [PluginReference]
        private Plugin DiscordMessages;

        private Dictionary<string, double> lastPingTime = new Dictionary<string, double>();

        [ChatCommand("ping")]
        private void chat_command_ping(BasePlayer player, string command, string[] args)
        {
            if (player == null || !player.IsConnected) return;

            string playerId = player.UserIDString;
            double currentTime = Time.realtimeSinceStartup;

            if (lastPingTime.TryGetValue(playerId, out double lastTime) && currentTime - lastTime < configData.cooldownTime)
            {
                player.ChatMessage($"[PONG] wait {configData.cooldownTime - (currentTime - lastTime):F1} sec.");
                return;
            }

            lastPingTime[playerId] = currentTime; 
            player.ChatMessage($"[PONG] Your message has been delivered!");

            int ping = 0;
            if (player?.net?.connection != null)
            {
                try
                {
                    var avg = Net.sv.GetAveragePing(player.net.connection);
                    ping = Convert.ToInt32(avg);
                }
                catch
                {
                    ping = 0;
                }
            }

            DisMessPing(player.UserIDString, player.displayName, ping);
        }

        void DisMessPing(string id, string name, int player_ping)
        {
            string Title = $"Ping Report ->`{ConVar.Server.hostname}`";
            int Color = 15844367;
            
            object fields = new[]
            {
                new {
                    name = $"Player - {name} https://steamcommunity.com/profiles/{id}",
                    value = $"Player ping: `{player_ping}`",
                    inline = true
                }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(fields);
			DiscordMessages?.Call("API_SendFancyMessage", configData.ds_hook, Title, Color, json);  
		}
        #region config
        private ConfigData configData;
        class ConfigData
        {
            [JsonProperty(PropertyName = "Cooldoun")]
            public int cooldownTime = 60;

            [JsonProperty(PropertyName = "Discord WebHook")]
            public string ds_hook = "";
        }

        private bool LoadConfigVariables()
        {
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                {
                    Puts("Config file is empty or invalid. Creating new config.");
                    configData = new ConfigData();
                    SaveConfig(configData);
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                Puts($"Config file is corrupted: {ex.Message}. Creating new config.");
                configData = new ConfigData();
                SaveConfig(configData);
                return false;
            }
        }
        protected override void LoadDefaultConfig()
        {
            Puts("Creating new config file.");
            configData = new ConfigData();
            SaveConfig(configData);
        }

        void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion
        void Init()
        {
            if (!LoadConfigVariables())
            {
                Puts("Config file issue detected. Please delete file, or check syntax and fix.");
                return;
            }
        }
    }
}
