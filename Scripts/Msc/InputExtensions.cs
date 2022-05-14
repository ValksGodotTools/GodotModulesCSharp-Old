using Godot;

namespace GodotModules 
{
    public static class InputExtensions 
    {
        private static Dictionary<ulong, string> _prevTexts = new();
        private static Dictionary<ulong, int> _prevNums = new();

        public static string Filter(this LineEdit lineEdit, Func<string, bool> filter) 
        {
            var text = lineEdit.Text;
            var id = lineEdit.GetInstanceId();

            if (string.IsNullOrWhiteSpace(text))
                if (_prevTexts.ContainsKey(id))
                    return _prevTexts[id];
                else
                    return null;

            if (!filter(text)) 
            {
                lineEdit.CaretPosition = text.Length;

                if (!_prevTexts.ContainsKey(id)) 
                {
                    lineEdit.Text = "";
                    return null;
                }
                else
                {
                    lineEdit.Text = _prevTexts[id];
                    return _prevTexts[id];
                }
            }

            _prevTexts[id] = text;
            return text;
        }

        public static int FilterRange(this LineEdit lineEdit, int maxRange, int minRange = 0) 
        {
            var text = lineEdit.Text;
            var id = lineEdit.GetInstanceId();

            if (string.IsNullOrWhiteSpace(text))
                return minRange - 1;

            if (text == "-")
                return minRange - 1;

            if (!int.TryParse(text.Trim(), out int numAttempts))
            {
                if (!_prevNums.ContainsKey(id)) 
                {
                    lineEdit.Text = "";
                    lineEdit.CaretPosition = "".Length;
                    return minRange - 1;
                }
                else
                {
                    lineEdit.Text = $"{_prevNums[id]}";
                    lineEdit.CaretPosition = $"{_prevNums[id]}".Length;
                    return _prevNums[id];
                }
            }

            if (text.Length > maxRange.ToString().Length && numAttempts <= maxRange)
            {
                var spliced = text.Remove(text.Length - 1);
                _prevNums[id] = int.Parse(spliced);

                lineEdit.Text = spliced;
                lineEdit.CaretPosition = spliced.Length;
                return _prevNums[id];
            }

            if (numAttempts > maxRange)
            {
                numAttempts = maxRange;
                lineEdit.Text = $"{maxRange}";
                lineEdit.CaretPosition = $"{maxRange}".Length;
            }

            if (numAttempts < minRange) 
            {
                numAttempts = minRange;
                lineEdit.Text = $"{minRange}";
                lineEdit.CaretPosition = $"{minRange}".Length;
            }

            _prevNums[id] = numAttempts;
            return numAttempts;
        }
    }
}