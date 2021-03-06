﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.ExpressRoute
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.ExpressRoute.Models;
    using Utilities.ExpressRoute;
    using System.ComponentModel;

    [Cmdlet(VerbsCommon.New, "AzureBGPPeering"), OutputType(typeof(BgpPeeringGetResponse))]
    public class NewAzureBGPPeeringCommand : ExpressRouteBaseCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Key representing Azure Circuit for which BGP peering needs to be created/modified")]
        [ValidateGuid]
        [ValidateNotNullOrEmpty]
        public string ServiceKey { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "Peer Asn")]
        public UInt32 PeerAsn { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Primary Peer Subnet")]
        [ValidateNotNullOrEmpty]
        public string PrimaryPeerSubnet { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Secondary Peer Subnet")]
        [ValidateNotNullOrEmpty]
        public string SecondaryPeerSubnet { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Shared Key")]
        [ValidateNotNullOrEmpty]
        public string SharedKey { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Vlan Id")]
        public UInt32 VlanId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Bgp Peering Access Type: Public or Private")]
        [DefaultValue("Private")]
        public BgpPeeringAccessType AccessType { get; set; }

        public override void ExecuteCmdlet()
        {
            var route = ExpressRouteClient.NewAzureBGPPeering(ServiceKey, PeerAsn, PrimaryPeerSubnet,
                SecondaryPeerSubnet, VlanId, AccessType, SharedKey);
            WriteObject(route);
        }
    }
}
