﻿// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using DDay.iCal;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.GroupTools.Controllers
{
    /// <summary>
    /// The controller class for the GroupTools
    /// </summary>
    public partial class GroupToolsController : Rock.Rest.ApiController<Rock.Model.Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToolsController"/> class.
        /// </summary>
        public GroupToolsController() : base( new Rock.Model.GroupService( new Rock.Data.RockContext() ) ) { }
    }

    public partial class GroupToolsController
    {
    
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/GroupTools/GetGroups" )]
        public IQueryable<GroupInformation> GetGroups(
            string groupTypeIds = "",
            string campusIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null )
        {
            IQueryable<Group> qry = FilterGroups( groupTypeIds, campusIds, meetingDays, categoryIds, age );

            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( new RockContext() );
            foreach ( var group in qry.ToList() )
            {
                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Description = group.Description;
                groupInfo.Color = "#428bca";

                group.LoadAttributes();
                var maximumAge = group.GetAttributeValue( "MaximumAge" ).AsIntegerOrNull();
                var minimumAge = group.GetAttributeValue( "MinimumAge" ).AsIntegerOrNull();
                if ( minimumAge.HasValue || maximumAge.HasValue )
                {
                    if ( !minimumAge.HasValue )
                    {
                        groupInfo.LifeStage = String.Format( "{0} & Under", maximumAge.Value );
                    }
                    else
                    {
                        if ( !maximumAge.HasValue )
                        {
                            groupInfo.LifeStage = String.Format( "{0} & Up", minimumAge.Value );
                        }
                        else
                        {
                            groupInfo.LifeStage = String.Format( "{0} - {1}", minimumAge.Value, maximumAge.Value );
                        }
                    }
                }
                else
                {
                    groupInfo.LifeStage = "";
                }

                var categoryGuids = group.GetAttributeValue( "Category" ).SplitDelimitedValues().AsGuidList();
                if ( categoryGuids.Any() )
                {
                    var categories = definedValueService.GetByGuids( categoryGuids );
                    if ( categories.Any() )
                    {
                        var category = categories.OrderBy( c => c.Order ).First();
                        category.LoadAttributes();
                        groupInfo.Category = category.Value;

                        var colorString = category.GetAttributeValue( "Color" );
                        if ( colorString.IsNotNullOrWhiteSpace() )
                        {
                            if ( colorString.Contains( "rgb" ) )
                            {
                                var colorList = colorString.Replace( "rgb(", "" ).Replace( ")", "" ).SplitDelimitedValues().AsIntegerList();
                                if ( colorList.Count == 3 )
                                {
                                    Color myColor = Color.FromArgb( colorList[0], colorList[1], colorList[2] );
                                    string hex = myColor.R.ToString( "X2" ) + myColor.G.ToString( "X2" ) + myColor.B.ToString( "X2" );

                                    groupInfo.Color = "#" + hex;
                                }
                            }
                            else
                            {
                                groupInfo.Color = colorString;
                            }
                        }

                    }
                }

                if ( group.Schedule != null )
                {
                    var schedule = group.Schedule;
                    groupInfo.Frequency = "Custom";

                    if ( schedule.WeeklyDayOfWeek.HasValue && schedule.WeeklyTimeOfDay.HasValue )
                    {
                        groupInfo.Frequency = "Weekly";
                        groupInfo.DayOfWeek = schedule.WeeklyDayOfWeek.Value.ConvertToString().Substring( 0, 3 );
                        groupInfo.TimeOfDay = schedule.WeeklyTimeOfDay.Value.ToTimeString();
                    }
                    else
                    {
                        var nextStartDate = schedule.GetNextStartDateTime( RockDateTime.Now );
                        if ( nextStartDate.HasValue )
                        {
                            groupInfo.FriendlyScheduleText = schedule.FriendlyScheduleText;

                            groupInfo.DayOfWeek = nextStartDate.Value.ToString( "ddd" );
                            groupInfo.TimeOfDay = nextStartDate.Value.TimeOfDay.ToTimeString();

                            DDay.iCal.Event calendarEvent = schedule.GetCalendarEvent();
                            if ( calendarEvent != null && calendarEvent.DTStart != null )
                            {
                                string startTimeText = calendarEvent.DTStart.Value.TimeOfDay.ToTimeString();
                                if ( calendarEvent.RecurrenceRules.Any() )
                                {
                                    // some type of recurring schedule

                                    IRecurrencePattern rrule = calendarEvent.RecurrenceRules[0];
                                    if ( rrule.Interval == 1 )
                                    {
                                        groupInfo.Frequency = rrule.Frequency.ToString();
                                    }
                                }
                            }
                        }
                    }

                }

                groupInfoList.Add( groupInfo );
            }

            var groupInfoQry = groupInfoList.AsQueryable()
                .OrderBy( g => g.Category == "Featured" )
                .ThenBy( g => g.Category == "New" )
                .ThenBy( g => g.Name )
                .AsQueryable();


            if ( offset.HasValue )
            {
                groupInfoQry = groupInfoQry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                groupInfoQry = groupInfoQry.Take( limit.Value );
            }

            return groupInfoQry;
        }

        private static IQueryable<Group> FilterGroups( string groupTypeIds, string campusIds, string meetingDays, string categoryIds, string age )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var qry = groupService.Queryable().AsNoTracking();

            var groupTypeIdList = groupTypeIds.SplitDelimitedValues().AsIntegerList();
            var campusIdList = campusIds.SplitDelimitedValues().AsIntegerList();
            var meetingDayList = meetingDays.SplitDelimitedValues().Select( i => i.ConvertToEnum<DayOfWeek>() ).ToList();
            var categoryIdList = categoryIds.SplitDelimitedValues().AsIntegerList();

            if ( groupTypeIdList.Any() )
            {
                qry = qry.Where( g => groupTypeIdList.Contains( g.GroupTypeId ) );
            }

            if ( campusIdList.Any() )
            {
                qry = qry.Where( g => g.CampusId.HasValue && campusIdList.Contains( g.CampusId.Value ) );
            }

            if ( meetingDayList.Any() )
            {
                qry = qry.Where( g => g.Schedule != null && g.Schedule.WeeklyDayOfWeek != null && meetingDayList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
            }

            if ( categoryIdList.Any() )
            {
                var categoryList = new DefinedValueService( rockContext ).GetByIds( categoryIdList ).Select( c => c.Guid.ToString() ).ToList();
                qry = qry.WhereAttributeValue( rockContext, av => av.Attribute.Key == "Category" && categoryList.Any( c => av.Value.Contains( c ) ) );
            }

            var ageInt = age.AsIntegerOrNull();
            if ( ageInt.HasValue )
            {
                int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

                var excludedMinimumAgeIds = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.Attribute.Key == "MinimumAge" )
                    .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                    .Where( a => a.ValueAsNumeric.HasValue && a.ValueAsNumeric > ageInt.Value )
                    .Select( a => a.EntityId );

                var excludedMaximumAgeIds = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.Attribute.Key == "MaximumAge" )
                    .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                    .Where( a => a.ValueAsNumeric.HasValue && a.ValueAsNumeric < ageInt.Value )
                    .Select( a => a.EntityId );

                qry = qry.Where( g => !excludedMaximumAgeIds.Contains( g.Id ) && !excludedMinimumAgeIds.Contains( g.Id ) );
            }

            return qry;
        }

        /// <summary>
        /// Gets the group count.
        /// </summary>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="meetingDays">The meeting days.</param>
        /// <param name="categoryIds">The category ids.</param>
        /// <param name="lifeStageIds">The life stage ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/GroupTools/GetGroupCount" )]
        public int GetGroupCount(
            string groupTypeIds = "",
            string campusIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "" )
        {
            IQueryable<Group> qry = FilterGroups( groupTypeIds, campusIds, meetingDays, categoryIds, age );

            return qry.Count();
        }

    }

    /// <summary>
    /// A class to store group data to be returned by the API
    /// </summary>
    public class GroupInformation
    {
        public int Id { get; set; }

        public Guid Guid { get; set; }

        public int GroupTypeId { get; set; }

        public int? CampusId { get; set; }

        public string Campus { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DayOfWeek { get; set; }
        public string TimeOfDay { get; set; }

        public string Frequency { get; set; }

        public string FriendlyScheduleText { get; set; }

        public string LifeStage { get; set; }

        public string Category { get; set; }

        public string Color { get; set; }

    }
}
