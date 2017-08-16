// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Net;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Word Cloud" )]
    [Category( "Examples" )]
    [Description( "Shows a cloud of words" )]
    public partial class WordCloud : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            pnlSvgContainer.Visible = false;

            RockPage.AddScriptLink( "~/Scripts/d3-cloud/d3.layout.cloud.js" );
            RockPage.AddScriptLink( "~/Scripts/d3-cloud/d3.min.js" );

            var script = string.Format( @"

Sys.Application.add_load(function() {{
    Rock.controls.wordcloud.initialize({{
        inputTextId: '{0}',
        svgContainerId: '{1}',
        width: 800,
        height: 300
    }});

}});", tbText.ClientID, pnlSvgContainer.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "wordcloud", script, true );
        }
        
        protected void btnMakeCloud_Click( object sender, EventArgs e )
        {
            pnlSvgContainer.Visible = true;
        }
    }
}
