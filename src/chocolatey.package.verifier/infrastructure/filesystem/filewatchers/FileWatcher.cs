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

namespace chocolatey.package.verifier.Infrastructure.FileSystem.FileWatchers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;

    /// <summary>
    ///   Implementation of IFileWatcher
    /// </summary>
    public class FileWatcher : IFileWatcher
    {
        private readonly string _filePath;
        private bool _renameFile = true;
        private double _intervalInSeconds = 10;
        private readonly IFileSystem _fileSystem;
        private readonly Timer _timer;
        private bool _disposing;


        /// <summary>
        ///   Initializes a new instance of the <see cref="FileWatcher" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileSystem">The file system.</param>
        public FileWatcher(string filePath, IFileSystem fileSystem)
        {
            _filePath = fileSystem.GetFullPath(filePath);
            _fileSystem = fileSystem;
            _timer = new Timer(TimeSpan.FromSeconds(_intervalInSeconds).TotalMilliseconds);
            _timer.Elapsed += TimerElapsed;
        }

        /// <summary>
        ///   Timers the elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The <see cref="System.Timers.ElapsedEventArgs" /> instance containing the event data.
        /// </param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            StopWatching();
            LookForFileAndGenerateNotification();
            StartWatching();
        }

        /// <summary>
        ///   Looks for file and generates a notification.
        /// </summary>
        public void LookForFileAndGenerateNotification()
        {
            try
            {
                IList<string> filesToProcess = new List<string>();

                if (IsDirectory(FilePath))
                {
                    filesToProcess = _fileSystem.GetFiles(FilePath, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    if (_fileSystem.FileExists(FilePath))
                    {
                        filesToProcess.Add(FilePath);
                    }
                }

                if (filesToProcess.Count != 0)
                {
                    foreach (string file in filesToProcess)
                    {
                        var tempFile = file;
                        if (_renameFile)
                        {
                            var renamedFile = string.Format("{0}_renamed{1}", _fileSystem.GetFileNameWithoutExtension(file), _fileSystem.GetExtension(file));
                            tempFile = _fileSystem.PathCombine(_fileSystem.GetDirectoryName(file), renamedFile);
                            _fileSystem.FileMove(file, tempFile);
                        }

                        FileFoundEvent.Invoke(new FileFoundEventArgs(tempFile));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log().Error(() => string.Format("Exception caught when watching for trigger files:{0}{1}", Environment.NewLine, ex));
            }
        }

        /// <summary>
        ///   Determines whether the specified file path is directory.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>
        ///   <c>true</c> if the specified file path is directory; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDirectory(string filePath)
        {
            return _fileSystem.DirectoryExists(filePath);
        }

        /// <summary>
        ///   Changes the timer interval and resets the timer if running.
        /// </summary>
        private void ChangeIntervalAndResetTimerIfRunning()
        {
            _timer.Interval = _intervalInSeconds;
            if (_timer.Enabled)
            {
                StopWatching();
                StartWatching();
            }
        }


        /// <summary>
        ///   Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get { return _filePath; }
        }

        public bool RenameFile
        {
            get { return _renameFile; }
            set { _renameFile = value; }
        }

        /// <summary>
        ///   Starts the timer.
        /// </summary>
        public void StartWatching()
        {
            _timer.Start();
        }

        /// <summary>
        ///   Stops the timer.
        /// </summary>
        public void StopWatching()
        {
            _timer.Stop();
        }

        /// <summary>
        ///   Gets or sets the interval in seconds.
        /// </summary>
        /// <value>The interval in seconds.</value>
        public double IntervalInSeconds
        {
            get { return _intervalInSeconds; }
            set
            {
                _intervalInSeconds = value;
                ChangeIntervalAndResetTimerIfRunning();
            }
        }

        /// <summary>
        ///   Occurs when [file found event].
        /// </summary>
        public event FileFoundEventHandler FileFoundEvent;

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposing)
            {
                _disposing = true;
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}