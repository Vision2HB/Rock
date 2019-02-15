using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;

using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "ServiceU Import" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Block for importing ServiceU files to a batch." )]

    [LinkedPage( "Batch Detail Page", "The page used to display details of a batch.", false, "", "", 1 )]
    public partial class ServiceUImporter : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _binaryFileId = null;
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();
        private static int _transactionTypeContributionId = Rock.Web.Cache.DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
        private decimal _totalAmount = 0.0M;

        protected string signalREventName = "serviceUImport";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            _binaryFileId = ViewState["BinaryFileId"] as int?;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            signalREventName = string.Format( "serviceUImport_{0}_{1}", this.BlockId, Session.SessionID );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {

                    // currency Types
                    var currencyTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid();
                    var currencyDefinedType = new DefinedTypeService( rockContext ).GetByGuid( currencyTypeGuid );

                    dvpCurrencyType.DefinedTypeId = currencyDefinedType.Id;
                }

                ShowEntry();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["BinaryFileId"] = _binaryFileId;
            return base.SaveViewState();
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

        /// <summary>
        /// Handles the FileUploaded event of the fuTellerFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuServiceUFile_FileUploaded( object sender, EventArgs e )
        {
            _binaryFileId = fuServiceUFile.BinaryFileId;

            var binaryFile = new BinaryFileService( new RockContext() ).Get( _binaryFileId.Value );
            if ( binaryFile != null )
            {
                tbBatchName.Text = Path.GetFileNameWithoutExtension( binaryFile.FileName );
            }
            // Validate the file (method will display error message if not valid
            IsFileValid();
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            // If the XML is valid, ask for a confirmation
            if ( true )
            {
                ShowConfirmation();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirm_Click( object sender, EventArgs e )
        {
            // send signalR message to start progress indicator
            int progress = 0;
            _hubContext.Clients.All.receiveNotification( signalREventName, "0" );

            ServiceUImport import = null;
            int? total = null;
            int? batchId = null;
            string batchName = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                // Get the XML
                var binaryFile = new BinaryFileService( rockContext ).Get( _binaryFileId.Value );
                if ( binaryFile != null )
                {
                    using ( var stream = new StreamReader( binaryFile.ContentStream ) )
                    {
                        import = ProcessFile( stream );
                    }
                }

                // Get the number of transactions
                if ( import != null )
                {
                    total = import.Records.Count();
                }

                if ( import != null && total.HasValue && total.Value > 0 )
                {
                    var batchService = new FinancialBatchService( rockContext );
                    FinancialBatch batch = null;

                    // create the batch
                    if ( batch == null )
                    {
                        batchName = Path.GetFileNameWithoutExtension( binaryFile.FileName );

                        batch = new FinancialBatch();
                        batch.Guid = Guid.NewGuid();
                        batch.Name = batchName;
                        batch.Status = BatchStatus.Open;
                        batch.BatchStartDateTime = RockDateTime.Today;
                        batch.BatchEndDateTime = batch.BatchStartDateTime.Value.AddDays( 1 );
                        batch.ControlAmount = 0;
                        batchService.Add( batch );

                        rockContext.SaveChanges();

                        batchId = batch.Id;
                    }
                    else
                    {
                        batchName = batch.Name;
                    }
                }
            }

            // Initialize the status variables
            int matchCount = 0;
            int unmatchCount = 0;
            int errorCount = 0;

            var allErrorMessages = new List<string>();

            // Process each transaction
            foreach ( var record in import.Records )
            {
                var errorMessages = new List<string>();

                var status = ProcessTransaction( record, import.Date, batchId, out errorMessages );

                switch ( status )
                {
                    case ProcessStatus.Matched: matchCount++; break;
                    case ProcessStatus.Unmatched: unmatchCount++; break;
                    case ProcessStatus.Error: errorCount++; break;
                }

                allErrorMessages.AddRange( errorMessages );

                // Update progress using signalR
                progress++;
                int percentage = ( progress * 100 ) / total.Value;
                _hubContext.Clients.All.receiveNotification( signalREventName, percentage.ToString( "N0" ) );
            }

            // update success message to indicate the txns that were updated
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "<li>{0:N0} {1} processed.</li>", total.Value, "transaction".PluralizeIf( total.Value > 1 ) );
            if ( errorCount > 0 )
            {
                sb.AppendFormat( "<li>{0:N0} {1} NOT imported due to error during import (see errors below).</li>", errorCount,
                    ( errorCount == 1 ? "transaction was" : "transactions were" ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var batch = new FinancialBatchService( rockContext ).Get( batchId.Value );
                if ( batch != null )
                {
                    // update batch control amount
                    batch.ControlAmount += _totalAmount;
                    rockContext.SaveChanges();

                    // Add link to batch
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "batchId", batchId.ToString() );
                    string batchUrl = LinkedPageUrl( "BatchDetailPage", qryParam );
                    string batchLink = string.Format( "<a href='{0}'>{1}</a>", batchUrl, batch.Name );

                    int totalTransactions = matchCount + unmatchCount;
                    string summaryformat = totalTransactions == 1 ?
                        "<li>{0} transaction of {1} was added to the {2} batch.</li>" :
                        "<li>{0} transactions totaling {1} were added to the {2} batch</li>";
                    sb.AppendFormat( summaryformat, totalTransactions.ToString( "N0" ), _totalAmount.FormatAsCurrency(), batchLink );
                }
            }

            nbSuccess.Text = string.Format( "<ul>{0}</ul>", sb.ToString() );

            // Display any errors that occurred
            if ( allErrorMessages.Any() )
            {
                StringBuilder sbErrors = new StringBuilder();
                foreach ( var errorMsg in allErrorMessages )
                {
                    sbErrors.AppendFormat( "<li>{0}</li>", errorMsg );
                }

                nbErrors.Text = string.Format( "<ul>{0}</ul>", sbErrors.ToString() );
                nbErrors.Visible = true;
            }
            else
            {
                nbErrors.Visible = false;
            }

            ShowResults();

        }

        /// <summary>
        /// Handles the Click event of the btnCancelConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelConfirm_Click( object sender, EventArgs e )
        {
            ShowEntry();
        }

        /// <summary>
        /// Handles the Click event of the btnImportAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportAnother_Click( object sender, EventArgs e )
        {
            fuServiceUFile.BinaryFileId = null;
            tbBatchName.Text = "";

            ShowEntry();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the entry.
        /// </summary>
        private void ShowEntry()
        {
            pnlEntry.Visible = true;
            pnlConfirm.Visible = false;
            pnlResults.Visible = false;
        }

        /// <summary>
        /// Shows the confirmation.
        /// </summary>
        private void ShowConfirmation()
        {
            // Get the number of transactions in the XML file
            int? txnCount = null;
            var import = GetImport();
            if ( import != null )
            {
                txnCount = import.Records.Count;
            }

            if ( txnCount.HasValue )
            {
                string batchName = "a new batch";
                var binaryFile = new BinaryFileService( new RockContext() ).Get( _binaryFileId.Value );
                if ( binaryFile != null )
                {
                    batchName = Path.GetFileNameWithoutExtension( binaryFile.FileName );
                }

                lConfirm.Text = string.Format( "This will import <strong>{0:N0}</strong> transactions to {1}. Click <em>Confirm</em> to continue.", txnCount.Value, batchName );

                // Show the confirm/status/result dialog 
                pnlEntry.Visible = false;
                pnlConfirm.Visible = true;
                pnlResults.Visible = false;
            }
        }

        /// <summary>
        /// Shows the results.
        /// </summary>
        private void ShowResults()
        {
            pnlEntry.Visible = false;
            pnlConfirm.Visible = false;
            pnlResults.Visible = true;
        }

        /// <summary>
        /// Processes a transaction.
        /// </summary>
        /// <param name="giftElement">The gift element.</param>
        /// <param name="batch">The batch.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="envelopeAttributeId">The envelope attribute identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private ProcessStatus ProcessTransaction( ServiceURecord record, string date, int? batchId, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    string checkNum = record.TransactionNumber;

                    // Start to create the transaction
                    var txn = new FinancialTransaction();
                    txn.BatchId = batchId;
                    txn.TransactionTypeValueId = _transactionTypeContributionId;

                    // re-arrange date string to be MM/DD/YY
                    var batchDate = date.Substring( 2, 2 ) + "/" + date.Substring( 4, 2 ) + "/" + date.Substring( 0, 2 );
                    txn.TransactionDateTime = batchDate.AsDateTime();
                    txn.FinancialPaymentDetail = new FinancialPaymentDetail();

                    txn.FinancialPaymentDetail.CurrencyTypeValueId = dvpCurrencyType.SelectedValueAsId();
                    txn.TransactionCode = checkNum;
                    txn.Summary = "Online";

                    FinancialTransactionDetail txnDetail = null;

                    //
                    string accountName = record.GLContributionCode;
                    decimal? amount = record.Amount.AsDecimalOrNull();

                    if ( !string.IsNullOrWhiteSpace( accountName ) && amount.HasValue )
                    {
                        // shift decimal over two places
                        amount = amount.Value * ( decimal ) 00.01;

                        var accountQry = new FinancialAccountService( rockContext ).Queryable().AsNoTracking();
                        accountQry = accountQry.Where( a => a.Name == accountName );
                        int? rockAccountId = accountQry.Select( a => a.Id ).FirstOrDefault();
                        if ( rockAccountId.HasValue && rockAccountId > 0 )
                        {
                            txnDetail = new FinancialTransactionDetail();
                            txnDetail.AccountId = rockAccountId.Value;
                            txnDetail.Amount = amount.Value;
                            txn.TransactionDetails.Add( txnDetail );

                            _totalAmount += amount.Value;
                        }
                    }

                    if ( txnDetail == null )
                    {
                        errorMessages.Add( string.Format( "Error: Invalid Account (Account Name:{0})", accountName ) );
                    }

                    if ( errorMessages.Any() )
                    {
                        return ProcessStatus.Error;
                    }

                    // Save the transaction and update the batch control amount
                    new FinancialTransactionService( rockContext ).Add( txn );
                    rockContext.SaveChanges();

                    return ProcessStatus.Unmatched;
                }

            }

            catch ( Exception ex )
            {
                errorMessages.Add( string.Format( "Error: {0}", ex.Message ) );
                return ProcessStatus.Error;
            }
        }

        /// <summary>
        /// Determines whether the binary file has valid information.
        /// </summary>
        /// <returns></returns>
        private bool IsFileValid()
        {
            try
            {
                var import = GetImport();
                if ( import == null )
                {
                    ShowError( "Invalid Import File", "Could not read text file." );
                    return false;
                }

                // re-arrange date string to be MM/DD/YY
                var date = import.Date.Substring( 2, 2 ) + "/" + import.Date.Substring( 4, 2 ) + "/" + import.Date.Substring( 0, 2 );
                if ( date.AsDateTime() == null )
                {
                    ShowError( "Invalid Import File", "The import file does not appear to have a valid date." );
                    return false;
                }

                if ( !import.Records.Any() )
                {
                    ShowError( "Warning", "The import file does not appear to contain any transactions." );
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ShowError( "Invalid Import File", ex.Message );
                return false;
            }
        }

        private ServiceUImport GetImport()
        {
            ServiceUImport import = new ServiceUImport();

            if ( _binaryFileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFile = new BinaryFileService( rockContext ).Get( _binaryFileId.Value );
                    if ( binaryFile != null )
                    {
                        using ( var stream = new StreamReader( binaryFile.ContentStream ) )
                        {
                            import = ProcessFile( stream );
                        }
                    }
                }
            }

            return import;
        }

        public ServiceUImport ProcessFile( StreamReader stream )
        {
            var import = new ServiceUImport();

            string firstLine = stream.ReadLine();

            // parse first line which contains date
            import.Date = firstLine.Substring( 13, 16 ).Trim();
            string line;

            // parse each transaction line till end of file
            while ( ( line = stream.ReadLine() ) != null )
            {
                var record = new ServiceURecord();

                record.Column1 = line.Substring( 0, 1 );
                record.TransactionNumber = line.Substring( 36, 8 );
                record.CustomerNumber = line.Substring( 44, 7 );
                record.GLContributionCode = line.Substring( 51, 8 );
                record.Amount = line.Substring( 66, 10 );

                if ( !string.IsNullOrWhiteSpace( record.TransactionNumber ) && 
                     !string.IsNullOrWhiteSpace( record.CustomerNumber ) &&
                     !string.IsNullOrWhiteSpace( record.Amount ) )
                {
                    import.Records.Add( record );
                }
            }

            return import;
        }

        #region Show Notifications

        /// <summary>
        /// Shows a warning.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string title, string message )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Warning;
            ShowMessage( title, message );
        }

        /// <summary>
        /// Shows a error.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string title, string message )
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            ShowMessage( title, message );
        }

        /// <summary>
        /// Shows a message.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowMessage( string title, string message )
        {
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
        }

        #endregion

        #endregion

        /// <summary>
        /// Enumeration for tracking transction status
        /// </summary>
        private enum ProcessStatus
        {
            Matched = 0,
            Unmatched = 1,
            Error = 2
        }

        public class ServiceUImport
        {
            public string Date { get; set; }

            public List<ServiceURecord> Records = new List<ServiceURecord>();
        }

        public class ServiceURecord
        {
            public string Column1 { get; set; }

            public string TransactionNumber { get; set; }

            public string CustomerNumber { get; set; }

            public string GLContributionCode { get; set; }

            public string Amount { get; set; }
        }

    }

}


