using Godot;
using System;

namespace GodotModules
{
    public static class GodotExtensions
    {
        public static string Validate(this string value, ref string previousValue, TextEdit input, Func<bool> condition)
        {
            if (value.Empty())
            {
                input.Text = "";
                return null;
            }

            if (!condition())
            {
                input.Text = previousValue;
                return null;
            }

            previousValue = value;
            return value;
        }

        public static string Validate(this string value, ref string previousValue, LineEdit input, Func<bool> condition)
        {
            if (value.Empty())
            {
                input.Text = "";
                return null;
            }

            if (!condition())
            {
                input.Text = previousValue;
                input.CaretPosition = value.Length;
                return null;
            }

            previousValue = value;
            return value;
        }

        public static void ValidateNumber(this string value, LineEdit input, int min, int max, ref int num)
        {
            // do NOT use text.Clear() as it will trigger _on_NumAttempts_text_changed and cause infinite loop -> stack overflow
            if (value.Empty())
            {
                num = 0;
                EditInputText(input, "");
                return;
            }

            if (!int.TryParse(value.Trim(), out int numAttempts))
            {
                EditInputText(input, $"{num}");
                return;
            }

            if (value.Length > max.ToString().Length && numAttempts <= max)
            {
                var spliced = value.Remove(value.Length - 1);
                num = int.Parse(spliced);
                EditInputText(input, spliced);
                return;
            }

            if (numAttempts < min)
            {
                numAttempts = min;
                EditInputText(input, $"{min}");
            }

            if (numAttempts > max)
            {
                numAttempts = max;
                EditInputText(input, $"{max}");
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