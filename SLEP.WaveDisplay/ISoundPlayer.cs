using System.ComponentModel;

namespace SLEP.WaveDisplay
{
	public interface ISoundPlayer : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets whether the sound player is currently playing audio.
        /// </summary>
        bool IsPlaying { get; }
    }
}
