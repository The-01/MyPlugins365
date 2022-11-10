using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PluginTasks
{
    public class UpdateContactInfo : IPlugin
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
                Entity contactInfo = (Entity)context.InputParameters["Target"];

                try
                {
                    if (context.MessageName.ToLower() == "create")
                    {
                        tracingService.Trace("Inside Create Message");

                        var customerEmail = "";
                        var customerPhone = "";

                        // Get customer lookup
                        EntityReference contactRef = (EntityReference)contactInfo["new_customer"];
                        tracingService.Trace("Contact id from target: " + contactRef.Id);

                        // Get contact method value
                        var contactMethod = contactInfo.GetAttributeValue<OptionSetValue>("new_contactmethod").Value;

                        // Retrieve contact record
                        Entity contactRecord = service.Retrieve(contactRef.LogicalName, contactRef.Id, new ColumnSet(new string[] { "emailaddress1", "telephone1" }));


                        // Update the contact information entity
                        Entity contactInfoUpdate = new Entity(contactInfo.LogicalName);
                        contactInfoUpdate.Id = context.PrimaryEntityId;

                        // Check for contact method (email=100000000, phone=100000001)
                        if (contactMethod == 100000000)
                        {
                            // Update the customer email field value
                            customerEmail = contactRecord.GetAttributeValue<string>("emailaddress1");
                            contactInfoUpdate["new_customeremail"] = customerEmail;
                        }
                        else
                        {
                            // Update the customer phone field value
                            customerPhone = contactRecord.GetAttributeValue<string>("telephone1");
                            contactInfoUpdate["new_customerphone"] = customerPhone;
                        }

                        tracingService.Trace("Email/Phone field updated (on create of contact info) successfully!");
                        service.Update(contactInfoUpdate);
                    }

                    if (context.MessageName.ToLower() == "update")
                    {
                        tracingService.Trace("Inside Update Message");

                        var customerEmail = "";
                        var customerPhone = "";

                        // To get pre images values
                        Entity preImage = context.PreEntityImages["preImage"];

                        // Get customer lookup
                        EntityReference contactRef = null;
                        if (contactInfo.Contains("new_customer") && contactInfo["new_customer"] != null)
                        {
                            contactRef = (EntityReference)contactInfo["new_customer"];
                            tracingService.Trace("Contact id from target: " + contactRef.Id);
                        }
                        else
                        {
                            contactRef = preImage.GetAttributeValue<EntityReference>("new_customer");
                            tracingService.Trace("Contact id from pre image: " + contactRef.Id);
                        }

                        var contactMethod = 0;
                        if (contactInfo.Contains("new_contactmethod") && contactInfo["new_contactmethod"] != null)
                        {
                            // Contact method value after modification
                            contactMethod = contactInfo.GetAttributeValue<OptionSetValue>("new_contactmethod").Value;
                            tracingService.Trace("Contact method from target: " + contactMethod);
                        }
                        else
                        {
                            // Contact method value before modification
                            contactMethod = preImage.GetAttributeValue<OptionSetValue>("new_contactmethod").Value;
                            tracingService.Trace("Contact method from pre image: " + contactMethod);
                        }

                        // Retrieve contact record based on new lookup value
                        Entity contactRecord = service.Retrieve(contactRef.LogicalName, contactRef.Id, new ColumnSet(new string[] { "emailaddress1", "telephone1" }));

                        // Update the contact information entity
                        Entity contactInfoUpdate = new Entity(contactInfo.LogicalName);
                        contactInfoUpdate.Id = context.PrimaryEntityId;


                        // Check for contact method (email=100000000, phone=100000001)
                        if (contactMethod == 100000000)
                        {
                            // Get and update customer email value
                            if (contactRecord.Contains("emailaddress1") && contactRecord["emailaddress1"] != null)
                            {
                                customerEmail = contactRecord.GetAttributeValue<string>("emailaddress1");
                                tracingService.Trace("Customer email: " + customerEmail);
                            }

                            // Update the customer email field value. Similarly remove the existing customer phone field value if any
                            contactInfoUpdate["new_customeremail"] = customerEmail;
                            contactInfoUpdate["new_customerphone"] = "";
                        }
                        else
                        {
                            // Get and update customer phone value
                            if (contactRecord.Contains("telephone1") && contactRecord["telephone1"] != null)
                            {
                                customerPhone = contactRecord.GetAttributeValue<string>("telephone1");
                                tracingService.Trace("Customer phone: " + customerPhone);
                            }

                            // Update the customer phone field value. Similarly remove the existing customer email field value if any
                            contactInfoUpdate["new_customerphone"] = customerPhone;
                            contactInfoUpdate["new_customeremail"] = "";
                        }

                        tracingService.Trace("Email/Phone field updated (on update of contact info) successfully!");
                        service.Update(contactInfoUpdate);
                    }
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
