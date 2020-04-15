using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutotaskNET;
using AutotaskNET.Entities;

using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Attribute;
using Rock.Data;

namespace com.bemaservices.AutoTask
{
    public class AutoTaskApi
    {
        private const string _FOREIGNKEY = "com_bemaservices_autotask";
        private ATWSInterface _atAPI;

        public AutoTaskApi( string userName, string trackingId )
        {
            _atAPI = new ATWSInterface();
            _atAPI.Connect( userName, trackingId );
        }

        public List<DefinedValue> getAccounts()
        {
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );

            List<DefinedValue> autoTaskAccounts = definedValueService.GetByDefinedTypeGuid( AutoTask.SystemGuid.DefinedType.AUTOTASK_ACCOUNT.AsGuid() ).Where( d => d.ForeignKey == _FOREIGNKEY ).ToList();

            try
            {
                List<PicklistValue> accountTypes = _atAPI.GetPicklistValues( typeof( Account ), "AccountType" );

                List<Account> customerAccounts = _atAPI.Query( typeof( Account ), new QueryFilter() {
                    new QueryField("AccountType", QueryFieldOperation.Equals, accountTypes.Find(type => type.Label == "Customer").Value)
                } ).OfType<Account>().ToList();

                foreach ( var customerAccount in customerAccounts )
                {
                    var accountDefinedValue = autoTaskAccounts.Where( a => a.ForeignId == customerAccount.id ).FirstOrDefault();

                    if ( accountDefinedValue.IsNull() )
                    {
                        accountDefinedValue = new DefinedValue();
                        accountDefinedValue.ForeignId = Convert.ToInt32( customerAccount.id );
                        accountDefinedValue.ForeignKey = _FOREIGNKEY;
                        accountDefinedValue.DefinedTypeId = DefinedTypeCache.GetId( AutoTask.SystemGuid.DefinedType.AUTOTASK_ACCOUNT.AsGuid() ).Value;
                        definedValueService.Add( accountDefinedValue );
                        autoTaskAccounts.Add( accountDefinedValue );
                    }

                    accountDefinedValue.Value = customerAccount.AccountName;

                }

                var autoTaskAccountsToRemove = autoTaskAccounts.Where( v => !customerAccounts.Any( a => a.id == v.ForeignId && v.ForeignKey == _FOREIGNKEY ) );

                definedValueService.DeleteRange( autoTaskAccountsToRemove );

                rockContext.SaveChanges();
            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return autoTaskAccounts;
        }

        public List<DefinedValue> getContracts( DefinedValueCache account )
        {
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );

            var accountAttribute = AttributeCache.Get( AutoTask.SystemGuid.Attribute.AUTOTASK_CONTRACT_ACCOUNT );

            var contractDefinedValues = definedValueService.GetByDefinedTypeGuid( AutoTask.SystemGuid.DefinedType.AUTOTASK_CONTRACT.AsGuid() )
                                                            .WhereAttributeValue( rockContext, accountAttribute.Key, account.Guid.ToString() );

            try
            {
                List<Contract> accountContracts = _atAPI.Query( typeof( Contract ), new QueryFilter() {
                    new QueryField( "AccountID", QueryFieldOperation.Equals, account.ForeignId )
                } ).OfType<Contract>().ToList();

                foreach( var accountContract in accountContracts )
                {
                    var contractDefinedValue = contractDefinedValues.Where( c => c.ForeignKey == _FOREIGNKEY && c.ForeignId == accountContract.id ).FirstOrDefault();

                    if( contractDefinedValue.IsNull() )
                    {
                        contractDefinedValue = new DefinedValue();
                        contractDefinedValue.ForeignId = Convert.ToInt32( accountContract.id );
                        contractDefinedValue.ForeignKey = _FOREIGNKEY;
                        contractDefinedValue.DefinedTypeId = DefinedTypeCache.GetId( AutoTask.SystemGuid.DefinedType.AUTOTASK_CONTRACT.AsGuid() ).Value;
                        definedValueService.Add( contractDefinedValue );

                        // Save the changes, because we need the Id to add the attribute values.
                        rockContext.SaveChanges();
                    }

                    contractDefinedValue.Value = accountContract.ContractName;

                    Rock.Attribute.Helper.SaveAttributeValue( contractDefinedValue, accountAttribute, account.Guid.ToString(), rockContext );
                }

                // Remove Contracts that are no longer in Autotask.
                var contractDefinedValuesToRemove = contractDefinedValues.Where( d => !accountContracts.Any( c => c.id == d.ForeignId && d.ForeignKey == _FOREIGNKEY ) );

                definedValueService.DeleteRange( contractDefinedValuesToRemove );

                rockContext.SaveChanges();

            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            // Reload the defined values are return them.
            return definedValueService.GetByDefinedTypeGuid( AutoTask.SystemGuid.DefinedType.AUTOTASK_CONTRACT.AsGuid() )
                                                            .WhereAttributeValue( rockContext, accountAttribute.Key, account.Guid.ToString() )
                                                            .ToList();
        }

    }
}
