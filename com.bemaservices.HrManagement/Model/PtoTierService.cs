﻿// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Data;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PtoTierService : Service<PtoTier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoTierService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PtoTierService( RockContext context ) : base( context ) { }

        public int Copy( int ptoTierId )
        {
            throw new NotImplementedException();
        }
    }
}
