using System;

namespace Hyperplasia.Core.Flow
{
    public sealed partial class ResolveEngine
    {
        public event Action OnLampTurnedOn;
        private void TurnOnLamp()
        {
            OnLampTurnedOn?.Invoke();
        }
        
        public event Action OnLampTurnedOff;

        private void TurnOffLamp()
        {
            OnLampTurnedOff?.Invoke();
        }

        public event Action OnWardrobeOpened;
        private void OpenWardrobe()
        {
            OnWardrobeOpened?.Invoke();
        }

        public event Action OnWardrobeClosed;
        private void CloseWardrobe()
        {
            OnWardrobeClosed?.Invoke();
        }

        public event Action OnTalkToOwl;
        private void TalkToOwl()
        {
            OnTalkToOwl?.Invoke();
        }
        
        public event Action OnSitOnSofa;
        private void SitOnSofa()
        {
            OnSitOnSofa?.Invoke();
        }

        public event Action OnStorageOpened;
        private void OpenStorage()
        {
            OnStorageOpened?.Invoke();
        }

        public event Action OnVaseBroken;
        private void BreakVase()
        {
            OnVaseBroken?.Invoke();
        }

        public event Action OnCleanerCanMixed;
        private void MixCleanerCan()
        {
            OnCleanerCanMixed?.Invoke();
        }

        public event Action OnMomPoisoned;
        private void PoisonMom()
        {
            OnMomPoisoned?.Invoke();
        }

        public event Action OnMomFed;
        private void FeedMom()
        {
            OnMomFed?.Invoke();
        }

        public event Action OnVentDisassembled;
        private void DisassembleVent()
        {
            OnVentDisassembled?.Invoke();
        }

        public event Action OnPhoneAnswered;
        private void AnswerPhone()
        {
            OnPhoneAnswered?.Invoke();
        }

        public event Action OnKnifeTaken;
        private void TakeKnife()
        {
            OnKnifeTaken?.Invoke();
        }

        public event Action OnMomStabbed;
        private void StabMom()
        {
            OnMomStabbed?.Invoke();
        }

        public event Action<int> OnDoorOpened;  // 参数为门实体的Id
        private void OpenDoor(int doorId)
        {
            OnDoorOpened?.Invoke(doorId);
        }

        public event Action<int> OnWindowBroken;  // 参数为窗户实体的Id
        private void BreakWindow(int windowId)
        {
            OnWindowBroken?.Invoke(windowId);
        }

        public event Action<int> OnEscapedWindow;  // 参数为窗户实体的Id
        private void EscapeWindow(int windowId)
        {
            OnEscapedWindow?.Invoke(windowId);
        }

        public event Action OnCandleLit;
        private void LightCandle()
        {
            OnCandleLit?.Invoke();
        }

        public event Action OnTableIgnited;
        private void IgniteTable()
        {
            OnTableIgnited?.Invoke();
        }
    }
}