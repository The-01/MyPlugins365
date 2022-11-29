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
    public class ContactCreate : IPlugin
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
                Entity account = service.Retrieve("account", context.PrimaryEntityId, new ColumnSet(new string[] { "name", "emailaddress1", "telephone1" }));

                var accountName = "";
                var email = "";
                var mobile = "";

                // Get account name
                if (account.Contains("name") && account["name"] != null)
                {
                    accountName = account["name"].ToString();
                }
                // Get email address
                if (account.Contains("emailaddress1") && account["emailaddress1"] != null)
                {
                    email = account["emailaddress1"].ToString();
                }
                // Get phone number
                if (account.Contains("telephone1") && account["telephone1"] != null)
                {
                    mobile = account["telephone1"].ToString();
                }

                try
                {
                    Entity contact = new Entity("contact");
                    var namesArray = accountName.ToString().Split(' ');
                    contact["firstname"] = namesArray[0];
                    contact["lastname"] = namesArray[1];
                    contact["emailaddress1"] = email;
                    contact["telephone1"] = mobile;
                    contact["mobilephone"] = mobile;
                    // contact["parentcustomerid"] = account.ToEntityReference();
                    contact["parentcustomerid"] = new EntityReference("account", context.PrimaryEntityId);
                    contact["gendercode"] = new OptionSetValue(1);
                    contact["birthdate"] = new DateTime(2001, 10, 22);
                    contact["creditlimit"] = new Money(20000);
                    contact["creditonhold"] = true;
                    

                    service.Create(contact);
                    tracingService.Trace("New contact record created (On create of account record)");
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
