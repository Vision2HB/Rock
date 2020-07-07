// <copyright>
// Copyright by the Spark Development Network
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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock;
using Rock.Rest.Filters;

namespace com_bemaservices.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricsXController : Rock.Rest.ApiController<Rock.Model.Metric>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsXController"/> class.
        /// </summary>
        public MetricsXController() : base ( new Rock.Model.MetricService ( new Rock.Data.RockContext () ) ) { }

        /// <summary>
        /// Gets the HTML for a LiquidDashboardWidget block
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="metricIds">The entity type identifier.</param>
        /// <param name="entityTypeEntityIds">The entity identifier.</param>
        /// <param name="options">The entity identifier.</param>
        /// <param name="dateRange">The entity identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/MetricsX/GetHtmlForBlock/{blockId}" )]
        public string GetHtmlForBlock( int blockId, string metricIds = null, string entityTypeEntityIds = null, string options = null, string dateRange = null )
        {
            RockContext rockContext = this.Service.Context as RockContext ?? new RockContext();
            Block block = new BlockService( rockContext ).Get( blockId );
            if ( block != null )
            {
                block.LoadAttributes();

                string liquidTemplate = block.GetAttributeValue( "LavaTemplate" );
                Dictionary<string, object> mergeValues = new Dictionary<string, object> ();

                bool roundYValues = false;
                if ( options != null )
                {
                    roundYValues = options.Contains ( "Round" );
                }

                if (metricIds == null)
                {
                    return @"<div class='alert alert-warning'> 
								    Please select a metric in the block settings.
							    </div>";
                }

                MetricService metricService = new MetricService ( rockContext );
                var metrics = metricService.GetByIds ( metricIds.Split ( '|' ).Select ( int.Parse ).ToList () ).Include ( a => a.MetricPartitions ).ToList ();
                List<object> metricsData = new List<object> ();

                if ( metrics.Count() == 0 )
                {
                    return @"<div class='alert alert-warning'> 
								Please select a metric in the block settings.
							</div>";
                }

                MetricValueService metricValueService = new MetricValueService( rockContext );

                DateTime firstDayOfYear = new DateTime ( RockDateTime.Now.Year, 1, 1 );
                DateTime currentDateTime = RockDateTime.Now;

                if ( dateRange != null && dateRange != "" )
                {
                    firstDayOfYear = dateRange.Split ( ',' ).FirstOrDefault ().AsDateTime ().Value;
                    currentDateTime = dateRange.Split ( ',' ).LastOrDefault ().AsDateTime ().Value;
                }
                currentDateTime = new DateTime ( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day);
                DateTime firstDayOfNextYear = new DateTime( RockDateTime.Now.Year + 1, 1, 1 );
                DateTime firstDayOfLastYear = new DateTime ( RockDateTime.Now.Year - 1, 1, 1 );
                DateTime currentDayOfLastYear = new DateTime ( currentDateTime.Year - 1, currentDateTime.Month, currentDateTime.Day );
                DateTime firstDayOf52w = currentDateTime.AddDays( -52 * 7 );
                DateTime firstDayOfPrev52w = currentDateTime.AddDays ( -104 * 7 );

                var cacheName = string.Format ( "MerticX{0}{1}{2}-{3}", metricIds, entityTypeEntityIds, firstDayOfYear.ToShortDateString(), currentDateTime.ToShortDateString() );
                var cachedContent = HtmlContentService.GetCachedContent ( blockId, cacheName );
                if ( cachedContent == null )
                {
                    foreach ( var metric in metrics )
                    {
                        if ( metric.IsAuthorized ( Rock.Security.Authorization.VIEW, GetPerson () ) )
                        {

                            var metricsXData = JsonConvert.DeserializeObject( metric.ToJson(), typeof( MetricsXData ) ) as MetricsXData;
                            metric.MetricCategories = new MetricCategoryService ( rockContext ).AsNoFilter()
                                .Where ( m => m.MetricId == metric.Id ).ToList ();
                            if ( metric.MetricCategories != null && metric.MetricCategories.Count() > 0 )
                            {
                                metricsXData.CategoryId = metric.MetricCategories.FirstOrDefault ().Id;
                            }
                            var endDate = new DateTime ( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day + 1 );

                            var qryMeasureValues = metricValueService.Queryable()
                                .Where( a => a.MetricId == metricsXData.Id )
                                .Where( a => a.MetricValueDateTime >= currentDateTime && a.MetricValueDateTime < endDate )
                                .Where( a => a.MetricValueType == MetricValueType.Measure );

                            //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
                            //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting

                            var entityTypeList = new List<EntityPair>();

                            if ( entityTypeEntityIds != null)
                            {
                                entityTypeList = entityTypeEntityIds.Split ( ',' ).Select ( a => a.Split ( '|' ) ).Where ( a => a.Length == 2 )
                                    .Select ( a => new EntityPair ( a[0].AsIntegerOrNull (), a[1] ) ).ToList ();

                                foreach ( var item in entityTypeList )
                                {
                                    if ( metric.MetricPartitions.Any ( a => a.EntityTypeId == item.EntityTypeId.Value ) && item.EntityIds.Count > 0 )
                                    {
                                        qryMeasureValues = qryMeasureValues.Where ( a => a.MetricValuePartitions.Any ( p => item.EntityIds.Contains( p.EntityId.Value ) && p.MetricPartition.EntityTypeId == item.EntityTypeId.Value ) );
                                    }

                                }
                            }

                            // get current value
                            var lastMetricValue = qryMeasureValues.OrderByDescending( a => a.MetricValueDateTime ).FirstOrDefault();
                            if ( lastMetricValue != null )
                            {
                                metricsXData.LastValueDate = lastMetricValue.MetricValueDateTime.HasValue ? lastMetricValue.MetricValueDateTime.Value.Date : DateTime.MinValue;

                                // get a sum of the values that for whole 24 hour day of the last Date
                                metricsXData.LastValue = qryMeasureValues.Sum( a => a.YValue ).HasValue ? Math.Round( qryMeasureValues.Sum( a => a.YValue ).Value, roundYValues ? 0 : 2 ) : (decimal?)null;
                            }
                            else
                            {
                                metricsXData.LastValue = 0;
                                metricsXData.LastValueDate = currentDateTime;
                            }

                            int dateCount; int weekCount;

                            metricsXData.CumulativeValue = GetValue ( metricValueService, metric,
                                firstDayOfYear, currentDateTime, entityTypeList, roundYValues,
                                out dateCount, out weekCount);

                            metricsXData.CumulativeValueDateCount = dateCount;
                            metricsXData.CumulativeValueWeekCount = weekCount;

                            // Adjust last year date if different number of Sundays so it's apples-to-apples
                            int diff = CountDays ( DayOfWeek.Sunday, firstDayOfYear, currentDateTime ) - CountDays ( DayOfWeek.Sunday, firstDayOfLastYear, currentDayOfLastYear );
                            currentDayOfLastYear = currentDayOfLastYear.AddDays ( diff * 7 );

                            //Get Previous Year YTD
                            metricsXData.PreviousYearCumulativeValue = GetValue ( metricValueService, metric,
                                firstDayOfLastYear, currentDayOfLastYear, entityTypeList, roundYValues,
                                out dateCount, out weekCount );

                            metricsXData.PreviousYearCumulativeValueDateCount = dateCount;
                            metricsXData.PreviousYearCumulativeValueWeekCount = weekCount;

                            // Get Last Week Value
                            DateTime lastWeek = metricsXData.LastValueDate.AddDays ( -7 );

                            metricsXData.PreviousWeekValue = GetValue ( metricValueService, metric,
                                lastWeek, lastWeek.AddDays(1), entityTypeList, roundYValues,
                                out dateCount, out weekCount );

                            //Get Last Year Value
                            DateTime lastYear = new DateTime ( metricsXData.LastValueDate.Year - 1, metricsXData.LastValueDate.Month, metricsXData.LastValueDate.Day );
                            // Get to the same day of the week
                            lastYear = lastYear.AddDays ( metricsXData.LastValueDate.DayOfWeek.ConvertToInt () - lastYear.DayOfWeek.ConvertToInt () );

                            metricsXData.ThisWeekLastYearValue = GetValue ( metricValueService, metric,
                                lastYear, lastYear.AddDays(1), entityTypeList, roundYValues,
                                out dateCount, out weekCount );

                            // Get 52w Total
                            metricsXData.Weeks52Value = GetValue ( metricValueService, metric,
                                firstDayOf52w, currentDateTime, entityTypeList, roundYValues,
                                out dateCount, out weekCount );
                            metricsXData.Weeks52ValueDateCount = dateCount;
                            metricsXData.Weeks52ValueWeekCount = weekCount;

                            // Get Previous 52w Total
                            metricsXData.PreviousWeeks52Value = GetValue ( metricValueService, metric,
                                firstDayOfPrev52w, firstDayOf52w, entityTypeList, roundYValues,
                                out dateCount, out weekCount );
                            metricsXData.PreviousWeeks52ValueDateCount = dateCount;
                            metricsXData.PreviousWeeks52ValueWeekCount = weekCount;

                            // figure out goal as of current date time by figuring out the slope of the goal
                            var qryGoalValuesCurrentYear = metricValueService.Queryable()
                                .Where( a => a.MetricId == metricsXData.Id )
                                .Where( a => a.MetricValueType == MetricValueType.Goal );

                            // if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
                            if ( entityTypeEntityIds != null )
                            {
                                foreach ( var item in entityTypeList )
                                {
                                    if ( metric.MetricPartitions.Any ( a => a.EntityTypeId == item.EntityTypeId.Value ) && item.EntityIds.Count > 0 )
                                    {
                                        qryGoalValuesCurrentYear = qryGoalValuesCurrentYear.Where ( a => a.MetricValuePartitions.Any ( p => item.EntityIds.Contains ( p.EntityId.Value ) && p.MetricPartition.EntityTypeId == item.EntityTypeId.Value ) );
                                    }

                                }
                            }

                            MetricValue goalLineStartPoint = qryGoalValuesCurrentYear.Where( a => a.MetricValueDateTime <= currentDateTime ).OrderByDescending( a => a.MetricValueDateTime ).FirstOrDefault();
                            MetricValue goalLineEndPoint = qryGoalValuesCurrentYear.Where( a => a.MetricValueDateTime >= currentDateTime ).FirstOrDefault();
                            if ( goalLineStartPoint != null && goalLineEndPoint != null )
                            {
                                var changeInX = goalLineEndPoint.DateTimeStamp - goalLineStartPoint.DateTimeStamp;
                                var changeInY = goalLineEndPoint.YValue - goalLineStartPoint.YValue;
                                if ( changeInX != 0 )
                                {
                                    decimal? slope = changeInY / changeInX;
                                    decimal goalValue = ( ( slope * ( currentDateTime.ToJavascriptMilliseconds() - goalLineStartPoint.DateTimeStamp ) ) + goalLineStartPoint.YValue ).Value;
                                    metricsXData.GoalValue = Math.Round( goalValue, roundYValues ? 0 : 2 );
                                }
                            }
                            else
                            {
                                // if there isn't a both a start goal and end goal within the date range, there wouldn't be a goal line shown in a line chart, so don't display a goal in liquid either
                                metricsXData.GoalValue = null;
                            }

                            metricsData.Add( metricsXData.ToLiquid() );
                        }
                    }

                    mergeValues.Add ( "MetricsXData", metricsData );
                    var detailPageReference = new Rock.Web.PageReference ( block.GetAttributeValue ( "DetailPage" ) );
                    mergeValues.Add ( "DetailPage",  detailPageReference.BuildUrl() );

                    if ( options != null )
                    {
                        mergeValues.Add ( "ShowTitle", options.Contains ( "MetricTitle" ) );
                        mergeValues.Add ( "ShowIcon", options.Contains ( "Icon" ) );
                        mergeValues.Add ( "ShowSubTitle", options.Contains ( "SubTitle" ) );
                        mergeValues.Add ( "ShowDescription", options.Contains ( "Description" ) );
                        mergeValues.Add ( "ShowDate", options.Contains ( "ShowDate" ) );
                        mergeValues.Add ( "ShowCurrent", options.Contains ( "ShowCurrent" ) );
                        mergeValues.Add ( "ShowGoal", options.Contains ( "ShowGoal" ) );
                        mergeValues.Add ( "ShowLastWeek", options.Contains ( "LastWeek" ) );
                        mergeValues.Add ( "ShowPreviousYear", options.Contains ( "PreviousYear" ) );
                        mergeValues.Add ( "ShowYTD", options.Contains ( "YTD" ) );
                        mergeValues.Add ( "Show52w", options.Contains ( "52w" ) );
                        mergeValues.Add ( "ShowPercent", options.Contains ( "Percent" ) );
                        mergeValues.Add ( "ShowTrend", options.Contains ( "ShowTrend" ) );
                        mergeValues.Add ( "RoundValue", options.Contains ( "Round" ) );
                        mergeValues.Add ( "UseDateCountForAverage", options.Contains ( "DateCount" ) );
                    }

                    string resultHtml = liquidTemplate.ResolveMergeFields( mergeValues );

                    // show liquid help for debug
                    if ( block.GetAttributeValue( "EnableDebug" ).AsBoolean() )
                    {
                        resultHtml += mergeValues.lavaDebugInfo();
                    }
                    // use long 4 hour cache timeout since metrics usually change daily
                    // disable for dev
                    //HtmlContentService.AddCachedContent ( blockId, cacheName, resultHtml, 14400 );
                    return resultHtml;
                }
                else
                {
                    return cachedContent;
                }
            }

            return string.Format(
                @"<div class='alert alert-danger'> 
                    unable to find block_id: {0}
                </div>",
                blockId );
        }

        private decimal? GetValue (MetricValueService metricValueService, Metric metric, DateTime startDate, DateTime endDate,
            List<EntityPair> entityTypeList, bool roundYValues,
            out int dateCount, out int weekCount )
        {
            var qryMeasureValues = metricValueService.Queryable ()
                    .Where ( a => a.MetricId == metric.Id )
                    .Where ( a => a.MetricValueDateTime >= startDate && a.MetricValueDateTime < endDate )
                    .Where ( a => a.MetricValueType == MetricValueType.Measure );

            //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
            //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting
            if ( entityTypeList != null && entityTypeList.Count > 0 )
            {

                foreach ( var item in entityTypeList )
                {
                    if ( metric.MetricPartitions.Any ( a => a.EntityTypeId == item.EntityTypeId.Value ) && item.EntityIds.Count > 0 )
                    {
                        qryMeasureValues = qryMeasureValues.Where ( a => a.MetricValuePartitions.Any ( p => item.EntityIds.Contains ( p.EntityId.Value ) && p.MetricPartition.EntityTypeId == item.EntityTypeId.Value ) );
                    }

                }
            }

            decimal? sum = qryMeasureValues.Sum ( a => a.YValue );
            dateCount = qryMeasureValues.Select ( a => a.MetricValueDateTime ).Distinct ().Count ();
            weekCount = qryMeasureValues.Select ( a => SqlFunctions.DateDiff ( "ww", startDate, a.MetricValueDateTime ) ).Distinct ().Count ();

            return sum.HasValue ? Math.Round ( sum.Value, roundYValues ? 0 : 2 ) : (decimal?) null;

        }

        private int CountDays( DayOfWeek day, DateTime start, DateTime end )
        {
            TimeSpan ts = end - start;                       // Total duration
            int count = (int) Math.Floor ( ts.TotalDays / 7 );   // Number of whole weeks
            int remainder = (int) ( ts.TotalDays % 7 );         // Number of remaining days
            int sinceLastDay = (int) ( end.DayOfWeek - day );   // Number of days since last [day]
            if ( sinceLastDay < 0 ) sinceLastDay += 7;         // Adjust for negative days since last [day]

            // If the days in excess of an even week are greater than or equal to the number days since the last [day], then count this one, too.
            if ( remainder >= sinceLastDay ) count++;

            return count;
        }
}

    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Reporting" )]

    public class EntityPair
    {
        /// <summary>
        /// The Entity Type
        /// </summary>
        public int? EntityTypeId;

        /// <summary>
        /// The list of Entity Ids of the given type
        /// </summary>
        public List<int> EntityIds;

        /// <summary>
        /// Construtor from type and string which containc a list of integers
        /// </summary>
        public EntityPair ( int? entityTypeId, string entityIds ) {

            EntityTypeId = entityTypeId;

            EntityIds = entityIds.Split( ';' ).AsIntegerList();
        }
    }


    /// <summary>
    /// Class that extends Metric to include the output values
    /// </summary>
    public class MetricsXData : Metric
    {
        /// <summary>
        /// Gets or sets the last value.
        /// </summary>
        /// <value>
        /// The last value.
        /// </value>
        [DataMember]
        public object LastValue { get; set; }

        /// <summary>
        /// Gets or sets the last value date.
        /// </summary>
        /// <value>
        /// The last value date.
        /// </value>
        [DataMember]
        public DateTime LastValueDate { get; set; }

        /// <summary>
        /// Gets or sets the previous weeks value.
        /// </summary>
        /// <value>
        /// The previous weeks value.
        /// </value>
        [DataMember]
        public object PreviousWeekValue { get; set; }

        /// <summary>
        /// Gets or sets the same week last year value.
        /// </summary>
        /// <value>
        /// The value for the current week in the previous year.
        /// </value>
        [DataMember]
        public object ThisWeekLastYearValue { get; set; }

        /// <summary>
        /// Gets or sets the cumulative value.
        /// </summary>
        /// <value>
        /// The cumulative value.
        /// </value>
        [DataMember]
        public object CumulativeValue { get; set; }

        /// <summary>
        /// Gets or sets the YTD weeks value.
        /// </summary>
        /// <value>
        /// The number of weeks so far this year value. (so you can calculate averages)
        /// </value>
        [DataMember]
        public object CumulativeValueDateCount { get; set; }

        /// <summary>
        /// Gets or sets the YTD weeks value.
        /// </summary>
        /// <value>
        /// The number of weeks so far this year value. (so you can calculate averages)
        /// </value>
        [DataMember]
        public object CumulativeValueWeekCount { get; set; }

        /// <summary>
        /// Gets or sets the YTD value for the same period last year.
        /// </summary>
        /// <value>
        /// The previous year YTD value.
        /// </value>
        [DataMember]
        public object PreviousYearCumulativeValue { get; set; }

        /// <summary>
        /// Gets or sets the YTD weeks value.
        /// </summary>
        /// <value>
        /// The number of weeks so far this year value. (so you can calculate averages)
        /// </value>
        [DataMember]
        public object PreviousYearCumulativeValueDateCount { get; set; }

        /// <summary>
        /// Gets or sets the YTD weeks value.
        /// </summary>
        /// <value>
        /// The number of weeks so far this year value. (so you can calculate averages)
        /// </value>
        [DataMember]
        public object PreviousYearCumulativeValueWeekCount { get; set; }


        /// <summary>
        /// Gets or sets the 52 week total value.
        /// </summary>
        /// <value>
        /// The 52 week total value.
        /// </value>
        [DataMember]
        public object Weeks52Value { get; set; }

        /// <summary>
        /// Gets or sets the 52 week total value.
        /// </summary>
        /// <value>
        /// The 52 week total value.
        /// </value>
        [DataMember]
        public object Weeks52ValueDateCount { get; set; }

        /// <summary>
        /// Gets or sets the 52 week total value.
        /// </summary>
        /// <value>
        /// The 52 week total value.
        /// </value>
        [DataMember]
        public object Weeks52ValueWeekCount { get; set; }

        /// <summary>
        /// Gets or sets the previous 52 week value.
        /// </summary>
        /// <value>
        /// The total value for the previous 52 weeks.
        /// </value>
        [DataMember]
        public object PreviousWeeks52Value { get; set; }
        /// <summary>
        /// Gets or sets the previous 52 week value.
        /// </summary>
        /// <value>
        /// The total value for the previous 52 weeks.
        /// </value>
        [DataMember]
        public object PreviousWeeks52ValueDateCount { get; set; }

        /// <summary>
        /// Gets or sets the previous 52 week value.
        /// </summary>
        /// <value>
        /// The total value for the previous 52 weeks.
        /// </value>
        [DataMember]
        public object PreviousWeeks52ValueWeekCount { get; set; }

        /// <summary>
        /// Gets or sets the goal value.
        /// </summary>
        /// <value>
        /// The goal value.
        /// </value>
        [DataMember]
        public object GoalValue { get; set; }

        /// <summary>
        /// Gets or sets the MetricCategoryId value.
        /// </summary>
        /// <value>
        /// The goal value.
        /// </value>
        [DataMember]
        public object CategoryId { get; set; }

    }

}
