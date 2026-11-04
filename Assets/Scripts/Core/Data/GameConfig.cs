using Hyperplasia.Core.Flow;

namespace Hyperplasia.Core.Data
{
    public class GameConfig
    {
        public const int START_INK = 15;
        public const int INK_COST_PER_SENTENCE = 1;

        public const float START_GROWTH = 0f;
        public const int START_VIOLENCE = 0;
        public const float GROWTH_INCREASE_BASE_RATE = 0.01f;

        public const RoomEnum START_ROOM = RoomEnum.Bedroom;
        public const InputState START_INPUT_STATE = InputState.Start;
    }
}