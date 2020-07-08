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
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com_bemaservices.Reporting.Dashboard
{
    /// <summary>
    /// NOTE: Most of the logic for processing the Attributes is in Rock.Rest.MetricsController.GetHtmlForBlock
    /// </summary>
    /// <seealso cref="Rock.Reporting.Dashboard.DashboardWidget" />
    [DisplayName( "BEMA Metric Dashboard Widget" )]
    [Category( "BEMA Services > Reporting > Dashboard" )]
    [Description( "Dashboard Widget from Lava using YTD, Last Year, 52 Week, Last Week metric values" )]
    [CodeEditorField ( "Lava Template", "The text (or html) to display as a dashboard widget", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, Order = 6, DefaultValue =
@"{% include '~/PlugIns/com_bemaservices/Reporting/Dashboard/Lava/BEMAMetricsBlock.lava' %}" )]
    [OutputCache ( NoStore = true, Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, VaryByParam = "*" )]


    public partial class MetricDashboardWidget : MetricWidget
    {

        /// <summary>
        /// Gets the rest URL.
        /// </summary>
        /// <value>
        /// The rest URL.
        /// </value>
        public string RestUrl
        {
            get
            {
                string result = ResolveUrl( "~/api/MetricsX/GetHtmlForBlock/" ) + BlockId.ToString();
                var metricIds = PageParameter ( "MetricIds" ).Replace ( ',', '|' );
                if ( metricIds ==  null || metricIds == "")
                {
                    var metricCategories = new MetricCategoryService ( new RockContext () ).GetByIds ( MetricCategoryIds ).ToList ();
                    metricIds = string.Join ( "|", metricCategories.Select ( m => m.MetricId ).Select ( n => n.ToString () ).ToArray () );
                }
                if ( metricIds != null && metricIds != "" )
                {
                    result += string.Format ( "?metricIds={0}",  metricIds);

                    string metricPartitions = GetPartitionsFromPageParameters ( metricIds );
                    if ( metricPartitions == null || metricPartitions == "" )
                    {
                        metricPartitions = GetAttributeValue ( "MetricEntityTypeEntityIds" );
                    }

                    if ( metricPartitions != null && metricPartitions != "" )
                    {
                        result += string.Format ( "&entityTypeEntityIds={0}", metricPartitions );
                    }

                    string options = GetAttributeValue ( "Options" );
                    if ( options != null )
                    {
                        result += string.Format ( "&options={0}", options );
                    }

                    DateRange dateRange;
                    var pageDateRange = PageParameter ( "DateRange" );
                    if ( pageDateRange != null && pageDateRange != "")
                    {
                        dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( pageDateRange );
                    }
                    else
                    {
                        dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues ( GetAttributeValue ( "DateRange" ) );
                    }

                    if ( dateRange != null )
                    {
                        if (dateRange.End > RockDateTime.Now)
                        {
                            dateRange.End = RockDateTime.Now;
                        }
                        result += string.Format ( "&dateRange={0},{1}",
                            dateRange.Start.ToString(), dateRange.End.ToString());
                    }

                }

                return result;
            }
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected void ShowSettings()
        {
            phHtml.Visible = false;
            base.ShowSettings ();
            phHtml.Visible = true;

            // reload page
            Response.Redirect ( Request.RawUrl, true );

        }
    }
}