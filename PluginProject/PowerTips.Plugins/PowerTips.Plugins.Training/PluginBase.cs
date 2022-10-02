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

        
        public void Execute(IServiceProvider serviceProvider)
        {
            var watch = Stopwatch.StartNew();
            var cntx = new ContextBase(serviceProvider);

            try
            {
                Execute(cntx);
            }
            catch (Exception ex)
            {
                cntx.Trace(ex.Message, ex);
                throw;
            }
            finally
            {
                watch.Stop();
                cntx.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
            }
        }

        public abstract void Execute(ContextBase context);
    }
}
