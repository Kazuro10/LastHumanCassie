using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;

namespace LastHumanCassie
{
    public sealed class EventHandlers
    {
        private readonly Plugin _plugin;

        private bool _announcedThisRound;
        private bool _pendingAnnouncement;
        private bool _wasLastHumanState;

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
            _announcedThisRound = false;
            _pendingAnnouncement = false;
            _wasLastHumanState = false;
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
                _pendingAnnouncement = false;
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

            Exiled.API.Features.Cassie.DelayedMessage(
                _plugin.Config.CassieMessage,
                _plugin.Config.AnnouncementDelay,
                isHeld: false,
                isNoisy: true,
                isSubtitles: _plugin.Config.Subtitles);

            _pendingAnnouncement = false;

            if (_plugin.Config.Mode == AnnouncementMode.OncePerRound)
                _announcedThisRound = true;

            if (_plugin.Config.Debug)
                Log.Debug($"Scheduled CASSIE announcement: {_plugin.Config.CassieMessage}");
        }

        private bool IsCountedHuman(Player player)
        {
            if (player == null || !player.IsVerified || !player.IsAlive)
                return false;

            return !player.IsScp && player.Role.Team != Team.Dead;
        }
    }
}
