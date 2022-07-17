using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTips.Plugins.Training
{
    public class CalculateSuperHeroScore : PluginBase
    {
        public override void Execute(ContextBase context)
        {
            bool isPowerStatsSetToShow = context.Target.GetAttributeValue<bool>("powm_showhidepowerstats");

            context.Trace($"Value for Show/Hide: {isPowerStatsSetToShow}");

            if (isPowerStatsSetToShow)
            {
                int intellegence = context.PostImage.GetAttributeValue<int>("pmav_stats_intelligence");
                int strength = context.PostImage.GetAttributeValue<int>("pmav_stats_strength");
                int speed = context.PostImage.GetAttributeValue<int>("pmav_stats_speed");
                int durability = context.PostImage.GetAttributeValue<int>("pmav_stats_durability");
                int power = context.PostImage.GetAttributeValue<int>("pmav_stats_power");
                int combat = context.PostImage.GetAttributeValue<int>("pmav_stats_combat");

                context.Trace($"Intelligence: {intellegence};Strength: {strength}; Speed: {speed}; Durability: {durability}; Power: {power}; Combat: {combat}");

                int score = (intellegence + strength + speed + durability + power + combat) / 6;

                context.Trace($"Avg: {score}");
                
                Entity toUpdate = new Entity("pmav_superhero", context.Target.Id);
                toUpdate.Attributes.Add("powm_combinedpowerstats", score.ToString());
                context.Update(toUpdate);
            }
        }
    }
}
