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
    public class CalculateOpportunity : IPlugin
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
                tracingService.Trace("plugin start");
                // Obtain the target entity from the input parameters.
                Entity opportunityProduct = (Entity)context.InputParameters["Target"];
                tracingService.Trace("target entity name : " + opportunityProduct);

                try
                {
                    if (context.MessageName.ToLower() == "create")
                    {
                        decimal extendedAmount = 0;
                        var productType = 0;
                        decimal pricePerUnit = 0;
                        decimal quantity = 1;
                        decimal manualDiscountAmount = 0;
                        decimal tax = 0;

                        EntityReference oppRef = null;
                        if (opportunityProduct.Contains("opportunityid") && opportunityProduct["opportunityid"] != null)
                        {
                            oppRef = (EntityReference)opportunityProduct["opportunityid"];
                            tracingService.Trace("opporyunity Id from target: " + oppRef.Id);
                        }

                        // Get product type value
                        if (opportunityProduct.Contains("new_producttype") && opportunityProduct["new_producttype"] != null)
                        {
                            productType = ((OptionSetValue)opportunityProduct["new_producttype"]).Value;
                            tracingService.Trace("Product type from target: " + productType);
                        }

                        if (productType == 100000000)
                        {
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet = new ColumnSet(new string[] { "priceperunit", "quantity", "manualdiscountamount", "tax" });
                            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, oppRef.Id);
                            query.Criteria.AddCondition("new_producttype", ConditionOperator.Equal, productType);


                            EntityCollection collection = service.RetrieveMultiple(query);
                            if (collection.Entities.Count == 0)
                            {
                                throw new InvalidPluginExecutionException("No opportunity product found of type laptop!");
                            }

                            foreach (var entity in collection.Entities)
                            {
                                // Get price per unit value
                                pricePerUnit = ((Money)entity["priceperunit"]).Value;
                                tracingService.Trace("Price per unit from target: " + pricePerUnit);
                                // Get quantity value
                                quantity = (decimal)entity["quantity"];
                                tracingService.Trace("Quantity from target: " + quantity);
                                // Get manual discount amount value
                                if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = ((Money)entity["manualdiscountamount"]).Value;
                                    tracingService.Trace("Manual discount amount from target: " + manualDiscountAmount);
                                }
                                else
                                {
                                    manualDiscountAmount = 0;
                                    tracingService.Trace("Manual discount amount: " + manualDiscountAmount);
                                }

                                // Get tax value
                                if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = ((Money)entity["tax"]).Value;
                                    tracingService.Trace("Tax from target: " + tax);
                                }
                                else
                                {
                                    tax = 0;
                                    tracingService.Trace("tax: " + tax);
                                }

                                extendedAmount += ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                            }

                            Entity opportunityUpdate = new Entity("opportunity");
                            opportunityUpdate.Id = oppRef.Id;
                            opportunityUpdate["new_totallaptopvalue"] = extendedAmount;
                            tracingService.Trace("Total laptop value after new record is created: " + extendedAmount);

                            service.Update(opportunityUpdate);
                        }
                        else
                        {
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet = new ColumnSet(new string[] { "priceperunit", "quantity", "manualdiscountamount", "tax" });
                            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, oppRef.Id);
                            query.Criteria.AddCondition("new_producttype", ConditionOperator.Equal, productType);

                            EntityCollection collection = service.RetrieveMultiple(query);
                            //if (collection.Entities.Count == 0)
                            //{
                            //    throw new InvalidPluginExecutionException("No opportunity product found of type mobile!");
                            //}

                            foreach (var entity in collection.Entities)
                            {
                                // Get price per unit value
                                pricePerUnit = ((Money)entity["priceperunit"]).Value;
                                tracingService.Trace("Price per unit from target: " + pricePerUnit);
                                // Get quantity value
                                quantity = (decimal)entity["quantity"];
                                tracingService.Trace("Quantity from target: " + quantity);
                                // Get manual discount amount value
                                if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = ((Money)entity["manualdiscountamount"]).Value;
                                    tracingService.Trace("Manual discount amount from target: " + manualDiscountAmount);
                                }
                                else
                                {
                                    manualDiscountAmount = 0;
                                    tracingService.Trace("Manual discount amount: " + manualDiscountAmount);
                                }

                                // Get tax value
                                if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = ((Money)entity["tax"]).Value;
                                    tracingService.Trace("Tax from target: " + tax);
                                }
                                else
                                {
                                    tax = 0;
                                    tracingService.Trace("tax: " + tax);
                                }

                                extendedAmount += ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                            }

                            Entity opportunityUpdate = new Entity("opportunity");
                            opportunityUpdate.Id = oppRef.Id;
                            opportunityUpdate["new_totalmobilevalue"] = extendedAmount;
                            tracingService.Trace("Total mobile value after new record is created: " + extendedAmount);

                            service.Update(opportunityUpdate);
                        }
                    }

                    if (context.MessageName.ToLower() == "update")
                    {
                        Entity preImage = context.PreEntityImages["preImage"];
                        tracingService.Trace("Inside update Message");

                        decimal totalAmount = 0;
                        var productType = 0;
                        decimal pricePerUnit = 0;
                        decimal quantity = 1;
                        decimal manualDiscountAmount = 0;
                        decimal tax = 0;

                        EntityReference oppRef = null;
                        if (opportunityProduct.Contains("opportunityid") && opportunityProduct["opportunityid"] != null)
                        {
                            oppRef = (EntityReference)opportunityProduct["opportunityid"];
                            tracingService.Trace("opporyunity Id from target: " + oppRef.Id);
                        }
                        else
                        {
                            oppRef = preImage.GetAttributeValue<EntityReference>("opportunityid");
                            tracingService.Trace("opportunity id from pre image: " + oppRef.Id);
                        }

                        // Get product type value
                        if (opportunityProduct.Contains("new_producttype") && opportunityProduct["new_producttype"] != null)
                        {
                            productType = ((OptionSetValue)opportunityProduct["new_producttype"]).Value;
                            tracingService.Trace("Product type from target: " + productType);
                        }
                        else
                        {
                            productType = preImage.GetAttributeValue<OptionSetValue>("new_producttype").Value;
                            tracingService.Trace("Product type from pre image: " + productType);
                        }

                        if (productType == 100000000)
                        {
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet = new ColumnSet(new string[] { "priceperunit", "quantity", "manualdiscountamount", "tax" });
                            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, oppRef.Id);
                            query.Criteria.AddCondition("new_producttype", ConditionOperator.Equal, productType);

                            EntityCollection collection = service.RetrieveMultiple(query);
                            if (collection.Entities.Count == 0)
                            {
                                throw new InvalidPluginExecutionException("No opportunity product found of type mobile!");
                            }

                            foreach (var entity in collection.Entities)
                            {
                                // Get price per unit value
                                if (entity.Contains("priceperunit") && entity["priceperunit"] != null)
                                {
                                    pricePerUnit = ((Money)entity["priceperunit"]).Value;
                                    tracingService.Trace("price per unit from target: " + pricePerUnit);
                                }
                                else
                                {
                                    pricePerUnit = preImage.GetAttributeValue<Money>("priceperunit").Value;
                                    tracingService.Trace("Price per unit from pre image: " + pricePerUnit);
                                }

                                // Get quantity value
                                if (entity.Contains("quantity") && entity["quantity"] != null)
                                {
                                    quantity = (decimal)entity["quantity"];
                                    tracingService.Trace("Quantity from target: " + quantity);
                                }
                                else
                                {
                                    quantity = preImage.GetAttributeValue<decimal>("quantity");
                                    tracingService.Trace("Quantity from pre image: " + quantity);
                                }

                                // Get manual discount amount value
                                if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = ((Money)entity["manualdiscountamount"]).Value;
                                    tracingService.Trace("Manual discount amount from target: " + manualDiscountAmount);
                                }
                                else if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = preImage.GetAttributeValue<Money>("manualdiscountamount").Value;
                                    tracingService.Trace("Manual discount amount pre image: " + manualDiscountAmount);
                                }
                                else
                                {
                                    manualDiscountAmount = 0;
                                    tracingService.Trace("Manual discount amount: " + manualDiscountAmount);
                                }

                                // Get tax value
                                if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = ((Money)entity["tax"]).Value;
                                    tracingService.Trace("Tax from target: " + tax);
                                }
                                else if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = preImage.GetAttributeValue<Money>("tax").Value;
                                    tracingService.Trace("Tax pre image: " + tax);
                                }
                                else
                                {
                                    tax = 0;
                                    tracingService.Trace("tax: " + tax);
                                }

                                totalAmount += ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                            }

                            Entity opportunityUpdate = new Entity("opportunity");
                            opportunityUpdate.Id = oppRef.Id;
                            tracingService.Trace("Id: " + opportunityUpdate.Id);

                            tracingService.Trace("Total amount laptop: " + totalAmount);
                            opportunityUpdate["new_totallaptopvalue"] = totalAmount;

                            service.Update(opportunityUpdate);
                        }
                        else if (productType == 100000001)
                        {
                            QueryExpression query = new QueryExpression("opportunityproduct");
                            query.ColumnSet = new ColumnSet(new string[] { "priceperunit", "quantity", "manualdiscountamount", "tax" });
                            query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, oppRef.Id);
                            query.Criteria.AddCondition("new_producttype", ConditionOperator.Equal, productType);

                            EntityCollection collection = service.RetrieveMultiple(query);
                            if (collection.Entities.Count == 0)
                            {
                                throw new InvalidPluginExecutionException("No opportunity product found of type mobile!");
                            }

                            foreach (var entity in collection.Entities)
                            {
                                // Get price per unit value
                                if (entity.Contains("priceperunit") && entity["priceperunit"] != null)
                                {
                                    pricePerUnit = ((Money)entity["priceperunit"]).Value;
                                    tracingService.Trace("price per unit from target: " + pricePerUnit);
                                }
                                else
                                {
                                    pricePerUnit = preImage.GetAttributeValue<Money>("priceperunit").Value;
                                    tracingService.Trace("Price per unit from pre image: " + pricePerUnit);
                                }

                                // Get quantity value
                                if (entity.Contains("quantity") && entity["quantity"] != null)
                                {
                                    quantity = (decimal)entity["quantity"];
                                    tracingService.Trace("Quantity from target: " + quantity);
                                }
                                else
                                {
                                    quantity = preImage.GetAttributeValue<decimal>("quantity");
                                    tracingService.Trace("Quantity from pre image: " + quantity);
                                }

                                // Get manual discount amount value
                                if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = ((Money)entity["manualdiscountamount"]).Value;
                                    tracingService.Trace("Manual discount amount from target: " + manualDiscountAmount);
                                }
                                else if (entity.Contains("manualdiscountamount") && entity["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = preImage.GetAttributeValue<Money>("manualdiscountamount").Value;
                                    tracingService.Trace("Manual discount amount pre image: " + manualDiscountAmount);
                                }
                                else
                                {
                                    manualDiscountAmount = 0;
                                    tracingService.Trace("Manual discount amount: " + manualDiscountAmount);
                                }

                                // Get tax value
                                if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = ((Money)entity["tax"]).Value;
                                    tracingService.Trace("Tax from target: " + tax);
                                }
                                else if (entity.Contains("tax") && entity["tax"] != null)
                                {
                                    tax = preImage.GetAttributeValue<Money>("tax").Value;
                                    tracingService.Trace("Tax pre image: " + tax);
                                }
                                else
                                {
                                    tax = 0;
                                    tracingService.Trace("tax: " + tax);
                                }

                                totalAmount += ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                            }

                            Entity opportunityUpdate = new Entity("opportunity");
                            opportunityUpdate.Id = oppRef.Id;
                            tracingService.Trace("Id: " + opportunityUpdate.Id);

                            tracingService.Trace("Total amount mobile: " + totalAmount);
                            opportunityUpdate["new_totalmobilevalue"] = totalAmount;

                            service.Update(opportunityUpdate);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("Please select valid product type!");
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
            else
            {
                try
                {
                    if (context.MessageName.ToLower() == "delete")
                    {
                        EntityReference opportunityProduct = (EntityReference)context.InputParameters["Target"];
                        tracingService.Trace("Inside delete message");
                        tracingService.Trace("opporyunity Id from target: " + opportunityProduct.Id);

                        if (opportunityProduct.LogicalName == "opportunityproduct")
                        {
                            tracingService.Trace("Entity name: " + opportunityProduct.LogicalName);
                            decimal totalAmount = 0;
                            var productType = 0;
                            decimal pricePerUnit = 0;
                            decimal quantity = 1;
                            decimal manualDiscountAmount = 0;
                            decimal tax = 0;

                            tracingService.Trace("Just before retrieval");
                            // Retrieve data of deleted record
                            Entity deletedProductRecord = service.Retrieve(opportunityProduct.LogicalName, opportunityProduct.Id, new ColumnSet(new string[] { "new_producttype", "priceperunit", "quantity", "manualdiscountamount", "tax", "opportunityid" }));
                            tracingService.Trace("After retrieval");

                            // Get the opportunity id
                            EntityReference oppRef = null;
                            if (deletedProductRecord.Contains("opportunityid") && deletedProductRecord["opportunityid"] != null)
                            {
                                oppRef = (EntityReference)deletedProductRecord["opportunityid"];
                                tracingService.Trace("opporyunity Id from target: " + oppRef.Id);
                            }

                            // Get the required attributes
                            if (deletedProductRecord.Attributes.Contains("new_producttype") && deletedProductRecord["new_producttype"] != null)
                            {
                                productType = deletedProductRecord.GetAttributeValue<OptionSetValue>("new_producttype").Value;
                                tracingService.Trace("Product type from target: " + productType);
                            }

                            if (productType == 100000000)
                            {
                                pricePerUnit = deletedProductRecord.GetAttributeValue<Money>("priceperunit").Value;
                                quantity = deletedProductRecord.GetAttributeValue<decimal>("quantity");
                                if (deletedProductRecord.Attributes.Contains("manualdiscountamount") && deletedProductRecord["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = deletedProductRecord.GetAttributeValue<Money>("manualdiscountamount").Value;
                                }
                                if (deletedProductRecord.Attributes.Contains("tax") && deletedProductRecord["tax"] != null)
                                {
                                    tax = deletedProductRecord.GetAttributeValue<Money>("tax").Value;
                                }

                                totalAmount = ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                                tracingService.Trace("Total amount: " + totalAmount);

                                Entity opportunityRecord = service.Retrieve(oppRef.LogicalName, oppRef.Id, new ColumnSet(new string[] { "new_totallaptopvalue" }));

                                // Get total laptop value
                                decimal totalLaptopValue = opportunityRecord.GetAttributeValue<Money>("new_totallaptopvalue").Value;

                                Entity opportunityUpdate = new Entity("opportunity");
                                opportunityUpdate.Id = oppRef.Id;
                                tracingService.Trace("Total laptop value before deletion of record: " + totalLaptopValue);
                                // Update total laptop value
                                opportunityUpdate["new_totallaptopvalue"] = totalLaptopValue - totalAmount;
                                tracingService.Trace("Total Laptop Value after deletion of record: " + (totalLaptopValue - totalAmount));

                                service.Update(opportunityUpdate);
                            }
                            else
                            {
                                pricePerUnit = deletedProductRecord.GetAttributeValue<Money>("priceperunit").Value;
                                quantity = deletedProductRecord.GetAttributeValue<decimal>("quantity");
                                if (deletedProductRecord.Attributes.Contains("manualdiscountamount") && deletedProductRecord["manualdiscountamount"] != null)
                                {
                                    manualDiscountAmount = deletedProductRecord.GetAttributeValue<Money>("manualdiscountamount").Value;
                                }
                                if (deletedProductRecord.Attributes.Contains("tax") && deletedProductRecord["tax"] != null)
                                {
                                    tax = deletedProductRecord.GetAttributeValue<Money>("tax").Value;
                                }

                                totalAmount = ((pricePerUnit * quantity) - manualDiscountAmount) + tax;
                                tracingService.Trace("Total amount: " + totalAmount);

                                Entity opportunityRecord = service.Retrieve(oppRef.LogicalName, oppRef.Id, new ColumnSet(new string[] { "new_totalmobilevalue" }));

                                // Get total mobile value
                                decimal totalMobileValue = opportunityRecord.GetAttributeValue<Money>("new_totalmobilevalue").Value;

                                Entity opportunityUpdate = new Entity("opportunity");
                                opportunityUpdate.Id = oppRef.Id;
                                tracingService.Trace("Total mobile value before deletion of record: " + totalMobileValue);
                                // Update total mobile value
                                opportunityUpdate["new_totalmobilevalue"] = totalMobileValue - totalAmount;
                                tracingService.Trace("Total mobile value after deletion of record: " + (totalMobileValue - totalAmount));

                                service.Update(opportunityUpdate);
                            }
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
