/* 
 * Copyright(c) 2016 - 2017 Puma Security, LLC (https://www.pumascan.com)
 * 
 * Project Leader: Eric Johnson (eric.johnson@pumascan.com)
 * Lead Developer: Eric Mead (eric.mead@pumascan.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

using Puma.Security.Rules.Common;
using Puma.Security.Rules.Common.Extensions;
using Puma.Security.Rules.Diagnostics;
using Puma.Security.Rules.Model;

namespace Puma.Security.Rules.Analyzer.Configuration.Forms
{
    [SupportedDiagnostic(DiagnosticId.SEC0005)]
    public class CrossAppRedirectAnalyzer : IConfigurationFileAnalyzer
    {
        private const string FORMS_SEARCH_EXPRESSION = "configuration/system.web/authentication[@mode='Forms']/forms";

        public IEnumerable<DiagnosticInfo> GetDiagnosticInfo(IEnumerable<ConfigurationFile> srcFiles,
            CancellationToken cancellationToken)
        {
            var result = new List<DiagnosticInfo>();

            foreach (var config in srcFiles)
            {
                //Search for the element in question
                XElement element = config.ProductionConfigurationDocument.XPathSelectElement(FORMS_SEARCH_EXPRESSION);
                XAttribute crossAppRedirects = element?.Attribute("enableCrossAppRedirects");

                //Default is false, so we can bail if not defined
                if (crossAppRedirects == null)
                    continue;

                //If the value is true, we have an issue
                if (string.Compare(crossAppRedirects.Value, "True", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var lineInfo = config.GetProductionLineInfo(element, FORMS_SEARCH_EXPRESSION);
                    result.Add(new DiagnosticInfo(config.Source.Path, lineInfo.LineNumber, element.ToString()));
                }
            }

            return result;
        }
    }
}