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
using com.bemaservices.TrelloSync.Model;
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
    [Description( "Returns a Json Object of all the Lists and Cards on the given Client's Board" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Get Client's Cards" )]

    [WorkflowAttribute( "Organization Attribute", "The organization that you want actions for.",
        true, "", "", 0, ORGANIZATION_ATTRIBUTE_KEY, new string[] { "Rock.Field.Types.GroupFieldType" } )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of.", true, "", "", 1, ATTRIBUTE_ATTRIBUTE_KEY )]

    class GetClientsCards : ActionComponent
    {

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

                                    var trelloApi = new TrelloApi();

                                    var trelloLists = trelloApi.GetBoardLists( trelloBoard );

                                    var board = new TrelloSync.Model.Board();

                                    board.Id = trelloBoard.Value;
                                    board.Name = trelloBoard.Description;
                                    board.Lists = new List<Model.List>();

                                    foreach ( var list in trelloLists )
                                    {

                                        var cards = trelloApi.GetCardsForList( list );

                                        var newList = new TrelloSync.Model.List();

                                        newList.Id = list.Id;
                                        newList.Name = list.Name;
                                        newList.BoardId = board.Id;
                                        newList.Cards = cards.ToList();

                                        board.Lists.Add( newList );

                                    }

                                    SetWorkflowAttributeValue( action, attribute.Guid, board.ToJson() );
                                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, board.ToJson() ) );
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
