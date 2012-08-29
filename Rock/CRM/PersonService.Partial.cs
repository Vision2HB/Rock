//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Crm
{
	/// <summary>
	/// Person POCO Service class
	/// </summary>
    public partial class PersonService : Service<Person, PersonDto>
    {
		/// <summary>
		/// Gets People by Email
		/// </summary>
		/// <param name="email">Email.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByEmail( string email )
        {
            return Repository.Find( t => ( t.Email == email || ( email == null && t.Email == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Marital Status Id
		/// </summary>
		/// <param name="maritalStatusId">Marital Status Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByMaritalStatusId( int? maritalStatusId )
        {
            return Repository.Find( t => ( t.MaritalStatusId == maritalStatusId || ( maritalStatusId == null && t.MaritalStatusId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Person Status Id
		/// </summary>
		/// <param name="personStatusId">Person Status Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByPersonStatusId( int? personStatusId )
        {
            return Repository.Find( t => ( t.PersonStatusId == personStatusId || ( personStatusId == null && t.PersonStatusId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Record Status Id
		/// </summary>
		/// <param name="recordStatusId">Record Status Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByRecordStatusId( int? recordStatusId )
        {
            return Repository.Find( t => ( t.RecordStatusId == recordStatusId || ( recordStatusId == null && t.RecordStatusId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Record Status Reason Id
		/// </summary>
		/// <param name="recordStatusReasonId">Record Status Reason Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByRecordStatusReasonId( int? recordStatusReasonId )
        {
            return Repository.Find( t => ( t.RecordStatusReasonId == recordStatusReasonId || ( recordStatusReasonId == null && t.RecordStatusReasonId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Record Type Id
		/// </summary>
		/// <param name="recordTypeId">Record Type Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByRecordTypeId( int? recordTypeId )
        {
            return Repository.Find( t => ( t.RecordTypeId == recordTypeId || ( recordTypeId == null && t.RecordTypeId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Suffix Id
		/// </summary>
		/// <param name="suffixId">Suffix Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetBySuffixId( int? suffixId )
        {
            return Repository.Find( t => ( t.SuffixId == suffixId || ( suffixId == null && t.SuffixId == null ) ) );
        }
		
		/// <summary>
		/// Gets People by Title Id
		/// </summary>
		/// <param name="titleId">Title Id.</param>
		/// <returns>An enumerable list of Person objects.</returns>
	    public IEnumerable<Person> GetByTitleId( int? titleId )
        {
            return Repository.Find( t => ( t.TitleId == titleId || ( titleId == null && t.TitleId == null ) ) );
        }

        /// <summary>
        /// Gets a list of people with a matching full name
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public IQueryable<Person> GetByFullName( string fullName )
        {
            string firstName = string.Empty;
            string lastName = string.Empty;

            if ( fullName.Contains( ' ' ) )
            {
                firstName = fullName.Substring( 0, fullName.LastIndexOf( ' ' ) );
                lastName = fullName.Substring( fullName.LastIndexOf( ' ' ) + 1 );
            }
            else
                lastName = fullName;

            return Queryable().
                    Where( p => p.LastName.ToLower().StartsWith( lastName.ToLower() ) &&
                        ( p.NickName.ToLower().StartsWith( firstName.ToLower() ) ||
                        p.GivenName.StartsWith( firstName.ToLower() ) ) );
        }

        /// <summary>
        /// Gets the by encrypted ID.
        /// </summary>
        /// <param name="encryptedID">The encrypted ID.</param>
        /// <returns></returns>
        public Person GetByEncryptedID( string encryptedID )
        {
            string identifier = Rock.Security.Encryption.DecryptString( encryptedID );

            string[] idParts = identifier.Split( '|' );
            if ( idParts.Length == 2 )
            {
                Guid personGuid = new Guid( idParts[0] );
                int personId = Int32.Parse( idParts[1] );

                Person person = Queryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( person != null )
                    return person;

                // Check to see if the record was merged
                PersonMergedService personMergedService = new PersonMergedService();
                PersonMerged personMerged = personMergedService.Queryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( personMerged != null )
                    return Get( personMerged.Id, true );
            }

            return null;
        }

        /// <summary>
        /// Gets Person by Id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail.</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person Get( int id, bool followTrail )
        {
            if ( followTrail )
                id = new PersonMergedService().Current( id );

            return Get( id );
        }

        /// <summary>
        /// Gets Person by Guid
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Person GetByEncryptedKey( string encryptedKey, bool followTrail )
        {
            if ( followTrail )
                encryptedKey = new PersonMergedService().Current( encryptedKey );

            return GetByEncryptedKey( encryptedKey );
        }
    }
}
