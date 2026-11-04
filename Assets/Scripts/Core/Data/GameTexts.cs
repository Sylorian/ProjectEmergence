namespace Hyperplasia.Core.Data
{
    /// <summary>
    /// 玩家可见文案的统一入口：Core 流程反馈与 View 提示都从这里读取。
    /// 数据标签（如“可拾取”“破碎”）属于游戏数据键，不放在这里。
    /// </summary>
    public static class GameTexts
    {
        #region Input Hints
        public const string START_HINT = "输入START以开始";
        public const string FIRST_WORD_HINT = "输入第一个词（Enter提交，输入EXIT退出）";
        public const string SECOND_WORD_HINT = "输入第二个词（Enter提交）";
        public const string INSPECT_HINT = "检视中（Esc返回，Backspace回退）";
        #endregion

        #region Feedback
        public const string DEFAULT_FALLBACK = "没有什么发生，也许换个说法？";
        public const string DEFAULT_PRE_FAIL = "还差些什么，现在做不到。";
        public const string NO_ACTION = "这行不通";
        #endregion

        #region UI
        public const string INK_AMOUNT_PREFIX = "剩余墨量: ";
        public const string PICKED_UP_PREFIX = "已拾取：";
        public const string NO_TAGS_TEXT = "（无标签）";
        #endregion
    }
}
