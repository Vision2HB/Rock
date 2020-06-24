<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMigration.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.MigrationTools.GroupMigration" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbSuccess" Visible="false" NotificationBoxType="Success" Text="Groups copied successfully."></Rock:NotificationBox>
        <div class="row">
            <div class="col-sm-6">
                <Rock:RockLiteral ID="ltName" runat="server" Label="Group Name"></Rock:RockLiteral>
            </div>
            <div class="col-sm-6">
                <Rock:RockLiteral ID="ltGroupTypeName" runat="server" Label="Group Type"></Rock:RockLiteral>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6">
                <Rock:GroupPicker runat="server" ID="gpOldGroup" Label="Existing Group" Required="true" OnValueChanged="gpOldGroup_ValueChanged"  />
            </div>
            <div class="col-sm-6">
                
                <Rock:GroupPicker runat="server" ID="gpNewParent" Label="New Parent Group" Required="true" OnValueChanged="gpNewParent_ValueChanged" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockRadioButtonList ID="rblDelete" runat="server" Label="Delete Existing Groups (Y/N)?" RepeatDirection="Horizontal">
                    <asp:ListItem Value="1" Text="Yes" />
                    <asp:ListItem Value="0" Text="No" Selected="True" />
                </Rock:RockRadioButtonList>
            </div>
            <div class="col-md-6">
                <Rock:RockRadioButtonList ID="rblAttendance" runat="server" Label="Copy Attendance (Y/N)?" RepeatDirection="Horizontal">
                    <asp:ListItem Value="1" Text="Yes" Selected="True" />
                    <asp:ListItem Value="0" Text="No" />
                </Rock:RockRadioButtonList>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockRadioButtonList ID="rblIncludeRootGroup" runat="server" Label="New Parent Becomes Root Group (Y/N)?" RepeatDirection="Horizontal">
                    <asp:ListItem Value="1" Text="Yes (Root Group merges into New Parent Group)" Selected="True" />
                    <asp:ListItem Value="0" Text="No (Root Group goes under New Parent Group)" />
                </Rock:RockRadioButtonList>
            </div>

        </div>
        <Rock:RockDropDownList runat="server" ID="ddlGroupTypes" Label="New Group Type" DataValueField="Id" DataTextField="Name"
            Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupTypes_SelectedIndexChanged">
        </Rock:RockDropDownList>
        <div class="row">
        <div class="col-sm-6">
            <asp:Panel runat="server" ID="pnlRoles" Visible="false">
                <h3>Group Member Role Mappings</h3>
                <asp:PlaceHolder runat="server" ID="phRoles" />
            </asp:Panel>
        </div>
        <div class="col-sm-6">
            <asp:Panel runat="server" ID="pnlAttributes" Visible="false">
                <h3>Group Attribute Mappings</h3>
                <asp:PlaceHolder runat="server" ID="phAttributes" />
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlMemberAttributes" Visible="false">
                <h3>Group Member Attribute Mappings</h3>
                <asp:PlaceHolder runat="server" ID="phMemberAttributes" />
            </asp:Panel>
        </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <Rock:BootstrapButton runat="server" ID="btnAdd" CssClass="btn btn-success" Text="Add to Script"
                     Visible="false" OnClick="btnAdd_Click"></Rock:BootstrapButton>
            </div>
        </div>
        <br />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Script</h1>
            </div>
            <div class="panel-body">
        <div class="row">
            <div class="col-xs-12">
                <Rock:CodeEditor runat="server" ID="tScript" Rows="12" TextMode="MultiLine" ReadOnly="true" EditorMode="Sql" />
            </div>
        </div>
            </div>
        </div>


    </ContentTemplate>
</asp:UpdatePanel>
