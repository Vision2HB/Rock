<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ParentLocationEdit.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.ParentLocationEdit" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfConfigId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> Parent Locations</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gParentLocations" runat="server" DataKeyNames="Id, ParentLocations" DisplayType="Light">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Configuration" />
                            <Rock:RockBoundField DataField="ParentLocations" HeaderText="Parent Locations" />
                            <Rock:EditField OnClick ="gParentLocations_Edit" />                            
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalDialog id="dlgEdit" runat="server" Title="Edit Parent Locations" OnSaveClick="dlgEdit_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EditParentLocations">
            <Content>
                <p>Enter the list of location names, separated by commas, in the order they should appear on the check-in screen.</p>
                <p>If you want something different to be saved/printed on the tag than what is shown on the screen, use this format for the location: Screen Text^Label Text<br /></p>
                <Rock:RockTextBox id="tbLocations" runat="server" Label="Parent Locations" Required="true" TextMode="MultiLine" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>