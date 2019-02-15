using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Extension;
using Rock.Model;
using Rock.Web.UI;
using Rock.Data;
using Rock.Web;
using System.IO;
using Rock.Attribute;
using Rock.Web.UI.Controls;
using System.Data;

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Run SQL OnClick" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Specify SQL in Block Attribute. OnClicking button, it is run." )]
    [CodeEditorField( "Query", "The query to execute. Note that if you are providing SQL you can add items from the query string using Lava like {{ QueryParmName }}.", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, false, "" )]
    [IntegerField("Query Timeout", "Amount of time before query times out.",false, 180)]
    [TextField("Button Title", "Text you would like displayed on button", false, "Run Code")]
    [BooleanField("Refresh After Complete", "Indicates if you want the page to refresh after SQL has been run.", true)]
    public partial class RunSQLOnClick : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Setting button title
            var buttonTitle = GetAttributeValue( "ButtonTitle" );
            lbRunSQL.Text = "<i class='fa fa-share-square-o'></i> " + buttonTitle;

            this.AddConfigurationUpdateTrigger( upnlRunSQL );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
        }
        #endregion

        #region Method
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private int GetData( out string errorMessage )
        {
            errorMessage = string.Empty;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    var mergeFields = GetDynamicDataMergeFields();

                    // NOTE: there is already a PageParameters merge field from GetDynamicDataMergeFields, but for backwords compatibility, also add each of the PageParameters as plain merge fields
                    foreach ( var pageParam in PageParameters() )
                    {
                        mergeFields.AddOrReplace( pageParam.Key, pageParam.Value );
                    }

                    query = query.ResolveMergeFields( mergeFields );

                    int timeout = GetAttributeValue( "QueryTimeout" ).AsInteger();

                    return DbService.ExecuteCommand(query, CommandType.Text, null, timeout );
                }
                catch ( System.Exception ex )
                {
                    errorMessage = ex.Message;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the dynamic data merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDynamicDataMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( CurrentPerson != null )
            {
                // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                mergeFields.Add( "Person", CurrentPerson );
            }

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
            mergeFields.Add( "CurrentPage", this.PageCache );

            return mergeFields;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Click event of the lbRunSQL control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRunSQL_Click( object sender, EventArgs e )
        {
            string errorMessage = string.Empty;
            var rowCount = GetData( out errorMessage );

            // Checking if we want to refresh page after query
            if ( GetAttributeValue( "RefreshAfterComplete" ).AsBoolean() )
            {
                // Refreshing Page
                Response.Redirect( Request.RawUrl );
            }
        }
        #endregion
    }
}
