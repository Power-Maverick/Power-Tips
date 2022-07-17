using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTips.Plugins.Training
{
    public class ContextBase : ITracingService, IOrganizationService
    {
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

        public ContextBase(IServiceProvider serviceProvider)
        {
            factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            tracer = new Lazy<ITracingService>(() => (ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            context = new Lazy<IPluginExecutionContext>(() => (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(context.Value.UserId));

            Tracer.TraceContext(Context, true, true, true, true, Service);
        }

        public void Trace(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            Tracer.Trace("{0} {1}", DateTime.Now.ToString("HH:mm:ss:fff"), msg);
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Associate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var watch = Stopwatch.StartNew();
            Service.Associate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            Trace($"Associated in: {watch.ElapsedMilliseconds} ms");
        }

        public Guid Create(Entity entity)
        {
            Trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            var result = Service.Create(entity);
            watch.Stop();
            Trace($"Created in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Delete(string entityName, Guid id)
        {
            Trace($"Delete({entityName}, {id})");
            var watch = Stopwatch.StartNew();
            Service.Delete(entityName, id);
            watch.Stop();
            Trace($"Deleted in: {watch.ElapsedMilliseconds} ms");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var watch = Stopwatch.StartNew();
            Service.Disassociate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            Trace($"Disassociated in: {watch.ElapsedMilliseconds} ms");
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            Trace($"Execute({request.RequestName})");
            var watch = Stopwatch.StartNew();
            var result = Service.Execute(request);
            watch.Stop();
            Trace($"Executed in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            Trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            var watch = Stopwatch.StartNew();
            var result = Service.Retrieve(entityName, id, columnSet);
            watch.Stop();
            Trace($"Retrieved in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            Trace("RetrieveMultiple({0})", query is QueryExpression ? ((QueryExpression)query).EntityName : query is QueryByAttribute ? ((QueryByAttribute)query).EntityName : query is FetchExpression ? "fetchxml" : "unkstartn");
            var watch = Stopwatch.StartNew();
            var result = Service.RetrieveMultiple(query);
            watch.Stop();
            Trace($"Retrieved {result.Entities.Count} records in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Update(Entity entity)
        {
            Trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            Service.Update(entity);
            watch.Stop();
            Trace($"Updated in: {watch.ElapsedMilliseconds} ms");
        }

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
