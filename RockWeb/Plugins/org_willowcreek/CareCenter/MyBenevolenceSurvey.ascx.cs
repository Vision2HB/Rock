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
    [DisplayName( "My Benevolence Survey" )]
    [Category( "org_willowcreek > CareCenter" )]
    [Description( "Care Center block for finding your benevolence survey." )]

    [LinkedPage( "Survey Page", "Page for showing the survey.", true, "", "", 1 )]
    [WorkflowTypeField("Workflow Type", "The workflow type to limit showing.", false, false, order: 2)]
    public partial class MyBenevolenceSurvey : PersonBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lMessages.Text = string.Empty;
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
                
            }
        }

        #endregion

        #region Events

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            if ( txtSurveyId.Text.AsIntegerOrNull().HasValue )
            {
                var workflowId = txtSurveyId.Text.AsInteger();
                var workflowTypeGuid = GetAttributeValue( "WorkflowType" ).AsGuid();

                var workflow = new WorkflowService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( w => w.Id == workflowId 
                                        && w.WorkflowType.Guid == workflowTypeGuid )
                                    .FirstOrDefault();

                if (workflow != null )
                {
                    workflow.LoadAttributes();

                    var surveyPersonAliasGuid = workflow.AttributeValues["Person"].Value.AsGuid();

                    var surveyPersonAlias = new PersonAliasService( rockContext ).Get( surveyPersonAliasGuid );

                    if (surveyPersonAlias != null )
                    {
                        var queryParms = new Dictionary<string, string>();
                        queryParms.Add( "WorkflowTypeId", workflow.WorkflowTypeId.ToString() );
                        queryParms.Add( "WorkflowGuid", workflow.Guid.ToString() );
                        queryParms.Add( "rckipid", surveyPersonAlias.Person.UrlEncodedKey );
                        NavigateToLinkedPage( "SurveyPage", queryParms );
                    }
                    else
                    {
                        lMessages.Text = "<div class='alert alert-warning'>An application with that Id could not be found.</div>";
                    }
                }
                else
                {
                    lMessages.Text = "<div class='alert alert-warning'>An application with that Id could not be found.</div>";
                }
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>Please enter your application ID in the input above.</div>";
            }
        }

        #endregion


    }
}