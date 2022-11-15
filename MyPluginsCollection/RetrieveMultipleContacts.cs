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
    public class RetrieveMultipleContacts : IPlugin
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
                Entity preImage = context.PreEntityImages["preImage"];

                var firstName = "";
                if (contact.Contains("parentcustomerid") && contact["parentcustomerid"] != null)
                {
                    firstName = contact.GetAttributeValue<string>("firstname");
                }
                else
                {
                    firstName = preImage.GetAttributeValue<string>("firstname");
                }

                var mobile = contact.Contains("mobilephone") ? contact.GetAttributeValue<string>("mobilephone") : "";

                Entity contactUpdate = new Entity("contact");
                contactUpdate.Id = context.PrimaryEntityId;

                if (mobile == "12345")
                {
                    contactUpdate["fax"] = mobile + "999";
                }

                service.Update(contactUpdate);

                string fetchXML =
                    $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                        <entity name = 'contact' >
                            <attribute name='firstname' />
                            <attribute name = 'mobilephone' />
                            <order attribute = 'firstname' descending='false' />
                            <filter type = 'and' >
                                <condition attribute = 'firstname' operator='eq' value='{firstName}'/>
                            </filter>
                        </entity>
                    </fetch>";

                var retrieveQuery = new FetchExpression(fetchXML);

                try
                {
                    EntityCollection result = service.RetrieveMultiple(retrieveQuery);

                    Entity task = new Entity("task");

                    // Single line of text
                    task["subject"] = "Plugin Task - " + result.Entities[0].Attributes["firstname"];
                    // Multiple lines of text
                    task["description"] = "This task is created of the retrieved data from account record (through plugin)";
                    // Date
                    task["scheduledend"] = DateTime.Now.AddDays(2);
                    // Lookup
                    task["regardingobjectid"] = contact.ToEntityReference();
                    // Option set
                    task["actualdurationminutes"] = 45;
                    // Option set
                    task["prioritycode"] = new OptionSetValue(2);

                    tracingService.Trace("New task activity created (From existing retrieved contact record)");
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
