using Quartz;
using System;
using Rock.Data;
using Rock.Model;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Background History Collector Rock Job sweeps through the Attribute Values for a given person
/// and archives the background check into the background check table for historical purposes
/// Author Daniel J Rychlik daniel.rychlik@bemaservices.com
/// 
/// Built from 7.4
/// </summary>
namespace com.bemaservices.BackgroundHistoryCollector.Jobs
{
    class BackgroundHistoryCollector : IJob
    {
        #region Fields

        /// <summary>
        /// Attribute Guid list
        /// </summary>
        private List<Guid> attributeGuidList = new List<Guid>
        {
            new Guid("DAF87B87-3D1E-463D-A197-52227FE4EA28"),
            new Guid("3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F"),
            new Guid("44490089-E02C-4E54-A456-454845ABBC9D"),
            new Guid("F3931952-460D-43E0-A6E0-EB6B5B1F9167"),
            new Guid("6E2366B4-9F0E-454A-9DB1-E06263749C12"),
            new Guid("6DC6E992-4CAF-4C9F-B11D-5918D244BD40")
        };

        /// <summary>
        /// Attribute List of Background Check Attributes
        /// </summary>
        private List<Rock.Model.Attribute> backgroundCheckAttributeList;

        /// <summary>
        /// RockContext reference
        /// </summary>
        private RockContext rockContext = new RockContext();

        /// <summary>
        /// This jobs guid
        /// </summary>
        private Guid jobGuid = new Guid("27D27CF1-4D11-458E-98CC-CE03CC8FB64C");


        #endregion

        #region Methods

        /// <summary>
        /// Construct
        /// </summary>
        public BackgroundHistoryCollector()
        {
        }

        /// <summary>
        /// Execute the job.
        /// Gets the last time this job ran so that the queryable can have something to filter by.
        /// It's assumed that new background checks come in within the last run time so that the system 
        /// can use it as a filter to keep this job as fast as possible.  Method also archives the results into the
        /// background check table for historical purposes.  This is shown in the timeline feature added to the 
        /// person profile pages.
        /// </summary>
        /// <param name="context">IJobExecutionContext</param>
        public virtual void Execute(IJobExecutionContext context)
        {
            DateTime? lastRunTime = GetLastSuccessfulRunDate();
            backgroundCheckAttributeList = GetBackgroundCheckAttributeList();

            // Get the Background CheckDate Id
            int backgroundCheckDateId = backgroundCheckAttributeList.Where(p => p.Guid == Guid.Parse("3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F")).First().Id;

            // Get a list of recent background checks 
            List<int> entityIdList = GetPersonIdListFromBackGroundCheckDateSince(backgroundCheckDateId, lastRunTime);

            // Get List of just the attribute ids
            List<int> attributeIdList = backgroundCheckAttributeList.Select(p => p.Id).ToList();

            if (entityIdList.Any())
            {
                List<BackgroundCheck> backgroundCheckList = new List<BackgroundCheck>();

                foreach (var personId in entityIdList)
                {
                    Dictionary<int, List<AttributeValue>> backgroundCheckDictionary = GetPersonBackgroundCheckAttributeValueList(personId, attributeIdList);

                    // Build a BackgroundCheck Object from our aggregated data
                    backgroundCheckList.Add(GetBackgroundCheckItem(backgroundCheckDictionary));
                }

                // Save the list
                ArchiveBackgroundChecks(backgroundCheckList);
            }
            else
            {
                // Log no new background checks
            }
        }

        /// <summary>
        /// Builds the background check item
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns>Dictionary<int, List<AttributeValue>></returns>
        private BackgroundCheck GetBackgroundCheckItem(Dictionary<int, List<AttributeValue>> dictionary)
        {
            int personAliasId = dictionary.First().Key;

            BackgroundCheck backgroundCheck = new BackgroundCheck();
            backgroundCheck.PersonAliasId = personAliasId;
            backgroundCheck.ForeignKey = "BEMA";
            backgroundCheck.Guid = new Guid();
            backgroundCheck.ResponseDate = (DateTime?)GetBackgroundCheckItemValue(new Guid("3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F"), dictionary.First().Value);

            return backgroundCheck;
        }

        /// <summary>
        /// Saves or archives the background check list using the BackgroundCheckService
        /// </summary>
        /// <param name="backgroundChecks">IEnumerable<BackgroundCheck></param>
        private void ArchiveBackgroundChecks(IEnumerable<BackgroundCheck> backgroundChecks)
        {
            BackgroundCheckService backgroundCheckService = new BackgroundCheckService(rockContext);

            backgroundCheckService.AddRange(backgroundChecks);
            //backgroundCheckService.Context.SaveChanges();
        }

        /// <summary>
        /// Simple method for pulling out the values from the attribute value list by the referencing the attribute id
        /// from the always realiable Guid.  That maybe why you ask yourself, why not just use the attributeId.  That's because
        /// it's not entirely consistent.  9x's out of 10 it will be, but why risk it.  Yes it's worth the effort in my oppinion and
        /// LINQ makes this super easy to do.
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <param name="attributeValueList">List<AttributeValue></param>
        /// <returns>Object</returns>
        private Object GetBackgroundCheckItemValue(Guid guid, List<AttributeValue> attributeValueList)
        {
            int attributeId = backgroundCheckAttributeList.AsQueryable().Where(p => p.Guid == guid).Select(p => p.Id).First();

            Object value = attributeValueList.Where(p => p.AttributeId == attributeId).FirstOrDefault().Value;

            return value;
        }

        /// <summary>
        /// Gets the last time this job ran
        /// </summary>
        /// <returns>DateTime?</returns>
        private DateTime? GetLastSuccessfulRunDate()
        {
            var serviceJobQueryable = new ServiceJobService(rockContext).Queryable().Where(p => p.Guid == jobGuid);

            return serviceJobQueryable.First().LastRunDateTime.Value;
        }

        /// <summary>
        /// Gets the attributes for attribute context
        /// </summary>
        /// <returns>List<Rock.Model.Attribute></returns>
        private List<Rock.Model.Attribute> GetBackgroundCheckAttributeList()
        {
            // Query the Attribute Service and pull out the attribute ids from the Guids

            AttributeService attributeService = new AttributeService(rockContext);
            List<Rock.Model.Attribute> attributeList = attributeService.GetListByGuids(attributeGuidList);

            return attributeList;
        }

        /// <summary>
        /// Gets a list of people from the last background check date since.
        /// </summary>
        /// <param name="attributeId">int</param>
        /// <param name="sinceDateTime">DateTime?</param>
        /// <returns>List<int></returns>
        private List<int> GetPersonIdListFromBackGroundCheckDateSince(int attributeId, DateTime? sinceDateTime)
        {
            var queryable = new AttributeValueService(rockContext).Queryable();

            // Get the Entity Id (personId) from the attribute value where background check since date time
            List<AttributeValue> backgroundCheckDateList = queryable.
                Where(p => p.AttributeId == attributeId && p.ValueAsDateTime >= sinceDateTime).ToList();

            // Our list of people
            List<int> entityIdList = new List<int>();

            foreach (var listItem in backgroundCheckDateList)
            {
                if (listItem.EntityId != null)
                {
                    entityIdList.Add(listItem.EntityId.Value);
                }
            }

            return entityIdList;
        }

        /// <summary>
        /// Builds an attribute value list of background check items from the personId
        /// </summary>
        /// <param name="personId">int</param>
        /// <param name="backgroundCheckAttributeList">List<int></param>
        /// <returns>Dictionary<int, List<AttributeValue>></returns>
        private Dictionary<int, List<AttributeValue>> GetPersonBackgroundCheckAttributeValueList(int personId, List<int> backgroundCheckAttributeList)
        {
            Dictionary<int, List<AttributeValue>> dictionary = new Dictionary<int, List<AttributeValue>>();

            var attributeValueQueryable = new AttributeValueService(rockContext).Queryable();

            // Get all of the background check attribute values for the entity id
            var attributeValueList = attributeValueQueryable.Where(p => backgroundCheckAttributeList.Contains(p.AttributeId) && p.EntityId == personId).ToList();

            // Now get the Person Alias Id
            var person = new PersonService(rockContext).Queryable().Where(p => p.Id == personId).FirstOrDefault();

            if (person != null)
            {
                dictionary.Add(person.PrimaryAliasId.Value, attributeValueList);
            }

            return dictionary;
        }

        #endregion

    }
}
