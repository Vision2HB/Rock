<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourceList.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.ResourceList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bookmark"></i> Resource List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="tFilter" runat="server">
                        <Rock:RockTextBox id="tbName" runat="server" Label="Organization Name" />
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockDropDownList ID="ddlOwner" runat="server" Label="Owner" />
                        <Rock:RockDropDownList ID="ddlType" runat="server" Label="Resource Type" />
                        <Rock:SlidingDateRangePicker ID="drpInterviewDate" runat="server" Label="Interview Date" PreviewLocation="None" />
                        <Rock:RockCheckBox ID="cbBenevolenceCounselor" runat="server" Label="Benevolence Counselor" />
                        <Rock:RockCheckBox ID="cbSupportGroupsOfferred" runat="server" Label="Support Groups Offered" />
                        <Rock:RockCheckBox ID="cbSlidingFeeOffered" runat="server" Label="Sliding Fee Offered" />
                        <Rock:RockCheckBox ID="cbWillowAttender" runat="server" Label="Willow Attender" />

                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gResource" runat="server" AllowSorting="true" OnRowSelected="gResource_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                            <Rock:RockBoundField DataField="Name" SortExpression="Name" HeaderText="Organization Name" />
                            <Rock:RockBoundField DataField="FirstName" SortExpression="FirstName" HeaderText="First Name" />
                            <Rock:RockBoundField DataField="LastName" SortExpression="LastName" HeaderText="Last Name" />
                            <Rock:BoolField DataField="ReducedFeeProgramParticpant" SortExpression="ReducedFeeProgramParticpant" HeaderText="Benevolence Counselor" />
                            <Rock:BoolField DataField="SupportGroupsOfferred" SortExpression="SupportGroupsOfferred" HeaderText="Support Groups Offerred" />
                            <Rock:BoolField DataField="SlidingFeeOffered" SortExpression="SlidingFeeOffered" HeaderText="Sliding Fee Offered" />
                            <Rock:BoolField DataField="WillowAttender" SortExpression="WillowAttender" HeaderText="Willow Attender" />
                            <Rock:BoolField DataField="IsActive" SortExpression="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gResource_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
