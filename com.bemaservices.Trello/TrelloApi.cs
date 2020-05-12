using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using TrelloNet;
using TrelloNet.Extensions;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;

namespace com.bemaservices.TrelloSync
{
    public class TrelloApi
    {

        private ITrello _trello;
        private Organization _organization;

        public TrelloApi( )
        {

            var apiKey = GlobalAttributesCache.Value( AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_API_KEY_GLOBAL_ATTRIBUTE ).Key );
            var apiToken = GlobalAttributesCache.Value( AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_API_TOKEN_GLOBAL_ATTRIBUTE ).Key );
            var orgId = GlobalAttributesCache.Value( AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_API_ORGANIZATION_ID_GLOBAL_ATTRIBUTE ).Key );

            _trello = new Trello( apiKey );
            _trello.Authorize( apiToken );

            _organization = _trello.Organizations.WithId( orgId );

        }

        public List<DefinedValue> GetTrelloUsers()
        {
            
            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            var userDefinedValues = definedValueService.GetByDefinedTypeGuid( SystemGuid.SystemDefinedTypes.TRELLO_USER.AsGuid() ).ToList();

            var trelloUsers = _trello.Members.ForOrganization( _organization );

            foreach ( var trelloUser in trelloUsers )
            {
                var userDefinedValue = userDefinedValues.Where( dv => dv.Value == trelloUser.Id ).FirstOrDefault();

                if ( userDefinedValue is null )
                {

                    userDefinedValue = new DefinedValue();
                    userDefinedValue.Description = trelloUser.FullName;
                    userDefinedValue.Value = trelloUser.Id;
                    userDefinedValue.DefinedTypeId = DefinedTypeCache.Get( SystemGuid.SystemDefinedTypes.TRELLO_USER.AsGuid() ).Id ;
                    definedValueService.Add( userDefinedValue );

                }

                userDefinedValue.Description = trelloUser.FullName;
            }

            var userDefinedValuesToRemove = userDefinedValues.Where( dv => !trelloUsers.Any( tu => tu.Id == dv.Value ) );

            if ( userDefinedValuesToRemove.Any() )
            {
                definedValueService.DeleteRange( userDefinedValuesToRemove );
            }

            rockContext.SaveChanges();

            return definedValueService.GetByDefinedTypeGuid( SystemGuid.SystemDefinedTypes.TRELLO_USER.AsGuid() ).ToList();
        }

        public int MatchUsers()
        {
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );

            var trelloUsers = GetTrelloUsers();

            foreach( var trelloUser in trelloUsers )
            {
                var person = personService.Queryable().WhereAttributeValue( rockContext,
                                                                            AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_USER_PERSON_ATTRIBUTE.AsGuid() )
                                                                                          .Key,
                                                                            trelloUser.Guid.ToString() )
                                                        .FirstOrDefault();

                if( person is null )
                {
                    var searchOptions = new PersonService.PersonSearchOptions();
                    searchOptions.Name = trelloUser.Description;

                    person = personService.Search( searchOptions ).FirstOrDefault();

                    if ( person.IsNotNull() )
                    {
                        Rock.Attribute.Helper.SaveAttributeValue( person,
                                                                  AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_USER_PERSON_ATTRIBUTE.AsGuid() ),
                                                                  trelloUser.Value.ToString(),
                                                                  rockContext ); 
                    }
                }

            }

            return rockContext.SaveChanges();
        }

        public List<CommentCardAction> GetBoardActions( DefinedValueCache board, DateTime since )
        {

            var trelloBoard = _trello.Boards.WithId( board.Value );

            var comments = _trello.Actions
                                    .ForBoard( trelloBoard, new[] { ActionType.CommentCard }, Since.Date( since ) )
                                    .OfType<CommentCardAction>()
                                    .Where( c => c.Data.Text.Contains("@EOD" ) )
                                    .ToList();

            return comments;

        }

        public List<List> GetBoardLists( DefinedValueCache board )
        {
            var trelloBoard = _trello.Boards.WithId( board.Value );

            var lists = _trello.Lists
                           .ForBoard( trelloBoard )
                           .ToList();

            return lists;
        }

        public List<Card> GetCardsForList( List list )
        {
            var cards = _trello.Cards
                        .ForList( list )
                        .ToList();

            return cards;
        }

        public List<Card> GetBoardCards( DefinedValueCache board )
        {

            var trelloBoard = _trello.Boards.WithId( board.Value );

            var cards = _trello.Cards
                                    .ForBoard( trelloBoard )
                                    .Where( c => c.Closed == false )
                                    .ToList();

            return cards;

        }

        public List<CommentCardAction> GetUserEndOfDayActions( DefinedValueCache user, DateTime since )
        {

            var trelloUser = _trello.Members.WithId( user.Value );

            var t = Task.Run( async delegate
            {
                await Task.Delay( 1000 );
                return 42;
            } );
            t.Wait();

            var comments = _trello.Actions
                                    .AutoPaged(Paging.MaxLimit)
                                    .ForMember( trelloUser, new[] { ActionType.CommentCard }, Since.Date( since ) )
                                    .OfType<CommentCardAction>()
                                    .Where( c => c.Data.Text.Contains( "@EOD" ) )
                                    .ToList();

            return comments;

        }

        public Card GetCard( string id )
        {
            return _trello.Cards.WithId( id );
        }

        public List<DefinedValue> GetBoards()
        {

            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            var boardDefinedValues = definedValueService.GetByDefinedTypeGuid( SystemGuid.SystemDefinedTypes.TRELLO_BOARD.AsGuid() ).ToList();

            var trelloBoards = _trello.Boards.ForOrganization( _organization );

            foreach ( var trelloBoard in trelloBoards )
            {
                var boardDefinedValue = boardDefinedValues.Where( dv => dv.Value == trelloBoard.Id ).FirstOrDefault();

                if ( boardDefinedValue is null )
                {

                    boardDefinedValue = new DefinedValue();
                    boardDefinedValue.Description = trelloBoard.Name;
                    boardDefinedValue.Value = trelloBoard.Id;
                    boardDefinedValue.DefinedTypeId = DefinedTypeCache.Get( SystemGuid.SystemDefinedTypes.TRELLO_BOARD.AsGuid() ).Id;
                    definedValueService.Add( boardDefinedValue );

                    rockContext.SaveChanges();

                }

                boardDefinedValue.Description = trelloBoard.Name;

                boardDefinedValue.LoadAttributes();

                var boardShortLinkAttribute = AttributeCache.Get( SystemGuid.SystemAttributes.TRELLO_BOARD_SHORT_LINK_ATTRIBUTE.AsGuid() );

                boardDefinedValue.SetAttributeValue( boardShortLinkAttribute.Key, trelloBoard.Url );

                boardDefinedValue.SaveAttributeValues( rockContext );

            }

            var boardDefinedValuesToRemove = boardDefinedValues.Where( dv => !trelloBoards.Any( tu => tu.Id == dv.Value ) );

            if ( boardDefinedValuesToRemove.Any() )
            {
                definedValueService.DeleteRange( boardDefinedValuesToRemove );
            }

            rockContext.SaveChanges();



            return definedValueService.GetByDefinedTypeGuid( SystemGuid.SystemDefinedTypes.TRELLO_BOARD.AsGuid() ).ToList();

        }
    }
}
