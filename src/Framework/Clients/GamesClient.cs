﻿using Framework.Common;
using Framework.Infrastructure;
using Framework.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace Framework.Clients
{
    public class GamesClient : ClientBase
    {
        private const string BaseAddress = "https://live.xbox.com/en-US/Activity/Summary?compareTo=";

        internal GamesClient(Connection connection)
            : base(connection)
        {
        }

        public IEnumerable<Game> GetGames(string gamertag)
        {
            var document = WebAgent.DownloadDocumentNode("https://live.xbox.com/en-GB/Friends");
            var docNode = document.DocumentNode;

            EnsureAuthenticated();

            var token =
                docNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").Attributes["value"].Value;

            var requestUri = BaseAddress + gamertag + "&lc=1033";
            var response = WebAgent.Post(
                requestUri,
                "__RequestVerificationToken=" + token,
                new WebHeaderCollection {{"X-Requested-With", "XMLHttpRequest"}}
            );

            var content = response.GetResponseStream().ReadAsString();

            dynamic games = JObject.Parse(content)["Data"]["Games"];
            foreach (var game in games)
            {
                // remove any notion of the comparate  
                game.Progress.Replace(
                     JObject.FromObject(new
                     {
                         game.Progress[gamertag].Score,
                         game.Progress[gamertag].Achievements,
                         game.Progress[gamertag].LastPlayed
                     }));
            }

            return games.ToObject<IEnumerable<Game>>();
        }
    }
}