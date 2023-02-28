using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace PowerTips.Demo.DVManagedIdentity
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly ServiceClient _serviceClient;

        public Function1(ILogger<Function1> log, IOrganizationService orgSvc)
        {
            _logger = log;
            _serviceClient = (ServiceClient)orgSvc;
        }

        [FunctionName("Function1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            QueryExpression qe = new QueryExpression("pmav_superhero");
            qe.Criteria.AddCondition("pmav_name", ConditionOperator.Equal, name);

            EntityCollection ec = _serviceClient.RetrieveMultiple(qe);

            string responseMessage = string.Empty;

            if (ec != null && ec.Entities.Count > 0)
            {
                responseMessage = $"Hello, {name}. This HTTP triggered function executed successfully.";
            }
            else
            {
                responseMessage = "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response.";
            }

            return new OkObjectResult(responseMessage);
        }
    }
}

