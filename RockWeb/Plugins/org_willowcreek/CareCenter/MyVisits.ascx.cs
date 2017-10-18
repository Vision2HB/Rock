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
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of visits for a specific persons family.
    /// </summary>
    [DisplayName( "My Visits" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of visits for a specific persons family." )]

    [IntegerField("Max Items", "The maximum number of visits to show in the list", false, 10, "", 0)]
    [LinkedPage("Visit List Page", "Page for displaying all of the visits", true, "", "", 1)]
    [LinkedPage( "Visit Detail Page", "Page for displaying details of a specific visit", true, "", "", 2 )]
    public partial class MyVisits : PersonBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptVisits.ItemCommand += RptVisits_ItemCommand;
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
                GetData();
            }
        }

        #endregion

        #region Events

        private void RptVisits_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var parms = new Dictionary<string, string> { { "VisitId", e.CommandArgument.ToString() } };
            NavigateToLinkedPage( "VisitDetailPage", parms );
        }

        protected void lbShowAll_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                var parms = new Dictionary<string, string> { { "PersonGuid", Person.Guid.ToString() } };
                NavigateToLinkedPage( "VisitListPage", parms );
            }
        }

        #endregion

        #region Methods

        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var visitQry = new VisitService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => a.PersonAlias != null );

                if ( Person != null )
                {
                    var personIds = new List<int>();
                    foreach ( var family in Person.GetFamilies() )
                    {
                        foreach ( var personId in family.Members.Select( m => m.PersonId ).ToList() )
                        {
                            personIds.Add( personId );
                        }
                    }

                    visitQry = visitQry.Where( v => personIds.Contains( v.PersonAlias.PersonId ) );

                    int take = GetAttributeValue( "MaxItems" ).AsIntegerOrNull() ?? 10;

                    rptVisits.DataSource = visitQry
                        .OrderByDescending( v => v.VisitDate )
                        .Take( take )
                        .ToList()
                        .Select( v => new
                        {
                            v.Id,
                            v.VisitDate,
                            Person = v.PersonAlias.Person.FullName,
                            Workflows = v.Workflows
                                .OrderByDescending( w => w.WorkflowType.Order )
                                .Select( w => w.WorkflowType.Name )
                                .ToList().AsDelimited( ", " )
                        } );
                    rptVisits.DataBind();
                }
            }
        }

        #endregion



    }
}