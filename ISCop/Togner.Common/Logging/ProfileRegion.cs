using System;
using System.Diagnostics;
using log4net;

namespace Togner.Common.Logging
{
    /// <summary>
    /// This class describes a profiled region.
    /// </summary>
    public class ProfileRegion : IDisposable
    {
		private const string EndLogKeyWord = " end - ";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProfileRegion));
        private Stopwatch _watch;
        private TimeSpan _limit = new TimeSpan(0, 0, 30);
        private string mregionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ProfileRegion(string name)
        {
            this.mregionName = name;
            if (ProfileRegion.Logger.IsDebugEnabled)
            {
                this._watch = Stopwatch.StartNew();
                ProfileRegion.Logger.Debug(string.Concat(this.mregionName, " start"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The max elapsed time before logging warning.</param>
        public ProfileRegion(string name, TimeSpan limit)
            : this(name)
        {
            this._limit = limit;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ProfileRegion"/> is reclaimed by garbage collection.
        /// </summary>
        ~ProfileRegion()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposed)
        {
            if (!disposed)
            {
                ProfileRegion.Logger.Error(string.Concat("Profile region ", this.mregionName, " not finalized by Dispose call!"));
            }
            if (this._watch != null)
            {
                this._watch.Stop();
            }
            else
            {
                return;
            }
            if (this._watch.Elapsed < this._limit)
            {
                if (ProfileRegion.Logger.IsDebugEnabled)
                {
                    ProfileRegion.Logger.Debug(string.Concat(this.mregionName, ProfileRegion.EndLogKeyWord, this._watch.Elapsed));
                }
            }
            else
            {
                if (ProfileRegion.Logger.IsWarnEnabled)
                {
                    ProfileRegion.Logger.Warn(string.Concat(this.mregionName, ProfileRegion.EndLogKeyWord, this._watch.Elapsed, " - over limit"));
                }
            }
        }
    }
}
