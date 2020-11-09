<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransnationalTransactionReconciliation.ascx.cs" Inherits="RockWeb.Plugins.com_kevinrutledge.Finance.DownloadTransactionBatches" %>

<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>


        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-download"></i>
                    Download Transaction Batches
                </h1>
            </div>

            <div class="panel-body ">
                <div class="row align-items-bottom" style="display:flex; flex-direction:row; flex-wrap:wrap; align-items:center;">
                    <div class="col-lg-4">
                       <Rock:FinancialGatewayPicker ID="gpGateway" runat="server" Label="Payment Gateway" Required="true" ShowAll="false" />
                    </div>
                    <div class="col-lg-5">
                       <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" Required="true" />
                    </div>

                    <div class="col-lg-3">
                       <Rock:BootstrapButton ID="btnDownload" runat="server" CssClass="btn btn-primary" Text="Download Batches" DataLoadingText="Downloading..." CausesValidation="true" OnClick="btnDownload_Click" />
                    </div>
                </div>
          
                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Heading="Transaction Download Summary:" Visible="false" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false" />


                <div class="batches">
                    <asp:Literal ID="lContents" runat="server" />
                </div>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
