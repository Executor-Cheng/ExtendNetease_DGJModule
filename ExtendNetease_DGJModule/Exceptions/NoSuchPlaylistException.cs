namespace ExtendNetease_DGJModule.Exceptions
{
    public sealed class NoSuchPlaylistException : PlaylistException
    {
        public NoSuchPlaylistException() : this(0)
        {

        }

        public NoSuchPlaylistException(long playlistId) : base(playlistId, "给定的歌单不存在")
        {

        }
    }
}
