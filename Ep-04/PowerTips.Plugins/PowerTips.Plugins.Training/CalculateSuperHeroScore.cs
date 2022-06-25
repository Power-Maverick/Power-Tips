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
        public override void Execute()
        {
            bool isPowerStatsSetToShow = Target.GetAttributeValue<bool>("powm_showhidepowerstats");

            if (isPowerStatsSetToShow)
            {
                int intellegence = Target.GetAttributeValue<int>("pmav_stats_intelligence");
                int strength = Target.GetAttributeValue<int>("pmav_stats_strength");
                int speed = Target.GetAttributeValue<int>("pmav_stats_speed");
                int durability = Target.GetAttributeValue<int>("pmav_stats_durability");
                int power = Target.GetAttributeValue<int>("pmav_stats_power");
                int combat = Target.GetAttributeValue<int>("pmav_stats_combat");

                int score = (intellegence + strength + speed + durability + power + combat) / 6;

                Entity toUpdate = new Entity("pmav_superhero", Target.Id);
                toUpdate.Attributes.Add("powm_combinedpowerstats", score.ToString());
                Service.Update(toUpdate);
            }
        }
    }
}
