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

namespace Microsoft.WindowsAzure.Commands.Utilities.Automation.Models
{
    using AutomationManagement = Microsoft.Azure.Management.Automation;

    /// <summary>
    /// The Runbook Definition.
    /// </summary>
    public class RunbookDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunbookDefinition"/> class.
        /// </summary>
        /// <param name="runbookVersion">
        /// The runbook version.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        public RunbookDefinition(AutomationManagement.Models.RunbookVersion runbookVersion, byte[] content)
        {
            this.RunbookVersion = new RunbookVersion(runbookVersion);
            this.Content = System.Text.Encoding.UTF8.GetString(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunbookDefinition"/> class.
        /// </summary>
        public RunbookDefinition()
        {
        }

        /// <summary>
        /// Gets or sets the runbook version.
        /// </summary>
        public RunbookVersion RunbookVersion { get; set; }

        /// <summary>
        /// Gets or sets the runbook version content.
        /// </summary>
        public string Content { get; set; }
    }
}