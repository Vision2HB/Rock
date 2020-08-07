// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using TrelloNet;

namespace com.bemaservices.TrelloSync.Workflow.Action
{
    [ActionCategory( "BEMA Services > Trello" )]
    [Description( "Returns a Json Object of the user's actions" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Person's End of Day Actions" )]

    [WorkflowAttribute( "Person Attribute", "The person who's actions you want to recieve",
        true, "", "", 0, PERSON_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Organization Attribute", "The organization that you want actions for.  If not provided, it will pull actions across all Organizations",
        false, "", "", 1, ORGANIZATION_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.GroupFieldType" } )]

    [WorkflowTextOrAttribute( "Since", "Since Date Attribute", "The Date to go back to get the user's actions.", true
        , "", "", 2, SINCE_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.DateFieldType", "Rock.Field.Types.DateTimeFieldType" } )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of.", true, "", "", 3, ATTRIBUTE_ATTRIBUTE_KEY )]

    class GetUsersEndOfDayActions : ActionComponent
    {

        private const string PERSON_ATTRIBUTE_KEY = "Person";
        private const string SINCE_ATTRIBUTE_KEY = "Since";
        private const string ATTRIBUTE_ATTRIBUTE_KEY = "Attribute";
        private const string ORGANIZATION_ATTRIBUTE_KEY = "Organization";

        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var attribute = AttributeCache.Get( GetAttributeValue( action, ATTRIBUTE_ATTRIBUTE_KEY ).AsGuid(), rockContext );

            if ( attribute != null )
            {
                // get person
                string personAttributeValue = GetAttributeValue( action, PERSON_ATTRIBUTE_KEY );
                Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
                if ( guidPersonAttribute.HasValue )
                {
                    var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                    if ( attributePerson != null && attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                    {
                        string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value );
                        if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                        {
                            Guid personAliasGuid = attributePersonValue.AsGuid();
                            if ( !personAliasGuid.IsEmpty() )
                            {
                                var person = new PersonAliasService( rockContext ).Queryable()
                                                 .Where( a => a.Guid.Equals( personAliasGuid ) )
                                                 .Select( a => a.Person )
                                                 .FirstOrDefault();
                                if ( person != null )
                                {
                                    // Make sure Trello Users are up to date prior to loading attributes

                                    var trelloApi = new TrelloApi();

                                    trelloApi.MatchUsers();

                                    // update person attribute
                                    person.LoadAttributes();
                                    var trelloUserDefinedValueGuid = person.GetAttributeValue( AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_USER_PERSON_ATTRIBUTE.AsGuid() ).Key  ).AsGuidOrNull();

                                    if ( trelloUserDefinedValueGuid.HasValue )
                                    {
                                        var trelloUser = DefinedValueCache.Get( trelloUserDefinedValueGuid.Value );

                                        string sinceValue = GetAttributeValue( action, SINCE_ATTRIBUTE_KEY );
                                        Guid? sinceGuid = sinceValue.AsGuidOrNull();
                                        if ( sinceGuid.HasValue )
                                        {
                                            sinceValue = action.GetWorklowAttributeValue( sinceGuid.Value );

                                            if ( sinceValue == null )
                                            {
                                                errorMessages.Add( string.Format( "The date attribute provided was empty." ) );
                                            }
                                        }
                                        else
                                        {
                                            sinceValue = sinceValue.ResolveMergeFields( GetMergeFields( action ) );
                                        }

                                        DateTime since;

                                        if ( DateTime.TryParse( sinceValue, out since ) )
                                        {
                                            var trelloActions = trelloApi.GetUserEndOfDayActions( trelloUser, since );

                                            var trelloTasks = trelloApi.GetUserEndOfDayTasks( trelloUser, since );

                                            string groupAttributeValue = GetAttributeValue( action, ORGANIZATION_ATTRIBUTE_KEY );
                                            Guid? guidGroupAttributeValue = groupAttributeValue.AsGuidOrNull();
                                            if ( guidGroupAttributeValue.HasValue )
                                            {
                                                var attributeGroup = AttributeCache.Get( guidGroupAttributeValue.Value, rockContext );
                                                if ( attributeGroup != null && attributeGroup.FieldType.Class == "Rock.Field.Types.GroupFieldType" )
                                                {
                                                    string attributeGroupValue = action.GetWorklowAttributeValue( guidGroupAttributeValue.Value );
                                                    if ( !string.IsNullOrWhiteSpace( attributeGroupValue ) )
                                                    {
                                                        Guid groupGuid = attributeGroupValue.AsGuid();
                                                        if ( !groupGuid.IsEmpty() )
                                                        {
                                                            var groupService = new GroupService( rockContext );
                                                            var group = groupService.Get( groupGuid );
                                                            group.LoadAttributes();
                                                            var trelloBoardDefinedValueGuid = group.GetAttributeValue( "TrelloBoard" ).AsGuidOrNull();
                                                            if ( trelloBoardDefinedValueGuid.HasValue )
                                                            {
                                                                var trelloBoardDefinedValue = DefinedValueCache.Get( trelloBoardDefinedValueGuid.Value );

                                                                trelloActions = trelloActions.Where( x => x.Data.Board.Id == trelloBoardDefinedValue.Value ).ToList();

                                                                trelloTasks = trelloTasks.Where( x => x.Data.Board.Id == trelloBoardDefinedValue.Value ).ToList();
                                                            }
                                                        }
                                                    }
                                                }

                                            }    

                                            var Actions = trelloActions.Select( x => new Model.Comment
                                            {
                                                Id = x.Id,
                                                Date = x.Date,
                                                Text = x.Data.Text,
                                                CardId = x.Data.Card.Id
                                            } ).ToList();

                                            var tmpCards = trelloActions
                                                    .GroupBy( x => new {
                                                        Id = x.Data.Card.Id,
                                                        Name = x.Data.Card.Name,
                                                        BoardId = x.Data.Board.Id
                                                    } )
                                                    .Select( x => new
                                                    {
                                                        CardId = x.Key.Id,
                                                        Name = x.Key.Name,
                                                        BoardId = x.Key.BoardId
                                                    } )
                                                    .ToList();

                                            var Cards = new List<Model.CardComments>();

                                            var cardsToAdd = trelloTasks
                                                    .GroupBy( x => new
                                                    {
                                                        Id = x.Data.Card.Id,
                                                        Name = x.Data.Card.Name,
                                                        BoardId = x.Data.Board.Id
                                                    } )
                                                    .Select( x => new
                                                    {
                                                        CardId = x.Key.Id,
                                                        Name = x.Key.Name,
                                                        BoardId = x.Key.BoardId
                                                    } )
                                                    .Where( c => !tmpCards.Select( x => x.CardId ).Contains( c.CardId ) )
                                                    .ToList();

                                            foreach ( var tmpCard in tmpCards )
                                            {

                                                var trelloCard = trelloApi.GetCard( tmpCard.CardId );

                                                if( trelloCard.IsNull() )
                                                {
                                                    trelloCard = new TrelloNet.Card()
                                                    {
                                                        Id = tmpCard.CardId,
                                                        Name = tmpCard.Name,
                                                        IdBoard = tmpCard.BoardId
                                                    };
                                                }
                                                var card = new Model.CardComments
                                                {
                                                    Card = trelloCard,
                                                    Comments = Actions.Where( a => a.CardId == trelloCard.Id ).ToList(),
                                                    Tasks = trelloTasks.Where( a => a.Data.Card.Id == trelloCard.Id ).ToList()
                                                };

                                                Cards.Add( card );

                                            }


                                            var Boards = trelloActions
                                                    .GroupBy( x => new {
                                                        Id = x.Data.Board.Id,
                                                        Name = x.Data.Board.Name,
                                                        ShortLink = x.Data.Board.ShortLink
                                                    } )
                                                    .Select( x => new Model.Board
                                                    {
                                                        Id = x.Key.Id,
                                                        Name = x.Key.Name,
                                                        ShortLink = x.Key.ShortLink,
                                                        Cards = Cards.Where( c => c.Card.IdBoard == x.Key.Id ).ToList()
                                                    } )
                                                    .ToList();

                                            Boards.AddRange(
                                                trelloTasks
                                                    .GroupBy( x => new
                                                    {
                                                        Id = x.Data.Board.Id,
                                                        Name = x.Data.Board.Name,
                                                        ShortLink = x.Data.Board.ShortLink
                                                    } )
                                                    .Select( x => new Model.Board
                                                    {
                                                        Id = x.Key.Id,
                                                        Name = x.Key.Name,
                                                        ShortLink = x.Key.ShortLink,
                                                        Cards = Cards.Where( c => c.Card.IdBoard == x.Key.Id ).ToList()
                                                    } )
                                                    .Where( b => !Boards.Select( x => x.Id ).Contains( b.Id ) )
                                                    .ToList()
                                                );

                                            var Comments = new Model.Comments
                                            {
                                                Boards = Boards,
                                                Cards = Cards,
                                                Actions = Actions
                                            };


                                            SetWorkflowAttributeValue( action, attribute.Guid, Comments.ToJson() );
                                            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, trelloActions ) );
                                        }
                                        else
                                        {
                                            errorMessages.Add( string.Format( "The Provided ( {0} ) is not valid", sinceValue ) );
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

            }

            return true;

        }

    }
}
