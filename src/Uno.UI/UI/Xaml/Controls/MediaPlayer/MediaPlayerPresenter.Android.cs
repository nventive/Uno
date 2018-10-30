using System;
using Android.Views;
using Uno.Media.Playback;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Windows.UI.Xaml.Controls
{
	public partial class MediaPlayerPresenter
	{
		private void SetVideoSurface(IVideoSurface videoSurface)
		{
			this.Child = videoSurface as SurfaceView;
		}

		private void OnStretchChanged(Stretch newValue, Stretch oldValue)
		{
			switch (newValue)
			{
				case Stretch.Uniform:
					MediaPlayer.UpdateVideoStretch(Windows.Media.Playback.MediaPlayer.VideoStretch.Uniform);
					break;

				case Stretch.Fill:
					MediaPlayer.UpdateVideoStretch(Windows.Media.Playback.MediaPlayer.VideoStretch.Fill);
					break;

				case Stretch.None:
					MediaPlayer.UpdateVideoStretch(Windows.Media.Playback.MediaPlayer.VideoStretch.None);
					break;

				case Stretch.UniformToFill:
					MediaPlayer.UpdateVideoStretch(Windows.Media.Playback.MediaPlayer.VideoStretch.UniformToFill);
					break;

				default:
					throw new NotSupportedException($"Strech mode {newValue} is not supported");
			}
		}
	}
}
