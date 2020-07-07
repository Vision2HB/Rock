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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;

namespace com.bemadev.Blocks.Reporting
{
    /// <summary>
    /// Block for easily adding/editing metric values for any metric that has partitions of campus and service time.
    /// </summary>
    [DisplayName( "Service Metrics Entry (5 Column)" )]
    [Category( "Reporting" )]
    [Description( "Block for easily adding/editing metric values for any metric that has partitions of campus and service time." )]

    [IntegerField( "Weeks Back", "The number of weeks back to display in the 'Week of' selection.", false, 8, "", 1 )]
    [IntegerField( "Weeks Ahead", "The number of weeks ahead to display in the 'Week of' selection.", false, 0, "", 2 )]
    [CampusesField("Campuses", "The campuses to display.", true, "", "", 3)]
    [SchedulesField("Schedules", "The schedules to display.", true, "", "", 4)]
    [MetricCategoriesField("Metric1", "Select the metric to display (note: only metrics with a campus and schedule partition will displayed).", true, "", "", 5)]
    [MetricCategoriesField("Metric2", "Select the metric to display (note: only metrics with a campus and schedule partition will displayed).", false, "", "", 6)]
    [MetricCategoriesField("Metric3", "Select the metric to display (note: only metrics with a campus and schedule partition will displayed).", false, "", "", 7)]
    [MetricCategoriesField("Metric4", "Select the metric to display (note: only metrics with a campus and schedule partition will displayed).", false, "", "", 8)]
    [MetricCategoriesField("Metric5", "Select the metric to display (note: only metrics with a campus and schedule partition will displayed).", false, "", "", 9)]
    public partial class ServiceMetricsEntry5 : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _selectedCampusId { get; set; }
        private DateTime? _selectedWeekend { get; set; }
        private int? _selectedServiceId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _selectedCampusId = ViewState["sme5SelectedCampusId"] as int?;
            _selectedWeekend = ViewState["sme5SelectedWeekend"] as DateTime?;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMetricsSaved.Visible = false;

            if ( !Page.IsPostBack )
            {
                _selectedCampusId = GetBlockUserPreference( "CampusId" ).AsIntegerOrNull();

                if ( CheckSelection() )
                {
                    LoadDropDowns();
                    BindMetrics();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["sme5SelectedCampusId"] = _selectedCampusId;
            ViewState["sme5SelectedWeekend"] = _selectedWeekend;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindMetrics();
        }


        protected void rptrSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            switch( e.CommandName )
            {
                case "Campus":
                    _selectedCampusId = e.CommandArgument.ToString().AsIntegerOrNull();
                    break;
                case "Weekend":
                    _selectedWeekend = e.CommandArgument.ToString().AsDateTime();
                    break;

            }

            if ( CheckSelection() )
            {
                LoadDropDowns();
                BindMetrics();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMetric control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMetric_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item )
            {
                var nbMetricValue1 = e.Item.FindControl( "nbMetricValue1" ) as NumberBox;
                if ( nbMetricValue1 != null )
                {
                    nbMetricValue1.ValidationGroup = BlockValidationGroup;
                }
                var nbMetricValue2 = e.Item.FindControl("nbMetricValue2") as NumberBox;
                if (nbMetricValue2 != null)
                {
                    nbMetricValue2.ValidationGroup = BlockValidationGroup;
                }
                var nbMetricValue3 = e.Item.FindControl("nbMetricValue3") as NumberBox;
                if (nbMetricValue3 != null)
                {
                    nbMetricValue3.ValidationGroup = BlockValidationGroup;
                }
                var nbMetricValue4 = e.Item.FindControl("nbMetricValue4") as NumberBox;
                if (nbMetricValue4 != null)
                {
                    nbMetricValue4.ValidationGroup = BlockValidationGroup;
                }
                var nbMetricValue5 = e.Item.FindControl("nbMetricValue5") as NumberBox;
                if (nbMetricValue5 != null)
                {
                    nbMetricValue5.ValidationGroup = BlockValidationGroup;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            if ( campusId.HasValue && weekend.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var metricService = new MetricService( rockContext );
                    var metricValueService = new MetricValueService( rockContext );

                    foreach ( RepeaterItem item in rptrMetric.Items )
                    {
                        var hfServiceId = item.FindControl("hfServiceId") as HiddenField;
                        string[] columns = { "1", "2", "3", "4", "5" };
                        foreach ( var column in columns )
                        {
                            var hfMetricIId = item.FindControl( "hfMetricId" + column ) as HiddenField;
                            var nbMetricValue = item.FindControl( "nbMetricValue" + column ) as NumberBox;

                            if ( hfMetricIId != null && hfMetricIId.ValueAsInt() != 0 
                                && nbMetricValue != null && nbMetricValue.Text != "" 
                                && hfServiceId != null)
                            {
                                int metricId = hfMetricIId.ValueAsInt();
                                int serviceId = hfServiceId.ValueAsInt();
                                var metric = new MetricService( rockContext ).Get( metricId );

                                if ( metric != null )
                                {
                                    int campusPartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault();
                                    int schedulePartitionId = metric.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault();

                                    var metricValue = metricValueService
                                        .Queryable()
                                        .Where( v =>
                                            v.MetricId == metric.Id &&
                                            v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                            v.MetricValuePartitions.Count == 2 &&
                                            v.MetricValuePartitions.Any( p => p.MetricPartitionId == campusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                            v.MetricValuePartitions.Any( p => p.MetricPartitionId == schedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == serviceId ) )
                                        .FirstOrDefault();

                                    if ( metricValue == null )
                                    {
                                        metricValue = new MetricValue();
                                        metricValue.MetricValueType = MetricValueType.Measure;
                                        metricValue.MetricId = metric.Id;
                                        metricValue.MetricValueDateTime = weekend.Value;
                                        metricValueService.Add( metricValue );

                                        var campusValuePartition = new MetricValuePartition();
                                        campusValuePartition.MetricPartitionId = campusPartitionId;
                                        campusValuePartition.EntityId = campusId.Value;
                                        metricValue.MetricValuePartitions.Add( campusValuePartition );

                                        var scheduleValuePartition = new MetricValuePartition();
                                        scheduleValuePartition.MetricPartitionId = schedulePartitionId;
                                        scheduleValuePartition.EntityId = serviceId;
                                        metricValue.MetricValuePartitions.Add( scheduleValuePartition );
                                    }

                                    metricValue.YValue = nbMetricValue.Text.AsDecimalOrNull();
                                    metricValue.Note = tbNote.Text;
                                }
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                nbMetricsSaved.Text = string.Format( "Your metrics on {0} at the {1} Campus have been saved.",
                    bddlWeekend.SelectedItem.Text, bddlCampus.SelectedItem.Text );
                nbMetricsSaved.Visible = true;

                BindMetrics();

            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the filter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddl_SelectionChanged( object sender, EventArgs e )
        {
            BindMetrics();
        }

        #endregion

        #region Methods

        private bool CheckSelection()
        {
            // If campus and schedule have been selected before, assume current weekend
            if ( _selectedCampusId.HasValue && !_selectedWeekend.HasValue )
            {
                _selectedWeekend = RockDateTime.Today.SundayDate();
            }

            var options = new List<ServiceMetricSelectItem>();

            if ( !_selectedCampusId.HasValue )
            {
                lSelection.Text = "Select Location:";
                foreach ( var campus in GetCampuses() )
                {
                    options.Add( new ServiceMetricSelectItem( "Campus", campus.Id.ToString(), campus.Name ) );
                }
            }

            if ( !options.Any() && !_selectedWeekend.HasValue )
            {
                lSelection.Text = "Select Week of:";
                foreach ( var weekend in GetWeekendDates( 1, 0 ) )
                {
                    options.Add( new ServiceMetricSelectItem( "Weekend", weekend.ToString( "o" ), "Sunday " + weekend.ToShortDateString() ) );
                }
            }

            if ( options.Any() )
            {
                rptrSelection.DataSource = options;
                rptrSelection.DataBind();

                pnlSelection.Visible = true;
                pnlMetrics.Visible = false;

                return false;
            }
            else
            {
                pnlSelection.Visible = false;
                pnlMetrics.Visible = true;

                return true;
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            bddlCampus.Items.Clear();
            bddlWeekend.Items.Clear();

            // Load Campuses
            foreach ( var campus in GetCampuses() )
            {
                bddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
            bddlCampus.SetValue( _selectedCampusId.Value );

            // Load Weeks
            var weeksBack = GetAttributeValue( "WeeksBack" ).AsInteger();
            var weeksAhead = GetAttributeValue( "WeeksAhead" ).AsInteger();
            foreach ( var date in GetWeekendDates( weeksBack, weeksAhead ) )
            {
                bddlWeekend.Items.Add( new ListItem( "Sunday " + date.ToShortDateString(), date.ToString( "o" ) ) );
            }
            bddlWeekend.SetValue( _selectedWeekend.Value.ToString( "o" ) );

        }

        /// <summary>
        /// Gets the campuses.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampuses()
        {
            var campuses = new List<CampusCache>();
            var configCampuses = GetAttributeValues("Campuses");

            var pageFilterCampus = PageParameter("Campus").AsGuidOrNull();
            if (pageFilterCampus.HasValue)
            {
                foreach (var campus in CampusCache.All()
                    .Where(c => c.IsActive.HasValue && c.IsActive.Value
                       && c.Guid == pageFilterCampus.Value)
                    .OrderBy(c => c.Name))
                {
                    campuses.Add(campus);
                }

            }
            else
            {
                foreach (var campus in CampusCache.All()
                    .Where(c => c.IsActive.HasValue && c.IsActive.Value
                       && configCampuses.Contains(c.Guid.ToString()))
                    .OrderBy(c => c.Name))
                {
                    campuses.Add(campus);
                }
            }

            return campuses;
        }

        /// <summary>
        /// Gets the weekend dates.
        /// </summary>
        /// <returns></returns>
        private List<DateTime> GetWeekendDates( int weeksBack, int weeksAhead )
        {
            var dates = new List<DateTime>();
            var pageSundayDate = PageParameter("SundayDate").AsDateTime();

            if (pageSundayDate.HasValue)
            {
                // Load Week
                var sundayDate = RockDateTime.ConvertLocalDateTimeToRockDateTime(pageSundayDate.Value);
                dates.Add(sundayDate.AddDays(-6).SundayDate());
            }
            else
            {
                // Load Weeks
                var sundayDate = RockDateTime.Today.SundayDate();
                var daysBack = weeksBack * 7;
                var daysAhead = weeksAhead * 7;
                var startDate = sundayDate.AddDays(0 - daysBack);
                var date = sundayDate.AddDays(daysAhead);
                while (date >= startDate)
                {
                    dates.Add(date);
                    date = date.AddDays(-7);
                }

            }

            return dates;
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <returns></returns>
        private List<Schedule> GetServices()
        {
            var services = new List<Schedule>();

            var scheduleCategory = CategoryCache.Get( GetAttributeValue( "ScheduleCategory" ).AsGuid() );
            if ( scheduleCategory != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var schedule in new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s =>
                            s.CategoryId.HasValue &&
                            s.CategoryId.Value == scheduleCategory.Id )
                        .OrderBy( s => s.Name ) )
                    {
                        services.Add( schedule );
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Binds the metrics.
        /// </summary>
        private void BindMetrics()
        {
            var serviceMetricValues = new List<ServiceMetric>();

            int campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            int scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            int? campusId = bddlCampus.SelectedValueAsInt();
            DateTime? weekend = bddlWeekend.SelectedValue.AsDateTime();

            var notes = new List<string>();

            if ( campusId.HasValue && weekend.HasValue )
            {

                SetBlockUserPreference( "CampusId", campusId.HasValue ? campusId.Value.ToString() : "" );

                string[] columns = { "1", "2", "3", "4", "5" };
                var metricGuids = new List<Guid>();
                foreach ( var column in columns)
                {
                    var metricCategories = MetricCategoriesFieldAttribute.GetValueAsGuidPairs(GetAttributeValue("Metric" + column));
                    if (metricCategories.Count > 0)
                    {
                        metricGuids.Add(metricCategories.Select(a => a.MetricGuid).FirstOrDefault());
                    }
                }

                using ( var rockContext = new RockContext() )
                {
                    var schedules = GetAttributeValues("Schedules");
                    if (schedules.Count > 0)
                    {
                        foreach (var schedule in new ScheduleService(rockContext)
                            .Queryable().AsNoTracking()
                            .Where(s =>
                               schedules.Contains(s.Guid.ToString()))
                            .OrderBy(s => s.Name))
                        {
                            int cnt = 1;
                            var serviceMetric = new ServiceMetric(schedule.Id, schedule.Name);

                            var metricValueService = new MetricValueService( rockContext );
                            foreach ( var metric in new MetricService( rockContext )
                                .GetByGuids( metricGuids )
                                .Where( m =>
                                    m.MetricPartitions.Count == 2 &&
                                    m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ) &&
                                    m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ) )
                                .OrderBy( m => m.Title )
                                .Select( m => new
                                {
                                    m.Id,
                                    m.Title,
                                    CampusPartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                                    SchedulePartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                                } ) )
                            {

                                var metricValue = metricValueService
                                    .Queryable().AsNoTracking()
                                    .Where( v =>
                                        v.MetricId == metric.Id &&
                                        v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekend.Value &&
                                        v.MetricValuePartitions.Count == 2 &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.CampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.SchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == schedule.Id ) )
                                    .FirstOrDefault();

                                switch (cnt++)
                                {
                                    case 1:
                                        tbHeader1.Text = metric.Title;
                                        tbHeader1.Visible = true;
                                        serviceMetric.Visible1 = true;
                                        serviceMetric.MetricId1 = metric.Id;
                                        if (metricValue != null) serviceMetric.Value1 = metricValue.YValue;
                                        break;
                                    case 2:
                                        tbHeader2.Text = metric.Title;
                                        tbHeader2.Visible = true;
                                        serviceMetric.Visible2 = true;
                                        serviceMetric.MetricId2 = metric.Id;
                                        if (metricValue != null) serviceMetric.Value2 = metricValue.YValue;
                                        break;
                                    case 3:
                                        tbHeader3.Text = metric.Title;
                                        tbHeader3.Visible = true;
                                        serviceMetric.Visible3 = true;
                                        serviceMetric.MetricId3 = metric.Id;
                                        if (metricValue != null) serviceMetric.Value3 = metricValue.YValue;
                                        break;
                                    case 4:
                                        tbHeader4.Text = metric.Title;
                                        tbHeader4.Visible = true;
                                        serviceMetric.Visible4 = true;
                                        serviceMetric.MetricId4 = metric.Id;
                                        if (metricValue != null) serviceMetric.Value4 = metricValue.YValue;
                                        break;
                                    case 5:
                                        tbHeader5.Text = metric.Title;
                                        tbHeader5.Visible = true;
                                        serviceMetric.Visible5 = true;
                                        serviceMetric.MetricId5 = metric.Id;
                                        if (metricValue != null) serviceMetric.Value5 = metricValue.YValue;
                                        break;
                                }

                                if ( metricValue.IsNotNull() && !string.IsNullOrWhiteSpace ( metricValue.Note) &&
                                    !notes.Contains( metricValue.Note ) )
                                {
                                    notes.Add( metricValue.Note );
                                }

                            }

                            serviceMetricValues.Add( serviceMetric );
                        }
                    }
                }
            }

            rptrMetric.DataSource = serviceMetricValues;
            rptrMetric.DataBind();

            tbNote.Text = notes.AsDelimited( Environment.NewLine + Environment.NewLine );
        }

        #endregion

    }

    public class ServiceMetricSelectItem
    {
        public string CommandName { get; set; }
        public string CommandArg { get; set; }
        public string OptionText { get; set; }
        public ServiceMetricSelectItem( string commandName, string commandArg, string optionText )
        {
            CommandName = commandName;
            CommandArg = commandArg;
            OptionText = optionText;   
        }
    }

    public class ServiceMetric
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public int MetricId1 { get; set; }
        public decimal? Value1 { get; set; }
        public bool Visible1 { get; set; }
        public int MetricId2 { get; set; }
        public decimal? Value2 { get; set; }
        public bool Visible2 { get; set; }
        public int MetricId3 { get; set; }
        public decimal? Value3 { get; set; }
        public bool Visible3 { get; set; }
        public int MetricId4 { get; set; }
        public decimal? Value4 { get; set; }
        public bool Visible4 { get; set; }
        public int MetricId5 { get; set; }
        public decimal? Value5 { get; set; }
        public bool Visible5 { get; set; }

        public ServiceMetric(int id, string name)
        {
            ServiceId = id;
            Name = name;
            Visible1 = false;
            Visible2 = false;
            Visible3 = false;
            Visible4 = false;
            Visible5 = false;
        }
    }
}
