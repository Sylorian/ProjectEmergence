using System;

namespace Hyperplasia.Core.Flow
{
    public sealed partial class ResolveEngine
    {
        public event Action<int> OnLampTurnedOn;
        private void TurnOnLamp(int lampId)
        {
            OnLampTurnedOn?.Invoke(lampId);
        }
        
        public event Action<int> OnLampTurnedOff;

        private void TurnOffLamp(int lampId)
        {
            OnLampTurnedOff?.Invoke(lampId);
        }

        public event Action<int> OnWardrobeOpened;
        private void OpenWardrobe(int wardrobeId)
        {
            OnWardrobeOpened?.Invoke(wardrobeId);
        }

        public event Action<int> OnWardrobeClosed;
        private void CloseWardrobe(int wardrobeId)
        {
            OnWardrobeClosed?.Invoke(wardrobeId);
        }

        public event Action<int> OnTalkToOwl;
        private void TalkToOwl(int owlId)
        {
            OnTalkToOwl?.Invoke(owlId);
        }
        
        public event Action<int> OnSitOnSofa;
        private void SitOnSofa(int sofaId)
        {
            OnSitOnSofa?.Invoke(sofaId);
        }

        public event Action<int> OnStorageOpened;
        private void OpenStorage(int storageId)
        {
            OnStorageOpened?.Invoke(storageId);
        }

        public event Action<int> OnVaseBroken;
        private void BreakVase(int vaseId)
        {
            OnVaseBroken?.Invoke(vaseId);
        }

        public event Action<int> OnCleanerCanMixed;
        private void MixCleanerCan(int cleanerCanId)
        {
            OnCleanerCanMixed?.Invoke(cleanerCanId);
        }

        public event Action<int> OnMomPoisoned;
        private void PoisonMom(int momId)
        {
            OnMomPoisoned?.Invoke(momId);
        }

        public event Action<int> OnMomFed;
        private void FeedMom(int momId)
        {
            OnMomFed?.Invoke(momId);
        }

        public event Action<int> OnVentDisassembled;
        private void DisassembleVent(int ventId)
        {
            OnVentDisassembled?.Invoke(ventId);
        }

        public event Action<int> OnPhoneAnswered;
        private void AnswerPhone(int phoneId)
        {
            OnPhoneAnswered?.Invoke(phoneId);
        }

        public event Action<int> OnKnifeTaken;
        private void TakeKnife(int knifeId)
        {
            OnKnifeTaken?.Invoke(knifeId);
        }

        public event Action<int> OnMomStabbed;
        private void StabMom(int momId)
        {
            OnMomStabbed?.Invoke(momId);
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

        public event Action<int> OnCandleLit;
        private void LightCandle(int candleId)
        {
            OnCandleLit?.Invoke(candleId);
        }

        public event Action<int> OnTableIgnited;
        private void IgniteTable(int tableId)
        {
            OnTableIgnited?.Invoke(tableId);
        }
    }
}