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
    using System.Diagnostics;
    using System.IO;
    using configuration;
    using filesystem;
    using Imgur.API;
    using Imgur.API.Authentication.Impl;
    using Imgur.API.Endpoints.Impl;
    using Imgur.API.Models;

    public class ImageUploadService : IImageUploadService
    {
        private readonly IConfigurationSettings _configuration;
        private readonly IFileSystem _fileSystem;

        public ImageUploadService(IConfigurationSettings configuration, IFileSystem fileSystem)
        {
            _configuration = configuration;
            _fileSystem = fileSystem;
        }
        
        public string upload_image(string imageLocation)
        {
            var imageLink = string.Empty;

            try
            {
                var client = new ImgurClient(_configuration.ImageUploadClientId,_configuration.ImageUploadClientSecret);
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var imageStream = new FileStream(imageLocation, FileMode.Open,FileAccess.Read))
                {
                    image = endpoint.UploadImageStreamAsync(imageStream).GetAwaiter().GetResult();
                }

                imageLink = image.Link;

                this.Log().Debug("Image uploaded to '{0}' from '{1}'".format_with(imageLink, imageLocation));
            }
            catch (ImgurException ex)
            {
                this.Log().Warn("Upload failed for image:{0} {1}".format_with(Environment.NewLine,ex.Message));
            }

            return imageLink;
        }
    }
}
