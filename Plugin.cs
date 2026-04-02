using System;
using Exiled.API.Features;
using PlayerHandlers = Exiled.Events.Handlers.Player;
using ServerHandlers = Exiled.Events.Handlers.Server;

namespace LastHumanCassie
{
    public sealed class Plugin : Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "LastHumanCassie";
        public override string Author => "you";
        public override string Prefix => "last_human_cassie";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 0, 0);

        private EventHandlers _handlers;

        public override void OnEnabled()
        {
            Instance = this;
            _handlers = new EventHandlers(this);

            PlayerHandlers.Died += _handlers.OnDied;
            PlayerHandlers.Left += _handlers.OnLeft;
            PlayerHandlers.ChangingRole += _handlers.OnChangingRole;

            ServerHandlers.RoundStarted += _handlers.OnRoundStarted;
            ServerHandlers.RestartingRound += _handlers.OnRestartingRound;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PlayerHandlers.Died -= _handlers.OnDied;
            PlayerHandlers.Left -= _handlers.OnLeft;
            PlayerHandlers.ChangingRole -= _handlers.OnChangingRole;

            ServerHandlers.RoundStarted -= _handlers.OnRoundStarted;
            ServerHandlers.RestartingRound -= _handlers.OnRestartingRound;

            _handlers = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}
