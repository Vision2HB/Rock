using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Security;

namespace RockWeb.Plugins.com_northpoint.RoomCheckIn
{
    [DisplayName( "Media Release" )]
    [Category( "com_northpoint > Check-in" )]
    [Description( "Parent Sign Media Release" )]
    [LinkedPage("Family Registration Page" )]
    public partial class MediaRelease : Rock.Web.UI.RockBlock
    {
        protected int? _campusId = null;
        protected List<int> _childIds = null;
        private List<Person> children = new List<Person>();
       
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            
            _campusId = PageParameter( "CampusId" ).AsIntegerOrNull();

            if ( PageParameter( "ChildIds" ).IsNotNullOrWhiteSpace() )
            {
                _childIds = PageParameter( "ChildIds" ).Split( ',' ).Select( int.Parse ).ToList();
            }


            // if there are no child ids, skip this page
            if ( _childIds == null || _childIds.Count == 0 )
            {
                BackToSearch();
                return;
            }
            
        }

        private void SaveResponseToAttributes( bool response )
        {
            RockContext rockContext = new RockContext();

            // Fetch the children from Ids
            children = new PersonService( rockContext ).GetByIds( _childIds ).ToList();
            foreach ( Person child in children )
            {
                child.LoadAttributes();
                if ( response == true ) //Media Release Accepted
                {
                    child.SetAttributeValue( "Arena-16-140", RockDateTime.Today ); //Media Release Accepted
                    child.SetAttributeValue( "Arena-16-155", null ); //Media Release Refused
                }
                else  // Media Release Denied
                {
                    child.SetAttributeValue( "Arena-16-140", null );
                    child.SetAttributeValue( "Arena-16-155", 1 ); //bool for 'Refused' true
                }

                child.SaveAttributeValue( "Arena-16-140", rockContext );
                child.SaveAttributeValue( "Arena-16-155", rockContext );
            }


        }

        protected void lbYes_Click( object sender, EventArgs e )
        {
            SaveResponseToAttributes( true ); //true for yes
            BackToSearch();
        }

        protected void lbNo_Click( object sender, EventArgs e )
        {
            SaveResponseToAttributes( false ); //false for refused
            BackToSearch();
        }

        private void BackToSearch()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            //queryParams.Add( "CampusId", _campusId.ToStringSafe() );
            foreach ( var keyvalue in PageParameters() )
            {
                queryParams.Add( keyvalue.Key, keyvalue.Value.ToStringSafe() );
            }
            NavigateToLinkedPage( "FamilyRegistrationPage", queryParams );
        }
    }
}