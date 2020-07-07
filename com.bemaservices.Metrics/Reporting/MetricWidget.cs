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
    [DisplayName( "Metric Dashboard Widget" )]
    [Category( "BEMA Services > Reporting > Dashboard" )]
    [Description( "Dashboard Widget from Lava using YTD, Last Year, 52 Week, Last Week metric values" )]
    [LinkedPage ( "Detail Page", "Select the page to navigate to when the chart is clicked", false, Order = 9 )]

    [MetricCategoriesField ( "Metric", "Select the metric(s) to be made available to liquid", false, "", "CustomSetting", Key = "MetricCategories", Order = 4 )]

    // The metric value partitions as a position sensitive comma-delimited list of EntityTypeId|EntityId
    [TextField ( "MetricEntityTypeEntityIds", "", false, "", "CustomSetting" )]

    [CustomCheckboxListField ( "Options to Show", "Select which options to display in the widget", "Title,SubTitle,Description,Icon,Date,Goal,Last Week,YTD/AVG,52 Week Total/Avg,Previous Year Comparison", false,"MetricTitle|Icon|SubTitle|Date|LastYear|PreviousYear|YTD|Percent|Round", "CustomSetting", Order = 4, Key = "Options" )]
    [SlidingDateRangeField ( "Date Range", Key = "DateRange", Category = "CustomSetting", DefaultValue = "1||4||", Order = 6 )]


    public abstract class MetricWidget : DashboardWidget
    {
        private Panel pnlEditModel
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel> ().First ( a => a.ID == "pnlEditModel" );
            }
        }

        private MetricCategoryPicker mpMetricCategoryPicker
        {
            get
            {
                return this.ControlsOfTypeRecursive<MetricCategoryPicker> ().First ( a => a.ID == "mpMetricCategoryPicker" );
            }
        }


        private PlaceHolder phMetricValuePartitions
        {
            get
            {
                return this.ControlsOfTypeRecursive<PlaceHolder> ().First ( a => a.ID == "phMetricValuePartitions" );
            }
        }

        private ModalDialog mdEdit
        {
            get
            {
                return this.ControlsOfTypeRecursive<ModalDialog> ().First ( a => a.ID == "mdEdit" );
            }
        }

        private NotificationBox nbMetricWarning
        {
            get
            {
                return this.ControlsOfTypeRecursive<NotificationBox> ().First ( a => a.ID == "nbMetricWarning" );
            }
        }

        private Panel pnlDashboardTitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel> ().First ( a => a.ID == "pnlDashboardTitle" );
            }
        }

        private Panel pnlDashboardSubtitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Panel> ().First ( a => a.ID == "pnlDashboardSubtitle" );
            }
        }

        private Literal lDashboardTitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Literal> ().First ( a => a.ID == "lDashboardTitle" );
            }
        }

        private Literal lDashboardSubtitle
        {
            get
            {
                return this.ControlsOfTypeRecursive<Literal> ().First ( a => a.ID == "lDashboardSubtitle" );
            }
        }

        private SlidingDateRangePicker drpSlidingDateRange
        {
            get
            {
                return this.ControlsOfTypeRecursive<SlidingDateRangePicker> ().First ( a => a.ID == "drpSlidingDateRange" );
            }
        }

        private RockCheckBoxList cblOptions
        {
            get
            {
                return this.ControlsOfTypeRecursive<RockCheckBoxList> ().First ( a => a.ID == "cblOptions" );
            }
        }

        /// <summary>
        /// Gets the metric identifier.
        /// </summary>
        /// <value>
        /// The metric identifier.
        /// </value>
        public List<int> MetricCategoryIds
        {
            get
            {
                var valueParts = GetAttributeValue ( "MetricCategories" ).Split ( '|' ).AsIntegerList ();
                var metricCategoryIds = new MetricCategoryService (new RockContext ()).GetByIds ( valueParts ).Select ( m => m.Id ).ToList ();
                return metricCategoryIds;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="metricIds">'|' separated list of Ids.</param>
        protected string GetPartitionsFromPageParameters( string metricIds )
        {
            var returnVal = string.Empty;
            var foundList = new List<int>();
            var rockContext = new RockContext ();

            var metricList = metricIds.Split ( '|' ).AsIntegerList ();
            List<Metric> metrics = new MetricService ( new RockContext () ).GetByIds ( metricList ).ToList ();
            if ( metrics != null && metrics.Count > 0 )
            {
                foreach ( var metricPartition in metrics.SelectMany ( m => m.MetricPartitions ).ToList () )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get ( metricPartition.EntityTypeId.Value );

                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null
                            && !foundList.Contains ( entityTypeCache.Id ) )
                        {
                            // Look for {EntityName}Id or {EntityName}Ids in the page parameters
                            var paramId = string.Format ( "{0}Id", entityTypeCache.FriendlyName );
                            var paramIds = string.Format ( "{0}Ids", entityTypeCache.FriendlyName );
                            var paramGuids = string.Format ( "{0}Guids", entityTypeCache.FriendlyName );
                            var param = PageParameter ( paramIds ).Replace ( ',', ';' );
                            if ( param == null || param == "" )
                            {
                                param = PageParameter ( paramId );
                            }
                            if ( param == null || param == "" )
                            {
                                var guids = PageParameter ( paramGuids );
                                if (guids != null && guids != "")
                                {
                                    switch (entityTypeCache.SingleValueFieldType.Guid.ToString().ToUpper())
                                    {
                                        case Rock.SystemGuid.FieldType.CAMPUS:
                                            var campusList = new CampusService( rockContext ).GetByGuids(guids.Split(',').AsGuidList()).ToList();
                                            param = string.Join(";", campusList.Select ( n => n.Id.ToString () ).ToArray () );
                                            break;
                                        case Rock.SystemGuid.FieldType.SCHEDULE:
                                            var scheduleList = new ScheduleService ( rockContext ).GetByGuids ( guids.Split ( ',' ).AsGuidList () ).ToList ();
                                            param = string.Join ( ";", scheduleList.Select ( n => n.Id.ToString () ).ToArray () );
                                            break;
                                        case Rock.SystemGuid.FieldType.DEFINED_VALUE:
                                            var definedValueList = new DefinedValueService ( rockContext ).GetByGuids ( guids.Split ( ',' ).AsGuidList () ).ToList ();
                                            param = string.Join ( ";", definedValueList.Select ( n => n.Id.ToString () ).ToArray () );
                                            break;
                                    }
                                }
                            }
                            if ( param != null && param != "")
                            {
                                // Add parameter to list
                                returnVal = string.Format ( "{0},{1}|{2}", returnVal, entityTypeCache.Id, param );
                                foundList.Add ( entityTypeCache.Id );
                            }
                        }
                    }
                }
            }

            // remove leading ,
            if ( returnVal != string.Empty )
            {
                returnVal = returnVal.Right ( returnVal.Length - 1 );
            }
            return returnVal;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );
            pnlDashboardTitle.Visible = !string.IsNullOrEmpty( Title );
            pnlDashboardSubtitle.Visible = !string.IsNullOrEmpty( Subtitle );
            lDashboardTitle.Text = Title;
            lDashboardSubtitle.Text = Subtitle;

            CreateDynamicControls ( MetricCategoryIds );
        }

        #region custom settings

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public virtual string SettingsToolTip
        {
            get { return "Settings"; }
        }

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="System.Boolean" /> flag that indicates if the user can configure the <see cref="Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="System.Boolean" /> flag that indicates if the user can edit the <see cref="Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{Control}" /> containing all the icon <see cref="System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control> ();

            if ( canEdit )
            {
                LinkButton lbEdit = new LinkButton ();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = SettingsToolTip;
                lbEdit.Click += lbEdit_Click;
                configControls.Add ( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl ( "i" );
                lbEdit.Controls.Add ( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add ( "class", "fa fa-pencil-square-o" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent ( Page ).RegisterAsyncPostBackControl ( lbEdit );
            }

            configControls.AddRange ( base.GetAdministrateControls ( canConfig, canEdit ) );

            return configControls;
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowSettings ();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected void ShowSettings()
        {
            pnlEditModel.Visible = true;

            var rockContext = new RockContext ();
            var metricCategoryService = new MetricCategoryService ( rockContext );
            List<MetricCategory> metricCategories = null;
            if ( MetricCategoryIds.Count > 0 )
            {
                mpMetricCategoryPicker.SetValues ( MetricCategoryIds );
                metricCategories = metricCategoryService.GetByIds ( MetricCategoryIds ).ToList ();
            }

            if ( metricCategories != null && metricCategories.Count > 0 )
            {
                var entityTypeEntityIds = ( GetAttributeValue ( "MetricEntityTypeEntityIds" ) ?? string.Empty ).Split ( ',' ).Select ( a => a.Split ( '|' ) ).Where ( a => a.Length == 2 ).Select ( a => new
                {
                    EntityTypeId = a[0].AsIntegerOrNull (),
                    EntityId = a[1].AsIntegerOrNull ()
                } ).ToList ();

                foreach ( var item in entityTypeEntityIds )
                {
                    var controlId = string.Format ( "metricPartition{0}_entityTypeEditControl", item.EntityTypeId );
                    Control entityTypeEditControl = phMetricValuePartitions.FindControl ( controlId );
                    var entityType = EntityTypeCache.Get ( item.EntityTypeId.Value );

                    ( entityType.SingleValueFieldType.Field as IEntityFieldType ).SetEditValueFromEntityId ( entityTypeEditControl, new Dictionary<string, ConfigurationValue> (), item.EntityId );

                }
            }

            var options = GetAttributeValue ( "Options" ).ToString ();
            if (options != null)
            {
                cblOptions.SetValues (options.Split ( '|' ).ToList () );
            }

            drpSlidingDateRange.DelimitedValues = GetAttributeValue ( "DateRange" );

            mdEdit.Show ();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEdit_SaveClick( object sender, EventArgs e )
        {
            var metricCategoryIds = mpMetricCategoryPicker.SelectedValuesAsInt ().ToList ();
            var metricCategories = new MetricCategoryService ( new RockContext () ).GetByIds ( metricCategoryIds ).ToList ();

            var strStored = string.Join ( "|", metricCategories.Select ( m => m.Id ).Select ( n => n.ToString () ).ToArray () );
            SetAttributeValue ( "MetricCategories", string.Join ( "|", metricCategories.Select ( m => m.Id ).Select ( n => n.ToString () ).ToArray () ) );

            var entityTypeEntityFilters = new List<string> ();
            if ( metricCategories != null && metricCategories.Count > 0 )
            {
                foreach ( var metricPartition in metricCategories.SelectMany ( m => m.Metric.MetricPartitions.ToList() ) )
                {
                    var metricPartitionEntityType = EntityTypeCache.Get ( metricPartition.EntityTypeId ?? 0 );
                    if (metricPartitionEntityType != null)
                    {
                        var controlId = string.Format ( "metricPartition{0}_entityTypeEditControl", metricPartition.EntityTypeId.Value );
                        Control entityTypeEditControl = phMetricValuePartitions.FindControl ( controlId );

                        int? entityId;

                        if ( entityTypeEditControl != null && metricPartitionEntityType != null && metricPartitionEntityType.SingleValueFieldType != null && metricPartitionEntityType.SingleValueFieldType.Field is IEntityFieldType )
                        {
                            entityId = ( metricPartitionEntityType.SingleValueFieldType.Field as IEntityFieldType ).GetEditValueAsEntityId ( entityTypeEditControl, new Dictionary<string, ConfigurationValue> () );

                            entityTypeEntityFilters.Add ( string.Format ( "{0}|{1}", metricPartitionEntityType.Id, entityId ) );
                        }
                    }
                }
            }

            string metricEntityTypeEntityIdsValue = entityTypeEntityFilters.ToList ().AsDelimited ( "," );
            SetAttributeValue ( "MetricEntityTypeEntityIds", metricEntityTypeEntityIdsValue );

            SetAttributeValue ( "Options", string.Join ( "|", cblOptions.SelectedValues.Select ( n => n.ToString () ).ToArray () ) );

            SetAttributeValue ( "DateRange", drpSlidingDateRange.DelimitedValues );

            SaveAttributeValues ();

            mdEdit.Hide ();
            pnlEditModel.Visible = false;

            // reload the full page since controls are dynamically created based on block settings
            NavigateToPage ( this.CurrentPageReference );

        }

        /// <summary>
        /// Handles the SelectItem event of the mpMetricCategoryPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mpMetricCategoryPicker_SelectItem( object sender, EventArgs e )
        {
            var metricCategoryIds = mpMetricCategoryPicker.SelectedValuesAsInt().ToList ();
            var metricCategory = new MetricCategoryService ( new RockContext () ).GetByIds ( metricCategoryIds );

            CreateDynamicControls ( metricCategoryIds );
        }

        /// <summary>
        /// Creates the dynamic controls.
        /// </summary>
        private void CreateDynamicControls( List<int> metricCategoryIds )
        {
            phMetricValuePartitions.Controls.Clear ();
            List<MetricCategory> metricCategories = new MetricCategoryService ( new RockContext () ).GetByIds ( metricCategoryIds ).ToList ();
            if ( metricCategories != null && metricCategories.Count > 0 )
            {
                foreach ( var metricPartition in metricCategories.Select ( i => i.Metric ).SelectMany ( m => m.MetricPartitions).ToList () )
                {
                    if ( metricPartition.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get ( metricPartition.EntityTypeId.Value );

                        if ( entityTypeCache != null && entityTypeCache.SingleValueFieldType != null )
                        {
                            // Only load an entity control that has not already been loaded
                            var controlId = string.Format ( "metricPartition{0}_entityTypeEditControl", metricPartition.EntityTypeId.Value );
                            if ( phMetricValuePartitions.FindControl ( controlId ) == null )
                            {
                                var fieldType = entityTypeCache.SingleValueFieldType;

                                Dictionary<string, Rock.Field.ConfigurationValue> configurationValues;
                                if ( fieldType.Field is IEntityQualifierFieldType )
                                {
                                    configurationValues = ( fieldType.Field as IEntityQualifierFieldType ).GetConfigurationValuesFromEntityQualifier ( metricPartition.EntityTypeQualifierColumn, metricPartition.EntityTypeQualifierValue );
                                }
                                else
                                {
                                    configurationValues = new Dictionary<string, ConfigurationValue> ();
                                }

                                var entityTypeEditControl = fieldType.Field.EditControl ( configurationValues, string.Format ( "metricPartition{0}_entityTypeEditControl", metricPartition.EntityTypeId.Value ) );
                                var panelCol4 = new Panel { CssClass = "col-md-4" };

                                if ( entityTypeEditControl != null )
                                {
                                    phMetricValuePartitions.Controls.Add ( entityTypeEditControl );
                                    if ( entityTypeEditControl is IRockControl )
                                    {
                                        var entityTypeRockControl = ( entityTypeEditControl as IRockControl );
                                        entityTypeRockControl.Label = metricPartition.Label;
                                    }
                                }
                                else
                                {
                                    var errorControl = new LiteralControl ();
                                    errorControl.Text = string.Format ( "<span class='label label-danger'>Unable to create Partition control for {0}. Verify that the metric partition settings are set correctly</span>", metricPartition.Label );
                                    phMetricValuePartitions.Controls.Add ( errorControl );
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion


    }

}