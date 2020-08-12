<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaRelease.ascx.cs" Inherits="RockWeb.Plugins.com_northpoint.RoomCheckIn.MediaRelease" %>

<meta name="viewport" content="width=device-width, initial-scale=1">

<style>
    .large-text {
        font-size:xx-large;
    }
</style>

<script type="text/javascript">
    
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

         <asp:Panel ID="pnlParent" runat="server">
            <div class="checkin-body">
                <div class="checkin-search-actions checkin-start">
                    <div class="checkin-header">
                        <h1>Family Ministries: Media Release</h1>
                    </div>
                    <h4>
                        I grant to North Point Ministries, Inc., its representitives, and its employees the right to take photographs, videos, and/or electronic images of any member of my family in Family Ministry environments.
                        I authorize North Point Ministries, Inc., to copyright, use, and publish the photographs, videos, and/or electronic images in print and/or electronically-- with or without names-- for any lawful purpose to highlight and promote Family Ministry environments.
                        My agreeing below indicates that I have read and understood the above statement of release.
                    </h4>
                    <br />
                    <div class="col-sm-6"><asp:LinkButton CssClass="btn btn-success btn-checkin btn-block large-text" ID="lbYes" OnClick="lbYes_Click" runat="server"><i class="fa fa-check-square"></i><span class="align-middle">Yes, I Agree</span></asp:LinkButton></div>
                    
                    <div class="col-sm-6"><asp:LinkButton CssClass="btn btn-danger btn-checkin btn-block large-text" ID="lbNo" OnClick="lbNo_Click" runat="server"><i class="fa fa-minus-square"></i><span class="align-middle">No, I Refuse</span></asp:LinkButton></div>
                    
                </div>
            </div>
            
        </asp:Panel>

        <Rock:ModalAlert ID="maWarning" runat="server" />


    </ContentTemplate>
</asp:UpdatePanel>

