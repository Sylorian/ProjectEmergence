using System;
using System.Text;
using Hyperplasia.Core.Data;

namespace Hyperplasia.Core.Flow.States
{
    /// <summary>
    /// 查看物品（InspectItem）
    /// </summary>
    public sealed class InspectItemState : IInputState
    {
        private const string PickableTag = "可拾取";

        private readonly InputController _c;

        public InspectItemState(InputController c)
        {
            _c = c;
        }

        public InputState StateId { get { return InputState.InspectItem; } }

        public void Enter()
        {
            _c.Router.TypingEnabled = false;
            _c.TyperEngine.Buffer.Clear();
            _c.TyperEngine.ClearCandidates();

            // 先解析并显示查看内容，再执行拾取（拾取会从数据库移除实体）
            Entity entity = ResolveInspectedEntity();
            _c.TyperEngine.RaiseInspection(BuildInspectionText(entity));

            if (entity != null && entity.HasTag(PickableTag) &&
                _c.Session.CollectToInventory(entity.Id))
            {
                _c.TyperEngine.RaiseFeedback("已拾取：" + entity.Name);
            }
        }

        public void OnText(string text)
        {
        }

        public void OnCommand(InputCommand command)
        {
            switch (command)
            {
                case InputCommand.Cancel:
                    _c.TransitionTo(InputState.Explore);
                    break;
                case InputCommand.Backspace:
                    _c.TransitionTo(InputState.FirstWord);
                    break;
            }
        }

        public void Exit()
        {
        }

        /// <summary>取当前查看目标对应的实体（FirstWord 为名词候选）。</summary>
        private Entity ResolveInspectedEntity()
        {
            WordCandidate word = _c.TyperEngine.FirstWord;
            if (word == null) return null;

            return _c.Session.Database.EntityById.TryGetValue(word.Id, out Entity entity)
                ? entity
                : null;
        }

        private string BuildInspectionText(Entity e)
        {
            if (e == null) return null;

            var sb = new StringBuilder();
            sb.Append(e.Name);
            sb.Append('：');
            if (e.Tags == null || e.Tags.Count == 0)
            {
                sb.Append("（无标签）");
            }
            else
            {
                foreach (string tag in e.Tags)
                {
                    sb.Append("[");
                    sb.Append(tag);
                    sb.Append("]");
                }
            }

            sb.Append('\n');
            var text = ResolveClue(e);
            sb.Append(text);
            return sb.ToString();
        }

        private string ResolveClue(Entity e)
        {
            if (e.Clues == null || e.Clues.Length == 0) return "";
            return e.Id switch
            {
                0 => ResolveClueWindow(e),  // 窗户
                1 => ResolveClueTypewriter(e),  // 打字机
                2 => ResolveClueSafe(e),  // 密码箱
                9 => RandomClue(e),  // 收音机
                13 => RandomClue(e),  // 电视机
                15 => ResolveClueVase(e),  // 花瓶
                16 => ResolveClueClock(e),  // 挂钟
                31 => ResolveClueMirror(e),  // 镜子
                38 => ResolveClueKnife(e),  // 刀
                _ => e.Clues[0],
            };
        }

        private string RandomClue(Entity e)
        {
            if (e.Clues == null || e.Clues.Length == 0) return "";
            var random = new Random();
            int index = random.Next(e.Clues.Length);
            return e.Clues[index];
        }

        private string ResolveClueWindow(Entity e)
        {
            if (e.HasTag("破碎") && !e.HasTag("已连接"))
            {
                return e.Clues[1];
            }
            if (e.HasTag("破碎") && e.HasTag("已连接"))
            {
                return e.Clues[2];
            }
            return e.Clues[0];
        }

        private string ResolveClueTypewriter(Entity e)
        {
            if (e.HasTag("已查看"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }

        private string ResolveClueSafe(Entity e)
        {
            if (e.HasTag("已解锁"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }

        private string ResolveClueVase(Entity e)
        {
            if (e.HasTag("破碎"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }

        private string ResolveClueClock(Entity e)
        {
            if (e.HasTag("停滞"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }

        private string ResolveClueMirror(Entity e)
        {
            if (e.HasTag("破碎"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }

        private string ResolveClueKnife(Entity e)
        {
            if (e.HasTag("带血"))
            {
                return e.Clues[1];
            }
            return e.Clues[0];
        }
    }
}