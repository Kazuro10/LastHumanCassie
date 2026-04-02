using System.ComponentModel;
using Exiled.API.Interfaces;

namespace LastHumanCassie
{
    public enum AnnouncementMode
    {
        OncePerRound = 0,
        RepeatWhenLastHuman = 1
    }

    public sealed class Config : IConfig
    {
        [Description("Enable or disable this plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable extra debug messages in the server console.")]
        public bool Debug { get; set; } = false;

        [Description("Text CASSIE will say when only one human is alive.")]
        public string CassieMessage { get; set; } = "$PITCH_0.90 attention . only 1 human alive detected . $PITCH_0.97 $STUTTER_0.043_0.13_5 commencing last survivor procedure";

        [Description("How many seconds to wait before playing the CASSIE announcement after last human is detected.")]
        public float AnnouncementDelay { get; set; } = 4f;

        [Description("If true, show subtitles for the CASSIE announcement.")]
        public bool Subtitles { get; set; } = true;

        [Description("OncePerRound = announce only the first time the round reaches 1 human.\nRepeatWhenLastHuman = announce every time the alive human count returns to 1.")]
        public AnnouncementMode Mode { get; set; } = AnnouncementMode.RepeatWhenLastHuman;
    }
}
