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
    public class DuplicateCheck : IPlugin
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
                    var email = "";

                    if (contact.Attributes.Contains("emailaddress1"))
                    {
                        email = contact["emailaddress1"].ToString();

                        QueryExpression query = new QueryExpression("contact");
                        query.ColumnSet = new ColumnSet(new string[] { "emailaddress1" });
                        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);

                        EntityCollection collection = service.RetrieveMultiple(query);

                        if (collection.Entities.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Contact with the same email address already exist!");
                        }
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


        //public Entity getOpportunityProduct(IOrganizationService service, Guid oppprdId)
        //{
        //    string query = $@"<fetch version=''.0' output-format='xml-platform' mapping='logical' distinct='false'>
        //                      <entity name='opportunityproduct'>
        //                        <attribute name='productid' />
        //                        <attribute name='priceperunit' />
        //                        <attribute name='quantity' />
        //                        <attribute name='extendedamount' />
        //                        <attribute name='opportunityproductid' />
        //                        <attribute name='productdescription' />
        //                        <attribute name='tax' />
        //                        <attribute name='new_producttype' />
        //                        <attribute name='manualdiscountamount' />
        //                        <order attribute='productid' descending='false' />
	       //                     <filter type='and'>
        //                          < condition attribute = 'opportunityproductid' operator= 'eq' value = '' />
        //                            </ filter >
        //                          </ entity >
        //                        </ fetch > ";
        //    EntityCollection oppProduct = service.RetrieveMultiple(new FetchExpression(query));
        //    if (oppProduct != null && oppProduct.Entities.Count > 0)
        //    {
        //        return oppProduct.Entities[0];
        //    }
        //    return null;
        //}
    }
}
