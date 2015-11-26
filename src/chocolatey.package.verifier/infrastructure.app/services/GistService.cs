// Copyright © 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.infrastructure.app.services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using configuration;
    using domain;
    using Octokit;

    public class GistService : IGistService
    {
        private readonly IConfigurationSettings _configuration;

        public GistService(IConfigurationSettings configuration)
        {
            _configuration = configuration;
        }

        public async Task<Uri> create_gist(string description, bool isPublic, IList<PackageTestLog> logs)
        {
            this.Log().Debug(() => "Creating gist with description '{0}'.".format_with(description.escape_curly_braces()));

            var gitHubClient = this.create_git_hub_client();

            var gist = new NewGist
            {
                Description = description,
                Public = isPublic
            };

            foreach (var log in logs)
            {
                gist.Files.Add(log.Name, log.Contents);
            }

            var createdGist = await gitHubClient.Gist.Create(gist); //.ConfigureAwait(continueOnCapturedContext:false);

            return new Uri(createdGist.HtmlUrl);
        }

        private GitHubClient create_git_hub_client()
        {
            // assume that these values will be correctly set
            Credentials credentials;
            if (!string.IsNullOrWhiteSpace(_configuration.GitHubToken)) credentials = new Credentials(_configuration.GitHubToken);
            else credentials = new Credentials(_configuration.GitHubUserName, _configuration.GitHubPassword);

            var gitHubClient = new GitHubClient(new ProductHeaderValue("ChocolateyPackageVerifier"))
            {
                Credentials = credentials
            };
            return gitHubClient;
        }
    }
}
