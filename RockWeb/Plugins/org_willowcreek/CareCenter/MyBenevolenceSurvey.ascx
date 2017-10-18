<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyBenevolenceSurvey.ascx.cs" Inherits="RockWeb.Plugins.org_willowcreek.CareCenter.MyBenevolenceSurvey
    " %>

<style>
    .survey-input {
        font-size: 48px;
        height: 68px;
        text-align: center;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
       
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-plus-o"></i> Visits</h1>
            </div>

            <div class="panel-body">
                <div class="text-center">
                    <Rock:RockTextBox ID="txtSurveyId" runat="server" CssClass="input-width-lg survey-input" style="display: inline;" />
                    <br />
                    <asp:LinkButton ID="btnSubmit" runat="server" Text="Start Application" CssClass="btn btn-primary btn-lg margin-v-md" OnClick="btnSubmit_Click" />

                    <asp:Literal ID="lMessages" runat="server"  />
                </div>

            </div>
        
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
