using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PluginTasks
{
    public class SendEmail : IPlugin
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
                EntityReference owner = (EntityReference)contact["ownerid"];

                try
                {
                    //create activityparty
                    Entity Fromparty = new Entity("activityparty");
                    Entity Toparty = new Entity("activityparty");

                    //To set to Contact
                    Toparty["partyid"] = new EntityReference("contact", contact.Id);

                    //From set to User
                    Fromparty["partyid"] = new EntityReference("systemuser", owner.Id);

                    //create email Object and set attributes
                    Entity email = new Entity("email");

                    // retrieve template id's on the basis of template title
                    string templateTitle = "New Contact Template";
                    QueryExpression query = new QueryExpression("template");
                    query.ColumnSet = new ColumnSet(new string[] { "templateid" });
                    query.Criteria.AddCondition("title", ConditionOperator.Equal, templateTitle);

                    EntityCollection collection = service.RetrieveMultiple(query);

                    // get template id
                    Guid templateId = collection.Entities[0].Id;

                    email["from"] = new Entity[] { Fromparty };
                    email["to"] = new Entity[] { Toparty };
                    email["directioncode"] = true;

                    //setting the Regarding as Contact
                    email["regardingobjectid"] = new EntityReference("contact", contact.Id);

                    SendEmailFromTemplateRequest emailUsingTemplateReq = new SendEmailFromTemplateRequest
                    {
                        // The Email Object created
                        Target = email,

                        // The Email Template Id
                        TemplateId = templateId,

                        // Template Regarding Record Id
                        RegardingId = contact.Id,

                        //Template Regarding Record’s Logical Name
                        RegardingType = contact.LogicalName
                    };

                    SendEmailFromTemplateResponse emailUsingTemplateResp = (SendEmailFromTemplateResponse)service.Execute(emailUsingTemplateReq);
                    Guid emailId = emailUsingTemplateResp.Id;
                    tracingService.Trace("Email created with id=" + emailId);
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
