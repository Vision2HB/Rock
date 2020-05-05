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
    [ExportMetadata( "ComponentName", "Get Client's End of Day Actions" )]

    [WorkflowAttribute( "Organization Attribute", "The organization that you want actions for.",
        true, "", "", 1, ORGANIZATION_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.GroupFieldType" } )]

    [WorkflowTextOrAttribute( "Since", "Since Date Attribute", "The Date to go back to get the user's actions.", true
        , "", "", 2, SINCE_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.DateFieldType", "Rock.Field.Types.DateTimeFieldType" } )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of.", true, "", "", 3, ATTRIBUTE_ATTRIBUTE_KEY )]

    class GetClientsEndOfDayActions : ActionComponent
    {

        private const string SINCE_ATTRIBUTE_KEY = "Since";
        private const string ATTRIBUTE_ATTRIBUTE_KEY = "Attribute";
        private const string ORGANIZATION_ATTRIBUTE_KEY = "Organization";

        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var attribute = AttributeCache.Get( GetAttributeValue( action, ATTRIBUTE_ATTRIBUTE_KEY ).AsGuid(), rockContext );

            if ( attribute != null )
            {

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

                                    var trelloBoard = DefinedValueCache.Get( trelloBoardDefinedValueGuid.Value );

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

                                        var trelloApi = new TrelloApi();

                                        var trelloActions = trelloApi.getBoardActions( trelloBoard, since );

                                        var Actions = trelloActions.Select( x => new Model.Comment
                                        {
                                            Id = x.Id,
                                            Date = x.Date,
                                            Text = x.Data.Text,
                                            CardId = x.Data.Card.Id
                                        } ).ToList();

                                        var tmpCards = trelloActions
                                                .GroupBy( x => new
                                                {
                                                    Id = x.Data.Card.Id,
                                                    Name = x.Data.Card.Name,
                                                    ShortLink = x.Data.Card.ShortLink,
                                                    Board = x.Data.Board
                                                } )
                                                .Select( x => new
                                                {
                                                    CardId = x.Key.Id,
                                                    Name = x.Key.Name,
                                                    BoardId = x.Key.Board.Id,
                                                    Comments = Actions.Where( a => a.CardId == x.Key.Id ).ToList()
                                                } )
                                                .ToList();

                                        var Cards = new List<Model.CardComments>();

                                        foreach ( var tmpCard in tmpCards )
                                        {

                                            var trelloCard = trelloApi.getCard( tmpCard.CardId );

                                            if ( trelloCard.IsNull() )
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
                                                Comments = tmpCard.Comments
                                            };

                                            Cards.Add( card );

                                        }


                                        var Boards = trelloActions
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
                                                .ToList();

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

            return true;

        }

    }
}
