using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTasks
{
    public class CopyAccountAction : IPlugin
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
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
                try
                {
                    tracingService.Trace("inside try");
                    // Get current account id from custom action
                    var currentAccountId = context.InputParameters["CurrentAccountId"].ToString();
                    Guid accountId = new Guid(currentAccountId);
                    tracingService.Trace("After accountid");
                    //throw new InvalidPluginExecutionException(accountId.ToString());
                    // Get current account data
                    Entity accountRecord = GetAccountRecord(accountId, service);

                    // Create new copy account from the retrieved account data
                    Entity newAccountRecord = new Entity("account");

                    // Get account name
                    if (accountRecord.Contains("name") && accountRecord["name"] != null)
                    {
                        newAccountRecord["name"] = "Copy(Custom Action) - " + accountRecord["name"];
                        tracingService.Trace("Copy(Custom Action) - " + accountRecord["name"]);
                    }
                    // Get email address
                    if (accountRecord.Contains("emailaddress1") && accountRecord["emailaddress1"] != null)
                    {
                        newAccountRecord["emailaddress1"] = accountRecord["emailaddress1"];
                    }
                    // Get phone number
                    if (accountRecord.Contains("telephone1") && accountRecord["telephone1"] != null)
                    {
                        newAccountRecord["telephone1"] = accountRecord["telephone1"];
                    }
                    // Get fax number
                    if (accountRecord.Contains("fax") && accountRecord["fax"] != null)
                    {
                        newAccountRecord["fax"] = accountRecord["fax"];
                    }
                    // Get state i.e KP
                    if (accountRecord.Contains("new_state") && accountRecord["new_state"] != null)
                    {
                        newAccountRecord["new_state"] = accountRecord["new_state"];
                    }
                    // Get website url
                    if (accountRecord.Contains("websiteurl") && accountRecord["websiteurl"] != null)
                    {
                        newAccountRecord["websiteurl"] = accountRecord["websiteurl"];
                    }
                    // Get parent account
                    if (accountRecord.Contains("parentaccountid") && accountRecord["parentaccountid"] != null)
                    {
                        newAccountRecord["parentaccountid"] = accountRecord["parentaccountid"];
                    }
                    // Get ticker symbol
                    if (accountRecord.Contains("tickersymbol") && accountRecord["tickersymbol"] != null)
                    {
                        newAccountRecord["tickersymbol"] = accountRecord["tickersymbol"];
                    }
                    // Get revenue
                    if (accountRecord.Contains("revenue") && accountRecord["revenue"] != null)
                    {
                        newAccountRecord["revenue"] = accountRecord["revenue"];
                    }
                    // Get number of employees
                    if (accountRecord.Contains("numberofemployees") && accountRecord["numberofemployees"] != null)
                    {
                        newAccountRecord["numberofemployees"] = accountRecord["numberofemployees"];
                    }
                    // Get account owner
                    if (accountRecord.Contains("ownerid") && accountRecord["ownerid"] != null)
                    {
                        newAccountRecord["ownerid"] = accountRecord["ownerid"];
                    }
                    // Get address1 street1
                    if (accountRecord.Contains("address1_line1") && accountRecord["address1_line1"] != null)
                    {
                        newAccountRecord["address1_line1"] = accountRecord["address1_line1"];
                    }
                    // Get address1 street2
                    if (accountRecord.Contains("address1_line2") && accountRecord["address1_line2"] != null)
                    {
                        newAccountRecord["address1_line2"] = accountRecord["address1_line2"];
                    }
                    // Get address1 street3
                    if (accountRecord.Contains("address1_line3") && accountRecord["address1_line3"] != null)
                    {
                        newAccountRecord["address1_line3"] = accountRecord["address1_line3"];
                    }
                    // Get address1 city
                    if (accountRecord.Contains("address1_city") && accountRecord["address1_city"] != null)
                    {
                        newAccountRecord["address1_city"] = accountRecord["address1_city"];
                    }
                    // Get address1 state/province
                    if (accountRecord.Contains("address1_stateorprovince") && accountRecord["address1_stateorprovince"] != null)
                    {
                        newAccountRecord["address1_stateorprovince"] = accountRecord["address1_stateorprovince"];
                    }
                    // Get address1 postal code
                    if (accountRecord.Contains("address1_postalcode") && accountRecord["address1_postalcode"] != null)
                    {
                        newAccountRecord["address1_postalcode"] = accountRecord["address1_postalcode"];
                    }
                    // Get address1 country
                    if (accountRecord.Contains("address1_country") && accountRecord["address1_country"] != null)
                    {
                        newAccountRecord["address1_country"] = accountRecord["address1_country"];
                    }
                    // Get international customer (yes/no)
                    if (accountRecord.Contains("new_internationalcustomer") && accountRecord["new_internationalcustomer"] != null)
                    {
                        newAccountRecord["new_internationalcustomer"] = accountRecord["new_internationalcustomer"];
                    }
                    // Get booking country i.e Pakistan
                    if (accountRecord.Contains("new_bookingcountry") && accountRecord["new_bookingcountry"] != null)
                    {
                        newAccountRecord["new_bookingcountry"] = accountRecord["new_bookingcountry"];
                    }
                    // Get booking allowed (yes/no)
                    if (accountRecord.Contains("new_bookingallowed") && accountRecord["new_bookingallowed"] != null)
                    {
                        newAccountRecord["new_bookingallowed"] = accountRecord["new_bookingallowed"];
                    }

                    //// Get account name
                    //newAccountRecord["name"] = (accountRecord.Contains("name") && accountRecord["name"] != null) ? "Copy(Custom Action) - " + accountRecord["name"] : "";
                    //tracingService.Trace("Copy(Custom Action) - " + accountRecord["name"]);
                    //// Get email address
                    //newAccountRecord["emailaddress1"] = (accountRecord.Contains("emailaddress1") && accountRecord["emailaddress1"] != null) ? accountRecord["emailaddress1"] : "";
                    //// Get phone number
                    //newAccountRecord["telephone1"] = (accountRecord.Contains("telephone1") && accountRecord["telephone1"] != null) ? accountRecord["telephone1"] : "";
                    //// Get fax number
                    //newAccountRecord["fax"] = (accountRecord.Contains("fax") && accountRecord["fax"] != null) ? accountRecord["fax"] : "";
                    //// Get state i.e KP
                    //newAccountRecord["new_state"] = (accountRecord.Contains("new_state") && accountRecord["new_state"] != null) ? accountRecord["new_state"] : null;
                    //// Get website url
                    //newAccountRecord["websiteurl"] = (accountRecord.Contains("websiteurl") && accountRecord["websiteurl"] != null) ? accountRecord["websiteurl"] : "";
                    //// Get parent account
                    //newAccountRecord["parentaccountid"] = (accountRecord.Contains("parentaccountid") && accountRecord["parentaccountid"] != null) ? accountRecord["parentaccountid"] : null;
                    //// Get ticker symbol
                    //newAccountRecord["tickersymbol"] = (accountRecord.Contains("tickersymbol") && accountRecord["tickersymbol"] != null) ? accountRecord["tickersymbol"] : "";
                    //// Get revenue
                    //newAccountRecord["revenue"] = (accountRecord.Contains("revenue") && accountRecord["revenue"] != null) ? accountRecord["revenue"] : null;
                    //// Get number of employees
                    //newAccountRecord["numberofemployees"] = (accountRecord.Contains("numberofemployees") && accountRecord["numberofemployees"] != null) ? accountRecord["numberofemployees"] : null;
                    //// Get account owner
                    //newAccountRecord["ownerid"] = (accountRecord.Contains("ownerid") && accountRecord["ownerid"] != null) ? accountRecord["ownerid"] : null;
                    //// Get address1 street1
                    //newAccountRecord["address1_line1"] = (accountRecord.Contains("address1_line1") && accountRecord["address1_line1"] != null) ? accountRecord["address1_line1"] : "";
                    //// Get address1 street2
                    //newAccountRecord["address1_line2"] = (accountRecord.Contains("address1_line2") && accountRecord["address1_line2"] != null) ? accountRecord["address1_line2"] : "";
                    //// Get address1 street3
                    //newAccountRecord["address1_line3"] = (accountRecord.Contains("address1_line3") && accountRecord["address1_line3"] != null) ? accountRecord["address1_line3"] : "";
                    //// Get address1 city
                    //newAccountRecord["address1_city"] = (accountRecord.Contains("address1_stateorprovince") && accountRecord["address1_stateorprovince"] != null) ? accountRecord["address1_city"] : "";
                    //// Get address1 state/province
                    //newAccountRecord["address1_stateorprovince"] = (accountRecord.Contains("name") && accountRecord["name"] != null) ? accountRecord["address1_stateorprovince"] : "";
                    //// Get address1 postal code
                    //newAccountRecord["address1_postalcode"] = (accountRecord.Contains("address1_postalcode") && accountRecord["address1_postalcode"] != null) ? accountRecord["address1_postalcode"] : "";
                    //// Get address1 country
                    //newAccountRecord["address1_country"] = (accountRecord.Contains("address1_country") && accountRecord["address1_country"] != null) ? accountRecord["address1_country"] : "";
                    //// Get international customer (yes/no)
                    //newAccountRecord["new_internationalcustomer"] = (accountRecord.Contains("new_internationalcustomer") && accountRecord["new_internationalcustomer"] != null) ? accountRecord["new_internationalcustomer"] : false;
                    //// Get booking country i.e Pakistan
                    //newAccountRecord["new_bookingcountry"] = (accountRecord.Contains("new_bookingcountry") && accountRecord["new_bookingcountry"] != null) ? accountRecord["new_bookingcountry"] : null;
                    //// Get booking allowed (yes/no)
                    //newAccountRecord["new_bookingallowed"] = (accountRecord.Contains("new_bookingallowed") && accountRecord["new_bookingallowed"] != null) ? accountRecord["new_bookingallowed"] : false;


                    var newAccountId = service.Create(newAccountRecord);
                    tracingService.Trace("New copy account created (custom action)");
                    tracingService.Trace(newAccountId.ToString());

                    // Supply the newly created account id to custom action as output parameter
                    context.OutputParameters["NewAccountId"] = newAccountId;
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("Error occured in [CustomActionPlugin]:" + ex.Message, ex);
                }
            }
        }


        private Entity GetAccountRecord(Guid accountId, IOrganizationService service)
        {
            // Retrieve current account details
            Entity accountRecord = service.Retrieve("account", accountId, new ColumnSet(new string[] { "emailaddress1", "address1_city", "new_bookingcountry", "websiteurl", "address1_line3", "numberofemployees", "revenue", "new_bookingallowed", "parentaccountid", "address1_line1", "name", "address1_country", "address1_stateorprovince", "new_state", "fax", "ownerid", "address1_line2", "address1_postalcode", "tickersymbol", "new_internationalcustomer", "telephone1" }));

            return accountRecord;
        }
    }
}
