using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using com_northpoint.GroupLaunch;
using com_northpoint.GroupLaunch.Models;


using Rock.Attribute;
using System.ComponentModel;
using Group = com_northpoint.GroupLaunch.Models.Group;
using Rock.Security;

namespace RockWeb.Plugins.com_northpoint.GroupLaunch
{

    [DisplayName( "Group Launch (Action Bar)" )]
    [Category( "North Point Ministries > GroupLaunch" )]
    [Description( "Lock and Publish A Group To GroupLaunch and check GroupLaunch Service for any updates on finalized status" )]

    [GroupTypesField( "Group Type Visibity", "Show this block for these group types", false )]
    [TextField("GroupLaunch Service URL","Full URL to the api service. ", true, "https://test.launch.grouplink.org")]

    public partial class GroupLaunch : Rock.Web.UI.RockBlock
    {
        private GroupLaunchService groupLaunchService;
        private Group group;
        private int currentRockGroupId;
        public string groupLaunchGroupName;
        private bool isGroupLaunchGroup = false;

        public bool IsConnected()
        {
            return group != null;
        }

        public bool IsFinalized()
        {
            return group.Finalized;
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }


        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad(e);

            currentRockGroupId = PageParameter( "GroupId" ).AsInteger();
            groupLaunchService = new GroupLaunchService( GetAttributeValue("GroupLaunchServiceURL") );

            

            if ( !Page.IsPostBack )
            {
                pnlGroupLaunch.Visible = false;
                pnlConnectDetails.Visible = false;
                mdConfirmRePublish.Hide();
                pnlConnectReadOnlyDetails.Visible = true;
                CheckBlockAppliesToGroup();
                SetBlockVisibility();
                CheckForExistingConnection();
            }
            
        }

        private void CheckBlockAppliesToGroup()
        {
            var groupTypeGuids = this.GetAttributeValue( "GroupTypeVisibity" ).SplitDelimitedValues().AsGuidList();

            Rock.Model.Group group = new GroupService( new RockContext() )
                .Queryable()
                .Include( g => g.GroupType )
                .Where( g => g.Id == currentRockGroupId )
                .FirstOrDefault();

            isGroupLaunchGroup = group.IsNotNull<Rock.Model.Group>() && groupTypeGuids.Contains( group.GroupType.Guid );


            CheckForEditPermissions( group );
        }

        private void SetBlockVisibility()
        {
            if ( isGroupLaunchGroup )
            {
                pnlGroupLaunch.Visible = true;

                // Check for auth token global attributes (if not available, throw up message)
                if (GlobalAttributesCache.Value("NPMAuthUrl").IsNullOrWhiteSpace())
                {
                    UnconnectedHighlight.Text = "Global Attribute: NPMAuthURL Not Available";
                    UnconnectedHighlight.Visible = true;
                }
                else if (GlobalAttributesCache.Value("NPMAuthClientId").IsNullOrWhiteSpace())
                {
                    UnconnectedHighlight.Text = "Global Attribute: NPMAuthClientId Not Available";
                    UnconnectedHighlight.Visible = true;
                }
                else if (GlobalAttributesCache.Value("NPMAuthClientSecret").IsNullOrWhiteSpace())
                {
                    UnconnectedHighlight.Text = "Global Attribute: NPMAuthClientSecret Not Available";
                    UnconnectedHighlight.Visible = true;
                }
            }
        }

        private void CheckForExistingConnection()
        {
            if ( isGroupLaunchGroup )
            {
                var groupResponse = groupLaunchService.FindGroupByRockId( currentRockGroupId );

                if ( groupResponse != null )
                {
                    group = groupResponse;

                    ManageGroupLaunchLink.NavigateUrl = groupResponse.GroupLaunchAdminUrl;
                    groupLaunchGroupName = group.Name;

                    //Hidden Fields
                    glGroupId.Value = group.Id.ToString();
                    glGroupCollectionId.Value = group.CollectionId.ToString();
                    glGroupName.Value = group.Name.ToString();

                    // Check if group was finalized in GroupLaunch, allowing Rock to unfreeze group security
                    if( group.Finalized )
                    {
                        UnFreezeRockGroup( currentRockGroupId );
                    }
                    else
                    {
                        // Else, lock down this Rock group
                        FreezeRockGroup( currentRockGroupId );
                    }
                }
            }
            else
            {
                //Do Not Unfreeze a Rock Group just because you don't receive a group object back from GroupLaunch
            }
        }

        private void CheckForEditPermissions( Rock.Model.Group group )
        {
            // Check this block's EDIT permissions OR group's permissions
            bool canEdit = this.IsUserAuthorized( Rock.Security.Authorization.EDIT );

            if ( group != null && !canEdit )
            {
                canEdit = group.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
            }

            ConnectButton.Visible = canEdit;
            ReConnectButton.Visible = canEdit;
        }

        protected void btnConnect_Click( object sender, EventArgs e )
        {
            PopulateGroupCollectionDropdown();
            PopulateGroupLeaderRoleDropdown();
            ShowConnectDetails();
        }

        protected void btnCancelConnect_Click( object sender, EventArgs e )
        {
            pnlConnectReadOnlyDetails.Visible = true;
            pnlConnectDetails.Visible = false;
        }

        protected void btnPublish_Click( object sender, EventArgs e )
        {
            string selectedGroupCollection = ddlGroupCollections.SelectedValue;
            string groupName = groupNameText.Text;
            string leaderRoleId = ddlGroupLeaderRole.SelectedValue;
            ConnectToGroupLaunch( selectedGroupCollection, groupName, leaderRoleId );
        }

        private void ShowConnectDetails()
        {
            pnlConnectReadOnlyDetails.Visible = false;
            pnlConnectDetails.Visible = true;

            nbWarning.Visible = false;

            

            //If Re-Populating, change options
            if ( glGroupId.Value.IsNotNullOrWhiteSpace() && glGroupName.Value.IsNotNullOrWhiteSpace() )
            {
                btnConnectPublish.Visible = false;
                btnRePublish.Visible = true;

                ddlGroupCollections.Enabled = false;
                groupNameText.Enabled = false;
                groupNameText.Text = glGroupName.Value;
            }
        }

        private void PopulateGroupCollectionDropdown()
        {
            var groupCollections = groupLaunchService.GroupCollections();

            if ( groupCollections != null )
            {
                ddlGroupCollections.DataSource = groupCollections;
                ddlGroupCollections.DataTextField = "Name";
                ddlGroupCollections.DataValueField = "Id";

                ddlGroupCollections.DataBind();
            }
        }

        private void PopulateGroupLeaderRoleDropdown()
        {
            var rockGroup = new GroupService( new RockContext() ).Get( currentRockGroupId );
            //Get current group's group type roles where IsLeader is True
            var groupLeaderRoles = rockGroup.GroupType.Roles.Where( r => r.IsLeader ).ToDictionary( r => r.Id, r => r.Name );

            ddlGroupLeaderRole.DataSource = groupLeaderRoles;
            ddlGroupLeaderRole.DataTextField = "Value";
            ddlGroupLeaderRole.DataValueField = "Key";
            ddlGroupLeaderRole.DataBind();

            //Set Group Name
            groupNameText.Text = rockGroup.Name;
        }

        private void ConnectToGroupLaunch( string groupCollectionId, string groupName, string leaderRoleId )
        {
            string warnings = "";
            Group creationResponse = groupLaunchService.CreateGroup( groupCollectionId, currentRockGroupId, groupName, leaderRoleId, out warnings );

            // If we get back a warning, post here
            if ( warnings.IsNotNullOrWhiteSpace() )
            {
                nbWarning.Visible = true;
                nbWarning.Text = warnings;
            }

            if ( creationResponse != null )
            {
                group = creationResponse;
            }
        }

        private void RePublishGroupLaunch( string leaderRoleId )
        {
            string warnings = "";
            Group creationResponse = groupLaunchService.ReCreateGroup( glGroupId.ValueAsInt(), glGroupCollectionId.ValueAsInt(), currentRockGroupId, leaderRoleId, out warnings );

            // If we get back a warning, post here
            if ( warnings.IsNotNullOrWhiteSpace() )
            {
                nbWarning.Visible = true;
                nbWarning.Text = warnings;
            }

            if ( creationResponse != null )
            {
                group = creationResponse;
            }
        }

        private void FreezeRockGroup( int rockGroupId )
        {
            var rockGroup = new GroupService( new RockContext() ).Get( currentRockGroupId );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.EDIT );

            // Check if Current Person has EDIT or MANAGE MEMBERS authorization, lets take it away by adding All Users Deny to Edit/ManageMembers
            if ( rockGroup.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson ) || rockGroup.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
            {
                // See if rules already has a deny all for manage members 
                var rules = Authorization.AuthRules( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );
                if ( rules.Any( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ) )
                {
                    // Already has a Deny
                }
                else
                {
                    // Add DENY Rule for ALL USERS (Special Role)
                    this.DenyAll( rockGroup, Authorization.MANAGE_MEMBERS );
                }

                // See if rules already has a deny all
                var rulesEDIT = Authorization.AuthRules( rockGroup.TypeId, rockGroupId, Authorization.EDIT );
                if( rulesEDIT.Any( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ))
                {
                    // Already has a Deny
                }
                else
                {
                    // Add DENY Rule for ALL USERS (Special Role)
                    this.DenyAll( rockGroup, Authorization.EDIT );
                }
            }

            // Add UI Text of "No Edits Allowed"
            ConnectedHighlight.Text = "Connected: Active In GroupLaunch";

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.EDIT );
        }

        private void UnFreezeRockGroup( int rockGroupId )
        {
            var rockGroup = new GroupService( new RockContext() ).Get( currentRockGroupId );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.EDIT );

            // See if rules already has a deny all for manage members 
            var rules = Authorization.AuthRules( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );
            if ( rules.Any( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ) )
            {
                // Already has a Deny
                foreach( var rule in rules.Where( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ) )
                {
                    this.RemoveRule( rule );
                }
            }

            // See if rules already has a deny all
            var rulesEDIT = Authorization.AuthRules( rockGroup.TypeId, rockGroupId, Authorization.EDIT );
            if ( rulesEDIT.Any( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ) )
            {
                // Already has a Deny
                foreach ( var rule in rulesEDIT.Where( r => r.AllowOrDeny == 'D' && r.SpecialRole == SpecialRole.AllUsers ) )
                {
                    this.RemoveRule( rule );
                }
            }
            
            // Add UI Text of "No Edits Allowed"
            ConnectedHighlight.Text = "Connected: GroupLaunch Finalized";

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.MANAGE_MEMBERS );

            Authorization.RefreshAction( rockGroup.TypeId, rockGroupId, Authorization.EDIT );
        }

        private void DenyAll( ISecured entity, string action, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var authService = new AuthService( rockContext );

            // Update the order for any existing rules in database
            var order = 1;
            foreach ( var existingAuth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                existingAuth.Order = order++;
            }

            // Add the new auth (with order of zero)
            var auth = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 0,
                Action = action,
                AllowOrDeny = "D",
                SpecialRole = SpecialRole.AllUsers
            };

            authService.Add( auth );

            rockContext.SaveChanges();

        }

        private void RemoveRule( AuthRule rule, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var authService = new AuthService( rockContext );


            // Add the new auth (with order of zero)
            var auth = authService.Get( rule.Id );

            authService.Delete( auth );

            rockContext.SaveChanges();
        }

        protected void mdConfirmRePublish_SaveClick( object sender, EventArgs e )
        {
            mdConfirmRePublish.Hide();

            string leaderRoleId = ddlGroupLeaderRole.SelectedValue;

            RePublishGroupLaunch( leaderRoleId );
        }

        protected void btnRePublish_Click( object sender, EventArgs e )
        {
            // Show Are you Sure?? Dialog
            mdConfirmRePublish.Show();
        }
    }
}

