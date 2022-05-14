using Godot;

namespace GodotModules 
{
    public static class InputExtensions 
    {
        private static Dictionary<ulong, string> _prevTexts = new Dictionary<ulong, string>();

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
                if (!_prevTexts.ContainsKey(id)) 
                {
                    lineEdit.Text = "";
                    lineEdit.CaretPosition = text.Length;
                    return null;
                }
                else
                {
                    lineEdit.Text = _prevTexts[id];
                    lineEdit.CaretPosition = text.Length;
                    return _prevTexts[id];
                }
            }

            _prevTexts[id] = text;
            return text;
        }

        /*public static int FilterNum(this LineEdit lineEdit, int maxRange, int minRange = 0) 
        {
            var text = lineEdit.Text;

            if (string.IsNullOrWhiteSpace(text))
                return minRange - 1;
        }*/
    }
}