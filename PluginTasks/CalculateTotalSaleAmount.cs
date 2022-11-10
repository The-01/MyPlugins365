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
    public class CalculateTotalSaleAmount : IPlugin
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
                tracingService.Trace("Plugin start...");
                // Obtain the target entity from the input parameters.
                Entity saleDetail = (Entity)context.InputParameters["Target"];
                tracingService.Trace("Target entity name: " + saleDetail.LogicalName);

                try
                {
                    if (context.MessageName.ToLower() == "create")
                    {
                        decimal totalSaleAmount = 0;
                        int quantity = 0;
                        decimal unitPrice = 0;

                        EntityReference saleRef = null;
                        if (saleDetail.Contains("new_sale") && saleDetail["new_sale"] != null)
                        {
                            saleRef = (EntityReference)saleDetail["new_sale"];
                            tracingService.Trace("Sale id from target: " + saleRef.Id);
                        }

                        QueryExpression query = new QueryExpression(saleDetail.LogicalName);
                        query.ColumnSet = new ColumnSet(new string[] { "new_quantity", "new_unitprice" });
                        query.Criteria.AddCondition("new_sale", ConditionOperator.Equal, saleRef.Id);

                        EntityCollection collection = service.RetrieveMultiple(query);
                        if (collection.Entities.Count == 0)
                        {
                            throw new InvalidPluginExecutionException("No sale product found!");
                        }

                        foreach (var entity in collection.Entities)
                        {
                            // Get quantity value
                            quantity = (int)entity["new_quantity"];
                            tracingService.Trace("quantity from target: " + quantity);
                            // Get unit price value
                            unitPrice = ((Money)entity["new_unitprice"]).Value;
                            tracingService.Trace("Unit price from target: " + unitPrice);

                            totalSaleAmount += quantity * unitPrice;
                        }

                        Entity saleUpdate = new Entity(saleRef.LogicalName);
                        saleUpdate.Id = saleRef.Id;
                        saleUpdate["new_totalsaleamount"] = totalSaleAmount;
                        tracingService.Trace("Total sale amount after new record is created: " + totalSaleAmount);

                        service.Update(saleUpdate);
                    }

                    if (context.MessageName.ToLower() == "update")
                    {
                        Entity preImage = context.PreEntityImages["preImage"];
                        tracingService.Trace("Inside update Message");

                        decimal totalSaleAmount = 0;
                        decimal _totalSaleAmount = 0;
                        int quantity = 0;
                        decimal unitPrice = 0;

                        EntityReference saleRef = null;
                        EntityReference _saleRef = null;
                        if (saleDetail.Contains("new_sale") && saleDetail["new_sale"] != null)
                        {
                            // Lookup value after modification
                            saleRef = (EntityReference)saleDetail["new_sale"];
                            tracingService.Trace("Sale id from target: " + saleRef.Id);
                            // Lookup value before modification
                            _saleRef = preImage.GetAttributeValue<EntityReference>("new_sale");
                            tracingService.Trace("Sale id from pre image: " + _saleRef.Id) ;
                        }
                        else
                        {
                            // Lookup value after modification
                            saleRef = preImage.GetAttributeValue<EntityReference>("new_sale");
                            tracingService.Trace("Sale id from pre image: " + saleRef.Id);
                            // Lookup value before modification
                            _saleRef = preImage.GetAttributeValue<EntityReference>("new_sale");
                            tracingService.Trace("Sale id from pre image: " + _saleRef.Id);
                        }

                        // Updating the total sale amount based on new lookup value
                        tracingService.Trace("Updating the total sale amount based on new lookup value");
                        QueryExpression query = new QueryExpression(saleDetail.LogicalName);
                        query.ColumnSet = new ColumnSet(new string[] { "new_quantity", "new_unitprice" });
                        query.Criteria.AddCondition("new_sale", ConditionOperator.Equal, saleRef.Id);

                        EntityCollection collection = service.RetrieveMultiple(query);
                        if (collection.Entities.Count == 0)
                        {
                            throw new InvalidPluginExecutionException("No sale product found!");
                        }

                        foreach (var entity in collection.Entities)
                        {
                            // Get quantity value
                            if (entity.Contains("new_quantity") && entity["new_quantity"] != null)
                            {
                                quantity = (int)entity["new_quantity"];
                                tracingService.Trace("Quantity from target: " + quantity);
                            }
                            else
                            {
                                quantity = preImage.GetAttributeValue<int>("new_quantity");
                                tracingService.Trace("Quantity from pre image: " + quantity);
                            }

                            // Get unit price value
                            if (entity.Contains("new_unitprice") && entity["new_unitprice"] != null)
                            {
                                unitPrice = ((Money)entity["new_unitprice"]).Value;
                                tracingService.Trace("Unit price from target: " + unitPrice);
                            }
                            else
                            {
                                unitPrice = preImage.GetAttributeValue<Money>("new_unitprice").Value;
                                tracingService.Trace("Unit price from pre image: " + unitPrice);
                            }

                            totalSaleAmount += quantity * unitPrice;
                        }

                        Entity saleUpdate = new Entity(saleRef.LogicalName);
                        saleUpdate.Id = saleRef.Id;
                        saleUpdate["new_totalsaleamount"] = totalSaleAmount;

                        service.Update(saleUpdate);

                        // Updating the total sale amount based on old lookup value
                        tracingService.Trace("Updating the total sale amount based on old lookup value");
                        QueryExpression _query = new QueryExpression(saleDetail.LogicalName);
                        _query.ColumnSet = new ColumnSet(new string[] { "new_quantity", "new_unitprice" });
                        _query.Criteria.AddCondition("new_sale", ConditionOperator.Equal, _saleRef.Id);
                        EntityCollection _collection = service.RetrieveMultiple(_query);
                        if (_collection.Entities.Count == 0)
                        {
                            throw new InvalidPluginExecutionException("No sale product found!");
                        }

                        foreach (var entity in _collection.Entities)
                        {
                            // Get quantity value
                            if (entity.Contains("new_quantity") && entity["new_quantity"] != null)
                            {
                                quantity = (int)entity["new_quantity"];
                                tracingService.Trace("Quantity from target: " + quantity);
                            }
                            else
                            {
                                quantity = preImage.GetAttributeValue<int>("new_quantity");
                                tracingService.Trace("Quantity from pre image: " + quantity);
                            }

                            // Get unit price value
                            if (entity.Contains("new_unitprice") && entity["new_unitprice"] != null)
                            {
                                unitPrice = ((Money)entity["new_unitprice"]).Value;
                                tracingService.Trace("Unit price from target: " + unitPrice);
                            }
                            else
                            {
                                unitPrice = preImage.GetAttributeValue<Money>("new_unitprice").Value;
                                tracingService.Trace("Unit price from pre image: " + unitPrice);
                            }

                            _totalSaleAmount += quantity * unitPrice;
                        }

                        Entity _saleUpdate = new Entity(_saleRef.LogicalName);
                        _saleUpdate.Id = _saleRef.Id;
                        _saleUpdate["new_totalsaleamount"] = _totalSaleAmount;

                        service.Update(_saleUpdate);
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
            else
            {
                try
                {
                    if (context.MessageName.ToLower() == "delete")
                    {
                        EntityReference saleDetail = (EntityReference)context.InputParameters["Target"];
                        tracingService.Trace("Inside delete message");
                        tracingService.Trace("Sale detail id from target: " + saleDetail.Id);

                        decimal amount = 0;
                        decimal totalSaleAount = 0;
                        var quantity = 0;
                        decimal unitPrice = 0;

                        // Retrieve data of deleted record
                        Entity deletedSaleRecord = service.Retrieve(saleDetail.LogicalName, saleDetail.Id, new ColumnSet(new string[] { "new_quantity", "new_unitprice", "new_sale" }));

                        // Get the sale id
                        EntityReference saleRef = null;
                        if (deletedSaleRecord.Contains("new_sale") && deletedSaleRecord["new_sale"] != null)
                        {
                            saleRef = (EntityReference)deletedSaleRecord["new_sale"];
                            tracingService.Trace("Sale id from target: " + saleRef.Id);
                        }

                        quantity = deletedSaleRecord.GetAttributeValue<int>("new_quantity");
                        unitPrice = deletedSaleRecord.GetAttributeValue<Money>("new_unitprice").Value;

                        amount = quantity * unitPrice;

                        Entity saleRecord = service.Retrieve(saleRef.LogicalName, saleRef.Id, new ColumnSet(new string[] { "new_totalsaleamount" }));

                        // Get total sale amount
                        totalSaleAount = saleRecord.GetAttributeValue<Money>("new_totalsaleamount").Value;

                        Entity saleUpdate = new Entity(saleRef.LogicalName);
                        saleUpdate.Id = saleRef.Id;
                        // Update total sale amount
                        saleUpdate["new_totalsaleamount"] = totalSaleAount - amount;

                        service.Update(saleUpdate);
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
