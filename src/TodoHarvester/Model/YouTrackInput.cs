namespace TodoHarvester.Model
{
	public sealed class YouTrackInput : TodoInput
	{
		[Oakton.Description("YouTrack url")]
		[Oakton.FlagAlias("yurl", true)]
		public string YouTrackUriFlag;
		[Oakton.Description("YouTrack user")]
		[Oakton.FlagAlias("yuser", true)]
		public string YouTrackUserFlag;
		[Oakton.Description("YouTrack password")]
		[Oakton.FlagAlias("ypw", true)]
		public string YouTrackPasswordFlag;
		[Oakton.Description("YouTrack Project")]
		[Oakton.FlagAlias("yproj", true)]
		public string YouTrackProjectFlag;
	}
}