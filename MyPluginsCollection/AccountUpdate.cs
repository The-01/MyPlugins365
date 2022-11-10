using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginsCollection
{
    public class AccountUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference which you will need for
            // web service calls.
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity accountRecord = service.Retrieve("account", context.PrimaryEntityId, new ColumnSet(new string[] { "numberofemployees", "creditlimit" }));

                var numberOfEmployees = accountRecord.Contains("numberofemployees") ? accountRecord.GetAttributeValue<int>("numberofemployees") : 0;
                var creditLimit = accountRecord.Contains("creditlimit") ? accountRecord.GetAttributeValue<Money>("creditlimit").Value : 0;

                try
                {
                    Entity accountUpdate = new Entity("account");
                    accountUpdate.Id = context.PrimaryEntityId;

                    if (numberOfEmployees < 10 && creditLimit < 100000)
                    {
                        accountUpdate["revenue"] = new Money(1500000);
                    }
                    else
                    {
                        accountUpdate["revenue"] = new Money(10000000);
                    }

                    tracingService.Trace("Account revenue field updated (On update of certain fields)");
                    service.Update(accountUpdate);
                }
                catch (FaultException ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
