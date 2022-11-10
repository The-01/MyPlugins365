using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginsCollection
{
    public class TaskCreate : IPlugin
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
                Entity contact = (Entity)context.InputParameters["Target"];

                try
                {
                    Entity task = new Entity("task");

                    // Single line of text
                    task["subject"] = "Plugin Task";
                    // Multiple lines of text
                    task["description"] = "This task is created when a new contact record is created (through plugin)";
                    // Date
                    task["scheduledend"] = DateTime.Now.AddDays(2);
                    // Lookup
                    task["regardingobjectid"] = contact.ToEntityReference();
                    // Option set
                    task["actualdurationminutes"] = 45;
                    // Option set
                    task["prioritycode"] = new OptionSetValue(2);

                    tracingService.Trace("New task activity created (On create of contact record)");
                    service.Create(task);
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
