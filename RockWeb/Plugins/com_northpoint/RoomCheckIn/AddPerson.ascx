<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddPerson.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.AddPerson" %>

<style>
    .form-control {
        font-size: 28px;
        height: inherit;
    }

    .split-date-picker {
        font-size: 1.5em;
    }

    .split-date-picker select {
        display: inline;
        width: 28%;
    }

    select {
        -moz-appearance: none; /* hide dropdown button in firefox */
        -webkit-appearance: none; /* hide dropdown button in chrome */
        width: 100%;
        max-width: 100%;
    }

    select::-ms-expand {
        display: none; /* hide dropdown button in IE */
    }





    /*.datepicker {
        color: #000;
    }*/


    

    input[type=radio],
    input[type='checkbox'] {
        display: none;
    }

    input[type=radio] + label {
        display: inline;
        font-size: 28px;
        padding-right: 28px;
    }

    input[type='radio'] + label:before {
        display: inline-block;
        font-family: FontAwesome;
        font-style: normal;
        font-weight: normal;
        line-height: 1;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        padding-right: 18px;
        width: 60px;
        font-size: 60px;
        vertical-align: middle;
    }

    input[type=radio] + label:before {
        content: "\f096"; /* Checkbox Unchecked */
    }

    input[type=radio]:checked + label:before {
        content: "\f14a"; /* Checkbox Checked */
    }

    .radio label,
    .checkbox label {
        padding-left: 0;
    }



    .rock-check-box-list {
        margin:0px;
    }
    .rock-check-box-list>.control-wrapper {
        visibility:hidden;
        height:0px;
    }
    .rock-checkbox-icon {
        font-size:xx-large;
        display:inline;
        padding-right:25px;
    }
</style>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        
        <asp:HiddenField ID="hfSameFamily" runat="server" />

        <asp:Panel ID="pnlParent" runat="server">
            <div class="checkin-body">
                <div class="checkin-search-actions checkin-start">
                    <div class="checkin-header">
                        <h1><asp:Label ID="lLegalGuardian" runat="server" /></h1>
                    </div>
                    <asp:LinkButton CssClass="btn btn-primary btn-checkin" ID="lbYes" OnClick="lbYes_Click" runat="server"><span><asp:Label ID="lLegalGuardianYes" runat="server" /></span></asp:LinkButton>
                    &nbsp;&nbsp;&nbsp;
                    <asp:LinkButton CssClass="btn btn-primary btn-checkin" ID="lbNo" OnClick="lbNo_Click" runat="server"><span><asp:Label ID="lLegalGuardianNo" runat="server" /></span></asp:LinkButton>
                </div>
            </div>
            <div class="checkin-footer">
                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-default" ID="lbParentBack" runat="server" Text="Back" OnClick="lbParentBack_Click" CausesValidation="false" />
                    <asp:LinkButton CssClass="btn btn-default" ID="lbParentCancel" runat="server" Text="Cancel" OnClick="lbParentCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlChild" runat="server" Visible="false">
            <Rock:ModalAlert ID="maWarning" runat="server" />
            <div class="checkin-body">
                <div class="checkin-header">
                    <h1><asp:Label ID="lChildInformation" runat="server" /></h1>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbFirstName" Label="First Name" runat="server" onClick="this.select();" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbLastName" Label="Last Name" runat="server" onClick="this.select();" />
                    </div>
                    <div class="col-md-4">
                        <label class="control-label">Birthday</label>
                        <div class="row">
                            <div class="col-xs-3">
                                <Rock:RockDropDownList ID="rddlBirthMonth" runat="server" Font-Size="X-Large"/>
                            </div>
                            <div class="col-xs-3">
                                <Rock:RockDropDownList ID="rddlBirthDay" runat="server" Font-Size="X-Large" />
                            </div>
                            <div class="col-xs-6">
                                <Rock:RockDropDownList ID="rddlBirthYear" runat="server" Font-Size="X-Large"/>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbAllergy" Label="Allergies" Placeholder="None" runat="server" onClick="this.select();" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbMedical" Label="Medical" Placeholder="None" runat="server" onClick="this.select();" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="tbLegal" Label="Legal/Custody" runat="server" Placeholder="None" onClick="this.select();" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockCheckBoxList ID="cblGender" runat="server" Label="Gender" >
                            <asp:ListItem Text="Male" Value="Male" />
                            <asp:ListItem Text="Female" Value="Female" />
                        </Rock:RockCheckBoxList>
                        <nobr>
                        <Rock:RockCheckBox ID="chkMale" CssClass="checks" runat="server" Text="Male" UnSelectedIconCssClass="fa fa-fw fa-square-o" SelectedIconCssClass="fa fa-fw fa-check-square" AutoPostBack="true" OnCheckedChanged="chkMale_CheckedChanged" DisplayInline="true" />
                        <Rock:RockCheckBox ID="chkFemale" CssClass="checks" runat="server" Text="Female" UnSelectedIconCssClass="fa fa-fw fa-square-o" SelectedIconCssClass="fa fa-fw fa-check-square" AutoPostBack="true"  OnCheckedChanged="chkFemale_CheckedChanged" DisplayInline="true" />
                            </nobr>
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlGrade" runat="server" Label="Age/Grade" DataValueField="Key" DataTextField="Value" />
                    </div>
                </div>
                

                
            </div>
            <div class="checkin-footer">
                <div class="checkin-actions">
                    <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" CausesValidation="false" />
                    <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" CausesValidation="false" />
                    <Rock:BootstrapButton id="lbNext" OnClick="lbNext_Click" Text="Next" DataLoadingText="loading" CssClass="btn btn-primary pull-right" runat="server"></Rock:BootstrapButton>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
