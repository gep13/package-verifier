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
    public class VagrantService : IVagrantService
    {
        public void prep()
        {
            /*
             place and update vagrantfile
             vagrant up

             rename %CHOCOLATEYINSTALL%\logs\chocolatey.log chocolatey.log.old
             vagrant sandbox on
              
vagrant provision

choco install -dvy
rename chocolatey.log.install
rename chocolatey.log.uninstall

grab files
vagrant sandbox rollback
swap vagrantfile for next install
             */
        }

        public void dosomething()
        {
            /*
            swap choco action file
            vagrant provision

            rename chocolatey.log.install
            rename chocolatey.log.uninstall

            grab files
            vagrant sandbox rollback
            swap vagrantfile for next install
             */
        }

        public void reset()
        {
            /*
             vagrant sandbox rollback
             swap vagrantfile for next install
             */
        }
    }
}
