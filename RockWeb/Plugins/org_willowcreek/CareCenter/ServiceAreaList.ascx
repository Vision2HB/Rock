<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceAreaList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ServiceAreaList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server" CssClass="panel panel-block" >

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-signs"></i> Service Areas</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:CategoryPicker ID="cpCategoriesFilter" runat="server" Label="Categories" AllowMultiSelect="true" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="rGrid" runat="server" AllowSorting="false" RowItemText="Service Area" OnRowSelected="rGrid_RowSelected" >
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Workflow Type" />
                            <Rock:RockBoundField DataField="Category.Name" HeaderText="Category" />
                            <Rock:BoolField DataField="HasSchedule" HeaderText="Has Schedule" />
                            <Rock:BoolField DataField="UsesPassport" HeaderText="Uses Passport" />
                            <Rock:SecurityField />
                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
