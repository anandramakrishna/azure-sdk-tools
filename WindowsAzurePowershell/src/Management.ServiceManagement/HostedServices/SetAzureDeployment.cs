﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Samples.WindowsAzure.ServiceManagement;
    using Helpers;
    using Cmdlets.Common;
    using Extensions;
    using Management.Model;

    /// <summary>
    /// Update deployment configuration, upgrade or status
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureDeployment")]
    public class SetAzureDeploymentCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public SetAzureDeploymentCommand()
        {
        }

        public SetAzureDeploymentCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Upgrade", HelpMessage = "Upgrade Deployment")]
        public SwitchParameter Upgrade
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Config", HelpMessage = "Change Configuration of Deployment")]
        public SwitchParameter Config
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Status", HelpMessage = "Change Status of Deployment")]
        public SwitchParameter Status
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Upgrade", ValueFromPipelineByPropertyName = true, HelpMessage = "Service name")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Config", ValueFromPipelineByPropertyName = true, HelpMessage = "Service name")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Status", ValueFromPipelineByPropertyName = true, HelpMessage = "Service name")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Upgrade", HelpMessage = "Package location. This parameter should have the local file path or URI to a .cspkg in blob storage whose storage account is part of the same subscription/project.")]
        [ValidateNotNullOrEmpty]
        public string Package
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Config", HelpMessage = "Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "Upgrade", HelpMessage = "Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
        [ValidateNotNullOrEmpty]
        public string Configuration
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ParameterSetName = "Upgrade", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "Config", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "Status", ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 5, ParameterSetName = "Upgrade", HelpMessage = "Upgrade mode. Auto | Manual")]
        [ValidateSet(UpgradeType.Auto, UpgradeType.Manual)]
        public string Mode
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = false, ParameterSetName = "Upgrade", HelpMessage = "Label name for the new deployment. Default: <Service Name> + <date time>")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        [Parameter(Position = 7, Mandatory = false, ParameterSetName = "Upgrade", HelpMessage = "Name of role to upgrade.")]
        public string RoleName
        {
            get;
            set;
        }

        [Parameter(Position = 8, Mandatory = false, ParameterSetName = "Upgrade", HelpMessage = "Force upgrade.")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "Status", HelpMessage = "New deployment status. Running | Suspended")]
        [ValidateSet(DeploymentStatus.Running, DeploymentStatus.Suspended, IgnoreCase = true)]
        public string NewStatus
        {
            get;
            set;
        }
        
        public void ExecuteCommand()
        {
            string configString = string.Empty;
            if (!string.IsNullOrEmpty(Configuration))
            {
                configString = Utility.GetConfiguration(Configuration);
            }
  
            // Upgrade Parameter Set
            if (string.Compare(ParameterSetName, "Upgrade", StringComparison.OrdinalIgnoreCase) == 0)
            {
                bool removePackage = false;
                CurrentSubscription = this.GetCurrentSubscription();
                var storageName = CurrentSubscription.CurrentStorageAccount;

                Uri packageUrl = null;
                if (Package.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    Package.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    packageUrl = new Uri(Package);
                }
                else
                {
                    if (string.IsNullOrEmpty(storageName))
                    {
                        throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
                    }

                    var progress = new ProgressRecord(0, "Please wait...", "Uploading package to blob storage");
                    WriteProgress(progress);
                    removePackage = true;
                    InvokeInOperationContext(() => packageUrl = RetryCall(s => AzureBlob.UploadPackageToBlob(this.Channel, storageName, s, Package)));
                }

                var upgradeDeploymentInput = new UpgradeDeploymentInput
                {
                    Mode = Mode ?? UpgradeType.Auto,
                    Configuration = configString,
                    PackageUrl = packageUrl,
                    Label = (Label != null) ? ServiceManagementHelper.EncodeToBase64String(Label) : ServiceManagementHelper.EncodeToBase64String(ServiceName),
                    Force = Force.IsPresent
                };

                if (!string.IsNullOrEmpty(RoleName))
                {
                    upgradeDeploymentInput.RoleToUpgrade = RoleName;
                }

                using (new OperationContextScope((IContextChannel)Channel))
                {
                    try
                    {
                        ExecuteClientAction(upgradeDeploymentInput, CommandRuntime.ToString(), s => this.Channel.UpgradeDeploymentBySlot(s, this.ServiceName, this.Slot, upgradeDeploymentInput), WaitForOperation);
                        if (removePackage == true)
                        {
                            this.RetryCall(s =>
                            AzureBlob.DeletePackageFromBlob(
                                    this.Channel,
                                    storageName,
                                    s,
                                    packageUrl));
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        this.WriteErrorDetails(ex);
                    }
                }
            }
            else if (string.Compare(this.ParameterSetName, "Config", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Config parameter set 
                var changeConfiguration = new ChangeConfigurationInput
                {
                    Configuration = configString
                };

                ExecuteClientActionInOCS(changeConfiguration, CommandRuntime.ToString(), s => this.Channel.ChangeConfigurationBySlot(s, this.ServiceName, this.Slot, changeConfiguration), WaitForOperation);
            }

            else
            {
                // Status parameter set
                var updateDeploymentStatus = new UpdateDeploymentStatusInput()
                {
                    Status = this.NewStatus
                };

                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.UpdateDeploymentStatusBySlot(s, this.ServiceName, this.Slot, updateDeploymentStatus), WaitForOperation);
            }
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

    }
}
