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
    public class UpdateStatus : IPlugin
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


            if (context.Depth > 1)
            {
                return;
            }
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                tracingService.Trace("Plugin start...");
                // Obtain the target entity from the input parameters.
                Entity status = (Entity)context.InputParameters["Target"];
                tracingService.Trace("Target entity name: " + status.LogicalName);

                try
                {
                    if (context.MessageName.ToLower() == "create")
                    {
                        tracingService.Trace("Inside Create Message...");
                        var statusDefault = false;

                        // Retrieve all status records
                        QueryExpression query = new QueryExpression(status.LogicalName);
                        query.ColumnSet = new ColumnSet(new string[] { "new_default" });

                        EntityCollection collection = service.RetrieveMultiple(query);

                        // Check for the total number of records
                        if (collection.Entities.Count == 0)
                        {
                            throw new InvalidPluginExecutionException("No sale product found!");
                        }
                        else if (collection.Entities.Count == 1)
                        {
                            tracingService.Trace("The very first record");
                            statusDefault = true;
                            Entity statusUpdate = new Entity(status.LogicalName);
                            statusUpdate.Id = context.PrimaryEntityId;
                            statusUpdate["new_default"] = statusDefault;

                            service.Update(statusUpdate);
                        }
                        else
                        {
                            //foreach (var entity in collection.Entities)
                            for (int i = collection.Entities.Count - 1; i >= 0; i--)
                            {
                                if (i == collection.Entities.Count - 1)
                                {
                                    if (collection.Entities[i].GetAttributeValue<bool>("new_default").Equals(true))
                                    {
                                        statusDefault = true;

                                        Entity statusUpdate = new Entity(status.LogicalName);
                                        statusUpdate.Id = collection.Entities[i].Id;
                                        tracingService.Trace("Record id from if: " + statusUpdate.Id.ToString());
                                        statusUpdate["new_default"] = statusDefault;

                                        service.Update(statusUpdate);
                                    }
                                    else
                                    {
                                        statusDefault = false;

                                        Entity statusUpdate = new Entity(status.LogicalName);
                                        statusUpdate.Id = collection.Entities[i].Id;
                                        tracingService.Trace("Record id from if: " + statusUpdate.Id.ToString());
                                        statusUpdate["new_default"] = statusDefault;

                                        service.Update(statusUpdate);

                                        break;
                                    }
                                }
                                else
                                {
                                    statusDefault = false;
                                    Entity statusUpdate = new Entity(status.LogicalName);
                                    statusUpdate.Id = collection.Entities[i].Id;
                                    tracingService.Trace("Record id from else: " + statusUpdate.Id.ToString());
                                    statusUpdate["new_default"] = statusDefault;

                                    service.Update(statusUpdate);
                                }
                            }
                        }
                    }

                    if (context.MessageName.ToLower() == "update")
                    {
                        tracingService.Trace("Inside Update Message");
                        var statusDefault = status.GetAttributeValue<bool>("new_default");
                        tracingService.Trace("statusDefault: "+statusDefault);
                        Guid currentStatusId = status.Id;

                        if (statusDefault == false)
                        {
                            throw new InvalidPluginExecutionException("Default status cannot be changed from true to false!");
                        }
                        else
                        {
                            statusDefault = true;
                        }

                        // Retrieve all status records except the current one
                        QueryExpression query = new QueryExpression(status.LogicalName);
                        query.ColumnSet = new ColumnSet(new string[] { "new_default" });
                        query.Criteria.AddCondition("new_statusid", ConditionOperator.NotEqual, currentStatusId);

                        EntityCollection collection = service.RetrieveMultiple(query);

                        if (collection.Entities.Count == 0)
                        {
                            throw new InvalidPluginExecutionException("No status record found!");
                        }

                        tracingService.Trace(context.Depth.ToString());
                        //throw new InvalidPluginExecutionException(collection.Entities.Count.ToString());
                        Entity statusUpdate = new Entity(status.LogicalName);
                        
                        foreach (var entity in collection.Entities)
                        {
                            tracingService.Trace("Default status of id=" + entity.Id + " will be updated to false");
                            statusUpdate.Id = entity.Id;
                            statusUpdate["new_default"] = false;
                            service.Update(statusUpdate);
                        }
                    }
                }
                catch (FaultException ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin." + ex.Message);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                try
                {
                    if (context.MessageName.ToLower() == "delete")
                    {
                        var statusDefault = false;

                        EntityReference status = (EntityReference)context.InputParameters["Target"];
                        tracingService.Trace("Inside delete message...");
                        tracingService.Trace("Status record id from target: " + status.Id);

                        // Retrieve data of status record to be delete
                        Entity statusRecord = service.Retrieve(status.LogicalName, status.Id, new ColumnSet(new string[] { "new_default" }));

                        statusDefault = statusRecord.GetAttributeValue<bool>("new_default");

                        if (statusDefault == true)
                        {
                            throw new InvalidPluginExecutionException("Default true status cannot be deleted!");
                        }
                        else
                        {
                            tracingService.Trace("Status record having id=" + status.Id + " has been deleted!");
                        }
                    }
                }
                catch (FaultException ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
