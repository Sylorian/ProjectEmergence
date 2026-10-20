using System;

namespace Hyperplasia.Core.Flow
{
    public sealed partial class ResolveEngine
    {
        public event Action<int> OnLampTurnedOn;  // 参数为台灯实体的Id
        private void TurnOnLamp(int lampId)
        {
            OnLampTurnedOn?.Invoke(lampId);
        }
        
        public event Action<int> OnLampTurnedOff;  // 参数为台灯实体的Id

        private void TurnOffLamp(int lampId)
        {
            OnLampTurnedOff?.Invoke(lampId);
        }

        public event Action<int> OnWardrobeOpened;  // 参数为衣柜实体的Id
        private void OpenWardrobe(int wardrobeId)
        {
            OnWardrobeOpened?.Invoke(wardrobeId);
        }

        public event Action<int> OnWardrobeClosed;  // 参数为衣柜实体的Id
        private void CloseWardrobe(int wardrobeId)
        {
            OnWardrobeClosed?.Invoke(wardrobeId);
        }

        public event Action<int> OnTalkToOwl;  // 参数为猫头鹰实体的Id
        private void TalkToOwl(int owlId)
        {
            OnTalkToOwl?.Invoke(owlId);
        }
        
        public event Action<int> OnSitOnSofa;  // 参数为沙发实体的Id
        private void SitOnSofa(int sofaId)
        {
            OnSitOnSofa?.Invoke(sofaId);
        }

        public event Action<int> OnStorageOpened;  // 参数为储物柜实体的Id
        private void OpenStorage(int storageId)
        {
            OnStorageOpened?.Invoke(storageId);
        }

        public event Action<int> OnVaseBroken;  // 参数为花瓶实体的Id
        private void BreakVase(int vaseId)
        {
            OnVaseBroken?.Invoke(vaseId);
        }

        public event Action<int> OnCleanerCanMixed;  // 参数为罐头实体的Id
        private void MixCleanerCan(int cleanerCanId)
        {
            OnCleanerCanMixed?.Invoke(cleanerCanId);
        }

        public event Action<int> OnMomPoisoned;  // 参数为妈妈实体的Id
        private void PoisonMom(int momId)
        {
            OnMomPoisoned?.Invoke(momId);
        }

        public event Action<int> OnMomFed;  // 参数为妈妈实体的Id
        private void FeedMom(int momId)
        {
            OnMomFed?.Invoke(momId);
        }

        public event Action<int> OnVentDisassembled;  // 参数为通风口实体的Id
        private void DisassembleVent(int ventId)
        {
            OnVentDisassembled?.Invoke(ventId);
        }

        public event Action<int> OnPhoneAnswered;  // 参数为电话实体的Id
        private void AnswerPhone(int phoneId)
        {
            OnPhoneAnswered?.Invoke(phoneId);
        }

        public event Action<int> OnKnifeTaken;  // 参数为刀实体的Id
        private void TakeKnife(int knifeId)
        {
            OnKnifeTaken?.Invoke(knifeId);
        }

        public event Action<int> OnMomStabbed;  // 参数为妈妈实体的Id
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

        public event Action<int> OnCandleLit;  // 参数为蜡烛实体的Id
        private void LightCandle(int candleId)
        {
            OnCandleLit?.Invoke(candleId);
        }

        public event Action<int> OnTableIgnited;  // 参数为桌子实体的Id
        private void IgniteTable(int tableId)
        {
            OnTableIgnited?.Invoke(tableId);
        }
    }
}