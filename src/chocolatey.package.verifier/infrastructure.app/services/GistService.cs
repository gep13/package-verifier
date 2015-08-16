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
    using domain;
    using Octokit;

    public class GistService : IGistService
    {
        public async Task<Uri> CreateGist(string description, bool isPublic, IList<PackageTestLog> logs)
        {
            var gitHubClient = this.CreateGitHubClient();

            var gist = new NewGist();
            gist.Description = description;
            gist.Public = isPublic;

            foreach (var log in logs)
            {
                gist.Files.Add(log.Name, log.Contents);
            }

            var createdGist = await gitHubClient.Gist.Create(gist);
            return new Uri(createdGist.HtmlUrl);
        }

        private GitHubClient CreateGitHubClient()
        {
            // TODO: What sort of error handling do we want around this?  Can we assume that these values will be correctly set?
            var credentials = new Credentials(ApplicationParameters.GitHubUserName, ApplicationParameters.GitHubPassword);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("ChocolateyPackageVerifier")) { Credentials = credentials };
            return gitHubClient;
        }
    }
}