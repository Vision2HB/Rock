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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.Mvc;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com_bemaservices.Reporting.Dashboard
{
    [DisplayName ( "BEMA Metric Chart" )]
    [Category ( "BEMA Services > Reporting > Dashboard" )]
    [Description ( "Block to display a chart using metrics and partitions as the chart datasource" )]

    [IntegerField ( "Chart Height", "", false, 200 )]
    [DefinedValueField ( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", order: 3 )]

    [BooleanField ( "Show Legend", "", true, order: 7 )]
    [CustomDropdownListField ( "Legend Position", "Select the position of the Legend (corner)", "ne,nw,se,sw", false, "ne", order: 8 )]
    [CustomDropdownListField ( "Chart Type", "", "Line,Bar,Pie", false, "Line", order: 9 )]
    [DecimalField ( "Pie Inner Radius", "If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.", false, 0, order: 10 )]
    [BooleanField ( "Pie Show Labels", "If this is a pie chart, specify if labels show be shown", true, "", order: 11 )]
    [OutputCache ( NoStore = true, Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, VaryByParam = "*" )]

    public partial class MetricChart : MetricWidget
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit ( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger ( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            base.OnLoad ( e );

            if ( PageParameter ( "GetChartData" ).AsBoolean () && ( PageParameter ( "GetChartDataBlockId" ).AsInteger () == this.BlockId ) && PageParameter ( "MetricId" ).AsInteger () > 0 )
            {
                GetChartData ( PageParameter ( "MetricId" ).AsInteger () );
            }
            else
            {

                var rockContext = new RockContext ();

                MetricService metricService = new MetricService ( rockContext );
                MetricValueService metricValueService = new MetricValueService ( rockContext );
                string options = GetAttributeValue ( "Options" );
                var pageReference = new Rock.Web.PageReference ( this.PageCache.Id );
                pageReference.QueryString = new System.Collections.Specialized.NameValueCollection ();
                pageReference.QueryString.Add ( this.Request.QueryString );
                pageReference.QueryString.Add ( "GetChartData", "true" );
                pageReference.QueryString.Add ( "GetChartDataBlockId", this.BlockId.ToString () );
                pageReference.QueryString.Add ( "TimeStamp", RockDateTime.Now.ToJavascriptMilliseconds ().ToString () );

                DateRange dateRange;
                var pageDateRange = PageParameter ( "DateRange" );
                if ( pageDateRange != null & pageDateRange != "" )
                {
                    dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( pageDateRange );
                }
                else
                {
                    dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( GetAttributeValue ( "DateRange" ) );
                }

                var metrics = PageParameter ( "MetricIds" );
                List<int> metricIds;
                if ( metrics == null || metrics == "" )
                {
                    var metricCategories = new MetricCategoryService ( rockContext ).GetByIds ( MetricCategoryIds ).ToList ();
                    metricIds = metricCategories.Select ( m => m.MetricId ).ToList ();
                }
                else
                {
                    metricIds = metrics.Split ( ',' ).Select ( Int32.Parse ).ToList ();
                }

                foreach ( var metric in metricService.GetByIds ( metricIds ) )
                {
                    if ( RockPage.CurrentPerson != null && metric.IsAuthorized ( Rock.Security.Authorization.VIEW, RockPage.CurrentPerson ) )
                    {
                        if ( options != null )
                        {
                            var sTitle = string.Empty;
                            if ( options.Contains ( "Icon" ) )
                            {
                                sTitle += "<i class=\"" + metric.IconCssClass + "\"></i> ";
                            }

                            if ( options.Contains ( "MetricTitle" ) )
                            {
                                sTitle += metric.Title;
                            }

                            if ( options.Contains ( "Date" ) )
                            {
                                sTitle += " (" + dateRange.Start.Value.ToShortDateString () + " - " + dateRange.End.Value.ToShortDateString () + ")";
                            }

                            var sSubTitle = string.Empty;
                            if ( options.Contains ( "SubTitle" ) )
                            {
                                sSubTitle += metric.Subtitle;
                            }

                            if ( options.Contains ( "Description" ) )
                            {
                                if ( sSubTitle != string.Empty )
                                {
                                    sSubTitle += "<br/>";
                                }
                                sSubTitle += metric.Description;
                            }

                            if ( sTitle != string.Empty )
                            {
                                var title = new RockLiteral ();
                                title.ID = "Title" + metric.Id.ToString ();
                                title.Visible = !string.IsNullOrEmpty ( metric.Title );
                                title.Text = "<div class=\"ChartTitle\">" + sTitle + "</div>";
                                if ( sSubTitle != string.Empty )
                                {
                                    title.Text += "<div class=\"ChartSubtitle\">" + sSubTitle + "</div>";
                                }
                                phCharts.Controls.Add ( title );
                            }
                        }
                        else
                        {
                            if ( !string.IsNullOrEmpty ( this.Title ) )
                            {
                                var title = new RockLiteral ();
                                title.ID = "Title" + metric.Id.ToString ();
                                title.Visible = true;
                                title.Text = this.Title;
                                phCharts.Controls.Add ( title );
                            }

                            if ( !string.IsNullOrEmpty ( this.Subtitle ) )
                            {
                                var title = new RockLiteral ();
                                title.ID = "SubTitle" + metric.Id.ToString ();
                                title.Visible = true;
                                title.Text = this.Subtitle;
                                phCharts.Controls.Add ( title );
                            }
                        }

                        var chartType = this.GetAttributeValue ( "ChartType" );
                        if ( chartType == "Pie" )
                        {
                            var newChart = new PieChart ();
                            newChart.ID = "PieChart" + metric.Id.ToString ();
                            newChart.DataSourceUrl = pageReference.BuildUrl () + "&MetricId=" + metric.Id.ToString ();
                            newChart.ChartHeight = this.GetAttributeValue ( "ChartHeight" ).AsIntegerOrNull () ?? 200;
                            newChart.Options.legend = newChart.Options.legend ?? new Legend ();
                            newChart.Options.legend.show = this.GetAttributeValue ( "ShowLegend" ).AsBooleanOrNull ();
                            newChart.Options.legend.position = this.GetAttributeValue ( "LegendPosition" );
                            // Set chart style after setting options so they are not overwritten.
                            newChart.Options.SetChartStyle ( this.GetAttributeValue ( "ChartStyle" ).AsGuidOrNull () );

                            phCharts.Controls.Add ( newChart );
                        }
                        else if ( chartType == "Bar" )
                        {
                            var newChart = new BarChart ();
                            newChart.ID = "BarChart" + metric.Id.ToString ();
                            newChart.DataSourceUrl = pageReference.BuildUrl () + "&MetricId=" + metric.Id.ToString ();
                            newChart.ChartHeight = this.GetAttributeValue ( "ChartHeight" ).AsIntegerOrNull () ?? 200;
                            newChart.Options.legend = newChart.Options.legend ?? new Legend ();
                            newChart.Options.legend.show = this.GetAttributeValue ( "ShowLegend" ).AsBooleanOrNull ();
                            newChart.Options.legend.position = this.GetAttributeValue ( "LegendPosition" );
                            // Set chart style after setting options so they are not overwritten.
                            newChart.Options.SetChartStyle ( this.GetAttributeValue ( "ChartStyle" ).AsGuidOrNull () );

                            phCharts.Controls.Add ( newChart );
                        }
                        else
                        {
                            var newChart = new LineChart ();
                            newChart.ID = "LineChart" + metric.Id.ToString ();
                            newChart.DataSourceUrl = pageReference.BuildUrl () + "&MetricId=" + metric.Id.ToString ();
                            newChart.ChartHeight = this.GetAttributeValue ( "ChartHeight" ).AsIntegerOrNull () ?? 200;
                            newChart.Options.legend = newChart.Options.legend ?? new Legend ();
                            newChart.Options.legend.show = this.GetAttributeValue ( "ShowLegend" ).AsBooleanOrNull ();
                            newChart.Options.legend.position = this.GetAttributeValue ( "LegendPosition" );
                            // Set chart style after setting options so they are not overwritten.
                            newChart.Options.SetChartStyle ( this.GetAttributeValue ( "ChartStyle" ).AsGuidOrNull () );

                            phCharts.Controls.Add ( newChart );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DynamicChartData : Rock.Chart.IChartData
        {
            /// <summary>
            /// Gets the date time stamp.
            /// </summary>
            /// <value>
            /// The date time stamp.
            /// </value>
            public long DateTimeStamp { get; set; }

            /// <summary>
            /// Gets the y value (for Line and Bar Charts)
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValue { get; set; }

            /// <summary>
            /// Gets the y value as a formatted string (for Line and Bar Charts)
            /// </summary>
            /// <value>
            /// The formatted y value.
            /// </value>
            public string YValueFormatted { get; set; }

            /// <summary>
            /// Gets or sets the metric title (for pie charts)
            /// </summary>
            /// <value>
            /// The metric title.
            /// </value>
            public string MetricTitle { get; set; }

            /// <summary>
            /// Gets the y value (for pie charts)
            /// </summary>
            /// <value>
            /// The y value.
            /// </value>
            public decimal? YValueTotal { get; set; }

            /// <summary>
            /// Gets the series identifier (obsolete)
            /// NOTE: Use MetricValuePartitionEntityIds if you are populating this with a EntityTypeId|EntityId list, or use SeriesName for a static series name
            /// </summary>
            /// <value>
            /// The series identifier.
            /// </value>
            [Obsolete ( "Use MetricValuePartitionEntityIds if you are populating this with a EntityTypeId|EntityId list, or use SeriesName for a static series name" )]
            public string SeriesId { get; set; }

            /// <summary>
            /// Gets or sets the name of the series. This will be the default name of the series if MetricValuePartitionEntityIds can't be resolved
            /// </summary>
            /// <value>
            /// The name of the series.
            /// </value>
            public string SeriesName { get; set; }

            /// <summary>
            /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
            /// </summary>
            /// <value>
            /// The metric value entityTypeId,EntityId partitions
            /// </value>
            public string MetricValuePartitionEntityIds { get; set; }
        }

        /// <summary>
        /// Gets the chart data (ajax call from Chart)
        /// </summary>
        private void GetChartData( int metricId )
        {
            var show52 = GetAttributeValue ( "Options" ).Contains ( "Show52" );
            var show26 = GetAttributeValue ( "Options" ).Contains ( "Show26" );
            var show13 = GetAttributeValue ( "Options" ).Contains ( "Show13" );
            var showTrend = GetAttributeValue ( "Options" ).Contains ( "ShowTrend" );

            var rockContext = new RockContext ();
            try
            {
                nbConfigurationWarning.Text = string.Empty;
                nbConfigurationWarning.Visible = false;

                MetricService metricService = new MetricService ( rockContext );
                MetricValueService metricValueService = new MetricValueService ( rockContext );
                var metric = metricService.Get ( metricId );

                DateRange dateRange;
                var pageDateRange = PageParameter ( "DateRange" );
                if ( pageDateRange != null & pageDateRange != "" )
                {
                    dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( pageDateRange );
                }
                else
                {
                    dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( GetAttributeValue ( "DateRange" ) );
                }

                var qryMeasureValues = metricValueService.Queryable ()
                    .Where ( a => a.MetricId == metric.Id );

                //// if an entityTypeId/EntityId filter was specified, and the entityTypeId is the same as the metric's partitions' EntityTypeId, filter the values to the specified entityId
                //// Note: if a Metric or it's Metric Value doesn't have a context, include it regardless of Context setting

                var entityTypeList = new List<EntityPair> ();

                string metricPartitions = GetPartitionsFromPageParameters ( metricId.ToString () );

                if ( metricPartitions == null || metricPartitions == "" )
                {
                    metricPartitions = GetAttributeValue ( "MetricEntityTypeEntityIds" );
                }

                if ( metricPartitions != null && metricPartitions != "" )
                {
                    entityTypeList = metricPartitions.Split ( ',' ).Select ( a => a.Split ( '|' ) ).Where ( a => a.Length == 2 )
                        .Select ( a => new EntityPair ( a[0].AsIntegerOrNull (), a[1] ) ).ToList ();

                    foreach ( var item in entityTypeList )
                    {
                        if ( metric.MetricPartitions.Any ( a => a.EntityTypeId == item.EntityTypeId.Value ) && item.EntityIds.Count > 0 )
                        {
                            qryMeasureValues = qryMeasureValues.Where ( a => a.MetricValuePartitions.Any ( p => item.EntityIds.Contains ( p.EntityId.Value ) && p.MetricPartition.EntityTypeId == item.EntityTypeId.Value ) );
                        }

                    }
                }

                var qryCurrentData = qryMeasureValues
                    .Where ( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime < dateRange.End )
                    .Where ( a => a.MetricValueType == MetricValueType.Measure );

                var results = qryCurrentData.GroupBy ( v => v.MetricValueDateTime )
                    .Select ( s => new PointValue() { YValue = s.Sum ( y => y.YValue ), MetricValueDateTime = s.Key } )
                    .OrderBy ( s => s.MetricValueDateTime )
                    .ToList ();

                var trendline = new Trendline ( results, metric.IsCumulative );

                var avgStart = dateRange.Start.Value.AddDays ( -52 * 7 );
                var qryAvgData = qryMeasureValues
                    .Where ( a => a.MetricValueDateTime >= avgStart
                        && a.MetricValueDateTime < dateRange.End.Value )
                    .Where ( a => a.MetricValueType == MetricValueType.Measure );

                var avgData = qryAvgData.GroupBy ( v => v.MetricValueDateTime )
                    .Select ( s => new PointValue ()
                     { YValue = s.Sum ( y => y.YValue ), MetricValueDateTime = s.Key } )
                    .OrderBy ( s => s.MetricValueDateTime )
                    .ToList ()
                    .AsQueryable ();

                List<DynamicChartData> chartDataList = new List<DynamicChartData> ();
                decimal cumulativeTotal = 0;
                foreach ( var row in results )
                {
                    var chartData = new DynamicChartData ();
                    chartData.SeriesName = chartData.MetricTitle = metric.Title;

                    if ( metric.IsCumulative == true )
                    {
                        cumulativeTotal += Convert.ToDecimal ( row.YValue );
                        chartData.YValue = chartData.YValueTotal = cumulativeTotal;
                    }
                    else
                    {
                        chartData.YValue = chartData.YValueTotal = Convert.ToDecimal ( row.YValue );
                    }
                    chartData.YValueFormatted = chartData.YValue.HasValue ? chartData.YValue.Value.ToString ( "G29" ) : string.Empty;
                    chartData.DateTimeStamp = row.MetricValueDateTime.Value.ToJavascriptMilliseconds ();

                    chartDataList.Add ( chartData );

                    if ( show52 )
                    {
                        /* Add 52 Week Moving Average */
                        var avgValue = GetMovingAverage ( row.MetricValueDateTime.Value, 52, avgData, metric.IsCumulative, "52w Avg" );
                        chartDataList.Add ( avgValue);
                    }

                    if ( show26 )
                    {
                        /* Add 26 Week Moving Average */
                        var avgValue = GetMovingAverage ( row.MetricValueDateTime.Value, 26, avgData, metric.IsCumulative, "26w Avg" );
                        chartDataList.Add ( avgValue );
                    }

                    if ( show13 )
                    {
                        /* Add 13 Week Moving Average */
                        var avgValue = GetMovingAverage ( row.MetricValueDateTime.Value, 13, avgData, metric.IsCumulative, "13w Avg" );
                        chartDataList.Add ( avgValue );
                    }
                }

                if ( showTrend && results.Count () > 2 )
                {
                    /* Add Trendline Value */
                    var trendValue = new DynamicChartData ();
                    trendValue.YValue = trendValue.YValueTotal = trendline.GetYValue ( dateRange.Start );
                    trendValue.SeriesName = trendValue.MetricTitle = "Trend";
                    trendValue.YValueFormatted = trendValue.YValue.HasValue ? trendValue.YValue.Value.ToString ( "G29" ) : string.Empty;
                    trendValue.DateTimeStamp = dateRange.Start.Value.ToJavascriptMilliseconds ();

                    chartDataList.Add ( trendValue );

                    trendValue = new DynamicChartData ();
                    trendValue.YValue = trendValue.YValueTotal = trendline.GetYValue ( dateRange.End );
                    trendValue.SeriesName = trendValue.MetricTitle = "Trend";
                    trendValue.YValueFormatted = trendValue.YValue.HasValue ? trendValue.YValue.Value.ToString ( "G29" ) : string.Empty;
                    trendValue.DateTimeStamp = dateRange.End.Value.ToJavascriptMilliseconds ();

                    chartDataList.Add ( trendValue );
                }

                /* Add Goal Line */
                if ( GetAttributeValue ( "Options" ).Contains ( "Goal" ) )
                {
                    qryCurrentData = qryMeasureValues
                        .Where ( a => a.MetricValueDateTime >= dateRange.Start && a.MetricValueDateTime < dateRange.End )
                        .Where ( a => a.MetricValueType == MetricValueType.Goal );

                    results = qryCurrentData.GroupBy ( v => v.MetricValueDateTime )
                        .Select ( s => new PointValue() { YValue = s.Sum ( y => y.YValue ), MetricValueDateTime = s.Key } )
                        .OrderBy ( s => s.MetricValueDateTime )
                        .ToList ();

                    foreach ( var row in results )
                    {
                        var chartData = new DynamicChartData ();
                        chartData.SeriesName = chartData.MetricTitle = "Goal";

                        chartData.YValue = chartData.YValueTotal = Convert.ToDecimal ( row.YValue );
                        chartData.YValueFormatted = chartData.YValue.HasValue ? chartData.YValue.Value.ToString ( "G29" ) : string.Empty;
                        chartData.DateTimeStamp = row.MetricValueDateTime.Value.ToJavascriptMilliseconds ();

                        chartDataList.Add ( chartData );
                    }
                }

                /* Add Previous Year Line */
                if ( GetAttributeValue ( "Options" ).Contains ( "PreviousYear" ) )
                {
                    var prevStart = dateRange.Start.Value.AddYears( -1 );
                    var prevEnd = dateRange.End.Value.AddYears ( -1 );

                    qryCurrentData = qryMeasureValues
                        .Where ( a => a.MetricValueDateTime >= prevStart && a.MetricValueDateTime < prevEnd )
                        .Where ( a => a.MetricValueType == MetricValueType.Measure );

                    results = qryCurrentData.GroupBy ( v => v.MetricValueDateTime )
                        .Select ( s => new PointValue() { YValue = s.Sum ( y => y.YValue ), MetricValueDateTime = s.Key } )
                        .OrderBy ( s => s.MetricValueDateTime )
                        .ToList ();

                    cumulativeTotal = 0;

                    foreach ( var row in results )
                    {
                        var chartData = new DynamicChartData ();
                        chartData.SeriesName = chartData.MetricTitle = "Last Year";

                        if ( metric.IsCumulative == true )
                        {
                            cumulativeTotal += Convert.ToDecimal ( row.YValue );
                            chartData.YValue = chartData.YValueTotal = cumulativeTotal;
                        }
                        else
                        {
                            chartData.YValue = chartData.YValueTotal = Convert.ToDecimal ( row.YValue );
                        }

                        chartData.YValueFormatted = chartData.YValue.HasValue ? chartData.YValue.Value.ToString ( "G29" ) : string.Empty;
                        chartData.DateTimeStamp = row.MetricValueDateTime.Value.AddYears ( 1 ).ToJavascriptMilliseconds ();

                        chartDataList.Add ( chartData );
                    }
                }

                /* Output Chart Data */
                chartDataList = chartDataList.OrderBy ( a => a.SeriesName ).ThenBy ( a => a.DateTimeStamp ).ToList ();

                Response.Clear ();
                Response.Write ( chartDataList.ToJson () );
                Response.End ();
            }
            catch ( System.Threading.ThreadAbortException )
            {
                // ignore the ThreadAbort exception from Response.End();
            }
            catch ( Exception ex )
            {
                LogException ( ex );
                throw;
            }
        }

        /// <summary>
        /// Get a Moving Average Value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected DynamicChartData GetMovingAverage( DateTime valueDate, int weeksBack, IQueryable< PointValue > rawData, Boolean isCumulative, string seriesName )
        {
            var returnVal = new DynamicChartData ();
            var avgStart = valueDate.AddDays ( weeksBack * -7 );
            var avgEnd = valueDate;

            var avgData = rawData
                .Where ( a => a.MetricValueDateTime >= avgStart && a.MetricValueDateTime < avgEnd );

            var avgValue = avgData.Sum ( y => y.YValue );
            var avgCount = avgData
                .Select ( y => y.MetricValueDateTime )
                .Distinct ()
                .Count ();

            returnVal.SeriesName = returnVal.MetricTitle = seriesName;

            if ( isCumulative == true )
            {
                /* annualize the value */
                returnVal.YValue = returnVal.YValueTotal = avgValue * 52 / weeksBack;
            }
            else
            {
                if (avgCount > 0)
                {
                    returnVal.YValue = returnVal.YValueTotal = Convert.ToDecimal ( avgValue ) / avgCount;
                }
                else
                {
                    returnVal.YValue = 0;
                }
            }

            returnVal.YValueFormatted = returnVal.YValue.HasValue ? returnVal.YValue.Value.ToString ( "G29" ) : string.Empty;
            returnVal.DateTimeStamp = valueDate.ToJavascriptMilliseconds ();

            return returnVal;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the full page since controls are dynamically created based on block settings
            NavigateToPage ( this.CurrentPageReference );
        }

    }
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
        public EntityPair( int? entityTypeId, string entityIds )
        {

            EntityTypeId = entityTypeId;

            EntityIds = entityIds.Split ( ';' ).AsIntegerList ();
        }
    }

    public class PointValue
    {
        /// <summary>
        /// The Metric Value Class
        /// </summary>
        public Decimal? YValue;

        /// <summary>
        /// The list of Entity Ids of the given type
        /// </summary>
        public DateTime? MetricValueDateTime;

        /// <summary>
        /// Construtor from Decimal and DateTime
        /// </summary>
        public PointValue( Decimal? yValue, DateTime? metricValueDateTime)
        {

            YValue = yValue;

            MetricValueDateTime = metricValueDateTime;
        }

        /// <summary>
        /// Construtor from Decimal and DateTime
        /// </summary>
        public PointValue ()
        {
            YValue = null;
            MetricValueDateTime = null;
        }

    }

    public class Trendline
    {
        public Trendline( IEnumerable<PointValue> dataPoints, Boolean isCumulative )
        {
            int count = 0;
            decimal sumX = 0;
            decimal sumX2 = 0;
            decimal sumY = 0;
            decimal sumXY = 0;
            decimal cumulativeTotal = 0;

            if ( dataPoints.Count () < 2 )
                return;

            Base = dataPoints.First ().MetricValueDateTime.Value.ToJavascriptMilliseconds () / 1000;

            foreach ( var dataPoint in dataPoints )
            {
                count++;
                var xValue = dataPoint.MetricValueDateTime.Value.ToJavascriptMilliseconds () / 1000 - Base;
                var yValue = dataPoint.YValue.Value;
                if (isCumulative)
                {
                    cumulativeTotal += yValue;
                    yValue = cumulativeTotal;
                }

                sumX += xValue;
                sumX2 += xValue * xValue;
                sumY += yValue;
                sumXY +=  xValue * yValue;
            }

            Slope = ( sumXY - ( ( sumX * sumY ) / count ) ) / ( sumX2 - ( ( sumX * sumX ) / count ) );
            Intercept = ( sumY / count ) - ( Slope * ( sumX / count ) );
        }

        public decimal Slope { get; private set; }
        public decimal Intercept { get; private set; }
        public decimal Start { get; private set; }
        public decimal End { get; private set; }
        public long Base { get; private set; }

        public decimal GetYValue( DateTime? metricValueDateTime )
        {
            var xValue = metricValueDateTime.Value.ToJavascriptMilliseconds () / 1000 - Base;
            return Slope * xValue + Intercept;
        }
    }
}
