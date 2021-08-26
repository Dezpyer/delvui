﻿using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using ImGuiNET;


namespace DelvUI.Helpers
{
    class MpTickHelper
    {
        private readonly Framework _framework;
        private readonly ClientState _clientState;

        public const double ServerTickRate = 3;
        private const float PollingRate = 1 / 30f;
        private double _lastUpdate;
        private int _lastMpValue = -1;

        public double LastTick { get; private set; }

        public MpTickHelper(Framework framework, ClientState clientState) {
            _framework = framework;
            _clientState = clientState;
            _framework.Update += FrameworkOnOnUpdateEvent;
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            Debug.Assert(_clientState.LocalPlayer != null, "_clientState.LocalPlayer != null");

            var now = ImGui.GetTime();
            if (now - _lastUpdate < PollingRate) {
                return;
            }
            _lastUpdate = now;

            var mp = _clientState.LocalPlayer.CurrentMp;

            // account for lucid dreaming screwing up mp calculations
            var lucidDreamingActive = _clientState.LocalPlayer.StatusList.Any(e => e.StatusId == 1204);
            if (!lucidDreamingActive && _lastMpValue < mp) {
                LastTick = now;
            }
            else if (LastTick + ServerTickRate <= now) {
                LastTick += ServerTickRate;
            }

            _lastMpValue = (int) mp;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _framework.Update -= FrameworkOnOnUpdateEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}