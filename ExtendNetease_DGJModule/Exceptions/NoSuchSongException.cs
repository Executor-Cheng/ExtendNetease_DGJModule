namespace ExtendNetease_DGJModule.Exceptions
{
    public sealed class NoSuchSongException : PlaylistException
    {
        public NoSuchSongException() : this(0)
        {

        }

        public NoSuchSongException(long playlistId) : base(playlistId, "给定的歌曲不存在")
        {

        }
    }

}
