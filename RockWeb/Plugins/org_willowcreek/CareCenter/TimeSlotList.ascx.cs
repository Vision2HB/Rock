using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.UI.Controls;
namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// User control for managing the Service Area for the Care Center
    /// </summary>
    [DisplayName( "Appointment Time Slot List" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "User control for managing the Service Area Time Slots for the Care Center." )]

    [LinkedPage( "Detail Page" )]
    public partial class TimeSlotList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += TimeSlotList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPanel );

            bool canAddEditDelete = UserCanEdit;

            gTimeSlots.DataKeyNames = new string[] { "Id" };
            gTimeSlots.Actions.AddClick += gTimeSlots_Add;
            gTimeSlots.GridRebind += gServiceAreasTimeSlot_GridRebind;
            gTimeSlots.Actions.ShowAdd = canAddEditDelete;
            gTimeSlots.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    int serviceAreaId = PageParameter( "ServiceAreaId" ).AsInteger();
                    var serviceArea = new ServiceAreaService( rockContext ).Get( serviceAreaId );
                    if ( serviceArea != null )
                    {
                        lTitle.Text = string.Format( "{0} Appointment Time Slots", serviceArea.Name );
                    }
                    else
                    {
                        lTitle.Text = "Appointment Time Slots";
                    }
                }
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>

        private void TimeSlotList_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gTimeSlots control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gTimeSlots_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "TimeSlotId", "0" );
            parms.Add( "ServiceAreaId", PageParameter( "ServiceAreaId" ) );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Edit event of the gServiceAreasTimeSlot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gTimeSlots_RowSelected( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "TimeSlotId", e.RowKeyValue.ToString() );
            parms.Add( "ServiceAreaId", PageParameter( "ServiceAreaId" ) );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the gServiceAreasTimeSlot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gTimeSlots_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceAreaTimeSlotService = new ServiceAreaAppointmentTimeslotService( rockContext );
                var serviceAreaTimeSlot = serviceAreaTimeSlotService.Get( e.RowKeyId );

                if ( serviceAreaTimeSlot != null )
                {
                    string errorMessage;
                    if ( !serviceAreaTimeSlotService.CanDelete( serviceAreaTimeSlot, out errorMessage ) )
                    {
                        mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    serviceAreaTimeSlotService.Delete( serviceAreaTimeSlot );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gServiceAreasTimeSlot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gServiceAreasTimeSlot_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                int serviceAreaId = PageParameter( "ServiceAreaId" ).AsInteger();
                var queryable = new ServiceAreaAppointmentTimeslotService( rockContext )
                    .GetByServiceAreaId( serviceAreaId ).ToList();

                gTimeSlots.DataSource = queryable
                    .OrderBy( q => q.Schedule.FirstStartDateTimeThisWeek )
                    .ToList();

                gTimeSlots.DataBind();
            }
        }

        #endregion

    }
}