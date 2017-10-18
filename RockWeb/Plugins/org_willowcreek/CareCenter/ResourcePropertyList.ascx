<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourcePropertyList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ResourcePropertyList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Properties</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                        <asp:Panel ID="pnlValues" runat="server">
                            <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                        
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gResourceProperties" runat="server" AllowPaging="false" DisplayType="Full" OnRowSelected="gResourceProperties_Edit" AllowSorting="False" RowItemText="Property Type">
                                    <Columns>
                                        <Rock:RockBoundField DataField="PropertyType" HeaderText="Property Type" />
                                        <Rock:RockBoundField DataField="Properties" HeaderText="Selected Values" />
                                    </Columns>
                                </Rock:Grid>
                            </div>

                        </asp:Panel>

                </asp:Panel>

            </div>

            

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Properties" ValidationGroup="Properties" >
                <Content>

                    <asp:HiddenField ID="hfDefinedTypeId" runat="server" />
                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Properties" />
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DefinedValuesPickerEnhanced ID="dvPicker" runat="server" Label="Selected Values" ValidationGroup="Properties" DisplayDropAsAbsolute="true" />
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
