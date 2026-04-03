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
        private CoroutineHandle _pendingAnnouncementHandle;

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
            CheckLastHuman();
        }

        private void ResetState()
        {
            if (_pendingAnnouncementHandle.IsRunning)
                Timing.KillCoroutines(_pendingAnnouncementHandle);

            _announcedThisRound = false;
            _pendingAnnouncement = false;
            _wasLastHumanState = false;
            _pendingAnnouncementHandle = default;
        }

        private void CheckLastHuman()
        {
            if (!Round.IsStarted || Round.IsEnded)
                return;

            int humanCount = Player.List.Count(IsCountedHuman);
            bool isLastHumanNow = humanCount == 1;

            if (_plugin.Config.Debug)
                Log.Debug($"HumanCount={humanCount}, IsLastHumanNow={isLastHumanNow}, WasLastHumanState={_wasLastHumanState}, AnnouncedThisRound={_announcedThisRound}, Pending={_pendingAnnouncement}");

            if (!isLastHumanNow)
            {
                _wasLastHumanState = false;

                if (_pendingAnnouncement)
                {
                    _pendingAnnouncement = false;

                    if (_pendingAnnouncementHandle.IsRunning)
                        Timing.KillCoroutines(_pendingAnnouncementHandle);

                    _pendingAnnouncementHandle = default;

                    if (_plugin.Config.Debug)
                        Log.Debug("Cancelled pending last-human announcement because round is no longer in last-human state.");
                }

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

                    StartAnnouncementCheck();
                    break;

                case AnnouncementMode.RepeatWhenLastHuman:
                    if (_wasLastHumanState)
                        return;

                    StartAnnouncementCheck();
                    break;
            }
        }

        private void StartAnnouncementCheck()
        {
            _pendingAnnouncement = true;
            _pendingAnnouncementHandle = Timing.CallDelayed(_plugin.Config.AnnouncementDelay, ConfirmAndAnnounce);

            if (_plugin.Config.Debug)
                Log.Debug($"Started delayed last-human confirmation for {_plugin.Config.AnnouncementDelay} seconds.");
        }

        private void ConfirmAndAnnounce()
        {
            _pendingAnnouncement = false;
            _pendingAnnouncementHandle = default;

            if (!Round.IsStarted || Round.IsEnded)
                return;

            int humanCount = Player.List.Count(IsCountedHuman);
            bool isStillLastHuman = humanCount == 1;

            if (_plugin.Config.Debug)
                Log.Debug($"Delayed check: HumanCount={humanCount}, IsStillLastHuman={isStillLastHuman}");

            if (!isStillLastHuman)
            {
                _wasLastHumanState = false;
                return;
            }

            _wasLastHumanState = true;

            Cassie.Message(
                _plugin.Config.CassieMessage,
                isHeld: false,
                isNoisy: true,
                isSubtitles: _plugin.Config.Subtitles);

            if (_plugin.Config.Mode == AnnouncementMode.OncePerRound)
                _announcedThisRound = true;

            if (_plugin.Config.Debug)
                Log.Debug($"Played CASSIE announcement: {_plugin.Config.CassieMessage}");
        }

        private bool IsCountedHuman(Player player)
        {
            if (player == null || !player.IsVerified || !player.IsAlive)
                return false;

            return !player.IsScp && player.Role.Team != Team.Dead;
        }
    }
}
