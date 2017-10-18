<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>


<asp:Content ID="ctSidebar" ContentPlaceHolderID="sidebar" runat="server">

    <!-- remove when page class is available -->
    <script>
        Sys.Application.add_load( function () {
            $('body').addClass('side-bar');
            });
    </script>

    <div class="side-cover-wrapper full-screen visible-lg visible-md">
		    <div class="fslider" data-speed="3000" data-pause="10000" data-animation="fade" data-arrows="false" data-pagi="false" style="position: absolute; width: 100%; height: 100%; top: 0; left: 0;">
			    <div class="flexslider" style="height: 100% !important;">
				    <div class="slider-wrap" style="height: inherit !important;">
					    <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/1.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
					    <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/2.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/9.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/3.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
<%--                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/4.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>--%>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/5.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/6.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/7.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
                        <div class="slide full-screen force-full-screen" style="background: url('/Themes/CareCenter/Assets/Images/8.jpg') center right; background-size: cover; height: 100% !important; opacity: 0;"></div>
				    </div>
			    </div>
		    </div>
	    </div>
</asp:Content>


<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">
    
	<main class="container-fluid">
        
        <!-- Start Content Area -->
        
        <!-- Page Title -->
        <Rock:PageIcon ID="PageIcon" runat="server" /> <h1 class="page-title"><Rock:PageTitle ID="PageTitle" runat="server" /></h1>
        
        <!-- Breadcrumbs -->    
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />

        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Main" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

	</main>

</asp:Content>

