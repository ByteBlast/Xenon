﻿using System.Collections.Generic;
using System.Linq;
using Durango.Common;
using Durango.Infrastructure;
using Durango.Models;
using Newtonsoft.Json.Linq;

namespace Durango.Clients
{
    public class AchievementsClient : ClientBase
    {
        private const string BaseAddress = "https://live.xbox.com/en-US/Activity/Details?titleId=";

        internal AchievementsClient(Connection connection) 
            : base(connection)
        {
        }

        public IEnumerable<Achievement> GetAchievements(string gamertag, string gameId)
        {
            EnsureAuthenticated();

            gamertag = gamertag.ToLower();

            var content = WebAgent.GetString(BaseAddress + gameId + "&compareto=" + gamertag);
            var contentJson = content.ParseBetween("broker.publish(routes.activity.details.load, ", ");");
            contentJson = contentJson.ToLower();

            if (contentJson == "")
            {
                return null;
            }

            dynamic achievements = JObject.Parse(contentJson)["achievements"];

            foreach (var achievement in achievements)
            {
                if (Enumerable.Any(achievement.earndates.Children()))
                {
                    achievement.earnedon = achievement.earndates[gamertag].earnedon;
                    achievement.isoffline = achievement.earndates[gamertag].isoffline;
                }

                ((JObject)achievement.earndates).Parent.Remove();
            }

            return achievements.ToObject<IEnumerable<Achievement>>();
        }
    }
}