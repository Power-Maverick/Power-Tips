using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTips.Plugins.Training
{
    public abstract class PluginBase : IPlugin
    {
        /*
       * **************************
       * IMPLEMENTED IN PLUGIN-BASE
       * **************************
       * Main Logging is already handled
       * Main Tracing is already handled 
       * Try-Catch already exists
       */

        /*
        * *****************************
        * WHAT YOU GET FROM PLUGIN-BASE
        * *****************************
        * Target        = Entity
        * Context       = Plugin Context
        * Service       = Organization Service
        * Tracer.Write  = Tracing the plugin execution
        * PreImage      = Provides first pre-image entity
        * PostImage     = Provides first post-image entity
        */

        private Lazy<ITracingService> tracer;
        private Lazy<IPluginExecutionContext> context;
        private Lazy<IOrganizationService> service;
        private IOrganizationServiceFactory factory;

        public ITracingService Tracer => tracer.Value;
        public IPluginExecutionContext Context => context.Value;
        public IOrganizationService Service => service.Value;
        public Entity Target => new Lazy<Entity>(() => getTarget()).Value;
        public Entity PreImage => new Lazy<Entity>(() => getImages(Context.PreEntityImages)).Value;
        public Entity PostImage => new Lazy<Entity>(() => getImages(Context.PostEntityImages)).Value;

        public void Execute(IServiceProvider serviceProvider)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                tracer = new Lazy<ITracingService>(() => (ITracingService)serviceProvider.GetService(typeof(ITracingService)));
                context = new Lazy<IPluginExecutionContext>(() => (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
                service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(context.Value.UserId));

                Tracer.TraceContext(Context, true, true, true, true, Service);
                Execute();
            }
            catch (Exception ex)
            {
                Tracer.Trace(ex.Message, ex);
                throw;
            }
            finally
            {
                watch.Stop();
                Tracer.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
            }
        }

        public abstract void Execute();

        #region Private

        private Entity getTarget()
        {
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity result)
            {
                return result;
            }
            return null;
        }

        private Entity getImages(EntityImageCollection images)
        {
            if (images.Count > 0 && images.FirstOrDefault().Value is Entity result)
            {
                return result;
            }
            return null;
        }

        #endregion
    }
}
