using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;

namespace PluginTasks
{
    public class GrantAccessToAccountRelatedContacts :IPlugin
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
                    if (context.MessageName == "GrantAccess")
                    {
                        // Obtain the target entity from the input parameter
                        EntityReference accountRef = (EntityReference)context.InputParameters["Target"];

                        // Obtain the principal access object from the input parameter
                        PrincipalAccess principalAccess = (PrincipalAccess)context.InputParameters["PrincipalAccess"];

                        var userOrTeam = principalAccess.Principal;
                        var userOrTeamId = userOrTeam.Id;
                        var userOrTeamName = userOrTeam.Name;
                        var userOrTeamLogicalName = userOrTeam.LogicalName;

                        // Call the GetContacts() function to retrieve all account related contacts
                        EntityCollection contacts = GetContacts(accountRef.Id, service);

                        foreach (var contact in contacts.Entities)
                        {
                            Guid contactId = (Guid)contact["contactid"];

                            // Use the logical Name to know whether this is User or Team!
                            if (userOrTeamLogicalName == "team")
                            {
                                var createdReference = new EntityReference("team", userOrTeam.Id);
                                var accessMask = principalAccess.AccessMask;

                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        // To get the Principal Access
                                        AccessMask = accessMask,
                                        Principal = createdReference
                                    },
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(userOrTeamId.ToString());
                                tracingService.Trace(userOrTeamLogicalName);
                                tracingService.Trace(accessMask.ToString());

                                service.Execute(grantAccessRequest);
                            }

                            if (userOrTeamLogicalName == "systemuser")
                            {
                                var createdReference = new EntityReference("systemuser", userOrTeam.Id);
                                var accessMask = principalAccess.AccessMask;

                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        // To get the Principal Access
                                        AccessMask = accessMask,
                                        Principal = createdReference
                                    },
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(userOrTeamId.ToString());
                                tracingService.Trace(userOrTeamLogicalName);
                                tracingService.Trace(accessMask.ToString());

                                service.Execute(grantAccessRequest);
                            }
                        }
                    }

                    else if (context.MessageName == "ModifyAccess")
                    {
                        // Obtain the target entity from the input parameter
                        EntityReference accountRef = (EntityReference)context.InputParameters["Target"];

                        // Obtain the principal access object from the input parameter
                        PrincipalAccess principalAccess = (PrincipalAccess)context.InputParameters["PrincipalAccess"];

                        var userOrTeam = principalAccess.Principal;
                        var userOrTeamId = userOrTeam.Id;
                        var userOrTeamName = userOrTeam.Name;
                        var userOrTeamLogicalName = userOrTeam.LogicalName;

                        // Call GetContacts function to retrieve all account related contacts
                        EntityCollection contacts = GetContacts(accountRef.Id, service);

                        foreach (var contact in contacts.Entities)
                        {
                            Guid contactId = (Guid)contact["contactid"];

                            // Use the logical Name to know whether this is User or Team!
                            if (userOrTeamLogicalName == "team")
                            {
                                var createdReference = new EntityReference("team", userOrTeam.Id);
                                var accessMask = principalAccess.AccessMask;

                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        // To get the Principal Access
                                        AccessMask = accessMask,
                                        Principal = createdReference
                                    },
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(userOrTeamId.ToString());
                                tracingService.Trace(userOrTeamLogicalName);
                                tracingService.Trace(accessMask.ToString());

                                service.Execute(grantAccessRequest);
                            }

                            if (userOrTeamLogicalName == "systemuser")
                            {
                                var createdReference = new EntityReference("systemuser", userOrTeam.Id);
                                var accessMask = principalAccess.AccessMask;

                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        // To get the Principal Access
                                        AccessMask = accessMask,
                                        Principal = createdReference
                                    },
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(userOrTeamId.ToString());
                                tracingService.Trace(userOrTeamLogicalName);
                                tracingService.Trace(accessMask.ToString());

                                service.Execute(grantAccessRequest);
                            }
                        }
                    }

                    else if (context.MessageName == "RevokeAccess")
                    {
                        // Obtain the target entity from the input parameter
                        EntityReference accountRef = (EntityReference)context.InputParameters["Target"];

                        // Obtain the principal access object from the input parameter
                        var revokee = (EntityReference)context.InputParameters["Revokee"];

                        // Call GetContacts function to retrieve all account related contacts
                        EntityCollection contacts = GetContacts(accountRef.Id, service);

                        foreach (var contact in contacts.Entities)
                        {
                            Guid contactId = (Guid)contact["contactid"];

                            // Use the logical Name to know whether this is User or Team!
                            if (revokee.LogicalName == "team")
                            {
                                var createdReference = new EntityReference("team", revokee.Id);

                                var revokeAccessRequest = new RevokeAccessRequest
                                {
                                    Revokee = createdReference,
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(revokee.Id.ToString());
                                tracingService.Trace(revokee.LogicalName);

                                service.Execute(revokeAccessRequest);
                            }

                            if (revokee.LogicalName == "systemuser")
                            {
                                var createdReference = new EntityReference("systemuser", revokee.Id);

                                var revokeAccessRequest = new RevokeAccessRequest
                                {
                                    Revokee = createdReference,
                                    Target = new EntityReference("contact", contactId)
                                };

                                tracingService.Trace(revokee.Id.ToString());
                                tracingService.Trace(revokee.LogicalName);

                                service.Execute(revokeAccessRequest);
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
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }


        /// <summary>
        /// This function returns all the related contacts to a specific account
        /// </summary>
        /// <param name="entityId">Used to get contacts related to the specific account id</param>
        /// <param name="service">Used to execute the query in order to retrieve multiple contacts</param>
        /// <returns>A collection of entites i.e contacts</returns>
        private EntityCollection GetContacts(Guid entityId, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet("contactid");
            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, entityId);
            EntityCollection collection = service.RetrieveMultiple(query);

            return collection;
        }
    }
}
