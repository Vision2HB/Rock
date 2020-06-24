using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.MigrationTools
{
    /// <summary>
    /// Block to execute a sql command and display the result (if any).
    /// </summary>
    [DisplayName( "BEMA Group Migration" )]
    [Category( "BEMA > Migration Tools" )]
    [Description( "Tool to copy a group structure and change group types." )]

    public partial class GroupMigration : RockBlock
    {

        Group group;
        RockContext rockContext;
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbSuccess.Visible = false;
            int groupId = gpOldGroup.SelectedValueAsId().GetValueOrDefault( 0 );
            //if ( groupId == 0 )
            //{
            //    groupId = PageParameter( "GroupId" ).AsInteger();
            //}

            if ( groupId != 0 )
            {
                rockContext = new RockContext();
                group = new GroupService( rockContext ).Get( groupId );
                if ( group != null )
                {
                    ltName.Text = string.Format( "<span style='font-size:1.5em;'>{0}</span>", group.Name );
                    ltGroupTypeName.Text = string.Format( "<span style='font-size:1.5em;'>{0}</span>", group.GroupType.Name );
                }
            }

            if ( !Page.IsPostBack )
            {
                BindGroupTypeDropDown();
            }
            else
            {
                var groupTypeId = ddlGroupTypes.SelectedValue.AsInteger();
                if ( groupTypeId == 0 || group == null )
                {
                    return;
                }
                var newGroupType = new GroupTypeService( new RockContext() ).Get( groupTypeId );
                BindRoles( newGroupType, group.GroupType.Roles );
                DisplayAttributes ();
                DisplayMemberAttributes ();
            }
        }

        private void BindGroupTypeDropDown()
        {
            var groupTypes = new GroupTypeService( new RockContext() ).Queryable()
                .Select( gt => new
                {
                    Id = gt.Id,
                    Name = gt.Name
                } ).ToList();

            groupTypes.Insert( 0, new { Id = 0, Name = "" } );

            ddlGroupTypes.DataSource = groupTypes;
            ddlGroupTypes.DataBind();
        }


        private void RefreshMappings()
        {
            var groupTypeId = ddlGroupTypes.SelectedValue.AsInteger();
            if ( groupTypeId != 0 && group != null )
            {
                btnAdd.Visible = true;
                var newGroupType = new GroupTypeService( rockContext ).Get( groupTypeId );

                BindRoles( newGroupType, group.GroupType.Roles );
                DisplayAttributes();
                DisplayMemberAttributes();

            }
            else
            {
                pnlAttributes.Visible = false;
                pnlRoles.Visible = false;
                btnAdd.Visible = false;
            }
        }

        private void BindRoles( GroupType newGroupType, ICollection<GroupTypeRole> roles )
        {
            if ( roles.Any() && newGroupType != null )
            {
                pnlRoles.Visible = true;
                phRoles.Controls.Clear();

                foreach ( var role in roles )
                {
                    RockDropDownList ddlRole = new RockDropDownList()
                    {
                        DataTextField = "Name",
                        DataValueField = "Id",
                        Label = role.Name,
                        ID = role.Id.ToString() + "_ddlRole"
                    };
                    BindRoleDropDown( newGroupType, ddlRole );
                    ddlRole.Required = false;
                    phRoles.Controls.Add( ddlRole );
                }
            }
            else
            {
                pnlRoles.Visible = false;
            }
        }

        private void DisplayMemberAttributes()
        {
            phMemberAttributes.Controls.Clear();

            var newGroupTypeId = ddlGroupTypes.SelectedValue.AsInteger();

            if ( newGroupTypeId == 0 )
            {
                pnlAttributes.Visible = false;
                return;
            }

            var attributeService = new AttributeService( rockContext );

            var groupMemberEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid() ).Id;
            var stringGroupTypeId = group.GroupTypeId.ToString();

            var attributes = attributeService.Queryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn == "GroupTypeId"
                    && a.EntityTypeQualifierValue == stringGroupTypeId
                    && a.EntityTypeId == groupMemberEntityId
                    ).ToList();
            if ( attributes.Any() )
            {
                var newGroupTypeIdString = newGroupTypeId.ToString();
                var selectableAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupTypeId"
                        && a.EntityTypeQualifierValue == newGroupTypeIdString
                        && a.EntityTypeId == groupMemberEntityId
                        )
                    .ToList()
                    .Select( a => new
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name + " [" + a.Key + "]"
                    } )
                    .ToList();

                var groupIdString = group.Id.ToString();
                var groupAttributes = attributeService.Queryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn == "GroupId"
                        && a.EntityTypeQualifierValue == groupIdString
                        && a.EntityTypeId == groupMemberEntityId
                        )
                    .ToList()
                    .Select( a => new
                    {
                        Id = a.Id.ToString(),
                        Name = a.Name + " [" + a.Key + "]"
                    } )
                    .ToList();

                selectableAttributes.AddRange( groupAttributes );

                pnlMemberAttributes.Visible = true;
                foreach ( var attribute in attributes )
                {
                    RockDropDownList ddlAttribute = new RockDropDownList()
                    {
                        ID = attribute.Id.ToString() + "_ddlMemberAttribute",
                        Label = attribute.Name,
                        DataValueField = "Id",
                        DataTextField = "Name"
                    };
                    ddlAttribute.DataSource = selectableAttributes;
                    ddlAttribute.DataBind();
                    var emptyItem = new ListItem ( "(Not Mapped)", "0" );
                    ddlAttribute.Items.Add ( emptyItem );
                    ddlAttribute.SelectedValue = "0";
                    phMemberAttributes.Controls.Add( ddlAttribute );
                }
            }
            else
            {
                pnlMemberAttributes.Visible = false;
            }
        }

        private void DisplayAttributes()
        {
            phAttributes.Controls.Clear ();

            var newGroupTypeId = ddlGroupTypes.SelectedValue.AsInteger ();

            if ( newGroupTypeId == 0 )
            {
                pnlAttributes.Visible = false;
                return;
            }

            var attributeService = new AttributeService ( rockContext );

            var groupEntityId = new EntityTypeService ( rockContext ).Get ( Rock.SystemGuid.EntityType.GROUP.AsGuid () ).Id;
            var stringGroupTypeId = group.GroupTypeId.ToString ();

            var attributes = attributeService.Queryable ()
                .Where ( a =>
                     a.EntityTypeQualifierColumn == "GroupTypeId"
                     && a.EntityTypeQualifierValue == stringGroupTypeId
                     && a.EntityTypeId == groupEntityId
                    ).ToList ();
            if ( attributes.Any () )
            {
                var newGroupTypeIdString = newGroupTypeId.ToString ();
                var selectableAttributes = attributeService.Queryable ()
                    .Where ( a =>
                         a.EntityTypeQualifierColumn == "GroupTypeId"
                         && a.EntityTypeQualifierValue == newGroupTypeIdString
                         && a.EntityTypeId == groupEntityId
                        )
                    .ToList ()
                    .Select ( a => new
                    {
                        Id = a.Id.ToString (),
                        Name = a.Name + " [" + a.Key + "]"
                    } )
                    .ToList ();

                pnlAttributes.Visible = true;
                foreach ( var attribute in attributes )
                {
                    RockDropDownList ddlAttribute = new RockDropDownList ()
                    {
                        ID = attribute.Id.ToString () + "_ddlAttribute",
                        Label = attribute.Name,
                        DataValueField = "Id",
                        DataTextField = "Name"
                    };
                    ddlAttribute.DataSource = selectableAttributes;
                    ddlAttribute.DataBind ();
                    var emptyItem = new ListItem ( "(Not Mapped)", "0" );
                    ddlAttribute.Items.Add ( emptyItem );
                    ddlAttribute.SelectedValue = "0";
                    phAttributes.Controls.Add ( ddlAttribute );
                }
            }
            else
            {
                pnlAttributes.Visible = false;
            }
        }

        private void BindRoleDropDown( GroupType newGroupType, RockDropDownList ddlRole )
        {
            ddlRole.DataSource = newGroupType.Roles;
            ddlRole.DataBind();
            var emptyItem = new ListItem ( "(Not Mapped)", "0" );
            ddlRole.Items.Add ( emptyItem );
            ddlRole.SelectedValue = "0";
        }

        protected void btnAdd_Click( object sender, EventArgs e )
        {
            var script = tScript.Text;
            script += "\nEXEC _com_bemaservices_spMigrateGroups @rootGroupId = " + group.Id
                + ", @oldGroupTypeId = " + group.GroupTypeId.ToString ()
                + ", @newParentGroupId = " + gpNewParent.GroupId.ToString ()
                + ", @newGroupTypeId = " + ddlGroupTypes.SelectedValue
                + ", @copyAttendanceYN = " + rblAttendance.SelectedValue
                + ", @includeRootGroup = " + rblIncludeRootGroup.SelectedValue
                + ", @personId = " + CurrentPerson.Id
                + ", @deleteExistingYN = " + rblDelete.SelectedValue;

            //Map group roles
            var sRoleMap = String.Empty;

            foreach ( var role in group.GroupType.Roles )
            {
                var ddlRole = ( RockDropDownList ) phRoles.FindControl( role.Id.ToString() + "_ddlRole" );
                if ( ddlRole != null && ddlRole.SelectedValue != "0" )
                {
                    sRoleMap += "<Item OldId=\"" + role.Id.ToString () + "\" NewId=\"" + ddlRole.SelectedValue + "\"/>";
                }
            }
            // Add to script
            if ( sRoleMap != String.Empty )
            {
                script += ", @roleMap = '<root>" + sRoleMap + "</root>'";
            }

            //Map attributes
            var sAttributeMap = String.Empty;

            var attributeService = new AttributeService( rockContext );
            var attributeValueService = new AttributeValueService( rockContext );

            var groupEntityId = new EntityTypeService( rockContext ).Get( Rock.SystemGuid.EntityType.GROUP.AsGuid() ).Id;

            var attributes = attributeService.Queryable ()
                .Where ( a =>
                     a.EntityTypeQualifierColumn == "GroupTypeId"
                     && a.EntityTypeQualifierValue == group.GroupTypeId.ToString ()
                     && a.EntityTypeId == groupEntityId
                     ).ToList ();
            foreach ( var attribute in attributes )
            {
                var ddlAttribute = ( RockDropDownList ) phAttributes.FindControl( attribute.Id.ToString () + "_ddlAttribute" );
                if ( ddlAttribute != null )
                {
                    var newAttributeId = ddlAttribute.SelectedValue.AsInteger();
                    if ( newAttributeId != 0 )
                    {
                        sAttributeMap += "<Item OldId=\"" + attribute.Id.ToString () + "\" NewId=\"" + newAttributeId.ToString () + "\"/>";
                    }
                }
            }
            if ( sAttributeMap != String.Empty )
            {
                script += ", @attributeMap  = '<root>" + sAttributeMap + "</root>'";
            }

            //Map group member attributes
            var sMemberAttributeMap = String.Empty;

            var groupMemberEntityId = new EntityTypeService ( rockContext ).Get ( Rock.SystemGuid.EntityType.GROUP_MEMBER.AsGuid () ).Id;

            attributes = attributeService.Queryable ()
                .Where ( a =>
                     a.EntityTypeQualifierColumn == "GroupTypeId"
                     && a.EntityTypeQualifierValue == group.GroupTypeId.ToString ()
                     && a.EntityTypeId == groupMemberEntityId
                     ).ToList ();
            foreach ( var attribute in attributes )
            {
                var ddlAttribute = (RockDropDownList) phMemberAttributes.FindControl ( attribute.Id.ToString () + "_ddlMemberAttribute" );
                if ( ddlAttribute != null )
                {
                    var newAttributeId = ddlAttribute.SelectedValue.AsInteger ();
                    if ( newAttributeId != 0 )
                    {
                        sMemberAttributeMap += "<Item OldId=\"" + attribute.Id.ToString () + "\" NewId=\"" + newAttributeId.ToString () + "\"/>";
                    }
                }
            }
            if ( sMemberAttributeMap != String.Empty )
            {
                script += ", @memberAttributeMap = '<root>" + sMemberAttributeMap + "</root>'";
            }

            tScript.Text = script;
            btnAdd.Enabled = true;
        }

        protected void gpOldGroup_ValueChanged( object sender, EventArgs e )
        {
            RefreshMappings();
        }

        protected void gpNewParent_ValueChanged( object sender, EventArgs e )
        {
            RefreshMappings();
        }

        protected void ddlGroupTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            RefreshMappings();
        }
    }
}