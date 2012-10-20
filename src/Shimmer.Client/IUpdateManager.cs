using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Shimmer.Core;

namespace Shimmer.Client
{
    [ContractClass(typeof(UpdateManagerContracts))]
    public interface IUpdateManager : IDisposable
    {
        /// <summary>
        /// Fetch the remote store for updates and compare against the current 
        /// version to determine what updates to download.
        /// </summary>
        /// <param name="ignoreDeltaUpdates">Set this flag if applying a release
        /// fails to fall back to a full release, which takes longer to download
        /// but is less error-prone.</param>
        /// <param name="progress">A Observer which can be used to report Progress - 
        /// will return values from 0-100 and Complete, or Throw</param>
        /// <returns>An UpdateInfo object representing the updates to install.
        /// </returns>
        IObservable<UpdateInfo> CheckForUpdate(bool ignoreDeltaUpdates, IObserver<int> progress);

        /// <summary>
        /// Download a list of releases into the local package directory.
        /// </summary>
        /// <param name="releasesToDownload">The list of releases to download, 
        /// almost always from UpdateInfo.ReleasesToApply.</param>
        /// <param name="progress">A Observer which can be used to report Progress - 
        /// will return values from 0-100 and Complete, or Throw</param>
        /// <returns>A completion Observable - either returns a single 
        /// Unit.Default then Complete, or Throw</returns>
        IObservable<Unit> DownloadReleases(IEnumerable<ReleaseEntry> releasesToDownload, IObserver<int> progress);

        /// <summary>
        /// Take an already downloaded set of releases and apply them, 
        /// copying in the new files from the NuGet package and rewriting 
        /// the application shortcuts.
        /// </summary>
        /// <param name="updateInfo">The UpdateInfo instance acquired from 
        /// CheckForUpdate</param>
        /// <param name="progress">A Observer which can be used to report Progress - 
        /// will return values from 0-100 and Complete, or Throw</param>
        /// <returns>A list of EXEs that should be started if this is a new 
        /// installation.</returns>
        IObservable<List<string>> ApplyReleases(UpdateInfo updateInfo, IObserver<int> progress);
    }

    [ContractClassFor(typeof(IUpdateManager))]
    public abstract class UpdateManagerContracts : IUpdateManager
    {
        public IDisposable AcquireUpdateLock()
        {
            return default(IDisposable);
        }

        public IObservable<UpdateInfo> CheckForUpdate(bool ignoreDeltaUpdates = false, IObserver<int> progress = null)
        {
            return default(IObservable<UpdateInfo>);
        }

        public IObservable<Unit> DownloadReleases(IEnumerable<ReleaseEntry> releasesToDownload, IObserver<int> progress = null)
        {
            // XXX: Why doesn't this work?
            Contract.Requires(releasesToDownload != null);
            Contract.Requires(releasesToDownload.Any());
            return default(IObservable<Unit>);
        }

        public IObservable<List<string>> ApplyReleases(UpdateInfo updateInfo, IObserver<int> progress = null)
        {
            Contract.Requires(updateInfo != null);
            return default(IObservable<List<string>>);
        }

        public void Dispose()
        {
        }
    }

    public static class UpdateManagerMixins
    {
        public static IObservable<ReleaseEntry> UpdateApp(this IUpdateManager This)
        {
            var ret = This.CheckForUpdate(false, null)
                .SelectMany(x => This.DownloadReleases(x.ReleasesToApply, null).TakeLast(1).Select(_ => x))
                .SelectMany(x => This.ApplyReleases(x, null).TakeLast(1).Select(_ => x.ReleasesToApply.MaxBy(y => y.Version).LastOrDefault()))
                .PublishLast();

            ret.Connect();
            return ret;
        }
    }
}