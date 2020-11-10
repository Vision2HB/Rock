// <copyright>
// Copyright by the Spark Development Network
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
using System.Web.UI;
using System.Linq;
using System.Linq.Dynamic;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Utility;


namespace RockWeb.Plugins.com_kevinrutledge.Finance
{
    /// <summary>
    /// Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range.
    /// </summary>
    [DisplayName("Transnational Transaction Reconciliation")]
    [Category("Kevin Rutledge > Finance")]
    [Description("Block used to download batch data from Transnational between the selected date ranges and show fund activity and accounts.")]

    [TextField("Bank Account Name", "Enter the name of the banking account to put on the report.", defaultValue:"Deposit Account", required: false, order: 1)]
    [TextField("Bank Account GL Code", "Enter the banking GL Code to put on the report.", required: false, order: 2)]
    [IntegerField("Days Back", "The number of days from the current date to set the default start date.", defaultValue:7, required: false, order: 3)]
    [FinancialGatewayField("Default Gateway", "Default Financial Gateway", required: false, order: 4)]
    [CodeEditorField("Contents", @"The Lava template to use for displaying activities assigned to current user.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{% include '~/Plugins/com_kevinrutledge/TransnationalTransactionReconciliation/Assets/Lava/TransnationalTransactionReconciliationDisplay.lava' %}", "", 5)]
    public partial class DownloadTransactionBatches : Rock.Web.UI.RockBlock
    {
        public class FinancialTransactionInformation : RockDynamic
        {
            //public Rock.Financial.Payment gatewayPayment { get; set; }

            public Rock.Model.FinancialTransaction RockTransaction { get; set; }

            //public List<Rock.Model.FinancialTransactionDetail> RockTransactionDetails { get; set; }



        }

        public class accountList : RockDynamic
        {
            public int AccountId { get; set; }
            public string AccountName { get; set; }
            public decimal? Amount { get; set; }
            public string GlCode { get; set; }
        }
        public class FinancialGatewayBatch : RockDynamic
        {
            public string SettlementBatchId { get; set; }
            public DateTime? SettlementDate { get; set; }
            public int TransactionCount { get; set; }
            public Decimal BatchTotal { get; set; }
            public List<FinancialTransactionInformation> Transactions { get; set; }
            public int ReconciledCount { get; set; }
            public List<accountList> accountSummary { get; set; }

        }


        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            
            RockPage.AddScriptLink("~/Plugins/com_kevinrutledge/TransnationalTransactionReconciliation/Assets/JS/TransnationalTransactionReconciliationDisplay.js");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        /// 
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Set timeout for up to 15 minutes (just like installer)
            Server.ScriptTimeout = 900;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 900;

            

            if ( !Page.IsPostBack )
            {
                double daysBack;
                daysBack = ExtensionMethods.AsDouble(GetAttributeValue("DaysBack"));
                 

                //if (selectedGateway == null)
                //{
                //    gpGateway = selectedGateway.;
                //}

                if (drpDates.UpperValue == null) { 
                drpDates.UpperValue = RockDateTime.Now.AddDays(-1);
                }
                if(drpDates.LowerValue == null) { 
                drpDates.LowerValue = drpDates.UpperValue.Value.AddDays(-1 * daysBack);
                }

                var financialGateway = GetSelectedGateway();
                if ( financialGateway != null )
                {
                    var today = RockDateTime.Today;
                    var days = today.DayOfWeek == DayOfWeek.Monday ? new TimeSpan( 3, 0, 0, 0 ) : new TimeSpan( 1, 0, 0, 0 );
                    var endDateTime = today.Add( financialGateway.GetBatchTimeOffset() );

                    drpDates.UpperValue = RockDateTime.Now.CompareTo( endDateTime ) < 0 ? today.AddDays( -1 ) : today;
                    drpDates.LowerValue = drpDates.UpperValue.Value.Subtract( days );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void btnDownload_Click( object sender, EventArgs e )
        {

            DateTime? startDateTime = drpDates.LowerValue;
            DateTime? endDateTime = drpDates.UpperValue;

            if ( startDateTime.HasValue && endDateTime.HasValue && endDateTime.Value.CompareTo(startDateTime.Value) >= 0)
            {
                var financialGateway = GetSelectedGateway();
                if ( financialGateway != null )
                {
                    var gateway = financialGateway.GetGatewayComponent();
                    if ( gateway != null )
                    {
                        DateTime start = startDateTime.Value;
                        DateTime end = endDateTime.Value.AddDays( 1 );

                        var rockContext = new RockContext();
                        var financialTransactionDetailService = new FinancialTransactionDetailService(rockContext);
                        

                        string errorMessage = string.Empty;
                        List<Rock.Financial.Payment> payments = gateway.GetPayments( financialGateway, start, end, out errorMessage );

                        var filtered = new List<Rock.Financial.Payment>();
                        
                        foreach (var item in payments)
                        {
                            if (item.IsSettled == true && item.Status == "Complete")
                            {
                                filtered.Add(item);
                            }
                        }
                        var grouped = filtered.Distinct().OrderByDescending(x => x.SettledDate)
                                        .GroupBy(x => x.SettledGroupId);
                       
                       var batches  = new List<FinancialGatewayBatch>();
                        
                        foreach (var batch in grouped) {
                            var batchitem = new FinancialGatewayBatch();

                                batchitem.SettlementBatchId = batch.Key;
                                batchitem.SettlementDate = batch.ElementAt(0).SettledDate;
                                batchitem.TransactionCount = batch.Count();
                                batchitem.BatchTotal = batch.Sum(i => i.Amount);
                                batchitem.ReconciledCount = 0;

                            batchitem.Transactions = new List<FinancialTransactionInformation>();

                            
                            var accounts = new List<accountList>();

                            foreach (var transaction in batch.ToList()) {
                                
                                var transactionItem = new FinancialTransactionInformation();

                                


                                var details = financialTransactionDetailService
                                          .Queryable()
                                          .Where(t =>
                                             t.Transaction.TransactionCode == transaction.TransactionCode)
                                          .ToList();
                                if (details.Count() > 0) {
                                    transactionItem.RockTransaction = details.First().Transaction;
                                    transactionItem.RockTransaction.LoadAttributes();
                                    string reconciledOn = transactionItem.RockTransaction.GetAttributeValue("ReconciledOn");

                                    if (transactionItem.RockTransaction.IsReconciled == true)
                                    {
                                        batchitem.ReconciledCount++;
                                    }
                                    //transactionItem.gatewayPayment = transaction;

                                    foreach (var account in details) {
                                        //transactionItem.RockTransactionDetails = new List<Rock.Model.FinancialTransactionDetail>();
                                        //transactionItem.RockTransactionDetails.Add(account);
                                        var newItem = new accountList();
                                        newItem.AccountId = account.Account.Id;
                                        newItem.AccountName = account.Account.Name;
                                        newItem.Amount = account.Amount;
                                        newItem.GlCode = account.Account.GlCode;
                                        accounts.Add(newItem);
                                    }
                                    batchitem.Transactions.Add(transactionItem);
                                }
                                
                            }
                            var groupedAccounts = accounts.OrderBy(x => x.AccountName).GroupBy(x => x.AccountId);
                            
                            var accountSummary = new List<accountList>();

                            foreach (var account in groupedAccounts) {
                                var newAccount = new accountList();
                                newAccount.AccountId = account.ElementAt(0).AccountId;
                                newAccount.AccountName = account.ElementAt(0).AccountName;
                                newAccount.Amount = account.Sum(s => s.Amount);
                                if (newAccount != null)
                                {
                                    accountSummary.Add(newAccount);
                                }

                            }
                            
                            batchitem.accountSummary = accountSummary;
                            
                            batches.Add(batchitem);

                            
                        }

                        
                        var batchesList = batches.ToList();

                        var mergeFields = new Dictionary<string, object>();

                        string contents = GetAttributeValue("Contents");
                        string bankAccountName = GetAttributeValue("BankAccountName");
                        string bankAccountGLCode = GetAttributeValue("BankAccountGLCode");

                        string appRoot = ResolveRockUrl("~/");
                        string themeRoot = ResolveRockUrl("~~/");
                        contents = contents.Replace("~~/", themeRoot).Replace("~/", appRoot);


                        mergeFields.Add("Batches", batches);
                        mergeFields.Add("BankAccountName", bankAccountName);
                        mergeFields.Add("BankAccountGlCode", bankAccountGLCode);


                       lContents.Text = contents.ResolveMergeFields(mergeFields);

                    }
                    else
                    {
                        ShowError( "Selected Payment Gateway does not have a valid payment processor!" );
                    }
                }
                else
                {
                    ShowError( "Please select a valid Payment Gateway!" );
                }
            }
            else
            {
                ShowError("Please select a valid Date Range!");
            }

        }

        #endregion

        #region Methods

        private FinancialGateway GetSelectedGateway()
        {
            int? gatewayId = gpGateway.SelectedValueAsInt();
            if ( gatewayId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialGateway = new FinancialGatewayService( rockContext ).Get( gatewayId.Value );
                    if ( financialGateway != null )
                    {
                        financialGateway.LoadAttributes( rockContext );
                        return financialGateway;
                    }
                }
            }

            return null;
        }

        private void ShowError(string message)
        {
            nbError.Text = message;
            nbError.Visible = true;
        }

        #endregion

    }
}