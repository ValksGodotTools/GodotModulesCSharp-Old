using Godot;

namespace Valk.Modules
{
    public static class Validator
    {
        public static void ValidateNumber(LineEdit input, string newText, int maxRange, ref int num)
        {
            // do NOT use text.Clear() as it will trigger _on_NumAttempts_text_changed and cause infinite loop -> stack overflow
            if (newText == "")
            {
                num = 0;
                EditInputText(input, "");
                return;
            }

            if (!int.TryParse(newText.Trim(), out int numAttempts))
            {
                EditInputText(input, $"{num}");
                return;
            }

            if (newText.Length > maxRange.ToString().Length && numAttempts <= maxRange)
            {
                var spliced = newText.Remove(newText.Length - 1);
                num = int.Parse(spliced);
                EditInputText(input, spliced);
                return;
            }

            if (numAttempts > maxRange)
            {
                numAttempts = maxRange;
                EditInputText(input, $"{maxRange}");
            }

            num = numAttempts;
        }

        private static void EditInputText(LineEdit input, string text)
        {
            input.Text = text;
            input.CaretPosition = input.Text.Length;
        }
    }
}