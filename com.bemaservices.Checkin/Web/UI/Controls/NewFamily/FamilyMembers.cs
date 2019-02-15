
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock;

namespace com.bemaservices.Checkin.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:NewFamilyMembers runat=server></{0}:NewFamilyMembers>" )]
    public class NewFamilyMembers : CompositeControl, INamingContainer
    {
        private LinkButton _lbAddGroupMember;

        /// <summary>
        /// Gets or sets a value indicating whether [show title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show title]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTitle
        {
            get { return ViewState["ShowTitle"] as bool? ?? false; }
            set { ViewState["ShowTitle"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show suffix].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show suffix]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSuffix
        {
            get { return ViewState["ShowSuffix"] as bool? ?? false; }
            set { ViewState["ShowSuffix"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require gender].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get { return ViewState["RequireGender"] as bool? ?? false; }
            set { ViewState["RequireGender"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireBirthdate
        {
            get { return ViewState["RequireBirthdate"] as bool? ?? false; }
            set { ViewState["RequireBirthdate"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require grade].
        /// </summary>
        /// <value>
        /// <c>true</c> if [require grade]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGrade
        {
            get { return ViewState["RequireGrade"] as bool? ?? false; }
            set { ViewState["RequireGrade"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGrade
        {
            get { return ViewState["ShowGrade"] as bool? ?? false; }
            set { ViewState["ShowGrade"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show middle name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show middle name]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMiddleName
        {
            get { return ViewState["ShowMiddleName"] as bool? ?? false; }
            set { ViewState["ShowMiddleName"] = value; }
        }

        /// <summary>
        /// Gets the group member rows.
        /// </summary>
        /// <value>
        /// The group member rows.
        /// </value>
        public List<NewFamilyMembersRow> FamilyMemberRows
        {
            get
            {
                var rows = new List<NewFamilyMembersRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyMembersRow )
                    {
                        var NewFamilyMembersRow = control as NewFamilyMembersRow;
                        if ( NewFamilyMembersRow != null )
                        {
                            rows.Add( NewFamilyMembersRow );
                        }
                    }
                }

                return rows;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _lbAddGroupMember = new LinkButton();
            Controls.Add( _lbAddGroupMember );
            _lbAddGroupMember.ID = this.ID + "_btnAddGroupMember";
            _lbAddGroupMember.Click += lbAddGroupMember_Click;
            _lbAddGroupMember.AddCssClass( "add btn btn-xs btn-action pull-right" );
            _lbAddGroupMember.CausesValidation = false;

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass( "fa fa-user" );
            _lbAddGroupMember.Controls.Add( iAddFilter );

            var spanAddFilter = new HtmlGenericControl( "span" );
            spanAddFilter.InnerHtml = " Add Person";
            _lbAddGroupMember.Controls.Add( spanAddFilter );
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroupMember_Click( object sender, EventArgs e )
        {
            if ( AddGroupMemberClick != null )
            {
                AddGroupMemberClick( this, e );
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table table-groupmembers" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                // th
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Role" );
                writer.RenderEndTag();

                if ( this.ShowTitle )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( "Title" );
                    writer.RenderEndTag();
                }

                // th
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Name" );
                writer.RenderEndTag();

                // th
                //writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Personal Info" );

                //if ( RequireGender )
                //{
                //    writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                //}
                writer.RenderEndTag();

                // th
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Other Info" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "" );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyMembersRow )
                    {
                        control.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();  // tbody

                writer.RenderBeginTag( HtmlTextWriterTag.Tfoot );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Colspan, "9" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _lbAddGroupMember.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // tfoot

                writer.RenderEndTag();  // table
            }
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                if ( Controls[i] is NewFamilyMembersRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

        /// <summary>
        /// Occurs when [add group member click].
        /// </summary>
        public event EventHandler AddGroupMemberClick;

    }
}