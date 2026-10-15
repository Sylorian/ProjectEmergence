using Hyperplasia.Core.Flow;
using UnityEngine;

namespace Hyperplasia.Presentation.Input
{
    public static class TextInputSources
    {
        public static ITextInputSource Create()
        {
#if ENABLE_INPUT_SYSTEM
            return new InputSystemTextSource();
#else
            throw new System.NotSupportedException("No suitable text input source available.");
#endif
        }
    }
}