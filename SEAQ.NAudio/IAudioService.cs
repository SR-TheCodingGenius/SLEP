using SLEP.NAudio.Enums;
using System;
using System.Collections.Generic;

namespace SLEP.Audio
{
	public interface IAudioService
	{
		string _audioFileName { get; set; }
		bool IntializeNAudioLibrary(int latency);
		void PlayAudio();
		void PauseAudio();
		void StopAudio();
		TimeSpan GetCurrentTime();
		TimeSpan GetTotalDuration();
		void SetCurrentTime(TimeSpan timeSpan);
		void FadeInOut(WavePlayer fadein, WavePlayer fadeout, int milliSeconds);
		void FadeIn(WavePlayer fadein, double time);
		void FadeOut(WavePlayer fadeout, double time);
		void FadeoutOnMouseClicksAndPause(WavePlayer fadeout, double time);
		void FadersInOut(WavePlayer fadeout, double time);

		void CopyofSampleProvider(WavePlayer wavePlayer, float selectbegintime,  float selectendTime, float crossfadeDuration);
		void UpdateSelectionTimesonMouseClicks(float selectbegintime, float selectendtime);
		void CrossFadeAtEnds(WavePlayer crossfade);
		void MixWaveProviders(IList<WavePlayer> inputs);
		PlayBackState GetPlayBackState();
		void Dispose();
	}
}
