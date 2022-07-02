using System;

namespace ExtendNetease_DGJModule.Exceptions
{
    public abstract class PlaylistException : Exception
    {
        public long PlaylistId { get; set; }

        protected PlaylistException(long playlistId) : this(playlistId, null)
        {
            PlaylistId = playlistId;
        }

        protected PlaylistException(long playlistId, string message) : base(message)
        {
            
        }
    }
}
