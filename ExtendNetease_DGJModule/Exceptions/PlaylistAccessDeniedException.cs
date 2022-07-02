namespace ExtendNetease_DGJModule.Exceptions
{
    public sealed class PlaylistAccessDeniedException : PlaylistException
    {
        public PlaylistAccessDeniedException() : this(0)
        {

        }

        public PlaylistAccessDeniedException(long playlistId) : base(playlistId, "无权访问给定歌单")
        {

        }
    }
}
