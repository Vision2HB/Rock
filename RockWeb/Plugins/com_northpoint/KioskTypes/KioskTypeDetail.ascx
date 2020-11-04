﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KioskTypeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.KioskTypes.KioskTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog()
    {
        $('#<%=hfAddLocationId.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlDevice" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfKioskTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-hand-pointer-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <Rock:NotificationBox ID="nbDuplicateDevice" runat="server" NotificationBoxType="Warning" Title="Sorry" Visible="false" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com_northpoint.KioskTypes.Model.KioskType, com_northpoint.KioskTypes" PropertyName="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox CausesValidation="false" ID="tbDescription" runat="server" SourceTypeName="com_northpoint.KioskTypes.Model.KioskType, com_northpoint.KioskTypes" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <Rock:DataDropDownList CausesValidation="false" runat="server" ID="ddlTemplates" Label="Checkin Template" OnSelectedIndexChanged="ddlTemplates_SelectedIndexChanged" AutoPostBack="true" DataTextField="Name" DataValueField="Id" SourceTypeName="com_northpoint.KioskTypes.Model.KioskType, com_northpoint.KioskTypes" PropertyName="CheckinTemplateId"></Rock:DataDropDownList>
                    <Rock:DataTextBox ID="tbTheme" runat="server" SourceTypeName="com_northpoint.KioskTypes.Model.KioskType, com_northpoint.KioskTypes" PropertyName="CheckinTheme" Required="false" />
                        
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList CausesValidation="false" ID="cblPrimaryGroupTypes" runat="server" Label="Check-in Area(s)" DataTextField="Name" DataValueField="Id"></Rock:RockCheckBoxList>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList CausesValidation="false" ID="cblAdditionalGroupTypes" runat="server" Label="Additional Area(s)" DataTextField="Name" DataValueField="Id"></Rock:RockCheckBoxList>
                        </div>
                    </div>
                    <h3>Locations</h3>
                    <Rock:Grid ID="gLocations" runat="server" DisplayType="Light" RowItemText="Location" ShowConfirmDeleteDialog="false">
                        <Columns>
                            <Rock:RockBoundField DataField="LocationPath" HeaderText="Name" />
                            <Rock:DeleteField OnClick="gLocations_Delete" />
                        </Columns>
                    </Rock:Grid>

                    <h3>Schedules</h3>
                    <Rock:Grid ID="gSchedules" runat="server" DisplayType="Light" RowItemText="Schedule" ShowConfirmDeleteDialog="false" Visible="false">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:DeleteField OnClick="gSchedules_Delete" />
                        </Columns>
                    </Rock:Grid>

                    <Rock:PagePicker ID="pageRedirect" runat="server" Label="Redirect Page" Help="Page to redirect kiosk to, then apply settings. Useful for multiple check-in sites; set to the AutoConfigure block's . (Optional)" />

                    
                </fieldset>



                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdLocationPicker" runat="server" SaveButtonText="Save" OnSaveClick="btnAddLocation_Click" Title="Select Check-in Location" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content ID="mdLocationPickerContent">
                <asp:HiddenField ID="hfAddLocationId" runat="server" />
                <Rock:LocationItemPicker ID="locationPicker" runat="server" Label="Check-in Location" ValidationGroup="Location" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdSchedulepicker" runat="server" SaveButtonText="Save" OnSaveClick="mdSchedulepicker_SaveClick" Title="Select Check-in Location" OnCancelScript="clearActiveDialog();" ValidationGroup="Location">
            <Content ID="mdsSchedulePickerContent">
                <asp:HiddenField ID="HiddenField1" runat="server" />
                <Rock:SchedulePicker runat="server" ID="schedulePicker" Label="Kiosk Schedule" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
