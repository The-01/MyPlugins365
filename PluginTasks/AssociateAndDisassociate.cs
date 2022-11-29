using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTasks
{
    public class AssociateAndDisassociate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain context object
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the organization service reference which you will need for web service calls
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            // Triggers on create of records
            if (context.MessageName.ToLower() == "create")
            {
                tracingService.Trace("Inside create...");

                Entity targetEntity = null;
                string relationshipName = "new_new_subject_new_student";

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    targetEntity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace("Target student id: " + targetEntity.Id);
                }

                // Get the guid of primary teacher from primary teacher lookup on target student
                Guid primaryTeacherId = (targetEntity.Contains("new_primaryteacher") && targetEntity["new_primaryteacher"] != null) ? targetEntity.GetAttributeValue<EntityReference>("new_primaryteacher").Id : new Guid("");
                tracingService.Trace("Primary teacher id: " + primaryTeacherId);

                // Retrieve all subjects related to the primary teacher
                var fetchXml =
                    $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                          <entity name=""new_subject"">
                            <attribute name=""new_subjectid"" />
                            <attribute name=""new_name"" />
                            <attribute name=""createdon"" />
                            <order attribute=""new_name"" descending=""false"" />
                            <link-entity name=""new_new_teacher_new_subject"" from=""new_subjectid"" to=""new_subjectid"" intersect=""true"">
                              <filter>
                                <condition attribute=""new_teacherid"" operator=""eq"" value=""{primaryTeacherId}"" />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";

                var query = new FetchExpression(fetchXml);
                EntityCollection subjects = service.RetrieveMultiple(query);

                foreach (var subject in subjects.Entities)
                {
                    tracingService.Trace(subject.Id.ToString());
                    Relationship relationship = new Relationship(relationshipName);
                    EntityReference relatedEntity = new EntityReference("new_subject", subject.Id);
                    EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                    relatedEntities.Add(relatedEntity);
                    service.Associate("new_student", targetEntity.Id, relationship, relatedEntities);
                }
            }

            // Triggers on update of primary teacher field
            if (context.MessageName.ToLower() == "update")
            {
                tracingService.Trace("Inside update...");

                Entity targetEntity = null;
                string relationshipName = "new_new_subject_new_student";

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    targetEntity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace("Target student id: " + targetEntity.Id);
                }

                // Get the guid of primary teacher from primary teacher lookup on target student
                Guid primaryTeacherId = (targetEntity.Contains("new_primaryteacher") && targetEntity["new_primaryteacher"] != null) ? targetEntity.GetAttributeValue<EntityReference>("new_primaryteacher").Id : new Guid("");
                tracingService.Trace("Primary teacher id: " + primaryTeacherId);

                // Retrieve all subjects related to the target student
                var fetchXml =
                    $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                          <entity name=""new_subject"">
                            <attribute name=""new_subjectid"" />
                            <attribute name=""new_name"" />
                            <attribute name=""createdon"" />
                            <filter>
                              <condition entityname=""new_new_subject_new_student"" attribute=""new_studentid"" operator=""eq"" value=""{targetEntity.Id}"" />
                            </filter>
                            <order attribute=""new_name"" descending=""false"" />
                            <filter type=""and"">
                              <condition attribute=""createdby"" operator=""eq-userid"" />
                            </filter>
                            <link-entity name=""new_new_subject_new_student"" from=""new_subjectid"" to=""new_subjectid"" intersect=""true"" />
                          </entity>
                        </fetch>";

                var query = new FetchExpression(fetchXml);
                EntityCollection subjects = service.RetrieveMultiple(query);

                foreach (var subject in subjects.Entities)
                {
                    tracingService.Trace(subject.Id.ToString());
                    Relationship relationship = new Relationship(relationshipName);
                    EntityReference relatedEntity = new EntityReference("new_subject", subject.Id);
                    EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                    relatedEntities.Add(relatedEntity);
                    service.Disassociate("new_student", targetEntity.Id, relationship, relatedEntities);
                }

                // Retrieve all subjects related to the primary teacher
                var _fetchXml = 
                    $@"<?xml version=""1.0"" encoding=""utf-16""?>
                        <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                          <entity name=""new_subject"">
                            <attribute name=""new_subjectid"" />
                            <attribute name=""new_name"" />
                            <attribute name=""createdon"" />
                            <order attribute=""new_name"" descending=""false"" />
                            <link-entity name=""new_new_teacher_new_subject"" from=""new_subjectid"" to=""new_subjectid"" intersect=""true"">
                              <filter>
                                <condition attribute=""new_teacherid"" operator=""eq"" value=""{primaryTeacherId}"" />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";

                var _query = new FetchExpression(_fetchXml);
                EntityCollection _subjects = service.RetrieveMultiple(_query);

                foreach (var _subject in _subjects.Entities)
                {
                    tracingService.Trace(_subject.Id.ToString());
                    Relationship relationship = new Relationship(relationshipName);
                    EntityReference relatedEntity = new EntityReference("new_subject", _subject.Id);
                    EntityReferenceCollection relatedEntities = new EntityReferenceCollection();
                    relatedEntities.Add(relatedEntity);
                    service.Associate("new_student", targetEntity.Id, relationship, relatedEntities);
                }
            }

            // Triggers on association of records
            if (context.MessageName.ToLower() == "associate")
            {
                tracingService.Trace("Inside associate...");

                string relationshipName = "";
                EntityReference targetEntity = null;
                EntityReferenceCollection relatedEntities = null;

                // Get Entity 1 reference from "Target" Key from context
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    targetEntity = (EntityReference)context.InputParameters["Target"];
                    tracingService.Trace("Target student id: " + targetEntity.Id);
                }

                if (targetEntity.LogicalName == "new_student")
                {
                    // Get the "Relationship" Key from context
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        relationshipName = context.InputParameters["Relationship"].ToString();
                    }

                    // Check the "Relationship Name" with your intended one
                    if (relationshipName != "new_new_subject_new_student.Referenced")
                    {
                        return;
                    }

                    // Get Entity 2 reference from "RelatedEntities" Key from context
                    if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;

                        Entity student = service.Retrieve("new_student", targetEntity.Id, new ColumnSet(new string[] { "new_primaryteacher" }));

                        foreach (var entity in relatedEntities)
                        {
                            if (student.Contains("new_primaryteacher") && student["new_primaryteacher"] != null)
                            {
                                throw new InvalidPluginExecutionException("New subject cannot be added to the student subjects if the primary teacher already exist!");
                            }
                        }
                    }
                }

                if (targetEntity.LogicalName == "new_teacher")
                {
                    // Get the "Relationship" Key from context
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        relationshipName = context.InputParameters["Relationship"].ToString();
                    }

                    // Check the "Relationship Name" with your intended one
                    if (relationshipName != "new_new_teacher_new_subject.Referencing")
                    {
                        return;
                    }

                    // Get Entity 2 reference from "RelatedEntities" Key from context
                    if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;

                        foreach (var entity in relatedEntities)
                        {
                            // Get all students related to target teacher
                            EntityCollection students = TeacherRelatedStudents(targetEntity, service);

                            // Get all subjects related to target teacher
                            EntityCollection subjects = TeacherRelatedSubjects(targetEntity, service);

                            foreach (var student in students.Entities)
                            {
                                EntityCollection teacherRelatedStudentsSubjects = TeacherRelatedStudentsSubjects(student, service);

                                foreach (var TRS_Subject in teacherRelatedStudentsSubjects.Entities)
                                {
                                    tracingService.Trace(TRS_Subject.Id.ToString());
                                    AssociateRecords(student, TRS_Subject, relatedEntities, service);
                                }

                                foreach (var subject in subjects.Entities)
                                {
                                    tracingService.Trace(subject.Id.ToString());
                                    DisassociateRecords(student, subject, relatedEntities, service);
                                }
                            }
                        }
                    }
                }
            }

            // Triggers on disassociation of records
            if (context.MessageName.ToLower() == "disassociate")
            {
                tracingService.Trace("Inside disassociate...");

                string relationshipName = "";
                EntityReference targetEntity = null;
                EntityReferenceCollection relatedEntities = null;

                // Get Entity 1 reference from "Target" Key from context
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    targetEntity = (EntityReference)context.InputParameters["Target"];
                }

                if (targetEntity.LogicalName == "new_student")
                {
                    tracingService.Trace("Target student id: " + targetEntity.Id);

                    // Get the "Relationship" Key from context
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        relationshipName = context.InputParameters["Relationship"].ToString();
                    }

                    // Check the "Relationship Name" with your intended one
                    if (relationshipName != "new_new_subject_new_student.Referenced")
                    {
                        return;
                    }

                    Entity student = service.Retrieve("new_student", targetEntity.Id, new ColumnSet(new string[] { "new_primaryteacher" }));

                    // Get Entity 2 reference from "RelatedEntities" Key from context
                    if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;

                        foreach (var entity in relatedEntities)
                        {
                            if (student.Contains("new_primaryteacher") && student["new_primaryteacher"] != null)
                            {
                                throw new InvalidPluginExecutionException("Subject cannot be removed from students subjects as primary teacher exist for the particular student!");
                            }
                        }
                    }
                }

                if (targetEntity.LogicalName == "new_teacher")
                {
                    tracingService.Trace("Target teacher id: " + targetEntity.Id);

                    // Get the "Relationship" Key from context
                    if (context.InputParameters.Contains("Relationship"))
                    {
                        relationshipName = context.InputParameters["Relationship"].ToString();
                    }

                    // Check the "Relationship Name" with your intended one
                    if (relationshipName != "new_new_teacher_new_subject.Referencing")
                    {
                        return;
                    }

                    // Get Entity 2 reference from "RelatedEntities" Key from context
                    if (context.InputParameters.Contains("RelatedEntities") && context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
                    {
                        relatedEntities = context.InputParameters["RelatedEntities"] as EntityReferenceCollection;

                        foreach (var entity in relatedEntities)
                        {
                            // Get all students related to target teacher
                            EntityCollection students = TeacherRelatedStudents(targetEntity, service);

                            // Get all subjects related to target teacher
                            EntityCollection subjects = TeacherRelatedSubjects(targetEntity, service);
                            
                            foreach (var student in students.Entities)
                            {
                                EntityCollection teacherRelatedStudentsSubjects = TeacherRelatedStudentsSubjects(student, service);

                                foreach (var TRS_Subject in teacherRelatedStudentsSubjects.Entities)
                                {
                                    tracingService.Trace(TRS_Subject.Id.ToString());
                                    AssociateRecords(student, TRS_Subject, relatedEntities, service);
                                }

                                foreach (var subject in subjects.Entities)
                                {
                                    tracingService.Trace(subject.Id.ToString());
                                    DisassociateRecords(student, subject, relatedEntities, service);
                                }
                            }
                        }
                    }
                }
            }
        }


        public EntityCollection TeacherRelatedStudents(EntityReference targetEntity, IOrganizationService service)
        {
            // Retrieve all students related to the target teacher
            var fetchXml =
                $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                        <entity name=""new_student"">
                        <attribute name=""new_studentid"" />
                        <attribute name=""new_name"" />
                        <attribute name=""createdon"" />
                        <attribute name=""new_primaryteacher"" />
                        <link-entity name=""new_teacher"" from=""new_teacherid"" to=""new_primaryteacher"">
                            <filter>
                            <condition attribute=""new_teacherid"" operator=""eq"" value=""{targetEntity.Id}"" />
                            </filter>
                        </link-entity>
                        </entity>
                    </fetch>";

            var query = new FetchExpression(fetchXml);
            EntityCollection students = service.RetrieveMultiple(query);

            return students;
        }

        public EntityCollection TeacherRelatedSubjects(EntityReference targetEntity, IOrganizationService service)
        {
            // Retrieve all subjects related to the target teacher
            var _fetchXml =
                $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                        <entity name=""new_subject"">
                        <attribute name=""new_subjectid"" />
                        <attribute name=""new_name"" />
                        <attribute name=""createdon"" />
                        <order attribute=""new_name"" descending=""false"" />
                        <link-entity name=""new_new_teacher_new_subject"" from=""new_subjectid"" to=""new_subjectid"" intersect=""true"">
                            <filter>
                            <condition attribute=""new_teacherid"" operator=""eq"" value=""{targetEntity.Id}"" />
                            </filter>
                        </link-entity>
                        </entity>
                    </fetch>";

            var _query = new FetchExpression(_fetchXml);
            EntityCollection subjects = service.RetrieveMultiple(_query);

            return subjects;
        }

        public EntityCollection TeacherRelatedStudentsSubjects(Entity student, IOrganizationService service)
        {
            // Retrieve all subjects of teacher related students
            var fetchXml =
                $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
                        <entity name=""new_subject"">
                        <attribute name=""new_subjectid"" />
                        <attribute name=""new_name"" />
                        <attribute name=""createdon"" />
                        <filter>
                            <condition entityname=""new_new_subject_new_student"" attribute=""new_studentid"" operator=""eq"" value=""{student.Id}"" />
                        </filter>
                        <order attribute=""new_name"" descending=""false"" />
                        <filter type=""and"">
                            <condition attribute=""createdby"" operator=""eq-userid"" />
                        </filter>
                        <link-entity name=""new_new_subject_new_student"" from=""new_subjectid"" to=""new_subjectid"" intersect=""true"" />
                        </entity>
                    </fetch>";

            var query = new FetchExpression(fetchXml);
            EntityCollection subjects = service.RetrieveMultiple(query);

            return subjects;
        }

        public void AssociateRecords(Entity student, Entity TRS_Subject, EntityReferenceCollection relatedEntities, IOrganizationService service)
        {
            string relationshipName = "new_new_subject_new_student";
            Relationship relationship = new Relationship(relationshipName);
            EntityReference relatedEntity = new EntityReference("new_subject", TRS_Subject.Id);
            relatedEntities = new EntityReferenceCollection();
            relatedEntities.Add(relatedEntity);
            service.Disassociate("new_student", student.Id, relationship, relatedEntities);
        }

        public void DisassociateRecords(Entity student, Entity subject, EntityReferenceCollection relatedEntities, IOrganizationService service)
        {
            string relationshipName = "new_new_subject_new_student";
            Relationship relationship = new Relationship(relationshipName);
            EntityReference relatedEntity = new EntityReference("new_subject", subject.Id);
            relatedEntities = new EntityReferenceCollection();
            relatedEntities.Add(relatedEntity);
            service.Associate("new_student", student.Id, relationship, relatedEntities);
        }
    }
}
