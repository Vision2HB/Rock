<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.plugins.com_thewoodlandsumc.Finance.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-list"></i> Pledge List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfPledges" runat="server">
                            <Rock:PersonPicker ID="ppFilterPerson" runat="server" Label="Filter by Person" IncludeBusinesses="true" />
                            <!--BEMA Custom Code: DisplayActiveOnly set to True -->
                            <Rock:AccountPicker ID="apFilterAccount" runat="server" Label="Filter by Account" AllowMultiSelect="True" DisplayActiveOnly="true" />
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                            <Rock:DateRangePicker ID="drpLastModifiedDates" runat="server" Label="Last Modified" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" ExportSource="ColumnOutput" AllowSorting="True" AllowPaging="True">
                            <Columns>
                                <!--BEMA Custom Code: Added Column -->
                                <Rock:SelectField></Rock:SelectField>
                                <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                                <Rock:RockBoundField DataField="Group.Name" HeaderText="For" SortExpression="Group.Name" />
                                <Rock:RockBoundField DataField="Account.Name" HeaderText="Account" SortExpression="Account.Name" />
                                <!--BEMA Custom Code: Header Changed -->
                                <Rock:CurrencyField DataField="TotalAmount" HeaderText="Total Pledge" SortExpression="TotalAmount" />
                                <!--BEMA Custom Code: Added Column -->
                                <Rock:CurrencyField DataField="AmountGiven" HeaderText="Total Given" />
                                <!--BEMA Custom Code: Added Column -->
                                <Rock:CurrencyField DataField="AmountRemaining" HeaderText="Amount Remaining" />
                                <Rock:DefinedValueField DataField="PledgeFrequencyValueId" HeaderText="Payment Schedule" SortExpression="PledgeFrequencyValue.Value" />
                                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate" />
                                <Rock:DateField DataField="EndDate" HeaderText="Ends" SortExpression="EndDate" />
                                <Rock:DateField DataField="ModifiedDateTime" HeaderText="Last Modified" SortExpression="ModifiedDateTime" />
                                <!--BEMA Custom Code: Added Column -->
                                <Rock:RockBoundField DataField="PersonAliasModified.Person" HeaderText="Modified By" SortExpression="PersonAliasModified.Person.LastName,PersonAliasModified.Person.NickName" />
                                <!--BEMA Custom Code: Added Column -->
                                <Rock:DeleteField OnClick="gPledges_Delete" />
                            </Columns>
                        </Rock:Grid>

                        <!--BEMA Custom Code: Added Control -->
                        <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" CssClass="margin-b-none" Dismissable="true"></Rock:NotificationBox>

                    </div>

                </div>
            </div>

            <div class="row">
                <div class="col-md-4 col-md-offset-8 margin-t-md">
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="panel panel-block">
                        <div class="panel-heading">
                            <h1 class="panel-title">Total Results</h1>
                            <div class="panel-labels">
                            </div>
                        </div>
                        <div class="panel-body">
                            <asp:Repeater ID="rptAccountSummary" runat="server">
                                <ItemTemplate>
                                    <div class='row'>
                                        <div class='col-xs-8'><%#Eval("Name")%></div>
                                        <div class='col-xs-4 text-right'><%#Eval("TotalAmount")%></div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <div class='row'>
                                <div class='col-xs-8'><b>Total: </div>
                                <div class='col-xs-4 text-right'>
                                    <asp:Literal ID="lGrandTotal" runat="server" /></b>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <!--BEMA Custom Code Start: Added Modal -->
        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgReassign" runat="server" Title="Reassign Pledges" ValidationGroup="Reassign"
            SaveButtonText="Reassign" OnSaveClick="dlgReassign_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>

                <Rock:PersonPicker ID="ppReassign" runat="server" Label="Reassign Selected Pledges To" Required="true" ValidationGroup="Reassign" />
                <Rock:AccountPicker ID="apReassign" runat="server" Label="Account" Required="false" ValidationGroup="Account" AllowMultiSelect="false" DisplayActiveOnly="true" />


            </Content>
        </Rock:ModalDialog>
        <!--BEMA Custom Code End: Added Modal -->
    </ContentTemplate>
</asp:UpdatePanel>
