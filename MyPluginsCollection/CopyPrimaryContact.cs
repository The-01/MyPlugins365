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
    public class CopyPrimaryContact : IPlugin
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
                Entity accountRecord = service.Retrieve("account", context.PrimaryEntityId, new ColumnSet(new string[] { "primarycontactid" }));

                if (accountRecord["primarycontactid"] != null)
                {
                    // throw new InvalidPluginExecutionException("Error");
                    Guid primaryContactId = ((EntityReference)accountRecord["primarycontactid"]).Id;
                    var primaryContactType = ((EntityReference)accountRecord["primarycontactid"]).LogicalName;

                    Entity primaryContactRecord = service.Retrieve(primaryContactType, primaryContactId, new ColumnSet(new string[] { "firstname", "lastname", "parentcustomerid", "emailaddress1", "telephone1" }));
                    var firstName = primaryContactRecord.Contains("firstname") ? primaryContactRecord["firstname"] : "";
                    var lastName = primaryContactRecord["lastname"];
                    var parentCustomerId = primaryContactRecord.Contains("parentcustomerid") ? accountRecord.ToEntityReference() : null;
                    var email = primaryContactRecord.Contains("emailaddress1") ? primaryContactRecord["emailaddress1"] : "";
                    var mobile = primaryContactRecord.Contains("telephone1") ? primaryContactRecord["telephone1"] : "";

                    try
                    {
                        Entity contactCopy = new Entity("contact");
                        contactCopy["firstname"] = "Copy - " + firstName;
                        contactCopy["lastname"] = lastName;
                        contactCopy["parentcustomerid"] = parentCustomerId;
                        contactCopy["emailaddress1"] = email;
                        contactCopy["telephone1"] = mobile;

                        tracingService.Trace("New copy of primary contact created (On update of primary contact field in account entity)");
                        service.Create(contactCopy);
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
}
