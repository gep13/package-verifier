// --------------------------------------------------------------------------------------------------------------------
// <copyright company="RealDimensions Software, LLC" file="FileWatcher.cs">
//   Copyright 2015 - Present RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
        private readonly IFileSystem fileSystem;
        private readonly Timer timer;
        private readonly string filePath;
        private bool renameFile = true;
        private double intervalInSeconds = 10;
        private bool disposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="fileSystem">
        /// The file system.
        /// </param>
        public FileWatcher(string filePath, IFileSystem fileSystem)
        {
            this.filePath = fileSystem.GetFullPath(filePath);
            this.fileSystem = fileSystem;
            this.timer = new Timer(TimeSpan.FromSeconds(this.intervalInSeconds).TotalMilliseconds);
            this.timer.Elapsed += this.TimerElapsed;
        }

        /// <summary>
        ///   Occurs when [file found event].
        /// </summary>
        public event FileFoundEventHandler FileFoundEvent;

        /// <summary>
        ///   Gets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get
            {
                return this.filePath;
            }
        }

        public bool RenameFile
        {
            get
            {
                return this.renameFile;
            }

            set
            {
                this.renameFile = value;
            }
        }

        /// <summary>
        ///   Gets or sets the Interval in seconds.
        /// </summary>
        /// <value>The Interval in seconds.</value>
        public double IntervalInSeconds
        {
            get
            {
                return this.intervalInSeconds;
            }

            set
            {
                this.intervalInSeconds = value;
                this.ChangeIntervalAndResetTimerIfRunning();
            }
        }

        /// <summary>
        ///   Looks for file and generates a notification.
        /// </summary>
        public void LookForFileAndGenerateNotification()
        {
            try
            {
                IList<string> filesToProcess = new List<string>();

                if (this.IsDirectory(this.FilePath))
                {
                    filesToProcess = this.fileSystem.GetFiles(this.FilePath, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    if (this.fileSystem.FileExists(this.FilePath))
                    {
                        filesToProcess.Add(this.FilePath);
                    }
                }

                if (filesToProcess.Count != 0)
                {
                    foreach (string file in filesToProcess)
                    {
                        var tempFile = file;
                        if (this.renameFile)
                        {
                            var renamedFile = string.Format(
                                "{0}_renamed{1}",
                                this.fileSystem.GetFileNameWithoutExtension(file),
                                this.fileSystem.GetExtension(file));
                            tempFile = this.fileSystem.PathCombine(this.fileSystem.GetDirectoryName(file), renamedFile);
                            this.fileSystem.FileMove(file, tempFile);
                        }

                        this.FileFoundEvent.Invoke(new FileFoundEventArgs(tempFile));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log()
                    .Error(
                        () =>
                        string.Format(
                            "Exception caught when watching for trigger files:{0}{1}",
                            Environment.NewLine,
                            ex));
            }
        }

        /// <summary>
        ///   Starts the timer.
        /// </summary>
        public void StartWatching()
        {
            this.timer.Start();
        }

        /// <summary>
        ///   Stops the timer.
        /// </summary>
        public void StopWatching()
        {
            this.timer.Stop();
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposing)
            {
                this.disposing = true;
                this.timer.Stop();
                this.timer.Dispose();
            }
        }

        /// <summary>
        /// Determines whether the specified file path is directory.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified file path is directory; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDirectory(string filePath)
        {
            return this.fileSystem.DirectoryExists(filePath);
        }

        /// <summary>
        ///   Changes the timer Interval and resets the timer if running.
        /// </summary>
        private void ChangeIntervalAndResetTimerIfRunning()
        {
            this.timer.Interval = this.intervalInSeconds;
            if (this.timer.Enabled)
            {
                this.StopWatching();
                this.StartWatching();
            }
        }

        /// <summary>
        /// Timers the elapsed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.
        /// </param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.StopWatching();
            this.LookForFileAndGenerateNotification();
            this.StartWatching();
        }
    }
}