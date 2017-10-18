using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using org.willowcreek.CareCenter.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_willowcreek.CareCenter
{
    /// <summary>
    /// Care Center block for viewing list of assessments for a specific persons family.
    /// </summary>
    [DisplayName( "My Assessments" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for viewing list of assessments for a specific persons family." )]

    [LinkedPage( "Assessment List Page", "Page for displaying all of the assessments", true, "", "", 1 )]
    [LinkedPage( "Assessment Detail Page", "Page for displaying details of a specific assessment", true, "", "", 2 )]
    public partial class MyAssessments : PersonBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptAssessments.ItemCommand += RptAssessments_ItemCommand;
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

        private void RptAssessments_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var parms = new Dictionary<string, string> { { "AssessmentId", e.CommandArgument.ToString() } };
            NavigateToLinkedPage( "AssessmentDetailPage", parms );
        }

        protected void lbShowAll_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                var parms = new Dictionary<string, string> { { "PersonGuid", Person.Guid.ToString() } };
                NavigateToLinkedPage( "AssessmentListPage", parms );
            }
        }

        #endregion


        #region Methods

        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var assessmentQry = new AssessmentService( rockContext )
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

                    assessmentQry = assessmentQry.Where( v => personIds.Contains( v.PersonAlias.PersonId ) );

                    rptAssessments.DataSource = assessmentQry
                        .OrderByDescending( v => v.AssessmentDate )
                        .ToList()
                        .Select( v => new
                        {
                            v.Id,
                            v.AssessmentDate,
                            Person = v.PersonAlias.Person.FullName,
                            Workflows = v.Workflows
                                .OrderByDescending( w => w.WorkflowType.Order )
                                .Select( w => w.WorkflowType.Name )
                                .ToList().AsDelimited( ", " )
                        } );
                    rptAssessments.DataBind();
                }
            }
        }

        #endregion



    }
}