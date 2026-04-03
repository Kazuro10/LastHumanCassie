# This plugin was made by AI!

# LastHumanCassie

Plays a **C.A.S.S.I.E.** announcement when only **one human remains alive**.

## Configuration

```yaml
# Enable or disable this plugin.
is_enabled: true
# Enable extra debug messages in the server console.
debug: false
# Text CASSIE will say when only one human is alive.
cassie_message: '$PITCH_0.90 attention . only 1 human alive detected . $PITCH_0.97 $STUTTER_0.043_0.13_5 commencing last survivor procedure'
# How many seconds to wait before playing the CASSIE announcement after last human is detected.
announcement_delay: 4
# If true, show subtitles for the CASSIE announcement.
subtitles: true
# OncePerRound = announce only the first time the round reaches 1 human.
# RepeatWhenLastHuman = announce every time the alive human count returns to 1.
mode: RepeatWhenLastHuman
```

## Installation

1. Download `LastHumanCassie.dll`
2. Put it in your EXILED plugins folder
3. Restart the server
