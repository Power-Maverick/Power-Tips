using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using PowerTips.Plugins.Training;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerTips.Plugins.UnitTests
{
    [TestClass]
    public class CalculateSuperHeroScoreTest
    {
        [TestMethod]
        public void ExecutePluginTest()
        {
            // Init
            var context = new XrmFakedContext();
            var pluginContext = context.GetDefaultPluginContext();

            // Prepare
            var id = Guid.NewGuid();

            var target = new Entity("pmav_superhero") { Id = id };
            target.Attributes.Add("powm_showhidepowerstats", true);

            var postImage = new Entity("pmav_superhero") { Id = id };
            postImage.Attributes.Add("powm_showhidepowerstats", true);
            postImage.Attributes.Add("pmav_stats_intelligence", 50);
            postImage.Attributes.Add("pmav_stats_strength", 50);
            postImage.Attributes.Add("pmav_stats_speed", 50);
            postImage.Attributes.Add("pmav_stats_durability", 50);
            postImage.Attributes.Add("pmav_stats_power", 50);
            postImage.Attributes.Add("pmav_stats_combat", 50);

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", target);

            EntityImageCollection postImages = new EntityImageCollection();
            postImages.Add("PostImage", postImage);

            pluginContext.InputParameters = inputParameters;
            pluginContext.PostEntityImages = postImages;

            context.Initialize(new List<Entity>() { postImage });

            // Execute
            context.ExecutePluginWith<CalculateSuperHeroScore>(pluginContext);

            // Assert
            var superHeroAfterExecution = context.CreateQuery("pmav_superhero").FirstOrDefault();
            Assert.AreEqual("50", superHeroAfterExecution["powm_combinedpowerstats"]);
        }
    }
}
