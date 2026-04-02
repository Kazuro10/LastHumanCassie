using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;

namespace LastHumanCassie
{
    public sealed class EventHandlers
    {
        private readonly Plugin _plugin;

        private bool _announcedThisRound;
        private bool _pendingAnnouncement;
        private bool _wasLastHumanState;
        private CoroutineHandle _delayCoroutine;

        public EventHandlers(Plugin plugin)
        {
            _plugin = plugin;
        }

        public void OnRoundStarted()
        {
            ResetState();
        }

        public void OnRestartingRound()
        {
            ResetState();
        }

        public void OnDied(DiedEventArgs ev)
        {
            CheckLastHuman();
        }

        public void OnLeft(LeftEventArgs ev)
        {
            CheckLastHuman();
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(0.1f, CheckLastHuman);
        }

        private void ResetState()
        {
            _announcedThisRound = false;
            _pendingAnnouncement = false;
            _wasLastHumanState = false;

            if (_delayCoroutine.IsRunning)
                Timing.KillCoroutines(_delayCoroutine);
        }

        private void CheckLastHuman()
        {
            if (!Round.IsStarted || Round.IsEnded)
                return;

            int humanCount = Player.List.Count(IsCountedHuman);
            bool isLastHumanNow = humanCount == 1;

            if (_plugin.Config.Debug)
            {
                Log.Debug(
                    $"HumanCount={humanCount}, IsLastHumanNow={isLastHumanNow}, WasLastHumanState={_wasLastHumanState}, AnnouncedThisRound={_announcedThisRound}, Pending={_pendingAnnouncement}",
                    _plugin.Config.Debug);
            }

            if (!isLastHumanNow)
            {
                _wasLastHumanState = false;
                _pendingAnnouncement = false;

                if (_delayCoroutine.IsRunning)
                    Timing.KillCoroutines(_delayCoroutine);

                return;
            }

            if (_pendingAnnouncement)
                return;

            switch (_plugin.Config.Mode)
            {
                case AnnouncementMode.OncePerRound:
                    if (_announcedThisRound)
                    {
                        _wasLastHumanState = true;
                        return;
                    }

                    StartAnnouncement();
                    break;

                case AnnouncementMode.RepeatWhenLastHuman:
                    if (_wasLastHumanState)
                        return;

                    StartAnnouncement();
                    break;
            }
        }

        private void StartAnnouncement()
        {
            _pendingAnnouncement = true;
            _wasLastHumanState = true;

            if (_delayCoroutine.IsRunning)
                Timing.KillCoroutines(_delayCoroutine);

            _delayCoroutine = Timing.RunCoroutine(DelayedAnnouncement());
        }

        private IEnumerator<float> DelayedAnnouncement()
        {
            yield return Timing.WaitForSeconds(_plugin.Config.AnnouncementDelay);

            _pendingAnnouncement = false;

            if (!Round.IsStarted || Round.IsEnded)
                yield break;

            int humanCount = Player.List.Count(IsCountedHuman);
            if (humanCount != 1)
            {
                _wasLastHumanState = false;
                yield break;
            }

            Cassie.Message(_plugin.Config.CassieMessage, isSubtitles: _plugin.Config.Subtitles);

            if (_plugin.Config.Mode == AnnouncementMode.OncePerRound)
                _announcedThisRound = true;

            if (_plugin.Config.Debug)
                Log.Debug($"CASSIE announced: {_plugin.Config.CassieMessage}", _plugin.Config.Debug);
        }

        private bool IsCountedHuman(Player player)
        {
            if (player == null || !player.IsVerified || !player.IsAlive)
                return false;

            return !player.IsScp && player.Role.Team != Team.Dead;
        }
    }
}
