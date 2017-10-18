<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceAreaBanList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ServiceAreaBanList" %>


<asp:UpdatePanel ID="upServiceAreaBan" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfServiceAreaId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-ban"></i> Service Areas Bans</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                    <asp:Panel ID="pnlValues" runat="server">

                        <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gServiceAreaBanList" runat="server" AllowPaging="true" DisplayType="Full" RowItemText="Ban" OnRowSelected="gServiceAreasBan_Edit" AllowSorting="true">
                                <Columns>
                                    <Rock:RockBoundField DataField="PersonAlias.Person.FullName" HeaderText="Person Name" />
                                    <Rock:DateTimeField DataField="BanExpireDate" HeaderText="Expiration Date" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </div>

            <Rock:ModalDialog ID="modalServiceAreaBan" runat="server" Title="Service Areas Ban" ValidationGroup="Value">
                <Content>
                    <asp:HiddenField ID="hfServiceAreaBanId" runat="server" />
                    <asp:ValidationSummary ID="valSummaryServiceBan" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ServiceBanGroup" />
                    <legend>
                        <asp:Literal ID="lActionTitleServiceAreaBan" runat="server" />
                    </legend>
                    <fieldset>
                        <div class="row-fluid">
                            <div class="span12">
                                <Rock:PersonPicker ID="ppContact" runat="server" Label="Contact" Required="true" ValidationGroup="ServiceBanGroup" />
                                <Rock:DatePicker ID="dtpBanExpiryDate" runat="server" SourceTypeName="org.willowcreek.CareCenter.Model.ServiceAreaBan, org.willowcreek.CareCenter" PropertyName="BanExpireDate" Label="Ban Expiration Date" />
                            </div>
                        </div>
                    </fieldset>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
